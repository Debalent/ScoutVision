namespace ScoutVision.Core.Entities;

public class ScoutingReport : BaseEntity
{
    public int PlayerId { get; set; }
    public string ScoutName { get; set; } = string.Empty;
    public string ScoutOrganization { get; set; } = string.Empty;
    
    public DateTime ReportDate { get; set; }
    public string MatchContext { get; set; } = string.Empty; // Match details, venue, etc.
    
    // Overall Assessment
    public decimal OverallRating { get; set; } // 1-10
    public string ExecutiveSummary { get; set; } = string.Empty;
    
    // Detailed Ratings (1-10)
    public decimal TechnicalSkills { get; set; }
    public decimal PhysicalAttributes { get; set; }
    public decimal TacticalAwareness { get; set; }
    public decimal MentalStrength { get; set; }
    public decimal Potential { get; set; }
    
    // Detailed Analysis
    public string StrengthsDetail { get; set; } = string.Empty;
    public string WeaknessesDetail { get; set; } = string.Empty;
    public string TacticalAnalysis { get; set; } = string.Empty;
    public string RecommendationsForDevelopment { get; set; } = string.Empty;
    
    // Specific Observations
    public string GamePerformance { get; set; } = string.Empty;
    public string InteractionWithTeammates { get; set; } = string.Empty;
    public string ReactionToPressure { get; set; } = string.Empty;
    
    // Scout Recommendation
    public bool RecommendForAcquisition { get; set; }
    public string RecommendationLevel { get; set; } = string.Empty; // Immediate, Monitor, Pass
    public string AdditionalNotes { get; set; } = string.Empty;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}