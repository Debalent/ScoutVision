using System.ComponentModel.DataAnnotations;

namespace ScoutVision.Core.DTOs;

public class TaxCalculationRequest
{
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string CountryCode { get; set; } = string.Empty;
    
    public string? StateCode { get; set; }
    
    [Required]
    public string TaxType { get; set; } = string.Empty;
}

public class TaxCalculationResult
{
    public decimal Amount { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public decimal TaxRate { get; set; }
    public string CountryCode { get; set; } = string.Empty;
    public string? StateCode { get; set; }
    public string TaxType { get; set; } = string.Empty;
    public DateTime CalculatedAt { get; set; }
    public List<TaxBreakdown> Breakdown { get; set; } = new();
}

public class TaxEstimationRequest
{
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string CountryCode { get; set; } = string.Empty;
    
    public string? StateCode { get; set; }
}

public class TaxRatesResponse
{
    public string CountryCode { get; set; } = string.Empty;
    public string? StateCode { get; set; }
    public decimal StandardRate { get; set; }
    public decimal ReducedRate { get; set; }
    public List<TaxRateDetail> Details { get; set; } = new();
    public List<SpecialRate> SpecialRates { get; set; } = new();
}

public class TaxRateDetail
{
    public string TaxType { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class TaxBreakdown
{
    public string TaxType { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public decimal Rate { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class SpecialRate
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ApplicableConditions { get; set; } = string.Empty;
}