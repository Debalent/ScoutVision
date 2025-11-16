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
        public async Task<IActionResult> ValidateLocation(
            [FromQuery] string countryCode,
            [FromQuery] string? stateCode = null,
            [FromQuery] string? postalCode = null)
        {
            try
            {
                var rates = await _taxService.GetTaxRatesAsync(countryCode, stateCode);
                var isValid = rates != null;
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
}
