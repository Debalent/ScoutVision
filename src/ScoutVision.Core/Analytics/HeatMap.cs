using ScoutVision.Core.Enums;

using ScoutVision.Core.Entities;

namespace ScoutVision.Core.Analytics;

public class HeatMap : BaseEntity
{
    public int PlayerAnalyticsId { get; set; }
    public string MatchId { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public HeatMapType Type { get; set; }
    
    // Heat map data as JSON coordinates with intensity values
    public string HeatMapData { get; set; } = string.Empty;
    
    // Summary statistics
    public decimal AverageXPosition { get; set; }
    public decimal AverageYPosition { get; set; }
    public decimal FieldCoverage { get; set; }
    public decimal ActivityIntensity { get; set; }
    
    // Zone-based activity (JSON)
    public string ZoneActivity { get; set; } = string.Empty;
    
    // Navigation properties
    public PlayerAnalytics PlayerAnalytics { get; set; } = null!;
}