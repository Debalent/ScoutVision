using ScoutVision.Core.Entities;

namespace ScoutVision.Core.Analytics;

public class MatchAnalysis : BaseEntity
{
    public int TeamAnalyticsId { get; set; }
    public string MatchId { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Competition { get; set; } = string.Empty;
    public string Opponent { get; set; } = string.Empty;
    public bool IsHome { get; set; }
    
    // Match Result
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public string Result { get; set; } = string.Empty; // Win, Draw, Loss
    
    // Team Performance Metrics
    public decimal Possession { get; set; }
    public int Shots { get; set; }
    public int ShotsOnTarget { get; set; }
    public int Corners { get; set; }
    public int Fouls { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    
    // Advanced Statistics
    public decimal PassAccuracy { get; set; }
    public decimal ExpectedGoals { get; set; } // xG
    public decimal ExpectedGoalsAgainst { get; set; } // xGA
    public int Tackles { get; set; }
    public int Interceptions { get; set; }
    
    // Formation and Tactics
    public string Formation { get; set; } = string.Empty;
    public string TacticalApproach { get; set; } = string.Empty;
    
    // Performance Rating
    public decimal TeamRating { get; set; }
    public string PerformanceNotes { get; set; } = string.Empty;
    
    // Navigation properties
    public TeamAnalytics TeamAnalytics { get; set; } = null!;
}
