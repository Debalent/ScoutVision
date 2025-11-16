using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutVision.Core.Search;
using ScoutVision.Core.Entities;
using ScoutVision.Core.Enums;
using ScoutVision.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace ScoutVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class SearchController : ControllerBase
{
    private readonly ScoutVisionDbContext _context;
    private readonly ILogger<SearchController> _logger;

    public SearchController(ScoutVisionDbContext context, ILogger<SearchController> logger)
    {
        _context = context;
        _logger = logger;
    }

    [HttpGet("footage")]
    [SwaggerOperation(Summary = "Search game footage", Description = "Intelligent search for game footage with advanced filtering")]
    public async Task<ActionResult<object>> SearchFootage(
        [FromQuery] string query = "",
        [FromQuery] string? team = null,
        [FromQuery] string? player = null,
        [FromQuery] string? competition = null,
        [FromQuery] string? season = null,
        [FromQuery] DateTime? dateFrom = null,
        [FromQuery] DateTime? dateTo = null,
        [FromQuery] string? quality = null,
        [FromQuery] bool highlightsOnly = false,
        [FromQuery] bool fullMatchOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var searchQuery = new SearchQuery
        {
            QueryText = query,
            SearchType = SearchType.GameFootage,
            ExecutedAt = DateTime.UtcNow,
            PageSize = pageSize,
            PageNumber = page
        };

        var footageQuery = _context.GameFootage.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(query))
        {
            footageQuery = footageQuery.Where(f => 
                f.Title.Contains(query) || 
                f.Description.Contains(query) ||
                f.Keywords.Contains(query) ||
                f.HomeTeam.Contains(query) ||
                f.AwayTeam.Contains(query));
        }

        if (!string.IsNullOrEmpty(team))
        {
            footageQuery = footageQuery.Where(f => 
                f.HomeTeam.Contains(team) || f.AwayTeam.Contains(team));
        }

        if (!string.IsNullOrEmpty(player))
        {
            footageQuery = footageQuery.Where(f => 
                f.PlayersInvolved.Contains(player));
        }

        if (!string.IsNullOrEmpty(competition))
        {
            footageQuery = footageQuery.Where(f => f.Competition.Contains(competition));
        }

        if (!string.IsNullOrEmpty(season))
        {
            footageQuery = footageQuery.Where(f => f.Season == season);
        }

        if (dateFrom.HasValue)
        {
            footageQuery = footageQuery.Where(f => f.MatchDate >= dateFrom.Value);
        }

        if (dateTo.HasValue)
        {
            footageQuery = footageQuery.Where(f => f.MatchDate <= dateTo.Value);
        }

        if (!string.IsNullOrEmpty(quality) && Enum.TryParse<VideoQuality>(quality, out var videoQuality))
        {
            footageQuery = footageQuery.Where(f => f.Quality == videoQuality);
        }

        if (highlightsOnly)
        {
            footageQuery = footageQuery.Where(f => f.IsHighlightsOnly);
        }

        if (fullMatchOnly)
        {
            footageQuery = footageQuery.Where(f => f.IsFullMatch);
        }

        // Count total results
        var totalResults = await footageQuery.CountAsync();
        searchQuery.TotalResults = totalResults;

        // Apply pagination and sorting
        var results = await footageQuery
            .Include(f => f.FootagePlayers)
            .ThenInclude(fp => fp.Player)
            .OrderByDescending(f => f.RelevanceScore)
            .ThenByDescending(f => f.MatchDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Log search query
        _context.SearchQueries.Add(searchQuery);
        await _context.SaveChangesAsync();

        // Generate recommendations
        var recommendations = await GenerateFootageRecommendations(query, results);
        var relatedSearches = await GetRelatedSearches(query, SearchType.GameFootage);

        return Ok(new
        {
            Query = query,
            Results = results.Select(r => new
            {
                r.Id,
                r.Title,
                r.Description,
                r.VideoUrl,
                r.ThumbnailUrl,
                r.Duration,
                r.MatchDate,
                r.Competition,
                r.HomeTeam,
                r.AwayTeam,
                r.Quality,
                r.IsHighlightsOnly,
                r.IsFullMatch,
                r.RelevanceScore,
                FeaturedPlayers = r.FootagePlayers.Where(fp => fp.IsFeaturedPlayer)
                    .Select(fp => new { fp.Player.FullName, fp.ScreenTime })
            }),
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalResults = totalResults,
                TotalPages = (int)Math.Ceiling((double)totalResults / pageSize)
            },
            Filters = new
            {
                AppliedFilters = new { team, player, competition, season, quality, highlightsOnly, fullMatchOnly },
                AvailableCompetitions = await GetAvailableCompetitions(),
                AvailableSeasons = await GetAvailableSeasons(),
                QualityOptions = Enum.GetNames<VideoQuality>()
            },
            Recommendations = recommendations,
            RelatedSearches = relatedSearches
        });
    }

    [HttpGet("statbooks")]
    [SwaggerOperation(Summary = "Search statbooks", Description = "Search comprehensive statistical databases and reports")]
    public async Task<ActionResult<object>> SearchStatbooks(
        [FromQuery] string query = "",
        [FromQuery] string? league = null,
        [FromQuery] string? season = null,
        [FromQuery] string? type = null,
        [FromQuery] bool realTimeOnly = false,
        [FromQuery] bool advancedMetricsOnly = false,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var searchQuery = new SearchQuery
        {
            QueryText = query,
            SearchType = SearchType.StatBooks,
            ExecutedAt = DateTime.UtcNow,
            PageSize = pageSize,
            PageNumber = page
        };

        var statbookQuery = _context.StatBooks.AsQueryable();

        // Apply filters
        if (!string.IsNullOrEmpty(query))
        {
            statbookQuery = statbookQuery.Where(s => 
                s.Title.Contains(query) || 
                s.SearchableContent.Contains(query) ||
                s.Keywords.Contains(query));
        }

        if (!string.IsNullOrEmpty(league))
        {
            statbookQuery = statbookQuery.Where(s => s.League.Contains(league));
        }

        if (!string.IsNullOrEmpty(season))
        {
            statbookQuery = statbookQuery.Where(s => s.Season == season);
        }

        if (!string.IsNullOrEmpty(type) && Enum.TryParse<StatBookType>(type, out var statBookType))
        {
            statbookQuery = statbookQuery.Where(s => s.Type == statBookType);
        }

        if (realTimeOnly)
        {
            statbookQuery = statbookQuery.Where(s => s.IsRealTime);
        }

        if (advancedMetricsOnly)
        {
            statbookQuery = statbookQuery.Where(s => s.HasAdvancedMetrics);
        }

        var totalResults = await statbookQuery.CountAsync();
        searchQuery.TotalResults = totalResults;

        var results = await statbookQuery
            .Include(s => s.Entries.Take(5))
            .OrderByDescending(s => s.UserRating)
            .ThenByDescending(s => s.LastUpdated)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Log search query
        _context.SearchQueries.Add(searchQuery);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Query = query,
            Results = results.Select(r => new
            {
                r.Id,
                r.Title,
                r.League,
                r.Season,
                r.Competition,
                r.Type,
                r.DataProvider,
                r.DataAccuracy,
                r.LastUpdated,
                r.IsRealTime,
                r.HasAdvancedMetrics,
                r.TotalMatches,
                r.UserRating,
                r.DownloadCount,
                PreviewEntries = r.Entries.Take(3)
            }),
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalResults = totalResults,
                TotalPages = (int)Math.Ceiling((double)totalResults / pageSize)
            },
            Filters = new
            {
                AppliedFilters = new { league, season, type, realTimeOnly, advancedMetricsOnly },
                AvailableLeagues = await GetAvailableLeagues(),
                AvailableStatbookTypes = Enum.GetNames<StatBookType>()
            }
        });
    }

    [HttpGet("players")]
    [SwaggerOperation(Summary = "Intelligent player search", Description = "AI-powered player search with natural language processing")]
    public async Task<ActionResult<object>> SearchPlayers(
        [FromQuery] string query = "",
        [FromQuery] string? position = null,
        [FromQuery] string? team = null,
        [FromQuery] string? nationality = null,
        [FromQuery] int? ageMin = null,
        [FromQuery] int? ageMax = null,
        [FromQuery] string? priority = null,
        [FromQuery] decimal? minRating = null,
        [FromQuery] bool useNLP = true,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var searchQuery = new SearchQuery
        {
            QueryText = query,
            SearchType = SearchType.Players,
            ExecutedAt = DateTime.UtcNow,
            UsedNLP = useNLP,
            PageSize = pageSize,
            PageNumber = page
        };

        var playerQuery = _context.Players.AsQueryable();

        // Natural Language Processing for intelligent search
        if (useNLP && !string.IsNullOrEmpty(query))
        {
            var interpretedQuery = await InterpretNaturalLanguageQuery(query);
            searchQuery.InterpretedQuery = interpretedQuery?.ToString() ?? string.Empty;
            
            // Apply NLP-derived filters
            ApplyNLPFilters(ref playerQuery, interpretedQuery);
        }
        else if (!string.IsNullOrEmpty(query))
        {
            playerQuery = playerQuery.Where(p => 
                p.FirstName.Contains(query) || 
                p.LastName.Contains(query) ||
                p.CurrentTeam.Contains(query) ||
                p.Biography.Contains(query));
        }

        // Apply standard filters
        if (!string.IsNullOrEmpty(position))
        {
            playerQuery = playerQuery.Where(p => p.Position.Contains(position));
        }

        if (!string.IsNullOrEmpty(team))
        {
            playerQuery = playerQuery.Where(p => p.CurrentTeam.Contains(team));
        }

        if (!string.IsNullOrEmpty(nationality))
        {
            playerQuery = playerQuery.Where(p => p.Nationality.Contains(nationality));
        }

        if (ageMin.HasValue)
        {
            var maxBirthDate = DateTime.Today.AddYears(-ageMin.Value);
            playerQuery = playerQuery.Where(p => p.DateOfBirth <= maxBirthDate);
        }

        if (ageMax.HasValue)
        {
            var minBirthDate = DateTime.Today.AddYears(-ageMax.Value);
            playerQuery = playerQuery.Where(p => p.DateOfBirth >= minBirthDate);
        }

        if (!string.IsNullOrEmpty(priority) && Enum.TryParse<ScoutingPriority>(priority, out var priorityEnum))
        {
            playerQuery = playerQuery.Where(p => p.Priority == priorityEnum);
        }

        var totalResults = await playerQuery.CountAsync();
        searchQuery.TotalResults = totalResults;

        var results = await playerQuery
            .Include(p => p.ContactInfo)
            .Include(p => p.Tags).ThenInclude(pt => pt.Tag)
            .Include(p => p.ScoutingReports.OrderByDescending(sr => sr.ReportDate).Take(1))
            .Include(p => p.TalentPredictions.OrderByDescending(tp => tp.PredictionDate).Take(1))
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        // Generate AI-powered suggestions
        var suggestions = await GeneratePlayerSearchSuggestions(query, results);
        var similarPlayers = await FindSimilarPlayers(results.FirstOrDefault());

        // Log search query
        _context.SearchQueries.Add(searchQuery);
        await _context.SaveChangesAsync();

        return Ok(new
        {
            Query = query,
            InterpretedQuery = searchQuery.InterpretedQuery,
            Results = results.Select(r => new
            {
                r.Id,
                r.FullName,
                r.Age,
                r.Position,
                r.CurrentTeam,
                r.Nationality,
                r.Priority,
                r.Status,
                LatestRating = r.ScoutingReports.FirstOrDefault()?.OverallRating,
                PotentialScore = r.TalentPredictions.FirstOrDefault()?.OverallPotentialScore,
                Tags = r.Tags.Select(pt => new { pt.Tag.Name, pt.Tag.Color })
            }),
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalResults = totalResults,
                TotalPages = (int)Math.Ceiling((double)totalResults / pageSize)
            },
            Suggestions = suggestions,
            SimilarPlayers = similarPlayers,
            Filters = new
            {
                AppliedFilters = new { position, team, nationality, ageMin, ageMax, priority, minRating },
                AvailablePositions = await GetAvailablePositions(),
                AvailableTeams = await GetAvailableTeams(),
                AvailableNationalities = await GetAvailableNationalities()
            }
        });
    }

    [HttpGet("comprehensive")]
    [SwaggerOperation(Summary = "Universal search", Description = "Search across all content types with unified results")]
    public async Task<ActionResult<object>> ComprehensiveSearch(
        [FromQuery] string query,
        [FromQuery] string[] types = null!,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        var searchTypes = types?.Select(t => Enum.Parse<SearchType>(t)).ToArray() 
                         ?? new[] { SearchType.Players, SearchType.GameFootage, SearchType.StatBooks };

        var allResults = new List<object>();

        foreach (var searchType in searchTypes)
        {
            var typeResults = await ExecuteSearchByType(searchType, query, pageSize / searchTypes.Length);
            allResults.AddRange(typeResults);
        }

        // Sort by relevance across all types
        var sortedResults = allResults
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToList();

        var aggregatedResults = await AggregateSearchResults(query, allResults);

        return Ok(new
        {
            Query = query,
            SearchTypes = searchTypes,
            Results = sortedResults,
            Aggregation = aggregatedResults,
            Pagination = new
            {
                CurrentPage = page,
                PageSize = pageSize,
                TotalResults = allResults.Count,
                TotalPages = (int)Math.Ceiling((double)allResults.Count / pageSize)
            }
        });
    }

    [HttpGet("suggestions")]
    [SwaggerOperation(Summary = "Get search suggestions", Description = "AI-powered search suggestions and auto-complete")]
    public async Task<ActionResult<object>> GetSearchSuggestions(
        [FromQuery] string query,
        [FromQuery] string type = "all")
    {
        var suggestions = new List<string>();
        
        if (Enum.TryParse<SearchType>(type, true, out var searchType))
        {
            suggestions = await GenerateTypedSuggestions(query, searchType);
        }
        else
        {
            suggestions = await GenerateGeneralSuggestions(query);
        }

        var popularSearches = await GetPopularSearches(searchType);
        var recentSearches = await GetRecentSearches("user123"); // Get from authenticated user

        return Ok(new
        {
            Query = query,
            Suggestions = suggestions.Take(10),
            PopularSearches = popularSearches.Take(5),
            RecentSearches = recentSearches.Take(5)
        });
    }

    // Private helper methods
    private async Task<object> InterpretNaturalLanguageQuery(string query)
    {
        // In production, integrate with NLP service (OpenAI, Azure Cognitive Services, etc.)
        var interpretation = new
        {
            Intent = "FindPlayer",
            Entities = new List<object>(),
            Confidence = 0.85m
        };

        // Basic keyword extraction for demo
        if (query.ToLower().Contains("fast"))
        {
            interpretation.Entities.Add(new { Type = "Attribute", Value = "Speed", Modifier = "High" });
        }
        
        if (query.ToLower().Contains("young"))
        {
            interpretation.Entities.Add(new { Type = "Age", Value = "Under25" });
        }

        return interpretation;
    }

    private void ApplyNLPFilters(ref IQueryable<Player> query, object interpretation)
    {
        // Apply filters based on NLP interpretation
        // This would integrate with the interpretation results
    }

    private async Task<List<string>> GenerateFootageRecommendations(string query, List<GameFootage> results)
    {
        return new List<string>
        {
            "Try searching for specific player names",
            "Filter by competition for better results",
            "Use date ranges to narrow down matches"
        };
    }

    private async Task<List<string>> GetRelatedSearches(string query, SearchType searchType)
    {
        var firstWord = query.Split(' ').FirstOrDefault() ?? "";
        return await _context.SearchQueries
            .Where(sq => sq.SearchType == searchType && sq.QueryText.Contains(firstWord))
            .Select(sq => sq.QueryText)
            .Distinct()
            .Take(5)
            .ToListAsync();
    }

    private async Task<List<string>> GetAvailableCompetitions()
    {
        return await _context.GameFootage
            .Select(f => f.Competition)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    private async Task<List<string>> GetAvailableSeasons()
    {
        return await _context.GameFootage
            .Select(f => f.Season)
            .Distinct()
            .OrderByDescending(s => s)
            .ToListAsync();
    }

    private async Task<List<string>> GetAvailableLeagues()
    {
        return await _context.StatBooks
            .Select(s => s.League)
            .Distinct()
            .OrderBy(l => l)
            .ToListAsync();
    }

    private async Task<List<string>> GetAvailablePositions()
    {
        return await _context.Players
            .Select(p => p.Position)
            .Distinct()
            .OrderBy(p => p)
            .ToListAsync();
    }

    private async Task<List<string>> GetAvailableTeams()
    {
        return await _context.Players
            .Select(p => p.CurrentTeam)
            .Distinct()
            .OrderBy(t => t)
            .ToListAsync();
    }

    private async Task<List<string>> GetAvailableNationalities()
    {
        return await _context.Players
            .Select(p => p.Nationality)
            .Distinct()
            .OrderBy(n => n)
            .ToListAsync();
    }

    private async Task<List<string>> GeneratePlayerSearchSuggestions(string query, List<Player> results)
    {
        var suggestions = new List<string>();
        
        if (results.Any())
        {
            var commonPositions = results.GroupBy(r => r.Position)
                .OrderByDescending(g => g.Count())
                .Select(g => $"More {g.Key}s like these")
                .Take(2);
            suggestions.AddRange(commonPositions);
        }

        suggestions.Add("Search by specific attributes (fast, technical, young)");
        suggestions.Add("Try team names or competitions");

        return suggestions;
    }

    private async Task<List<object>> FindSimilarPlayers(Player? player)
    {
        if (player == null) return new List<object>();

        var similarPlayers = await _context.Players
            .Where(p => p.Position == player.Position && p.Id != player.Id)
            .Select(p => new { p.Id, p.FullName, p.Position, p.CurrentTeam })
            .Take(3)
            .ToListAsync();

        return similarPlayers.Cast<object>().ToList();
    }

    private async Task<List<object>> ExecuteSearchByType(SearchType searchType, string query, int limit)
    {
        return searchType switch
        {
            SearchType.Players => await _context.Players
                .Where(p => p.FirstName.Contains(query) || p.LastName.Contains(query))
                .Select(p => new { Type = "Player", p.Id, Title = p.FullName, p.Position })
                .Take(limit)
                .ToListAsync<object>(),
            
            SearchType.GameFootage => await _context.GameFootage
                .Where(f => f.Title.Contains(query) || f.Description.Contains(query))
                .Select(f => new { Type = "Footage", f.Id, f.Title, f.Competition })
                .Take(limit)
                .ToListAsync<object>(),
            
            SearchType.StatBooks => await _context.StatBooks
                .Where(s => s.Title.Contains(query) || s.League.Contains(query))
                .Select(s => new { Type = "StatBook", s.Id, s.Title, s.League })
                .Take(limit)
                .ToListAsync<object>(),
            
            _ => new List<object>()
        };
    }

    private async Task<object> AggregateSearchResults(string query, List<object> results)
    {
        return new
        {
            TotalResults = results.Count,
            ResultsByType = results.GroupBy(r => r.GetType().GetProperty("Type")?.GetValue(r))
                .ToDictionary(g => g.Key?.ToString() ?? "Unknown", g => g.Count()),
            TopMatches = results.Take(3)
        };
    }

    private async Task<List<string>> GenerateTypedSuggestions(string query, SearchType searchType)
    {
        return searchType switch
        {
            SearchType.Players => new List<string> { "young forwards", "experienced defenders", "creative midfielders" },
            SearchType.GameFootage => new List<string> { "championship final", "derby matches", "playoff games" },
            SearchType.StatBooks => new List<string> { "season stats", "player rankings", "team analytics" },
            _ => new List<string>()
        };
    }

    private async Task<List<string>> GenerateGeneralSuggestions(string query)
    {
        return new List<string>
        {
            $"{query} highlights",
            $"{query} statistics",
            $"{query} analysis",
            $"{query} performance",
            $"{query} scouting report"
        };
    }

    private async Task<List<string>> GetPopularSearches(SearchType searchType)
    {
        return await _context.SearchQueries
            .Where(sq => sq.SearchType == searchType)
            .GroupBy(sq => sq.QueryText)
            .OrderByDescending(g => g.Count())
            .Select(g => g.Key)
            .Take(5)
            .ToListAsync();
    }

    private async Task<List<string>> GetRecentSearches(string userId)
    {
        return await _context.SearchQueries
            .Where(sq => sq.UserId == userId)
            .OrderByDescending(sq => sq.ExecutedAt)
            .Select(sq => sq.QueryText)
            .Take(5)
            .ToListAsync();
    }
}