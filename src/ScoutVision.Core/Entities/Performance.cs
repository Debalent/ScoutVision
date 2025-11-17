namespace ScoutVision.Core.Entities;

public class Performance
{
    public int Id { get; set; }
    public int PlayerId { get; set; }
    public int? MatchId { get; set; }
    public int Goals { get; set; }
    public int Assists { get; set; }
    public int MinutesPlayed { get; set; }
    public double Rating { get; set; }
    public DateTime PerformanceDate { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    // Navigation properties
    public virtual Player? Player { get; set; }
}