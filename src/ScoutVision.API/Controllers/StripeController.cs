using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using ScoutVision.API.Services;
using System.Threading.Tasks;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class StripeController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        public StripeController(IStripeService stripeService)
        {
            _stripeService = stripeService;
        }

        [HttpPost("create-payment-intent")]
        public async Task<ActionResult<string>> CreatePaymentIntent([FromQuery] long amount, [FromQuery] string currency = "usd")
        {
            var intent = await _stripeService.CreatePaymentIntentAsync(amount, currency);
            return Ok(intent.ClientSecret);
        }

        // Add endpoints for subscriptions, webhook, etc.
    }
}
