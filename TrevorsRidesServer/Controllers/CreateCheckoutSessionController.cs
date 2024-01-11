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
        //[HttpPut]
        //public async Task Put([FromHeader(Name = "User-ID")] Guid userId, [FromHeader(Name = "Session-Token")] string token, [FromBody] RoutesRequest routesRequest)
        //{
        //    Guid rideId = Guid.NewGuid();
        //    using (RidesModel context = new RidesModel())
        //    {
        //        //Verify User Credentials
        //        RiderAccountEntry account;
        //        try
        //        {
        //            account = context.RiderAccounts.Where(e => e.Id == userId).ToList().Single();
        //        }
        //        catch (InvalidOperationException)
        //        {
        //            Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //            Response.WriteAsync("User does not exist");
        //            return;
        //        }
        //        if (!account.VerifySessionToken(token))
        //        {
        //            foreach (var hashedToken in account.SessionTokens)
        //            {
        //                _logger.LogDebug($"Account HashedToken: {hashedToken.Token}");
        //            }
        //            _logger.LogDebug($"Account Unhashed Token: {token}");
        //            Response.StatusCode = (int)HttpStatusCode.BadRequest;
        //            Response.WriteAsync("Invalid Token");
        //            return;
        //        }




        //        //Get google route
        //        JsonContent googleContent = JsonContent.Create<RoutesRequest>(routesRequest);
        //        HttpClient client = new HttpClient();
        //        Uri uri = new Uri("https://routes.googleapis.com/directions/v2:computeRoutes");
        //        googleContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
        //        googleContent.Headers.Add("X-Goog-Api-Key", APIKeys.GoogleEverythingKey);
        //        googleContent.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.distanceMeters,routes.legs.steps.startLocation,routes.legs.steps.endLocation");
        //        HttpResponseMessage googleResponse = await client.PostAsync(uri, googleContent);
        //        _logger.LogDebug($"Google Response: {await googleResponse.Content.ReadAsStringAsync()}");
        //        RoutesResponse routesResponse = await googleResponse.Content.ReadFromJsonAsync<RoutesResponse>();


        //        //Calculate Route Cost From Response
        //        int TotalRideCostCents =
        //            (int)(RideCost.CentsPerMinute * double.Parse(routesResponse!.routes[0].duration!.Substring(0, routesResponse!.routes[0].duration!.Length - 1)) / 60
        //            + RideCost.CentsPerMile * routesResponse.routes[0].distanceMeters / 1609)!;


        //        //Get start and End points for use in CheckoutSession Details
        //        Location startLocation = routesResponse!.routes[0].legs[0].steps[0].startLocation;
        //        Location endLocation = routesResponse!.routes[0].legs[routesResponse.routes[0].legs.Length - 1].steps[routesResponse.routes[0].legs[routesResponse.routes[0].legs.Length - 1].steps.Length - 1].endLocation;




        //        //Create Checkout Session
        //        SessionCreateOptions options = new SessionCreateOptions()
        //        {
        //            LineItems = new List<SessionLineItemOptions>()
        //            {
        //                new SessionLineItemOptions()
        //                {
        //                    PriceData = new SessionLineItemPriceDataOptions()
        //                    {
        //                        UnitAmount = TotalRideCostCents,
        //                        Currency = "usd",
        //                        ProductData = new SessionLineItemPriceDataProductDataOptions()
        //                        {
        //                            Name = $"Trip from {startLocation.latLng.latitude}, {startLocation.latLng.longitude} to {endLocation.latLng.latitude}, {endLocation.latLng.longitude} @ {DateTime.UtcNow}"
        //                        }



        //                    },
        //                    Quantity = 1
        //                }
        //            },
        //            Mode = "payment",
        //            SuccessUrl = "https://www.example.com/success/",
        //            CustomerEmail = account.Email,
        //            Metadata = new Dictionary<string, string>() { { "RideId", rideId.ToString() } }


        //        };

        //        SessionService service = new SessionService();
        //        Session session = await service.CreateAsync(options);

        //        _logger.LogInformation(session.Url);

        //        await Response.WriteAsync(session.Url);

        //        return;
        //    }
        //}
        [HttpPut]
        public async Task PutTripRequest([FromHeader(Name = "User-ID")] Guid userId, [FromHeader(Name = "Session-Token")] string token, [FromBody] TripRequest tripRequest)
        {
            Guid rideId = Guid.NewGuid();
            using (RidesModel context = new RidesModel())
            {
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
                                await service.ExpireAsync(ride.CheckoutSessionId);
                            }
                            catch (StripeException)
                            {
                                session = await service.GetAsync(ride.CheckoutSessionId);
                                if (session.Status == "complete")
                                {
                                    _logger.LogInformation("sessionCompleted");
                                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                                    await Response.WriteAsync("Only one trip request per rider");
                                    return;
                                }
                                else if (session.Status == "expired")
                                {
                                    
                                    _logger.LogInformation("Trip Removed");
                                }

                                
                            }
                            finally
                            {
                                context.RidesInProgress.Remove(ride);
                                await context.SaveChangesAsync();
                            }


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


                //Get start and End points for use in CheckoutSession Details
                Location startLocation = routesResponse!.routes[0].legs[0].steps[0].startLocation;
                Location endLocation = routesResponse!.routes[0].legs[routesResponse.routes[0].legs.Length - 1].steps[routesResponse.routes[0].legs[routesResponse.routes[0].legs.Length - 1].steps.Length - 1].endLocation;

                PaymentIntentCreateOptions payment = new PaymentIntentCreateOptions()
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
                    Metadata = new Dictionary<string, string>() { { "RideId", rideId.ToString() } }
                };


                session = await service.CreateAsync(options);

                _logger.LogInformation(session.Url);

                ride = new RideInProgress(userId, session.Id, tripRequest.Pickup, tripRequest.Dropoff, tripRequest.Stops);
                try
                {
                    context.RidesInProgress.Add(ride);
                    await context.SaveChangesAsync();
                }
                catch (DbUpdateException ex)
                {
                    _logger.LogError(ex.Message);
                    _logger.LogError(ex.StackTrace);
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
