using Microsoft.AspNetCore.Mvc;
using ScoutVision.API.Services;
using System.ComponentModel.DataAnnotations;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PricingController : ControllerBase
    {
        private readonly ILogger<PricingController> _logger;

        public PricingController(ILogger<PricingController> logger)
        {
            _logger = logger;
        }

        /// <summary>
        /// Calculate pricing for a given configuration
        /// </summary>
        [HttpPost("calculate")]
        public ActionResult<PricingCalculation> CalculatePricing([FromBody] PricingRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var calculation = PricingService.CalculatePricing(request);
                
                _logger.LogInformation("Pricing calculated for {UserCount} users in {Region} with {BillingCycle} billing",
                    request.UserCount, request.Region, request.BillingCycle);

                return Ok(calculation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating pricing");
                return StatusCode(500, new { error = "Failed to calculate pricing" });
            }
        }

        /// <summary>
        /// Get available pricing tiers
        /// </summary>
        [HttpGet("tiers")]
        public ActionResult<List<PricingTier>> GetPricingTiers()
        {
            try
            {
                var tiers = PricingService.GetPricingTiers();
                return Ok(tiers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving pricing tiers");
                return StatusCode(500, new { error = "Failed to retrieve pricing tiers" });
            }
        }

        /// <summary>
        /// Get recommended pricing based on user count and requirements
        /// </summary>
        [HttpGet("recommend")]
        public ActionResult<PricingRecommendation> GetPricingRecommendation(
            [FromQuery, Required] int userCount,
            [FromQuery] string region = "US",
            [FromQuery] string customerType = "business",
            [FromQuery] string? features = null)
        {
            try
            {
                var requestedFeatures = features?.Split(',').Select(f => f.Trim()).ToList() ?? new List<string>();
                
                var tiers = PricingService.GetPricingTiers();
                var recommendedTier = tiers.FirstOrDefault(t => userCount >= t.MinUsers && userCount <= t.MaxUsers);
                
                if (recommendedTier == null)
                {
                    recommendedTier = tiers.Last(); // Default to highest tier
                }

                var monthlyCalculation = PricingService.CalculatePricing(new PricingRequest
                {
                    UserCount = userCount,
                    BillingCycle = "monthly",
                    Region = region,
                    CustomerType = customerType
                });

                var annualCalculation = PricingService.CalculatePricing(new PricingRequest
                {
                    UserCount = userCount,
                    BillingCycle = "annual",
                    Region = region,
                    CustomerType = customerType
                });

                var recommendation = new PricingRecommendation
                {
                    RecommendedTier = recommendedTier,
                    MonthlyPricing = monthlyCalculation,
                    AnnualPricing = annualCalculation,
                    AnnualSavings = monthlyCalculation.Total * 12 - annualCalculation.Total,
                    RecommendedAddons = GetRecommendedAddons(requestedFeatures, userCount),
                    CustomizationOptions = GetCustomizationOptions(userCount)
                };

                return Ok(recommendation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting pricing recommendation");
                return StatusCode(500, new { error = "Failed to get pricing recommendation" });
            }
        }

        /// <summary>
        /// Generate a pricing quote for download
        /// </summary>
        [HttpPost("quote")]
        public ActionResult<PricingQuote> GenerateQuote([FromBody] PricingQuoteRequest request)
        {
            try
            {
                var calculation = PricingService.CalculatePricing(new PricingRequest
                {
                    UserCount = request.UserCount,
                    BillingCycle = request.BillingCycle,
                    Region = request.Region,
                    CustomerType = request.CustomerType,
                    SelectedAddons = request.SelectedAddons
                });

                var quote = new PricingQuote
                {
                    QuoteNumber = $"SV-{DateTime.UtcNow:yyyyMMdd}-{Random.Shared.Next(1000, 9999)}",
                    GeneratedDate = DateTime.UtcNow,
                    ValidUntil = DateTime.UtcNow.AddDays(30),
                    CustomerInfo = request.CustomerInfo,
                    PricingDetails = calculation,
                    Terms = GetQuoteTerms(),
                    ContactInfo = new ContactInfo
                    {
                        SalesEmail = "sales@scoutvision.com",
                        SalesPhone = "+1-555-SCOUT-99",
                        SupportEmail = "support@scoutvision.com"
                    }
                };

                return Ok(quote);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error generating pricing quote");
                return StatusCode(500, new { error = "Failed to generate quote" });
            }
        }

        private List<AddonRecommendation> GetRecommendedAddons(List<string> requestedFeatures, int userCount)
        {
            var recommendations = new List<AddonRecommendation>();

            // Logic to recommend addons based on user requirements
            if (requestedFeatures.Any(f => f.Contains("analytics", StringComparison.OrdinalIgnoreCase)) || userCount > 10)
            {
                recommendations.Add(new AddonRecommendation
                {
                    Name = "Advanced Analytics",
                    Price = 10m,
                    Reason = "Recommended for teams needing detailed performance insights"
                });
            }

            if (requestedFeatures.Any(f => f.Contains("transfer", StringComparison.OrdinalIgnoreCase)) || userCount > 15)
            {
                recommendations.Add(new AddonRecommendation
                {
                    Name = "Transfer Engine",
                    Price = 15m,
                    Reason = "Essential for clubs actively managing player transfers"
                });
            }

            return recommendations;
        }

        private List<string> GetCustomizationOptions(int userCount)
        {
            var options = new List<string>();

            if (userCount >= 50)
            {
                options.Add("White-label branding available");
                options.Add("Custom integration development");
                options.Add("Dedicated cloud infrastructure");
            }

            if (userCount >= 20)
            {
                options.Add("Custom report templates");
                options.Add("Priority feature requests");
                options.Add("Extended data retention");
            }

            options.Add("Volume discounts for 100+ users");
            options.Add("Multi-year contract discounts available");

            return options;
        }

        private List<string> GetQuoteTerms()
        {
            return new List<string>
            {
                "Prices valid for 30 days from quote date",
                "All subscriptions include 14-day free trial",
                "Setup and onboarding included at no additional cost",
                "30-day money-back guarantee on annual plans",
                "Pricing excludes applicable taxes",
                "Custom terms available for enterprise accounts",
                "Payment terms: Net 30 for annual subscriptions over $10,000"
            };
        }
    }

    // Supporting models
    public class PricingRecommendation
    {
        public PricingTier RecommendedTier { get; set; } = new();
        public PricingCalculation MonthlyPricing { get; set; } = new();
        public PricingCalculation AnnualPricing { get; set; } = new();
        public decimal AnnualSavings { get; set; }
        public List<AddonRecommendation> RecommendedAddons { get; set; } = new();
        public List<string> CustomizationOptions { get; set; } = new();
    }

    public class AddonRecommendation
    {
        public string Name { get; set; } = "";
        public decimal Price { get; set; }
        public string Reason { get; set; } = "";
    }

    public class PricingQuoteRequest
    {
        [Required]
        [Range(1, 1000)]
        public int UserCount { get; set; }

        [Required]
        public string BillingCycle { get; set; } = "monthly";

        [Required]
        public string Region { get; set; } = "US";

        public string CustomerType { get; set; } = "business";
        public List<string>? SelectedAddons { get; set; }
        public CustomerInfo? CustomerInfo { get; set; }
    }

    public class CustomerInfo
    {
        public string CompanyName { get; set; } = "";
        public string ContactName { get; set; } = "";
        public string Email { get; set; } = "";
        public string Phone { get; set; } = "";
        public string Country { get; set; } = "";
    }

    public class PricingQuote
    {
        public string QuoteNumber { get; set; } = "";
        public DateTime GeneratedDate { get; set; }
        public DateTime ValidUntil { get; set; }
        public CustomerInfo? CustomerInfo { get; set; }
        public PricingCalculation PricingDetails { get; set; } = new();
        public List<string> Terms { get; set; } = new();
        public ContactInfo ContactInfo { get; set; } = new();
    }

    public class ContactInfo
    {
        public string SalesEmail { get; set; } = "";
        public string SalesPhone { get; set; } = "";
        public string SupportEmail { get; set; } = "";
    }
}