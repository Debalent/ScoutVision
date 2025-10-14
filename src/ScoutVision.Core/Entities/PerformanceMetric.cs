using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class PerformanceMetric : BaseEntity
{
    public int PlayerId { get; set; }
    public PerformanceMetricType Type { get; set; }
    
    public string MetricName { get; set; } = string.Empty;
    public decimal Value { get; set; }
    public string Unit { get; set; } = string.Empty;
    public DateTime MeasuredAt { get; set; }
    
    public string Description { get; set; } = string.Empty;
    public string Context { get; set; } = string.Empty; // Game, Training, Test, etc.
    
    // Benchmarking
    public decimal? LeagueAverage { get; set; }
    public decimal? PositionAverage { get; set; }
    public int? Percentile { get; set; }
    
    // Navigation properties
    public Player Player { get; set; } = null!;
}