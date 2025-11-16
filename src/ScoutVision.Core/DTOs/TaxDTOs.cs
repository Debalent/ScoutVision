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
    
    public bool IsB2B { get; set; }
    public string? VatNumber { get; set; }
    public string Currency { get; set; } = "USD";
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
    
    // Additional properties used in TaxController
    public decimal Subtotal { get; set; }
    public decimal TotalTax { get; set; }
    public decimal Total { get; set; }
    public List<TaxBreakdown> TaxBreakdown { get; set; } = new();
    public string Currency { get; set; } = "USD";
    public bool TaxIncluded { get; set; }
    public string TaxJurisdiction { get; set; } = string.Empty;
    public bool IsVatApplicable { get; set; }
    public bool IsReverseCharge { get; set; }
}

public class TaxEstimationRequest
{
    [Required]
    public decimal Amount { get; set; }
    
    [Required]
    public string CountryCode { get; set; } = string.Empty;
    
    public string? StateCode { get; set; }
    
    public bool IsB2B { get; set; }
    public string ProductType { get; set; } = string.Empty;
}

public class TaxRatesResponse
{
    public string CountryCode { get; set; } = string.Empty;
    public string? StateCode { get; set; }
    public decimal StandardRate { get; set; }
    public decimal ReducedRate { get; set; }
    public List<TaxRateDetail> Details { get; set; } = new();
    public List<SpecialRate> SpecialRates { get; set; } = new();
    
    public string CountryName { get; set; } = string.Empty;
    public string? StateName { get; set; }
    public string TaxType { get; set; } = "VAT";
    public bool IsEuCountry { get; set; }
    public DateTime LastUpdated { get; set; } = DateTime.UtcNow;
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
    public string Jurisdiction { get; set; } = string.Empty;
}

public class SpecialRate
{
    public string Name { get; set; } = string.Empty;
    public decimal Rate { get; set; }
    public string Description { get; set; } = string.Empty;
    public string ApplicableConditions { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
}