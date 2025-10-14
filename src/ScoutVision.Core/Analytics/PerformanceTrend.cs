namespace ScoutVision.Core.Analytics;

public class PerformanceTrend : BaseEntity
{
    public int PlayerAnalyticsId { get; set; }
    public string MetricName { get; set; } = string.Empty;
    public DateTime Date { get; set; }
    public decimal Value { get; set; }
    public decimal MovingAverage { get; set; }
    public string TrendDirection { get; set; } = string.Empty; // Improving, Declining, Stable
    public decimal ChangePercentage { get; set; }
    
    // Navigation properties
    public PlayerAnalytics PlayerAnalytics { get; set; } = null!;
}