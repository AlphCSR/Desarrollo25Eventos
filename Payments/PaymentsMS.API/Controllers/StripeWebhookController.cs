using MediatR;
using System;
using Microsoft.AspNetCore.Mvc;
using Stripe;
using System.IO;
using System.Threading.Tasks;
using PaymentsMS.Application.Commands.CapturePayment;
using PaymentsMS.Application.Commands.FailPayment;
using Microsoft.Extensions.Configuration;

namespace PaymentsMS.API.Controllers
{
    [Route("webhook/stripe")]
    [ApiController]
    public class StripeWebhookController : ControllerBase
    {
        private readonly string _webhookSecret;
        private readonly IMediator _mediator;

        public StripeWebhookController(IConfiguration config, IMediator mediator)
        {
            _webhookSecret = config["Stripe:WebhookSecret"] ?? throw new ArgumentNullException("Stripe:WebhookSecret");
            _mediator = mediator;
        }

        [HttpPost]
        public async Task<IActionResult> Handle()
        {
            var json = await new StreamReader(HttpContext.Request.Body).ReadToEndAsync();

            try
            {
                var stripeEvent = EventUtility.ConstructEvent(json, Request.Headers["Stripe-Signature"], _webhookSecret);

                if (stripeEvent.Type == "payment_intent.succeeded")
                {
                    var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                    if (paymentIntent != null)
                    {
                        await _mediator.Send(new CapturePaymentCommand(paymentIntent.Id));
                    }
                }
                else if (stripeEvent.Type == "payment_intent.payment_failed")
                {
                     var paymentIntent = stripeEvent.Data.Object as PaymentIntent;
                     if (paymentIntent != null)
                     {
                         var reason = paymentIntent.LastPaymentError?.Message ?? "Error desconocido";
                         await _mediator.Send(new FailPaymentCommand(paymentIntent.Id, reason));
                     }
                }

                return Ok();
            }
            catch (StripeException)
            {
                return BadRequest();
            }
        }
    }
}
