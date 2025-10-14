using ScoutVision.Core.Enums;

using ScoutVision.Core.Entities;

namespace ScoutVision.Core.Search;

public class StatBook : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public string League { get; set; } = string.Empty;
    public string Season { get; set; } = string.Empty;
    public string Competition { get; set; } = string.Empty;
    public DateTime PublishedDate { get; set; }
    public StatBookType Type { get; set; }
    
    // Data Source Information
    public string DataProvider { get; set; } = string.Empty;
    public string SourceUrl { get; set; } = string.Empty;
    public decimal DataAccuracy { get; set; }
    public DateTime LastUpdated { get; set; }
    public bool IsRealTime { get; set; }
    
    // Content Information
    public string CoveredTeams { get; set; } = string.Empty; // JSON array
    public string CoveredPlayers { get; set; } = string.Empty; // JSON array
    public int TotalMatches { get; set; }
    public DateTime SeasonStartDate { get; set; }
    public DateTime SeasonEndDate { get; set; }
    
    // Statistical Categories
    public string AvailableMetrics { get; set; } = string.Empty; // JSON array
    public bool HasAdvancedMetrics { get; set; }
    public bool HasPlayerTracking { get; set; }
    public bool HasEventData { get; set; }
    
    // Search and Filtering
    public string SearchableContent { get; set; } = string.Empty;
    public string Keywords { get; set; } = string.Empty;
    public int DownloadCount { get; set; }
    public decimal UserRating { get; set; }
    
    // Navigation properties
    public ICollection<StatBookEntry> Entries { get; set; } = [];
}

public class StatBookEntry : BaseEntity
{
    public int StatBookId { get; set; }
    public int? PlayerId { get; set; }
    public int? TeamId { get; set; }
    public string EntityName { get; set; } = string.Empty; // Player or team name
    public string Position { get; set; } = string.Empty;
    
    // Statistical Data (JSON)
    public string StatisticsData { get; set; } = string.Empty;
    public string PerformanceMetrics { get; set; } = string.Empty;
    public string RankingData { get; set; } = string.Empty;
    
    // Context Information
    public string MatchesPlayed { get; set; } = string.Empty;
    public string TimeFrame { get; set; } = string.Empty;
    public string Notes { get; set; } = string.Empty;
    
    // Navigation properties
    public StatBook StatBook { get; set; } = null!;
    public Player? Player { get; set; }
}