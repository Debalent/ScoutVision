using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrialController : ControllerBase
    {
        [HttpPost("apply")]
        public async Task<IActionResult> Apply([FromBody] TrialRequest req)
        {
            // Simulate trial and coupon logic
            if (string.IsNullOrWhiteSpace(req.Email))
                return BadRequest("Email required.");
            bool couponValid = string.IsNullOrWhiteSpace(req.Coupon) || req.Coupon == "SCOUT10";
            if (!couponValid)
                return BadRequest("Invalid coupon code.");
            // In real app, create trial subscription and apply coupon
            return Ok(new { trialStarted = true, couponApplied = !string.IsNullOrWhiteSpace(req.Coupon) });
        }
    }

    public class TrialRequest
    {
        public string Email { get; set; }
        public string Coupon { get; set; }
    }
}
