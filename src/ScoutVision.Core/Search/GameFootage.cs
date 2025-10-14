using ScoutVision.Core.Enums;

namespace ScoutVision.Core.Search;

public class GameFootage : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string VideoUrl { get; set; } = string.Empty;
    public string ThumbnailUrl { get; set; } = string.Empty;
    public TimeSpan Duration { get; set; }
    public DateTime MatchDate { get; set; }
    public string Competition { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    
    // Match Information
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public string Venue { get; set; } = string.Empty;
    public string MatchResult { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    
    // Video Quality and Source
    public VideoQuality Quality { get; set; } = VideoQuality.HD720p;
    public string SourceProvider { get; set; } = string.Empty;
    public decimal SourceRating { get; set; }
    public bool IsHighlightsOnly { get; set; }
    public bool IsFullMatch { get; set; }
    
    // Content Metadata
    public string Keywords { get; set; } = string.Empty; // Space-separated keywords
    public string PlayersInvolved { get; set; } = string.Empty; // JSON array of player IDs
    public string EventTimestamps { get; set; } = string.Empty; // JSON of key events
    public string TacticalHighlights { get; set; } = string.Empty; // JSON
    
    // Search and Discovery
    public int ViewCount { get; set; }
    public decimal RelevanceScore { get; set; }
    public DateTime IndexedAt { get; set; }
    public string SearchTags { get; set; } = string.Empty;
    
    // AI Analysis Results
    public bool IsAnalyzed { get; set; }
    public string AnalysisMetadata { get; set; } = string.Empty; // JSON
    
    // Navigation properties
    public ICollection<FootagePlayer> FootagePlayers { get; set; } = [];
    public ICollection<FootageHighlight> Highlights { get; set; } = [];
}

public class FootagePlayer : BaseEntity
{
    public int GameFootageId { get; set; }
    public int PlayerId { get; set; }
    public decimal ScreenTime { get; set; } // Percentage of video featuring this player
    public bool IsFeaturedPlayer { get; set; }
    public string PlayerHighlights { get; set; } = string.Empty; // JSON timestamps
    
    // Navigation properties
    public GameFootage GameFootage { get; set; } = null!;
    public Player Player { get; set; } = null!;
}