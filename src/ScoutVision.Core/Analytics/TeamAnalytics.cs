namespace ScoutVision.Core.Analytics;

public class TeamAnalytics : BaseEntity
{
    public string TeamName { get; set; } = string.Empty;
    public string League { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    
    // Team Performance
    public int MatchesPlayed { get; set; }
    public int Wins { get; set; }
    public int Draws { get; set; }
    public int Losses { get; set; }
    public int GoalsFor { get; set; }
    public int GoalsAgainst { get; set; }
    public int CleanSheets { get; set; }
    public decimal AverageGoalsPerGame { get; set; }
    public decimal AverageGoalsConcededPerGame { get; set; }
    
    // Tactical Analysis
    public string PreferredFormation { get; set; } = string.Empty;
    public decimal AveragePossession { get; set; }
    public decimal PassAccuracy { get; set; }
    public decimal PressureIntensity { get; set; }
    public decimal DefensiveLine { get; set; }
    public string PlayingStyle { get; set; } = string.Empty; // Possession, Counter-Attack, Direct, etc.
    
    // Set Pieces
    public int CornerKicks { get; set; }
    public int CornerKickGoals { get; set; }
    public int FreeKicks { get; set; }
    public int FreeKickGoals { get; set; }
    public int Penalties { get; set; }
    public int PenaltyGoals { get; set; }
    
    // Advanced Metrics
    public decimal ExpectedGoalsFor { get; set; }
    public decimal ExpectedGoalsAgainst { get; set; }
    public decimal TeamEfficiencyRating { get; set; }
    public decimal FormIndex { get; set; }
    
    // Player Performance Distribution
    public string PlayerContributions { get; set; } = string.Empty; // JSON
    public string FormationAnalysis { get; set; } = string.Empty; // JSON
    
    // Navigation properties
    public ICollection<TeamPlayer> TeamPlayers { get; set; } = [];
    public ICollection<MatchAnalysis> MatchAnalyses { get; set; } = [];
}

public class TeamPlayer : BaseEntity
{
    public int TeamAnalyticsId { get; set; }
    public int PlayerId { get; set; }
    public string Position { get; set; } = string.Empty;
    public bool IsStarter { get; set; }
    public decimal ContributionScore { get; set; }
    
    // Navigation properties
    public TeamAnalytics TeamAnalytics { get; set; } = null!;
    public Player Player { get; set; } = null!;
}