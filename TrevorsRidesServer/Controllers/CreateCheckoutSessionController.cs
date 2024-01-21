using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.Net;
using System.Text;
using TrevorsRidesServer.Models;
using TrevorsRidesHelpers.GoogleApiClasses;
using System.Text.Json;
using System.Net.Http.Headers;
using TrevorsRidesHelpers;
using Help = TrevorsRidesHelpers;
using Stripe.Checkout;
using TrevorsRidesHelpers.Ride;
using Microsoft.EntityFrameworkCore;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CreateCheckoutSessionController : ControllerBase
    {
        private ILogger _logger;
        public CreateCheckoutSessionController(ILogger<CreateCheckoutSessionController> logger)
        {
            _logger = logger;
            StripeConfiguration.ApiKey = APIKeys.StripeSecretAPIKey;
        }
        [FromHeader(Name="EmailOfRequestedDriver")]
        public string? DriverEmail { get; set; }
        private Guid driverId;

        [HttpPut]
        public async Task PutTripRequest([FromHeader(Name = "User-ID")] Guid userId, [FromHeader(Name = "Session-Token")] string token, [FromBody] TripRequest tripRequest)
        {
            if (RideMatchingService.IsShuttingDown)
            {
                Response.StatusCode = (int)HttpStatusCode.BadRequest;
                await Response.WriteAsync(RideMatchingService.ServiceIsShuttingDownMessage);
                return;
            }
            using (RidesModel context = new RidesModel())
            {
                if (string.IsNullOrEmpty(DriverEmail))
                {
                    driverId = RideMatchingService.TrevorsId;
                }
                else
                {
                    try
                    {
                        driverId = context.DriverAccounts.Single(e => e.Email == DriverEmail).Id;
                    }
                    catch
                    {
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await Response.WriteAsync("Driver does not exist");
                        return;
                    }
                    
                }
                Guid rideId = Guid.NewGuid();
                RideInProgress ride;
                SessionService service = new SessionService();
                HttpClient client = new HttpClient();
                Session session;
                //Verify User Credentials
                RiderAccountEntry account;
                try
                {
                    account = context.RiderAccounts.Where(e => e.Id == userId).ToList().Single();
                }
                catch (InvalidOperationException)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await Response.WriteAsync("User does not exist");
                    return;
                }
                if (!account.VerifySessionToken(token))
                {
                    foreach (var hashedToken in account.SessionTokens)
                    {
                        _logger.LogDebug($"Account HashedToken: {hashedToken.Token}");
                    }
                    _logger.LogDebug($"Account Unhashed Token: {token}");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await Response.WriteAsync("Invalid Token");
                    return;
                }



                //Verify this is the rider's only active trip
                if (context.RidesInProgress.Where(e => e.RiderID == userId).ToList().Count() != 0)
                {
                    try
                    {
                        ride = context.RidesInProgress.Single(e => e.RiderID == userId);
                        if (ride.Status == RideEventType.Requested)
                        {

                            try
                            {
                                session = await service.GetAsync(ride.CheckoutSessionId);
                                if ( session.Status == "complete")
                                {
                                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    await Response.WriteAsync("Only one trip request per rider");
                                    return;
                                } else if (session.Status == "open")
                                {
                                    await service.ExpireAsync(ride.CheckoutSessionId);
                                }
                                else
                                {
                                    //It's already expired remove from rides In progress
                                }
                                
                            }
                            catch (StripeException)
                            {
                                session = await service.GetAsync(ride.CheckoutSessionId);
                                if (session.Status == "complete") //Between the milliseconds we retrieved it from the database to the time we tried to expire the session the payment was processed or there was an error marking the trip as Paid
                                {
                                    _logger.LogInformation("sessionCompleted");
                                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    await Response.WriteAsync("Only one trip request per rider");
                                    return;
                                }
                                else if (session.Status == "expired")
                                {
                                    //It's already expired remove from rides In progress
                                }


                            }
                            finally
                            {
                                context.RidesInProgress.Remove(ride);
                                await context.SaveChangesAsync();
                                _logger.LogInformation("Trip Removed");
                            }


                        }
                        else
                        {
                            _logger.LogInformation("Trip in progress");
                            Response.StatusCode = (int)HttpStatusCode.BadRequest;
                            await Response.WriteAsync("Only one trip request per rider");
                            return;
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex.GetType().ToString());
                        _logger.LogError(ex.Message);
                        _logger.LogError(ex.StackTrace);
                        Response.StatusCode = (int)HttpStatusCode.BadRequest;
                        await Response.WriteAsync("An Error Has Occured");
                        return;
                    }

                }

                //Get google route
                Waypoint[] intermediates = new Waypoint[tripRequest.Stops.Length];
                for (int i = 0; i < tripRequest.Stops.Length; i++)
                {
                    intermediates[i] = tripRequest.Stops[i].ToWaypoint();
                }
                RoutesRequest routesRequest = new RoutesRequest()
                {
                    origin = tripRequest.Pickup.ToWaypoint(),
                    destination = tripRequest.Dropoff.ToWaypoint(),
                    intermediates = intermediates
                };
                JsonContent googleContent = JsonContent.Create<RoutesRequest>(routesRequest);
                
                Uri uri = new Uri("https://routes.googleapis.com/directions/v2:computeRoutes");
                googleContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                googleContent.Headers.Add("X-Goog-Api-Key", APIKeys.GoogleEverythingKey);
                googleContent.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.distanceMeters,routes.legs.steps.startLocation,routes.legs.steps.endLocation");
                HttpResponseMessage googleResponse = await client.PostAsync(uri, googleContent);
                _logger.LogDebug($"Google Response: {await googleResponse.Content.ReadAsStringAsync()}");
                RoutesResponse routesResponse = await googleResponse.Content.ReadFromJsonAsync<RoutesResponse>();


                //Calculate Route Cost From Response
                int TotalRideCostCents =
                    (int)(RideCost.CentsPerMinute * double.Parse(routesResponse!.routes[0].duration!.Substring(0, routesResponse!.routes[0].duration!.Length - 1)) / 60
                    + RideCost.CentsPerMile * routesResponse.routes[0].distanceMeters / 1609)!;

                TotalRideCostCents = RideCost.CostInCents(routesResponse!.routes[0].distanceMeters!.Value, routesResponse!.routes[0].duration!);


                
                PaymentIntentCreateOptions payment = new PaymentIntentCreateOptions() //TODO: Create a payment intent to capture funds later
                {

                };


                //Create Checkout Session
                SessionCreateOptions options = new SessionCreateOptions()
                {
                    LineItems = new List<SessionLineItemOptions>()
                    {
                        new SessionLineItemOptions()
                        {
                            PriceData = new SessionLineItemPriceDataOptions()
                            {
                                UnitAmount = TotalRideCostCents,
                                Currency = "usd",
                                ProductData = new SessionLineItemPriceDataProductDataOptions()
                                {
                                    Name = $"Trip from {tripRequest.Pickup.Name} to {tripRequest.Dropoff.Name} @ {DateTime.UtcNow}"
                                }



                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = "https://www.example.com/success/",
                    CustomerEmail = account.Email,
                    Metadata = new Dictionary<string, string>() { 
                        { $"{Help.Stripe.TripIdKey}", rideId.ToString() },
                        { $"{Help.Stripe.IsLiveKey}", $"{Helpers.IsLive}" },
                        { Help.Stripe.IdOfRequestedDriverKey, driverId.ToString() }
                    }
                };


                session = await service.CreateAsync(options);

                _logger.LogInformation(session.Url);

                ride = new RideInProgress(rideId, userId, session.Id, tripRequest.Pickup, tripRequest.Dropoff, tripRequest.Stops);
                try
                {
                    context.RidesInProgress.Add(ride);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex.Message);
                    _logger.LogError(ex.StackTrace);
                    if (ex.InnerException != null)
                    {
                        _logger.LogError(ex.InnerException.Message);
                        _logger.LogError(ex.InnerException.StackTrace);
                    }
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    await Response.WriteAsync("An Error Occured Saving the RideInProgress to the Database");
                    return;
                }

                await Response.WriteAsync(session.Url);

                return;
            }
        }
    }
}
