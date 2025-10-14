namespace ScoutVision.Core.Entities;

public class VideoTimestamp : BaseEntity
{
    public int VideoAnalysisId { get; set; }
    
    public TimeSpan Timestamp { get; set; }
    public string Event { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal? Score { get; set; }
    public string Category { get; set; } = string.Empty; // Goal, Assist, Tackle, etc.
    
    // Navigation properties
    public VideoAnalysis VideoAnalysis { get; set; } = null!;
}