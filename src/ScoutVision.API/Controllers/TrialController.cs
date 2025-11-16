using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using ScoutVision.API.Services;
using Microsoft.Extensions.Logging;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TrialController : ControllerBase
    {
        private readonly IStripeService _stripeService;
        private readonly IEmailService _emailService;
        private readonly ILogger<TrialController> _logger;
        private readonly ICouponService _couponService;

        public TrialController(IStripeService stripeService, IEmailService emailService, 
            ILogger<TrialController> logger, ICouponService couponService)
        {
            _stripeService = stripeService;
            _emailService = emailService;
            _logger = logger;
            _couponService = couponService;
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartTrial([FromBody] TrialRequest request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Email))
                    return BadRequest("Email is required.");

                // Create customer in Stripe
                var customer = await _stripeService.CreateCustomerAsync(request.Email, request.Name ?? "");
                
                // Validate coupon if provided
                string? validatedCoupon = null;
                if (!string.IsNullOrWhiteSpace(request.CouponCode))
                {
                    var couponValidation = await _couponService.ValidateCouponAsync(request.CouponCode);
                    if (!couponValidation.IsValid)
                        return BadRequest($"Invalid coupon: {couponValidation.ErrorMessage}");
                    
                    validatedCoupon = request.CouponCode;
                }

                // Create trial subscription (14 days free)
                var trialData = new TrialData
                {
                    CustomerId = customer.Id,
                    Email = request.Email,
                    Name = request.Name ?? "",
                    PlanId = request.PlanId ?? "professional",
                    TrialDays = 14,
                    CouponCode = validatedCoupon,
                    StartDate = DateTime.UtcNow,
                    EndDate = DateTime.UtcNow.AddDays(14)
                };

                // Send welcome email
                await _emailService.SendTrialWelcomeAsync(request.Email, request.Name ?? "", trialData);
                
                _logger.LogInformation("Trial started for {Email} with plan {PlanId}", request.Email, request.PlanId);
                
                return Ok(new TrialResponse
                {
                    Success = true,
                    CustomerId = customer.Id,
                    TrialEndDate = trialData.EndDate,
                    CouponApplied = validatedCoupon != null,
                    Message = "Trial started successfully! Check your email for next steps."
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to start trial for {Email}", request.Email);
                return StatusCode(500, "An error occurred while starting your trial.");
            }
        }

        [HttpPost("extend")]
        public async Task<IActionResult> ExtendTrial([FromBody] ExtendTrialRequest request)
        {
            try
            {
                // Validate extension request
                var trial = await GetTrialByCustomerIdAsync(request.CustomerId);
                if (trial == null)
                    return NotFound("Trial not found.");

                if (trial.ExtensionUsed)
                    return BadRequest("Trial extension already used.");

                // Extend trial by additional days
                var newEndDate = trial.EndDate.AddDays(request.ExtensionDays);
                await UpdateTrialEndDateAsync(request.CustomerId, newEndDate);

                await _emailService.SendTrialExtendedAsync(trial.Email, trial.Name, newEndDate);
                
                return Ok(new { Success = true, NewEndDate = newEndDate });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to extend trial for customer {CustomerId}", request.CustomerId);
                return StatusCode(500, "An error occurred while extending your trial.");
            }
        }

        [HttpGet("{customerId}/status")]
        public async Task<IActionResult> GetTrialStatus(string customerId)
        {
            try
            {
                var trial = await GetTrialByCustomerIdAsync(customerId);
                if (trial == null)
                    return NotFound("Trial not found.");

                var daysRemaining = (trial.EndDate - DateTime.UtcNow).Days;
                
                return Ok(new TrialStatus
                {
                    IsActive = DateTime.UtcNow < trial.EndDate,
                    DaysRemaining = Math.Max(0, daysRemaining),
                    EndDate = trial.EndDate,
                    PlanId = trial.PlanId,
                    ExtensionAvailable = !trial.ExtensionUsed && daysRemaining <= 3
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to get trial status for customer {CustomerId}", customerId);
                return StatusCode(500, "An error occurred while checking trial status.");
            }
        }

        [HttpPost("convert")]
        public async Task<IActionResult> ConvertToSubscription([FromBody] ConvertTrialRequest request)
        {
            try
            {
                var trial = await GetTrialByCustomerIdAsync(request.CustomerId);
                if (trial == null)
                    return NotFound("Trial not found.");

                // Create paid subscription
                var subscription = await _stripeService.CreateSubscriptionAsync(
                    request.CustomerId, 
                    request.PriceId,
                    new Dictionary<string, string> { { "converted_from_trial", "true" } }
                );

                // Mark trial as converted
                await MarkTrialAsConvertedAsync(request.CustomerId);

                await _emailService.SendTrialConvertedAsync(trial.Email, trial.Name, request.PlanName);
                
                return Ok(new { Success = true, SubscriptionId = subscription.Id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to convert trial for customer {CustomerId}", request.CustomerId);
                return StatusCode(500, "An error occurred while converting your trial.");
            }
        }

        // Mock database operations (replace with actual database calls)
        private async Task<TrialData?> GetTrialByCustomerIdAsync(string customerId)
        {
            // TODO: Replace with actual database query
            await Task.Delay(10);
            return new TrialData
            {
                CustomerId = customerId,
                Email = "user@example.com",
                Name = "Test User",
                PlanId = "professional",
                StartDate = DateTime.UtcNow.AddDays(-7),
                EndDate = DateTime.UtcNow.AddDays(7),
                ExtensionUsed = false
            };
        }

        private async Task UpdateTrialEndDateAsync(string customerId, DateTime newEndDate)
        {
            // TODO: Replace with actual database update
            await Task.Delay(10);
        }

        private async Task MarkTrialAsConvertedAsync(string customerId)
        {
            // TODO: Replace with actual database update
            await Task.Delay(10);
        }
    }

    public class TrialRequest
    {
        public string Email { get; set; } = "";
        public string? Name { get; set; }
        public string? PlanId { get; set; }
        public string? CouponCode { get; set; }
    }

    public class ExtendTrialRequest
    {
        public string CustomerId { get; set; } = "";
        public int ExtensionDays { get; set; } = 7;
    }

    public class ConvertTrialRequest
    {
        public string CustomerId { get; set; } = "";
        public string PriceId { get; set; } = "";
        public string PlanName { get; set; } = "";
    }

    public class TrialResponse
    {
        public bool Success { get; set; }
        public string CustomerId { get; set; } = "";
        public DateTime TrialEndDate { get; set; }
        public bool CouponApplied { get; set; }
        public string Message { get; set; } = "";
    }

    public class TrialStatus
    {
        public bool IsActive { get; set; }
        public int DaysRemaining { get; set; }
        public DateTime EndDate { get; set; }
        public string PlanId { get; set; } = "";
        public bool ExtensionAvailable { get; set; }
    }

    public class TrialData
    {
        public string CustomerId { get; set; } = "";
        public string Email { get; set; } = "";
        public string Name { get; set; } = "";
        public string PlanId { get; set; } = "";
        public int TrialDays { get; set; }
        public string? CouponCode { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime EndDate { get; set; }
        public bool ExtensionUsed { get; set; }
        public bool Converted { get; set; }
    }
}
