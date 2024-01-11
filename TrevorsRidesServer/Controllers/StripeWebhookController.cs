using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Primitives;
using Stripe;
using Stripe.Checkout;
using TrevorsRidesHelpers;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/Stripe/Webhooks")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        ILogger<StripeWebhookController> _logger;
        RideMatchingService _rideMatchingService;
        public StripeWebhookController(ILogger<StripeWebhookController> logger, RideMatchingService rideMatchingService)
        {
            _logger = logger;
            _rideMatchingService = rideMatchingService;

        }
        [HttpPost]
        public async Task<IActionResult> Post()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();
            
            string endpointSecret;
            try
            {
                Event stripeEvent = EventUtility.ParseEvent(json);
                if (stripeEvent.Livemode)
                {
                    endpointSecret = APIKeys.StripeWebhookSecret;
                }
                else
                {
                    endpointSecret = APIKeys.StripeWebhookTestSecret;
                }
                StringValues signatureHeader = Request.Headers["Stripe-Signature"];

                stripeEvent = EventUtility.ConstructEvent(json,
                        signatureHeader, endpointSecret);

                if (stripeEvent.Type == Events.PaymentIntentSucceeded)
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    Console.WriteLine("A successful payment for ${0} was made.", Math.Round((double)paymentIntent.Amount / 100, 2));
                    // Then define and call a method to handle the successful payment intent.
                    // handlePaymentIntentSucceeded(paymentIntent);
                    //_rideMatchingService(rideId);
                    //Console.
                }
                else if (stripeEvent.Type == Events.CheckoutSessionCompleted)
                {
                    Session session = stripeEvent.Data.Object as Session;
                    await _rideMatchingService.TripPaid(Guid.Parse(session.Metadata["RiderId"]));
                }
                else
                {
                    Console.WriteLine("Unhandled event type: {0}", stripeEvent.Type);
                }
                return Ok();
            }
            catch (StripeException e)
            {
                Console.WriteLine("Error: {0}", e.Message);
                return BadRequest();
            }
            catch (Exception e)
            {
                return StatusCode(500);
            }
        }
    }
}
