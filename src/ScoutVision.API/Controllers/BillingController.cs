using Microsoft.AspNetCore.Mvc;
using ScoutVision.API.Services;
using System.Threading.Tasks;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BillingController : ControllerBase
    {
        [HttpGet("price-per-seat")]
        public ActionResult<decimal> GetPricePerSeat([FromQuery] int userCount)
        {
            var price = PricingService.GetPerSeatPrice(userCount);
            return Ok(price);
        }

        [HttpGet("annual-discount")]
        public ActionResult<decimal> GetAnnualDiscount([FromQuery] int userCount)
        {
            var monthly = PricingService.GetPerSeatPrice(userCount);
            var annual = PricingService.GetAnnualDiscount(monthly);
            return Ok(annual);
        }

        [HttpPost("generate-invoice")]
        public async Task<ActionResult<Invoice>> GenerateInvoice([FromQuery] string orgId, [FromQuery] int userCount, [FromQuery] bool annual)
        {
            var billingService = new BillingService();
            var invoice = await billingService.GenerateInvoiceAsync(orgId, userCount, annual);
            return Ok(invoice);
        }
    }
}
