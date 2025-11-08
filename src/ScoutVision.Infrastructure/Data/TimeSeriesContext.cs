using Microsoft.EntityFrameworkCore;
using ScoutVision.Core.Entities;

namespace ScoutVision.Infrastructure.Data;

/// <summary>
/// TimescaleDB context for time-series data (performance metrics, injuries, predictions)
/// Optimized for fast queries on high-volume time-series data
/// </summary>
public class TimeSeriesContext : DbContext
{
    public TimeSeriesContext(DbContextOptions<TimeSeriesContext> options) : base(options)
    {
    }

    // Performance metrics (hypertable in TimescaleDB)
    public DbSet<PlayerPerformanceMetric> PlayerPerformanceMetrics { get; set; }
    public DbSet<InjuryRiskMetric> InjuryRiskMetrics { get; set; }
    public DbSet<TransferMarketMetric> TransferMarketMetrics { get; set; }
    public DbSet<MatchAnalyticsMetric> MatchAnalyticsMetrics { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Configure time-series tables
        modelBuilder.Entity<PlayerPerformanceMetric>()
            .ToTable("player_performance_metrics")
            .HasIndex(m => new { m.PlayerId, m.Timestamp });

        modelBuilder.Entity<InjuryRiskMetric>()
            .ToTable("injury_risk_metrics")
            .HasIndex(m => new { m.PlayerId, m.Timestamp });

        modelBuilder.Entity<TransferMarketMetric>()
            .ToTable("transfer_market_metrics")
            .HasIndex(m => new { m.PlayerId, m.Timestamp });

        modelBuilder.Entity<MatchAnalyticsMetric>()
            .ToTable("match_analytics_metrics")
            .HasIndex(m => new { m.MatchId, m.Timestamp });
    }
}

/// <summary>
/// Time-series entity for player performance metrics
/// Stored with nanosecond precision timestamps for real-time analysis
/// </summary>
public class PlayerPerformanceMetric
{
    public long Id { get; set; }
    public string PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
    
    // Real-time metrics
    public decimal SprintDistance { get; set; }
    public decimal MaxSpeed { get; set; }
    public decimal Acceleration { get; set; }
    public int PassesCompleted { get; set; }
    public int PassesAttempted { get; set; }
    public decimal PassAccuracy { get; set; }
    public int ShotAttempts { get; set; }
    public int Tackles { get; set; }
    public int Interceptions { get; set; }
    
    // Calculated metrics
    public decimal Intensity { get; set; } // 0-100
    public decimal FormRating { get; set; } // 0-10
    
    public string TenantId { get; set; }
}

/// <summary>
/// Time-series entity for injury risk metrics
/// Updated in real-time from wearable data and video analysis
/// </summary>
public class InjuryRiskMetric
{
    public long Id { get; set; }
    public string PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
    
    // Movement analysis
    public decimal PosturalAsymmetry { get; set; }
    public decimal LoadRatio { get; set; }
    public decimal RecoveryStatus { get; set; }
    
    // Risk indicators
    public int RiskScore { get; set; } // 0-100
    public string PrimaryRiskType { get; set; }
    public string SecondaryRiskType { get; set; }
    public decimal RiskTrend { get; set; } // -1.0 to 1.0
    
    // Workload metrics
    public decimal CumulativeLoad { get; set; }
    public decimal AcuteLoad { get; set; }
    public decimal LoadRatioIndicator { get; set; }
    public bool IsAnomalous { get; set; }
    
    public string TenantId { get; set; }
}

/// <summary>
/// Time-series entity for transfer market valuations
/// Tracks market value changes throughout the season
/// </summary>
public class TransferMarketMetric
{
    public long Id { get; set; }
    public string PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
    
    // Valuation
    public decimal EstimatedMarketValue { get; set; }
    public decimal PreviousValue { get; set; }
    public decimal ValueChange { get; set; }
    public decimal ValueChangePercent { get; set; }
    
    // Market indicators
    public decimal TransferProbability { get; set; } // 0-1
    public string BuySellHoldRecommendation { get; set; }
    public int TransferInterestCount { get; set; }
    
    // Comparable analysis
    public decimal ComparablePlayersAverageValue { get; set; }
    public int ComparablePlaysAnalyzed { get; set; }
    
    public string TenantId { get; set; }
}

/// <summary>
/// Time-series entity for match analytics
/// Real-time aggregated data from all players during a match
/// </summary>
public class MatchAnalyticsMetric
{
    public long Id { get; set; }
    public string MatchId { get; set; }
    public string TeamId { get; set; }
    public DateTime Timestamp { get; set; }
    public int MinuteOfMatch { get; set; }
    
    // Aggregate metrics
    public decimal AveragePacerating { get; set; }
    public decimal PossessionPercent { get; set; }
    public decimal PassCompletionRate { get; set; }
    public int TotalPasses { get; set; }
    public int TotalTackles { get; set; }
    public int TotalShotsOnTarget { get; set; }
    
    // Formation and positioning
    public string FormationType { get; set; }
    public int PlayersCoveredByHeatmap { get; set; }
    
    // Performance indicators
    public decimal DefensiveIntensity { get; set; } // 0-100
    public decimal OffensiveThreat { get; set; } // 0-100
    
    public string TenantId { get; set; }
}