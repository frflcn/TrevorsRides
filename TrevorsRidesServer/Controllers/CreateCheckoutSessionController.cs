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
        [HttpPut]
        public async Task Put([FromHeader(Name = "User-ID")] Guid userId, [FromHeader(Name = "Session-Token")] string token, [FromBody]RoutesRequest routesRequest)
        {
            
            using (RidesModel context = new RidesModel())
            {
                AccountEntry account;
                try
                {
                    account = context.Accounts.Where(e => e.Id == userId).ToList().Single();
                }
                catch (InvalidOperationException)
                {
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    Response.WriteAsync("User does not exist");
                    return;
                }
                if (!account.VerifySessionToken(token))
                {
                    foreach(var hashedToken in account.SessionTokens)
                    {
                        _logger.LogDebug($"Account HashedToken: {hashedToken.Token}");
                    }
                    _logger.LogDebug($"Account Unhashed Token: {token}");
                    Response.StatusCode = (int)HttpStatusCode.BadRequest;
                    Response.WriteAsync("Invalid Token");
                    return;
                }
                JsonContent googleContent = JsonContent.Create<RoutesRequest>(routesRequest);
                HttpClient client = new HttpClient();
                Uri uri = new Uri("https://routes.googleapis.com/directions/v2:computeRoutes");
                googleContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                googleContent.Headers.Add("X-Goog-Api-Key", APIKeys.GoogleEverythingKey);
                googleContent.Headers.Add("X-Goog-FieldMask", "routes.duration,routes.distanceMeters,routes.legs.steps.startLocation,routes.legs.steps.endLocation");
                HttpResponseMessage googleResponse = await client.PostAsync(uri, googleContent);
                _logger.LogDebug($"Google Response: {await googleResponse.Content.ReadAsStringAsync()}");
                RoutesResponse routesResponse = await googleResponse.Content.ReadFromJsonAsync<RoutesResponse>();
                int TotalRideCostCents =
                    (int)(RideCost.CentsPerMinute * double.Parse(routesResponse!.routes[0].duration!.Substring(0, routesResponse!.routes[0].duration!.Length - 1)) / 60
                    + RideCost.CentsPerMile * routesResponse.routes[0].distanceMeters / 1609)!;
                Location startLocation = routesResponse!.routes[0].legs[0].steps[0].startLocation;
                Location endLocation = routesResponse!.routes[0].legs[routesResponse.routes[0].legs.Length - 1].steps[routesResponse.routes[0].legs[routesResponse.routes[0].legs.Length - 1].steps.Length - 1].endLocation;
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
                                    Name = $"Trip from {startLocation.latLng.latitude}, {startLocation.latLng.longitude} to {endLocation.latLng.latitude}, {endLocation.latLng.longitude} @ {DateTime.UtcNow}"
                                }
                                


                            },
                            Quantity = 1
                        }
                    },
                    Mode = "payment",
                    SuccessUrl = "https://www.example.com/success/",
                    CustomerEmail = account.Email
                    
                    
                };

                SessionService service = new SessionService();
                Session session = await service.CreateAsync(options);
                
                _logger.LogInformation(session.Url);

                await Response.WriteAsync(session.Url);

                return;






            }
        }
    }
}
