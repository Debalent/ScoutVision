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
            await _emailService.SendInvoiceEmailAsync(request.To, request.Details);
            return Ok();
        }

        [HttpPost("send-confirmation")]
        public async Task<IActionResult> SendConfirmation([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Details))
                return BadRequest("Recipient and details are required.");
            await _emailService.SendPaymentConfirmationAsync(request.To, request.Details);
            return Ok();
        }

        [HttpPost("send-renewal")]
        public async Task<IActionResult> SendRenewal([FromBody] EmailRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.To) || string.IsNullOrWhiteSpace(request.Details))
                return BadRequest("Recipient and details are required.");
            await _emailService.SendRenewalReminderAsync(request.To, request.Details);
            return Ok();
        }
    }

    public class EmailRequest
    {
    public string? To { get; set; }
    public string? Details { get; set; }
    }
}
