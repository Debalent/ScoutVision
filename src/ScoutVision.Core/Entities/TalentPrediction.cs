using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class TalentPrediction : BaseEntity
{
    public int PlayerId { get; set; }
    
    public string ModelVersion { get; set; } = string.Empty;
    public DateTime PredictionDate { get; set; }
    
    // Prediction Results
    public decimal OverallPotentialScore { get; set; } // 0-100
    public PredictionConfidence Confidence { get; set; }
    
    // Specific Predictions
    public decimal? ProfessionalSuccessLikelihood { get; set; }
    public decimal? InjuryRiskScore { get; set; }
    public decimal? PeakPerformanceAge { get; set; }
    public decimal? CareerLongevityScore { get; set; }
    public decimal? LeadershipPotential { get; set; }
    
    // Market Value Predictions
    public decimal? CurrentMarketValue { get; set; }
    public decimal? ProjectedMarketValue1Year { get; set; }
    public decimal? ProjectedMarketValue3Years { get; set; }
    public decimal? ProjectedMarketValue5Years { get; set; }
    
    public string PredictionSummary { get; set; } = string.Empty;
    public string KeyFactors { get; set; } = string.Empty;
    public string RiskFactors { get; set; } = string.Empty;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}