using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Analytics;

public class PlayerAnalytics : BaseEntity
{
    public int PlayerId { get; set; }
    public string Season { get; set; } = string.Empty;
    public string Competition { get; set; } = string.Empty;
    public DateTime AnalysisDate { get; set; }
    
    // Game Statistics
    public int GamesPlayed { get; set; }
    public int GamesStarted { get; set; }
    public decimal MinutesPlayed { get; set; }
    public decimal AverageMinutesPerGame { get; set; }
    
    // Offensive Statistics
    public int Goals { get; set; }
    public int Assists { get; set; }
    public decimal GoalsPerGame { get; set; }
    public decimal AssistsPerGame { get; set; }
    public int Shots { get; set; }
    public int ShotsOnTarget { get; set; }
    public decimal ShotAccuracy { get; set; }
    public decimal ExpectedGoals { get; set; } // xG
    public decimal ExpectedAssists { get; set; } // xA
    
    // Passing Statistics
    public int PassesAttempted { get; set; }
    public int PassesCompleted { get; set; }
    public decimal PassAccuracy { get; set; }
    public int KeyPasses { get; set; }
    public int CrossesAttempted { get; set; }
    public int CrossesCompleted { get; set; }
    public decimal AveragePassLength { get; set; }
    public int ProgressivePasses { get; set; }
    
    // Defensive Statistics
    public int Tackles { get; set; }
    public int TacklesWon { get; set; }
    public decimal TackleSuccessRate { get; set; }
    public int Interceptions { get; set; }
    public int Clearances { get; set; }
    public int BlockedShots { get; set; }
    public int AerialDuels { get; set; }
    public int AerialDuelsWon { get; set; }
    public decimal AerialDuelSuccessRate { get; set; }
    
    // Physical Statistics
    public decimal DistanceCovered { get; set; }
    public decimal SprintDistance { get; set; }
    public decimal MaxSpeed { get; set; }
    public decimal AverageSpeed { get; set; }
    public int Sprints { get; set; }
    public int HighIntensityRuns { get; set; }
    
    // Disciplinary
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public int Fouls { get; set; }
    public int FoulsDrawn { get; set; }
    
    // Advanced Metrics
    public decimal PlayerEfficiencyRating { get; set; }
    public decimal ImpactScore { get; set; }
    public decimal ConsistencyIndex { get; set; }
    public decimal FormRating { get; set; }
    public decimal InjuryRisk { get; set; }
    
    // Position-Specific Metrics (JSON)
    public string PositionSpecificMetrics { get; set; } = string.Empty;
    
    // Navigation properties
    public Player Player { get; set; } = null!;
    public ICollection<PerformanceTrend> PerformanceTrends { get; set; } = [];
    public ICollection<HeatMap> HeatMaps { get; set; } = [];
}