using Microsoft.AspNetCore.Mvc;
using ScoutVision.API.Services;
using System.Threading.Tasks;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class EmailController : ControllerBase
    {
        private readonly EmailService _emailService;

        public EmailController(EmailService emailService)
        {
            _emailService = emailService;
        }

        [HttpPost("send-invoice")]
        public async Task<IActionResult> SendInvoice([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Details))
                return BadRequest("Recipient and details are required.");
            await _emailService.SendInvoiceAsync(request.To, "Customer", "INV-001", 100.0m, DateTime.Now.AddDays(30), new byte[0]);
            return Ok();
        }

        [HttpPost("send-confirmation")]
        public async Task<IActionResult> SendConfirmation([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Details))
                return BadRequest("Recipient and details are required.");
            await _emailService.SendPaymentConfirmationAsync(request.To, "Customer", 100.0m, "INV-001", DateTime.Now);
            return Ok();
        }

        [HttpPost("send-renewal")]
        public async Task<IActionResult> SendRenewal([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Details))
                return BadRequest("Recipient and details are required.");
            await _emailService.SendSubscriptionRenewalReminderAsync(request.To, "Customer", "Pro Plan", 100.0m, DateTime.Now.AddMonths(1));
            return Ok();
        }
    }

    public class EmailRequest
    {
    public string? To { get; set; }
    public string? Details { get; set; }
    }
}
