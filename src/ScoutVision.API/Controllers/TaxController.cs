using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxController : ControllerBase
    {
        private static readonly Dictionary<string, double> TaxRates = new()
        {
            { "US", 0.07 },
            { "UK", 0.20 },
            { "DE", 0.19 },
            { "FR", 0.20 },
            { "IN", 0.18 },
            { "Other", 0.15 }
        };

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] TaxRequest req)
        {
            double seatPrice = 49.99; // Example per-seat price
            double subtotal = seatPrice * req.Seats;
            double taxRate = TaxRates.TryGetValue(req.Country, out var rate) ? rate : TaxRates["Other"];
            double tax = subtotal * taxRate;
            double total = subtotal + tax;
            var result = new TaxResult { Subtotal = subtotal, Tax = tax, Total = total };
            return Ok(result);
        }
    }

    public class TaxRequest
    {
        public int Seats { get; set; }
        public string Country { get; set; }
    }

    public class TaxResult
    {
        public double Subtotal { get; set; }
        public double Tax { get; set; }
        public double Total { get; set; }
    }
}
