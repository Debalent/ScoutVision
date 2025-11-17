using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using ScoutVision.Core.Entities;
using ScoutVision.Core.Search;
using ScoutVision.Core.Enums;
using ScoutVision.Infrastructure.Data;

namespace ScoutVision.Infrastructure.Services;

public interface IFootageAnalysisService
{
    Task<GameFootage> AnalyzeFootageAsync(string videoUrl, GameFootage metadata);
    Task<List<FootageHighlight>> ExtractHighlightsAsync(int footageId);
    Task<VideoAnalysisResult> PerformAdvancedAnalysisAsync(int footageId);
    Task UpdateFootageMetadataAsync(int footageId, Dictionary<string, object> metadata);
}

public interface IStatBookService
{
    Task<StatBook> ImportStatBookAsync(string dataSource, string league, string season);
    Task<List<StatBookEntry>> GetPlayerStatsAsync(int playerId, string season);
    Task<List<StatBookEntry>> GetTeamStatsAsync(string teamName, string season);
    Task<ComparisonResult> ComparePlayersAsync(List<int> playerIds, string metric);
    Task<List<StatBookEntry>> GetLeagueLeadersAsync(string league, string metric, int count = 10);
}

public interface ISearchService
{
    Task<SearchResult<Player>> SearchPlayersAsync(PlayerSearchRequest request);
    Task<SearchResult<GameFootage>> SearchFootageAsync(FootageSearchRequest request);
    Task<SearchResult<StatBook>> SearchStatBooksAsync(StatBookSearchRequest request);
    Task<UniversalSearchResult> UniversalSearchAsync(string query, SearchOptions options);
    Task<List<string>> GetSearchSuggestionsAsync(string query, SearchType? type = null);
}

public class FootageAnalysisService : IFootageAnalysisService
{
    private readonly ScoutVisionDbContext _context;
    private readonly ILogger<FootageAnalysisService> _logger;
    private readonly HttpClient _httpClient;

    public FootageAnalysisService(
        ScoutVisionDbContext context, 
        ILogger<FootageAnalysisService> logger,
        HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<GameFootage> AnalyzeFootageAsync(string videoUrl, GameFootage metadata)
    {
        try
        {
            // Call AI service for video analysis
            var analysisRequest = new
            {
                VideoUrl = videoUrl,
                AnalysisType = "comprehensive",
                ExtractHighlights = true,
                DetectPlayers = true,
                AnalyzePerformance = true
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:8000/analyze/video", analysisRequest);
            var analysisResult = await response.Content.ReadFromJsonAsync<VideoAnalysisResponse>();

            // Update metadata with analysis results
            metadata.Duration = analysisResult?.Duration ?? TimeSpan.Zero;
            metadata.Quality = analysisResult?.Quality ?? VideoQuality.HD720p;
            // Store FileSize in AnalysisMetadata JSON
            var analysisMetadata = new { FileSize = analysisResult?.FileSize ?? 0, AnalysisCompletedAt = DateTime.UtcNow };
            metadata.AnalysisMetadata = System.Text.Json.JsonSerializer.Serialize(analysisMetadata);
            metadata.RelevanceScore = analysisResult != null ? CalculateRelevanceScore(analysisResult) : 0m;

            // Extract and save player involvement
            foreach (var playerData in analysisResult?.DetectedPlayers ?? new List<DetectedPlayerData>())
            {
                var player = await _context.Players
                    .FirstOrDefaultAsync(p => p.FullName == playerData.Name);

                if (player != null)
                {
                    var footagePlayer = new FootagePlayer
                    {
                        GameFootageId = metadata.Id,
                        PlayerId = player.Id,
                        ScreenTime = (decimal)playerData.ScreenTime.TotalSeconds,
                        IsFeaturedPlayer = playerData.ScreenTime.TotalSeconds > 60,
                        // Store PerformanceScore in PlayerHighlights JSON
                        PlayerHighlights = System.Text.Json.JsonSerializer.Serialize(new { PerformanceScore = playerData.PerformanceScore })
                    };

                    _context.FootagePlayers.Add(footagePlayer);
                }
            }

            // Save highlights
            foreach (var highlight in analysisResult?.Highlights ?? new List<HighlightData>())
            {
                var footageHighlight = new FootageHighlight
                {
                    GameFootageId = metadata.Id,
                    Title = highlight.Title,
                    StartTime = highlight.StartTime,
                    EndTime = highlight.EndTime,
                    HighlightType = highlight.Type,
                    Significance = highlight.Significance,
                    ThumbnailUrl = highlight.ThumbnailUrl
                };
                
                _context.FootageHighlights.Add(footageHighlight);
            }

            _context.GameFootage.Update(metadata);
            await _context.SaveChangesAsync();

            return metadata;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error analyzing footage for URL: {VideoUrl}", videoUrl);
            throw;
        }
    }

    public async Task<List<FootageHighlight>> ExtractHighlightsAsync(int footageId)
    {
        var footage = await _context.GameFootage
            .Include(f => f.Highlights)
            .FirstOrDefaultAsync(f => f.Id == footageId);

        if (footage == null) return new List<FootageHighlight>();

        // If highlights already exist, return them
        if (footage.Highlights.Any())
        {
            return footage.Highlights.ToList();
        }

        // Extract highlights using AI service
        var extractionRequest = new
        {
            FootageId = footageId,
            VideoUrl = footage.VideoUrl,
            ExtractGoals = true,
            ExtractSaves = true,
            ExtractSkills = true,
            ExtractTacticalMoments = true
        };

        var response = await _httpClient.PostAsJsonAsync("http://localhost:8000/extract/highlights", extractionRequest);
        var highlights = await response.Content.ReadFromJsonAsync<List<HighlightData>>();

        var footageHighlights = highlights?.Select(h => new FootageHighlight
        {
            GameFootageId = footageId,
            Title = h.Title,
            StartTime = h.StartTime,
            EndTime = h.EndTime,
            HighlightType = h.Type,
            Significance = h.Significance,
            ThumbnailUrl = h.ThumbnailUrl
        }).ToList();

        if (footageHighlights != null)
            _context.FootageHighlights.AddRange(footageHighlights);
        await _context.SaveChangesAsync();

        return footageHighlights ?? new List<FootageHighlight>();
    }

    public async Task<VideoAnalysisResult> PerformAdvancedAnalysisAsync(int footageId)
    {
        var footage = await _context.GameFootage.FindAsync(footageId);
        if (footage == null) throw new ArgumentException("Footage not found");

        var analysisRequest = new
        {
            FootageId = footageId,
            VideoUrl = footage.VideoUrl,
            AnalysisDepth = "advanced",
            IncludeTacticalAnalysis = true,
            IncludePlayerTracking = true,
            IncludeHeatMaps = true,
            IncludeStatistics = true
        };

        var response = await _httpClient.PostAsJsonAsync("http://localhost:8000/analyze/advanced", analysisRequest);
        return await response.Content.ReadFromJsonAsync<VideoAnalysisResult>() ?? new VideoAnalysisResult();
    }

    public async Task UpdateFootageMetadataAsync(int footageId, Dictionary<string, object> metadata)
    {
        var footage = await _context.GameFootage.FindAsync(footageId);
        if (footage == null) return;

        foreach (var kvp in metadata)
        {
            switch (kvp.Key.ToLower())
            {
                case "quality":
                    if (kvp.Value != null && Enum.TryParse<VideoQuality>(kvp.Value.ToString(), out var quality))
                        footage.Quality = quality;
                    break;
                case "relevancescore":
                    if (kvp.Value != null && decimal.TryParse(kvp.Value.ToString(), out var score))
                        footage.RelevanceScore = score;
                    break;
                case "keywords":
                    footage.Keywords = kvp.Value?.ToString() ?? string.Empty;
                    break;
            }
        }

        _context.GameFootage.Update(footage);
        await _context.SaveChangesAsync();
    }

    private decimal CalculateRelevanceScore(VideoAnalysisResponse analysisResult)
    {
        var score = 50m; // Base score

        // Adjust based on video quality
        score += analysisResult.Quality switch
        {
            VideoQuality.UHD4K => 30m,
            VideoQuality.HD1080p => 20m,
            VideoQuality.HD720p => 10m,
            VideoQuality.SD480p => 0m,
            _ => 0m
        };

        // Adjust based on detected players and events
        score += Math.Min(analysisResult.DetectedPlayers.Count * 2m, 20m);
        score += Math.Min(analysisResult.Highlights.Count * 3m, 30m);

        return Math.Min(score, 100m);
    }
}

public class StatBookService : IStatBookService
{
    private readonly ScoutVisionDbContext _context;
    private readonly ILogger<StatBookService> _logger;
    private readonly HttpClient _httpClient;

    public StatBookService(
        ScoutVisionDbContext context, 
        ILogger<StatBookService> logger,
        HttpClient httpClient)
    {
        _context = context;
        _logger = logger;
        _httpClient = httpClient;
    }

    public async Task<StatBook> ImportStatBookAsync(string dataSource, string league, string season)
    {
        try
        {
            var importRequest = new
            {
                DataSource = dataSource,
                League = league,
                Season = season,
                ImportType = "full"
            };

            var response = await _httpClient.PostAsJsonAsync("http://localhost:8000/import/statbook", importRequest);
            var importResult = await response.Content.ReadFromJsonAsync<StatBookImportResult>();

            var statBook = new StatBook
            {
                Title = $"{league} {season} Statistics",
                League = league,
                Season = season,
                Competition = importResult?.Competition ?? "Unknown",
                Type = StatBookType.SeasonSummary,
                DataProvider = dataSource,
                DataAccuracy = importResult?.AccuracyScore ?? 0m,
                TotalMatches = importResult?.TotalMatches ?? 0,
                LastUpdated = DateTime.UtcNow,
                IsRealTime = importResult?.IsRealTime ?? false,
                HasAdvancedMetrics = importResult?.HasAdvancedMetrics ?? false,
                // Store FileSize in SearchableContent JSON
                SearchableContent = System.Text.Json.JsonSerializer.Serialize(new { FileSize = importResult?.FileSize ?? 0 }),
                DownloadCount = 0,
                UserRating = 0m
            };

            _context.StatBooks.Add(statBook);
            await _context.SaveChangesAsync();

            // Import individual stat entries
            foreach (var entryData in importResult?.Entries ?? new List<StatEntryData>())
            {
                // Create statistics data JSON
                var statsData = new
                {
                    Goals = entryData.Goals,
                    Assists = entryData.Assists,
                    YellowCards = entryData.YellowCards,
                    RedCards = entryData.RedCards,
                    Rating = entryData.Rating
                };

                var entry = new StatBookEntry
                {
                    StatBookId = statBook.Id,
                    EntityName = entryData.PlayerName,
                    Position = entryData.Position,
                    MatchesPlayed = entryData.MatchesPlayed.ToString(),
                    StatisticsData = System.Text.Json.JsonSerializer.Serialize(statsData),
                    PerformanceMetrics = System.Text.Json.JsonSerializer.Serialize(entryData.AdditionalStats)
                };

                _context.StatBookEntries.Add(entry);
            }

            await _context.SaveChangesAsync();
            return statBook;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error importing stat book from {DataSource} for {League} {Season}", 
                dataSource, league, season);
            throw;
        }
    }

    public async Task<List<StatBookEntry>> GetPlayerStatsAsync(int playerId, string season)
    {
        var player = await _context.Players.FindAsync(playerId);
        if (player == null) return new List<StatBookEntry>();

        return await _context.StatBookEntries
            .Include(e => e.StatBook)
            .Where(e => e.EntityName == player.FullName && e.StatBook.Season == season)
            .OrderByDescending(e => e.StatBook.LastUpdated)
            .ToListAsync();
    }

    public async Task<List<StatBookEntry>> GetTeamStatsAsync(string teamName, string season)
    {
        return await _context.StatBookEntries
            .Include(e => e.StatBook)
            .Where(e => e.EntityName.Contains(teamName) && e.StatBook.Season == season)
            .OrderBy(e => e.EntityName)
            .ToListAsync();
    }

    public async Task<ComparisonResult> ComparePlayersAsync(List<int> playerIds, string metric)
    {
        var players = await _context.Players
            .Where(p => playerIds.Contains(p.Id))
            .ToListAsync();

        var playerStats = new List<PlayerComparisonData>();

        foreach (var player in players)
        {
            var latestStats = await _context.StatBookEntries
                .Include(e => e.StatBook)
                .Where(e => e.EntityName == player.FullName)
                .OrderByDescending(e => e.StatBook.LastUpdated)
                .FirstOrDefaultAsync();

            if (latestStats != null)
            {
                var value = GetMetricValue(latestStats, metric);
                playerStats.Add(new PlayerComparisonData
                {
                    PlayerId = player.Id,
                    PlayerName = player.FullName,
                    MetricValue = value,
                    Position = player.Position.ToString(),
                    Team = player.CurrentTeam
                });
            }
        }

        return new ComparisonResult
        {
            Metric = metric,
            Players = playerStats.OrderByDescending(p => p.MetricValue).ToList(),
            ComparisonDate = DateTime.UtcNow
        };
    }

    public async Task<List<StatBookEntry>> GetLeagueLeadersAsync(string league, string metric, int count = 10)
    {
        var latestStatBook = await _context.StatBooks
            .Where(s => s.League == league)
            .OrderByDescending(s => s.LastUpdated)
            .FirstOrDefaultAsync();

        if (latestStatBook == null) return new List<StatBookEntry>();

        var entries = await _context.StatBookEntries
            .Where(e => e.StatBookId == latestStatBook.Id)
            .ToListAsync();

        return entries
            .OrderByDescending(e => GetMetricValue(e, metric))
            .Take(count)
            .ToList();
    }

    private decimal GetMetricValue(StatBookEntry entry, string metric)
    {
        try
        {
            // Parse statistics from JSON
            var statsData = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(entry.StatisticsData ?? "{}");

            return metric.ToLower() switch
            {
                "goals" => statsData != null && statsData.ContainsKey("Goals") ? Convert.ToDecimal(statsData["Goals"]) : 0m,
                "assists" => statsData != null && statsData.ContainsKey("Assists") ? Convert.ToDecimal(statsData["Assists"]) : 0m,
                "rating" => statsData != null && statsData.ContainsKey("Rating") ? Convert.ToDecimal(statsData["Rating"]) : 0m,
                "matchesplayed" => int.TryParse(entry.MatchesPlayed, out var matches) ? matches : 0m,
                "yellowcards" => statsData != null && statsData.ContainsKey("YellowCards") ? Convert.ToDecimal(statsData["YellowCards"]) : 0m,
                "redcards" => statsData != null && statsData.ContainsKey("RedCards") ? Convert.ToDecimal(statsData["RedCards"]) : 0m,
                _ => 0m
            };
        }
        catch
        {
            return 0m;
        }
    }
}

public class SearchService : ISearchService
{
    private readonly ScoutVisionDbContext _context;
    private readonly ILogger<SearchService> _logger;

    public SearchService(ScoutVisionDbContext context, ILogger<SearchService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<SearchResult<Player>> SearchPlayersAsync(PlayerSearchRequest request)
    {
        var query = _context.Players.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(request.Name))
        {
            query = query.Where(p => p.FirstName.Contains(request.Name) || p.LastName.Contains(request.Name));
        }

        if (!string.IsNullOrEmpty(request.Position))
        {
            query = query.Where(p => p.Position.ToString().Contains(request.Position));
        }

        if (!string.IsNullOrEmpty(request.Team))
        {
            query = query.Where(p => p.CurrentTeam.Contains(request.Team));
        }

        if (request.AgeMin.HasValue)
        {
            var maxBirthDate = DateTime.Today.AddYears(-request.AgeMin.Value);
            query = query.Where(p => p.DateOfBirth <= maxBirthDate);
        }

        if (request.AgeMax.HasValue)
        {
            var minBirthDate = DateTime.Today.AddYears(-request.AgeMax.Value);
            query = query.Where(p => p.DateOfBirth >= minBirthDate);
        }

        var totalCount = await query.CountAsync();

        var results = await query
            .Include(p => p.ContactInfo)
            .Include(p => p.ScoutingReports)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new SearchResult<Player>
        {
            Results = results,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task<SearchResult<GameFootage>> SearchFootageAsync(FootageSearchRequest request)
    {
        var query = _context.GameFootage.AsQueryable();

        if (!string.IsNullOrEmpty(request.Query))
        {
            query = query.Where(f => f.Title.Contains(request.Query) || f.Description.Contains(request.Query));
        }

        if (!string.IsNullOrEmpty(request.Competition))
        {
            query = query.Where(f => f.Competition.Contains(request.Competition));
        }

        if (!string.IsNullOrEmpty(request.Team))
        {
            query = query.Where(f => f.HomeTeam.Contains(request.Team) || f.AwayTeam.Contains(request.Team));
        }

        if (request.DateFrom.HasValue)
        {
            query = query.Where(f => f.MatchDate >= request.DateFrom.Value);
        }

        if (request.DateTo.HasValue)
        {
            query = query.Where(f => f.MatchDate <= request.DateTo.Value);
        }

        var totalCount = await query.CountAsync();

        var results = await query
            .Include(f => f.FootagePlayers)
            .ThenInclude(fp => fp.Player)
            .Include(f => f.Highlights)
            .OrderByDescending(f => f.RelevanceScore)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new SearchResult<GameFootage>
        {
            Results = results,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task<SearchResult<StatBook>> SearchStatBooksAsync(StatBookSearchRequest request)
    {
        var query = _context.StatBooks.AsQueryable();

        if (!string.IsNullOrEmpty(request.Query))
        {
            query = query.Where(s => s.Title.Contains(request.Query) || s.League.Contains(request.Query));
        }

        if (!string.IsNullOrEmpty(request.League))
        {
            query = query.Where(s => s.League.Contains(request.League));
        }

        if (!string.IsNullOrEmpty(request.Season))
        {
            query = query.Where(s => s.Season == request.Season);
        }

        if (request.Type.HasValue)
        {
            query = query.Where(s => s.Type == request.Type.Value);
        }

        var totalCount = await query.CountAsync();

        var results = await query
            .Include(s => s.Entries.Take(5))
            .OrderByDescending(s => s.UserRating)
            .Skip((request.Page - 1) * request.PageSize)
            .Take(request.PageSize)
            .ToListAsync();

        return new SearchResult<StatBook>
        {
            Results = results,
            TotalCount = totalCount,
            Page = request.Page,
            PageSize = request.PageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / request.PageSize)
        };
    }

    public async Task<UniversalSearchResult> UniversalSearchAsync(string query, SearchOptions options)
    {
        var tasks = new List<Task>();
        var results = new UniversalSearchResult { Query = query };

        if (options.IncludePlayers)
        {
            tasks.Add(SearchPlayersForUniversalAsync(query, options.MaxResultsPerType)
                .ContinueWith(t => results.Players = t.Result));
        }

        if (options.IncludeFootage)
        {
            tasks.Add(SearchFootageForUniversalAsync(query, options.MaxResultsPerType)
                .ContinueWith(t => results.Footage = t.Result));
        }

        if (options.IncludeStatBooks)
        {
            tasks.Add(SearchStatBooksForUniversalAsync(query, options.MaxResultsPerType)
                .ContinueWith(t => results.StatBooks = t.Result));
        }

        await Task.WhenAll(tasks);
        return results;
    }

    public async Task<List<string>> GetSearchSuggestionsAsync(string query, SearchType? type = null)
    {
        var suggestions = new List<string>();

        if (string.IsNullOrEmpty(query)) return suggestions;

        if (!type.HasValue || type == SearchType.Players)
        {
            var playerSuggestions = await _context.Players
                .Where(p => p.FirstName.StartsWith(query) || p.LastName.StartsWith(query))
                .Select(p => p.FullName)
                .Take(5)
                .ToListAsync();
            suggestions.AddRange(playerSuggestions);
        }

        if (!type.HasValue || type == SearchType.GameFootage)
        {
            var footageSuggestions = await _context.GameFootage
                .Where(f => f.Title.StartsWith(query) || f.Competition.StartsWith(query))
                .Select(f => f.Title)
                .Take(5)
                .ToListAsync();
            suggestions.AddRange(footageSuggestions);
        }

        return suggestions.Distinct().Take(10).ToList();
    }

    private async Task<List<Player>> SearchPlayersForUniversalAsync(string query, int maxResults)
    {
        return await _context.Players
            .Where(p => p.FirstName.Contains(query) || p.LastName.Contains(query) || p.CurrentTeam.Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }

    private async Task<List<GameFootage>> SearchFootageForUniversalAsync(string query, int maxResults)
    {
        return await _context.GameFootage
            .Where(f => f.Title.Contains(query) || f.Description.Contains(query) || f.Competition.Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }

    private async Task<List<StatBook>> SearchStatBooksForUniversalAsync(string query, int maxResults)
    {
        return await _context.StatBooks
            .Where(s => s.Title.Contains(query) || s.League.Contains(query))
            .Take(maxResults)
            .ToListAsync();
    }
}

// Supporting classes and models
public class VideoAnalysisResponse
{
    public TimeSpan Duration { get; set; }
    public VideoQuality Quality { get; set; }
    public long FileSize { get; set; }
    public List<DetectedPlayerData> DetectedPlayers { get; set; } = new();
    public List<HighlightData> Highlights { get; set; } = new();
}

public class DetectedPlayerData
{
    public string Name { get; set; } = string.Empty;
    public int JerseyNumber { get; set; }
    public TimeSpan ScreenTime { get; set; }
    public decimal PerformanceScore { get; set; }
}

public class HighlightData
{
    public string Title { get; set; } = string.Empty;
    public TimeSpan StartTime { get; set; }
    public TimeSpan EndTime { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Significance { get; set; }
    public string ThumbnailUrl { get; set; } = string.Empty;
}

public class VideoAnalysisResult
{
    public int FootageId { get; set; }
    public Dictionary<string, object> TacticalAnalysis { get; set; } = new();
    public Dictionary<string, object> PlayerTracking { get; set; } = new();
    public Dictionary<string, object> HeatMaps { get; set; } = new();
    public Dictionary<string, object> Statistics { get; set; } = new();
}

public class StatBookImportResult
{
    public string Competition { get; set; } = string.Empty;
    public decimal AccuracyScore { get; set; }
    public int TotalMatches { get; set; }
    public bool IsRealTime { get; set; }
    public bool HasAdvancedMetrics { get; set; }
    public long FileSize { get; set; }
    public List<StatEntryData> Entries { get; set; } = new();
}

public class StatEntryData
{
    public string PlayerName { get; set; } = string.Empty;
    public string TeamName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public int MatchesPlayed { get; set; }
    public decimal Goals { get; set; }
    public decimal Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public decimal Rating { get; set; }
    public Dictionary<string, object> AdditionalStats { get; set; } = new();
}

public class ComparisonResult
{
    public string Metric { get; set; } = string.Empty;
    public List<PlayerComparisonData> Players { get; set; } = new();
    public DateTime ComparisonDate { get; set; }
}

public class PlayerComparisonData
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public decimal MetricValue { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Team { get; set; } = string.Empty;
}

public class SearchResult<T>
{
    public List<T> Results { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
}

public class UniversalSearchResult
{
    public string Query { get; set; } = string.Empty;
    public List<Player> Players { get; set; } = new();
    public List<GameFootage> Footage { get; set; } = new();
    public List<StatBook> StatBooks { get; set; } = new();
}

public class PlayerSearchRequest
{
    public string? Name { get; set; }
    public string? Position { get; set; }
    public string? Team { get; set; }
    public int? AgeMin { get; set; }
    public int? AgeMax { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class FootageSearchRequest
{
    public string? Query { get; set; }
    public string? Competition { get; set; }
    public string? Team { get; set; }
    public DateTime? DateFrom { get; set; }
    public DateTime? DateTo { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class StatBookSearchRequest
{
    public string? Query { get; set; }
    public string? League { get; set; }
    public string? Season { get; set; }
    public StatBookType? Type { get; set; }
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

public class SearchOptions
{
    public bool IncludePlayers { get; set; } = true;
    public bool IncludeFootage { get; set; } = true;
    public bool IncludeStatBooks { get; set; } = true;
    public int MaxResultsPerType { get; set; } = 10;
}