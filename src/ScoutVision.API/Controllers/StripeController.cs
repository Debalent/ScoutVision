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
        private readonly StripeService _stripeService;
        public StripeController(IConfiguration config)
        {
            _stripeService = new StripeService(config);
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
