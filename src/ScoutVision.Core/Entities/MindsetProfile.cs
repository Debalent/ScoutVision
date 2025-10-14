using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class MindsetProfile : BaseEntity
{
    public int PlayerId { get; set; }
    
    public DateTime AssessmentDate { get; set; }
    public string AssessmentMethod { get; set; } = string.Empty; // Interview, Survey, Observation
    public string AssessorName { get; set; } = string.Empty;
    
    // Mindset Scores (1-10)
    public decimal LeadershipScore { get; set; }
    public decimal ResilienceScore { get; set; }
    public decimal TeamWorkScore { get; set; }
    public decimal DisciplineScore { get; set; }
    public decimal MotivationScore { get; set; }
    public decimal AdaptabilityScore { get; set; }
    public decimal PressureHandlingScore { get; set; }
    
    public decimal OverallMindsetScore { get; set; }
    
    // Detailed Analysis
    public string StrengthsAnalysis { get; set; } = string.Empty;
    public string WeaknessesAnalysis { get; set; } = string.Empty;
    public string PersonalityTraits { get; set; } = string.Empty;
    public string RecommendedDevelopment { get; set; } = string.Empty;
    
    // Additional Insights
    public string CommunicationStyle { get; set; } = string.Empty;
    public string ConflictResolution { get; set; } = string.Empty;
    public string LearningPreference { get; set; } = string.Empty;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}