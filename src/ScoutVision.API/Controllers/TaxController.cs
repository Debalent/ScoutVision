using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Globalization;
using ScoutVision.Core.DTOs;
using ScoutVision.Infrastructure.Services;

namespace ScoutVision.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TaxController : ControllerBase
    {
        private readonly ILogger<TaxController> _logger;
        private readonly ITaxCalculationService _taxService;

        public TaxController(ILogger<TaxController> logger, ITaxCalculationService taxService)
        {
            _logger = logger;
            _taxService = taxService;
        }

        [HttpPost("calculate")]
        public async Task<IActionResult> Calculate([FromBody] TaxCalculationRequest request)
        {
            try
            {
                var result = await _taxService.CalculateTaxAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error calculating tax for request: {@Request}", request);
                return BadRequest(new { error = "Unable to calculate tax for the provided location" });
            }
        }

        [HttpGet("rates/{countryCode}")]
        public async Task<IActionResult> GetTaxRates(string countryCode, string? stateCode = null)
        {
            try
            {
                var rates = await _taxService.GetTaxRatesAsync(countryCode, stateCode);
                return Ok(rates);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error fetching tax rates for {Country}/{State}", countryCode, stateCode);
                return NotFound(new { error = "Tax rates not found for the specified location" });
            }
        }

        [HttpGet("validate-location")]
        public async Task<IActionResult> ValidateLocation(string countryCode, string? stateCode = null, string? postalCode = null)
        {
            try
            {
                var isValid = await _taxService.ValidateLocationAsync(countryCode, stateCode, postalCode);
                return Ok(new { isValid, countryCode, stateCode, postalCode });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating location {Country}/{State}/{Postal}", countryCode, stateCode, postalCode);
                return BadRequest(new { error = "Unable to validate location" });
            }
        }

        [HttpPost("estimate")]
        public async Task<IActionResult> EstimateTax([FromBody] TaxEstimationRequest request)
        {
            try
            {
                var estimation = await _taxService.EstimateTaxAsync(request);
                return Ok(estimation);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error estimating tax for request: {@Request}", request);
                return BadRequest(new { error = "Unable to estimate tax" });
            }
        }
    }

    // Enhanced request/response models
    public class TaxCalculationRequest
    {
        public decimal Amount { get; set; }
        public string CountryCode { get; set; } = "";
        public string? StateCode { get; set; }
        public string? PostalCode { get; set; }
        public string? City { get; set; }
        public string ProductType { get; set; } = "software"; // software, service, digital_goods
        public string CustomerType { get; set; } = "business"; // business, individual
        public bool IsB2B { get; set; } = true;
        public string? VatNumber { get; set; }
        public string Currency { get; set; } = "USD";
    }

    public class TaxEstimationRequest
    {
        public decimal Amount { get; set; }
        public string CountryCode { get; set; } = "";
        public string? StateCode { get; set; }
        public string ProductType { get; set; } = "software";
        public bool IsB2B { get; set; } = true;
    }

    public class TaxCalculationResult
    {
        public decimal Subtotal { get; set; }
        public decimal TotalTax { get; set; }
        public decimal Total { get; set; }
        public List<TaxBreakdown> TaxBreakdown { get; set; } = new();
        public string Currency { get; set; } = "";
        public bool TaxIncluded { get; set; }
        public string? TaxJurisdiction { get; set; }
        public bool IsVatApplicable { get; set; }
        public bool IsReverseCharge { get; set; }
    }

    public class TaxBreakdown
    {
        public string TaxType { get; set; } = ""; // VAT, Sales Tax, GST, etc.
        public string Jurisdiction { get; set; } = "";
        public decimal Rate { get; set; }
        public decimal Amount { get; set; }
        public string Description { get; set; } = "";
    }

    public class TaxRatesResponse
    {
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";
        public string? StateCode { get; set; }
        public string? StateName { get; set; }
        public decimal StandardRate { get; set; }
        public decimal? ReducedRate { get; set; }
        public string TaxType { get; set; } = ""; // VAT, Sales Tax, GST
        public List<SpecialRate> SpecialRates { get; set; } = new();
        public bool IsEuCountry { get; set; }
        public DateTime LastUpdated { get; set; }
    }

    public class SpecialRate
    {
        public string Category { get; set; } = "";
        public decimal Rate { get; set; }
        public string Description { get; set; } = "";
    }
}

// Tax Calculation Service Interface and Implementation
namespace ScoutVision.API.Services
{
    public interface ITaxCalculationService
    {
        Task<TaxCalculationResult> CalculateTaxAsync(TaxCalculationRequest request);
        Task<TaxEstimationRequest> EstimateTaxAsync(TaxEstimationRequest request);
        Task<TaxRatesResponse> GetTaxRatesAsync(string countryCode, string? stateCode = null);
        Task<bool> ValidateLocationAsync(string countryCode, string? stateCode = null, string? postalCode = null);
    }

    public class TaxCalculationService : ITaxCalculationService
    {
        private readonly ILogger<TaxCalculationService> _logger;
        private static readonly Dictionary<string, TaxJurisdiction> TaxJurisdictions = InitializeTaxData();

        public TaxCalculationService(ILogger<TaxCalculationService> logger)
        {
            _logger = logger;
        }

        public async Task<TaxCalculationResult> CalculateTaxAsync(TaxCalculationRequest request)
        {
            var jurisdiction = GetTaxJurisdiction(request.CountryCode, request.StateCode);
            var breakdown = new List<TaxBreakdown>();
            decimal totalTax = 0;

            // Handle EU VAT
            if (jurisdiction.IsEuCountry && !request.IsB2B)
            {
                var vatAmount = request.Amount * jurisdiction.VatRate;
                totalTax += vatAmount;
                breakdown.Add(new TaxBreakdown
                {
                    TaxType = "VAT",
                    Jurisdiction = jurisdiction.CountryName,
                    Rate = jurisdiction.VatRate,
                    Amount = vatAmount,
                    Description = $"EU VAT ({jurisdiction.VatRate:P})"
                });
            }
            // Handle B2B reverse charge in EU
            else if (jurisdiction.IsEuCountry && request.IsB2B && !string.IsNullOrEmpty(request.VatNumber))
            {
                breakdown.Add(new TaxBreakdown
                {
                    TaxType = "VAT",
                    Jurisdiction = jurisdiction.CountryName,
                    Rate = 0,
                    Amount = 0,
                    Description = "Reverse Charge - Customer VAT Number Provided"
                });
            }
            // Handle US Sales Tax
            else if (request.CountryCode.Equals("US", StringComparison.OrdinalIgnoreCase))
            {
                if (jurisdiction.SalesTaxRate > 0)
                {
                    var salesTaxAmount = request.Amount * jurisdiction.SalesTaxRate;
                    totalTax += salesTaxAmount;
                    breakdown.Add(new TaxBreakdown
                    {
                        TaxType = "Sales Tax",
                        Jurisdiction = $"{jurisdiction.StateName}, USA",
                        Rate = jurisdiction.SalesTaxRate,
                        Amount = salesTaxAmount,
                        Description = $"State Sales Tax ({jurisdiction.SalesTaxRate:P})"
                    });
                }
            }
            // Handle other countries
            else
            {
                if (jurisdiction.StandardTaxRate > 0)
                {
                    var taxAmount = request.Amount * jurisdiction.StandardTaxRate;
                    totalTax += taxAmount;
                    breakdown.Add(new TaxBreakdown
                    {
                        TaxType = jurisdiction.TaxType,
                        Jurisdiction = jurisdiction.CountryName,
                        Rate = jurisdiction.StandardTaxRate,
                        Amount = taxAmount,
                        Description = $"{jurisdiction.TaxType} ({jurisdiction.StandardTaxRate:P})"
                    });
                }
            }

            return new TaxCalculationResult
            {
                Subtotal = request.Amount,
                TotalTax = totalTax,
                Total = request.Amount + totalTax,
                TaxBreakdown = breakdown,
                Currency = request.Currency,
                TaxIncluded = false,
                TaxJurisdiction = jurisdiction.CountryName,
                IsVatApplicable = jurisdiction.IsEuCountry,
                IsReverseCharge = jurisdiction.IsEuCountry && request.IsB2B && !string.IsNullOrEmpty(request.VatNumber)
            };
        }

        public async Task<TaxEstimationRequest> EstimateTaxAsync(TaxEstimationRequest request)
        {
            var jurisdiction = GetTaxJurisdiction(request.CountryCode, request.StateCode);
            decimal estimatedRate = 0;

            if (jurisdiction.IsEuCountry && !request.IsB2B)
            {
                estimatedRate = jurisdiction.VatRate;
            }
            else if (request.CountryCode.Equals("US", StringComparison.OrdinalIgnoreCase))
            {
                estimatedRate = jurisdiction.SalesTaxRate;
            }
            else
            {
                estimatedRate = jurisdiction.StandardTaxRate;
            }

            return new TaxEstimationRequest
            {
                Amount = request.Amount,
                CountryCode = request.CountryCode,
                StateCode = request.StateCode,
                ProductType = request.ProductType,
                IsB2B = request.IsB2B
            };
        }

        public async Task<TaxRatesResponse> GetTaxRatesAsync(string countryCode, string? stateCode = null)
        {
            var jurisdiction = GetTaxJurisdiction(countryCode, stateCode);
            
            return new TaxRatesResponse
            {
                CountryCode = jurisdiction.CountryCode,
                CountryName = jurisdiction.CountryName,
                StateCode = jurisdiction.StateCode,
                StateName = jurisdiction.StateName,
                StandardRate = jurisdiction.StandardTaxRate,
                ReducedRate = jurisdiction.ReducedTaxRate,
                TaxType = jurisdiction.TaxType,
                IsEuCountry = jurisdiction.IsEuCountry,
                LastUpdated = DateTime.UtcNow,
                SpecialRates = new List<SpecialRate>
                {
                    new() { Category = "Digital Services", Rate = jurisdiction.StandardTaxRate, Description = "Standard rate for digital services" },
                    new() { Category = "Software Licensing", Rate = jurisdiction.StandardTaxRate, Description = "Standard rate for software licensing" }
                }
            };
        }

        public async Task<bool> ValidateLocationAsync(string countryCode, string? stateCode = null, string? postalCode = null)
        {
            if (string.IsNullOrWhiteSpace(countryCode)) return false;
            
            var jurisdiction = TaxJurisdictions.ContainsKey(countryCode.ToUpper());
            
            // Additional validation for US states
            if (countryCode.Equals("US", StringComparison.OrdinalIgnoreCase) && !string.IsNullOrEmpty(stateCode))
            {
                var key = $"{countryCode.ToUpper()}-{stateCode.ToUpper()}";
                return TaxJurisdictions.ContainsKey(key);
            }
            
            return jurisdiction;
        }

        private TaxJurisdiction GetTaxJurisdiction(string countryCode, string? stateCode = null)
        {
            var key = string.IsNullOrEmpty(stateCode) 
                ? countryCode.ToUpper() 
                : $"{countryCode.ToUpper()}-{stateCode.ToUpper()}";
            
            return TaxJurisdictions.TryGetValue(key, out var jurisdiction) 
                ? jurisdiction 
                : TaxJurisdictions.GetValueOrDefault("DEFAULT", new TaxJurisdiction
                {
                    CountryCode = countryCode,
                    CountryName = "Unknown",
                    StandardTaxRate = 0.10m,
                    TaxType = "Tax"
                });
        }

        private static Dictionary<string, TaxJurisdiction> InitializeTaxData()
        {
            return new Dictionary<string, TaxJurisdiction>
            {
                // EU Countries with VAT
                ["GB"] = new() { CountryCode = "GB", CountryName = "United Kingdom", VatRate = 0.20m, StandardTaxRate = 0.20m, TaxType = "VAT", IsEuCountry = false },
                ["DE"] = new() { CountryCode = "DE", CountryName = "Germany", VatRate = 0.19m, StandardTaxRate = 0.19m, TaxType = "VAT", IsEuCountry = true, ReducedTaxRate = 0.07m },
                ["FR"] = new() { CountryCode = "FR", CountryName = "France", VatRate = 0.20m, StandardTaxRate = 0.20m, TaxType = "VAT", IsEuCountry = true, ReducedTaxRate = 0.055m },
                ["IT"] = new() { CountryCode = "IT", CountryName = "Italy", VatRate = 0.22m, StandardTaxRate = 0.22m, TaxType = "VAT", IsEuCountry = true, ReducedTaxRate = 0.10m },
                ["ES"] = new() { CountryCode = "ES", CountryName = "Spain", VatRate = 0.21m, StandardTaxRate = 0.21m, TaxType = "VAT", IsEuCountry = true, ReducedTaxRate = 0.10m },
                ["NL"] = new() { CountryCode = "NL", CountryName = "Netherlands", VatRate = 0.21m, StandardTaxRate = 0.21m, TaxType = "VAT", IsEuCountry = true, ReducedTaxRate = 0.09m },
                
                // US States - Major ones with sales tax
                ["US-CA"] = new() { CountryCode = "US", StateCode = "CA", CountryName = "United States", StateName = "California", SalesTaxRate = 0.0725m, StandardTaxRate = 0.0725m, TaxType = "Sales Tax" },
                ["US-NY"] = new() { CountryCode = "US", StateCode = "NY", CountryName = "United States", StateName = "New York", SalesTaxRate = 0.08m, StandardTaxRate = 0.08m, TaxType = "Sales Tax" },
                ["US-TX"] = new() { CountryCode = "US", StateCode = "TX", CountryName = "United States", StateName = "Texas", SalesTaxRate = 0.0625m, StandardTaxRate = 0.0625m, TaxType = "Sales Tax" },
                ["US-FL"] = new() { CountryCode = "US", StateCode = "FL", CountryName = "United States", StateName = "Florida", SalesTaxRate = 0.06m, StandardTaxRate = 0.06m, TaxType = "Sales Tax" },
                ["US"] = new() { CountryCode = "US", CountryName = "United States", StandardTaxRate = 0.00m, TaxType = "No Tax" }, // Federal level
                
                // Other major countries
                ["CA"] = new() { CountryCode = "CA", CountryName = "Canada", StandardTaxRate = 0.13m, TaxType = "HST", ReducedTaxRate = 0.05m }, // Varies by province
                ["AU"] = new() { CountryCode = "AU", CountryName = "Australia", StandardTaxRate = 0.10m, TaxType = "GST" },
                ["IN"] = new() { CountryCode = "IN", CountryName = "India", StandardTaxRate = 0.18m, TaxType = "GST" },
                ["JP"] = new() { CountryCode = "JP", CountryName = "Japan", StandardTaxRate = 0.10m, TaxType = "Consumption Tax" },
                ["SG"] = new() { CountryCode = "SG", CountryName = "Singapore", StandardTaxRate = 0.07m, TaxType = "GST" },
                ["BR"] = new() { CountryCode = "BR", CountryName = "Brazil", StandardTaxRate = 0.17m, TaxType = "ICMS" },
                
                // Default fallback
                ["DEFAULT"] = new() { CountryCode = "DEFAULT", CountryName = "Other", StandardTaxRate = 0.10m, TaxType = "Tax" }
            };
        }
    }

    public class TaxJurisdiction
    {
        public string CountryCode { get; set; } = "";
        public string CountryName { get; set; } = "";
        public string? StateCode { get; set; }
        public string? StateName { get; set; }
        public decimal StandardTaxRate { get; set; }
        public decimal? ReducedTaxRate { get; set; }
        public decimal VatRate { get; set; }
        public decimal SalesTaxRate { get; set; }
        public string TaxType { get; set; } = "";
        public bool IsEuCountry { get; set; }
    }
}
