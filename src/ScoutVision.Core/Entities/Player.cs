using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class Player
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public int Age { get; set; }
    public Position Position { get; set; }
    public string? Team { get; set; }
    public string CurrentTeam { get; set; } = string.Empty;
    public string League { get; set; } = string.Empty;
    public string? Nationality { get; set; }
    public DateTime DateOfBirth { get; set; }
    public double Height { get; set; }
    public double Weight { get; set; }
    public string Status { get; set; } = "Active";
    public ScoutingPriority Priority { get; set; } = ScoutingPriority.Medium;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;
    public bool IsDeleted { get; set; } = false;
    public string? Biography { get; set; }
    
    // Computed properties
    public string FullName => $"{FirstName} {LastName}".Trim();
    
    // Navigation properties
    public virtual ICollection<Performance> Performances { get; set; } = new List<Performance>();
    public virtual ICollection<InjuryReport> InjuryReports { get; set; } = new List<InjuryReport>();
    public virtual PlayerContactInfo? ContactInfo { get; set; }
    public virtual ICollection<PerformanceMetric> PerformanceMetrics { get; set; } = new List<PerformanceMetric>();
    public virtual ICollection<VideoAnalysis> VideoAnalyses { get; set; } = new List<VideoAnalysis>();
    public virtual ICollection<TalentPrediction> TalentPredictions { get; set; } = new List<TalentPrediction>();
    public virtual ICollection<MindsetProfile> MindsetProfiles { get; set; } = new List<MindsetProfile>();
    public virtual ICollection<ScoutingReport> ScoutingReports { get; set; } = new List<ScoutingReport>();
    public virtual ICollection<PlayerTag> Tags { get; set; } = new List<PlayerTag>();
}
