using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class Player : BaseEntity
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}";
    public DateTime DateOfBirth { get; set; }
    public int Age => DateTime.Today.Year - DateOfBirth.Year - (DateTime.Today.DayOfYear < DateOfBirth.DayOfYear ? 1 : 0);
    
    public string Position { get; set; } = string.Empty;
    public string CurrentTeam { get; set; } = string.Empty;
    public string League { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    
    public decimal Height { get; set; } // in meters
    public decimal Weight { get; set; } // in kg
    
    public PlayerStatus Status { get; set; } = PlayerStatus.Active;
    public ScoutingPriority Priority { get; set; } = ScoutingPriority.Medium;
    
    public string Biography { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    // Navigation properties
    public PlayerContactInfo? ContactInfo { get; set; }
    public ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = [];
    public ICollection<VideoAnalysis> VideoAnalyses { get; set; } = [];
    public ICollection<ScoutingReport> ScoutingReports { get; set; } = [];
    public ICollection<TalentPrediction> TalentPredictions { get; set; } = [];
    public ICollection<MindsetProfile> MindsetProfiles { get; set; } = [];
    public ICollection<PlayerTag> Tags { get; set; } = [];
}