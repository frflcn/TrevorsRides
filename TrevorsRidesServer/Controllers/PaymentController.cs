using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;
using Stripe;
using System.Text.Json;
using TrevorsRidesHelpers;
//using Newtonsoft.Json;

namespace TrevorsRidesServer.Controllers
{
    [Route("api/create-payment-intent")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        [HttpPost]
        public ActionResult Create(Ride ride)
        {
            // Alternatively, set up a webhook to listen for the payment_intent.succeeded event
            // and attach the PaymentMethod to a new Customer
            var customers = new CustomerService();
            StripeConfiguration.ApiKey = APIKeys.StripeSecretAPIKey;
            var customer = customers.Create(new CustomerCreateOptions());
            var paymentIntentService = new PaymentIntentService();
            var paymentIntent = paymentIntentService.Create(new PaymentIntentCreateOptions
            {
                Customer = customer.Id,
                SetupFutureUsage = "off_session",
                Amount = ride.Cost,
                Currency = "usd",
                // In the latest version of the API, specifying the `automatic_payment_methods` parameter is optional because Stripe enables its functionality by default.
                AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                {
                    Enabled = true,
                },
            });

            return new JsonResult(new { clientSecret = paymentIntent.ClientSecret });
        }

        public class Ride
        {
            public int Cost { get; set; }
        }
    }
}
