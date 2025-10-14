using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Entities;

public class VideoAnalysis : BaseEntity
{
    public int PlayerId { get; set; }
    public VideoAnalysisType Type { get; set; }
    
    public string Title { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    
    public DateTime AnalyzedAt { get; set; }
    public string AnalyzedBy { get; set; } = string.Empty; // AI or Scout name
    
    // AI Analysis Results
    public decimal? OverallScore { get; set; }
    public string KeyHighlights { get; set; } = string.Empty;
    public string AreasForImprovement { get; set; } = string.Empty;
    public string TechnicalAnalysis { get; set; } = string.Empty;
    public string TacticalAnalysis { get; set; } = string.Empty;
    public string PhysicalAnalysis { get; set; } = string.Empty;
    public string MentalAnalysis { get; set; } = string.Empty;
    
    // Motion Tracking Data
    public string MotionData { get; set; } = string.Empty; // JSON
    public decimal? AverageSpeed { get; set; }
    public decimal? MaxSpeed { get; set; }
    public decimal? DistanceCovered { get; set; }
    
    // Navigation properties
    public Player Player { get; set; } = null!;
    public ICollection<VideoTimestamp> Timestamps { get; set; } = [];
}