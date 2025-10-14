using ScoutVision.Core.Entities;

namespace ScoutVision.Core.Search;

public class FootagePlayer
{
    public int Id { get; set; }
    public int GameFootageId { get; set; }
    public GameFootage GameFootage { get; set; } = null!;
    public int PlayerId { get; set; }
    public Player Player { get; set; } = null!;
    public TimeSpan ScreenTime { get; set; }
    public bool IsFeaturedPlayer { get; set; }
    public decimal PerformanceRating { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

public class FootageHighlight
{
    public int Id { get; set; }
    public int GameFootageId { get; set; }
    public GameFootage GameFootage { get; set; } = null!;
    public string Title { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string HighlightType { get; set; } = string.Empty;
    public decimal Significance { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}

