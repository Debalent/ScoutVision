using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/admin/billing")]
    public class AdminBillingController : ControllerBase
    {
        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            // Simulated metrics; replace with real data source
            var stats = new List<BillingStat>
            {
                new BillingStat { Metric = "Total Revenue", Value = "$12,340" },
                new BillingStat { Metric = "Active Subscriptions", Value = "87" },
                new BillingStat { Metric = "Churn Rate", Value = "2.1%" },
                new BillingStat { Metric = "Upgrades This Month", Value = "5" },
                new BillingStat { Metric = "Trials Started", Value = "23" },
                new BillingStat { Metric = "Coupons Used", Value = "14" }
            };
            return Ok(stats);
        }
    }

    public class BillingStat
    {
        public string Metric { get; set; }
        public string Value { get; set; }
    }
}
