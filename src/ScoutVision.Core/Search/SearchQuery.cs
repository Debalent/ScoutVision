using ScoutVision.Core.Entities;

namespace ScoutVision.Core.Search;

public class SearchQuery : BaseEntity
{
    public string UserId { get; set; } = string.Empty;
    public string QueryText { get; set; } = string.Empty;
    public SearchType SearchType { get; set; }
    public DateTime ExecutedAt { get; set; }
    
    // Search Parameters
    public string Filters { get; set; } = string.Empty; // JSON
    public string SortCriteria { get; set; } = string.Empty;
    public int PageSize { get; set; } = 20;
    public int PageNumber { get; set; } = 1;
    
    // Results Information
    public int TotalResults { get; set; }
    public decimal ExecutionTimeMs { get; set; }
    public bool WasCached { get; set; }
    
    // AI Enhancement
    public bool UsedNLP { get; set; }
    public string InterpretedQuery { get; set; } = string.Empty;
    public string Suggestions { get; set; } = string.Empty; // JSON
    
    // Navigation properties
    public ICollection<SearchResult> SearchResults { get; set; } = [];
}

public class SearchResult : BaseEntity
{
    public int SearchQueryId { get; set; }
    public string ResultType { get; set; } = string.Empty; // Player, Team, Footage, StatBook
    public string ResultId { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal RelevanceScore { get; set; }
    public int Position { get; set; } // Position in search results
    
    // Result Metadata
    public string ThumbnailUrl { get; set; } = string.Empty;
    public string PreviewData { get; set; } = string.Empty; // JSON
    public bool IsPromoted { get; set; }
    
    // Interaction Tracking
    public bool WasClicked { get; set; }
    public DateTime? ClickedAt { get; set; }
    
    // Navigation properties
    public SearchQuery SearchQuery { get; set; } = null!;
}

public enum SearchType
{
    Players,
    Teams,
    GameFootage,
    StatBooks,
    All,
    Analytics,
    Predictions
}