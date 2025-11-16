using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ScoutVision.Core.Analytics;
using ScoutVision.Core.Enums;
using ScoutVision.Infrastructure.Data;
using Swashbuckle.AspNetCore.Annotations;

namespace ScoutVision.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AnalyticsController : ControllerBase
{
    private readonly ScoutVisionDbContext _context;

    public AnalyticsController(ScoutVisionDbContext context)
    {
        _context = context;
    }

    [HttpGet("player/{playerId}")]
    [SwaggerOperation(Summary = "Get comprehensive player analytics", Description = "Retrieves detailed analytics and performance metrics for a specific player")]
    public async Task<ActionResult<object>> GetPlayerAnalytics(
        int playerId, 
        [FromQuery] string? season = null,
        [FromQuery] string? competition = null)
    {
        var query = _context.PlayerAnalytics
            .Where(pa => pa.PlayerId == playerId);

        if (!string.IsNullOrEmpty(season))
            query = query.Where(pa => pa.Season == season);

        if (!string.IsNullOrEmpty(competition))
            query = query.Where(pa => pa.Competition == competition);

        var analytics = await query
            .Include(pa => pa.PerformanceTrends)
            .Include(pa => pa.HeatMaps)
            .OrderByDescending(pa => pa.AnalysisDate)
            .ToListAsync();

        if (!analytics.Any())
        {
            return NotFound($"No analytics found for player {playerId}");
        }

        var latestAnalytics = analytics.First();
        var trends = GetPerformanceTrends(analytics);
        var comparison = await GetPlayerComparison(playerId, latestAnalytics.Competition);

        return Ok(new
        {
            PlayerId = playerId,
            CurrentSeason = latestAnalytics,
            PerformanceTrends = trends,
            SeasonalComparison = analytics.Take(3),
            PositionalComparison = comparison,
            KeyInsights = GenerateKeyInsights(latestAnalytics, trends)
        });
    }

    [HttpGet("player/{playerId}/heatmaps")]
    [SwaggerOperation(Summary = "Get player heat maps", Description = "Retrieves heat map data for player positioning and movement analysis")]
    public async Task<ActionResult<IEnumerable<HeatMap>>> GetPlayerHeatMaps(
        int playerId,
        [FromQuery] string? matchId = null,
        [FromQuery] string? heatMapType = null)
    {
        var query = _context.HeatMaps
            .Where(hm => hm.PlayerAnalytics.PlayerId == playerId);

        if (!string.IsNullOrEmpty(matchId))
            query = query.Where(hm => hm.MatchId == matchId);

        if (!string.IsNullOrEmpty(heatMapType))
        {
            if (Enum.TryParse<HeatMapType>(heatMapType, out var type))
                query = query.Where(hm => hm.Type == type);
        }

        var heatMaps = await query
            .OrderByDescending(hm => hm.MatchDate)
            .Take(10)
            .ToListAsync();

        return Ok(heatMaps);
    }

    [HttpGet("player/{playerId}/performance-trends")]
    [SwaggerOperation(Summary = "Get player performance trends", Description = "Analyzes performance trends over time with predictions")]
    public async Task<ActionResult<object>> GetPerformanceTrends(
        int playerId,
        [FromQuery] string metric = "OverallRating",
        [FromQuery] int days = 90)
    {
        var startDate = DateTime.UtcNow.AddDays(-days);
        
        var trends = await _context.PerformanceTrends
            .Where(pt => pt.PlayerAnalytics.PlayerId == playerId 
                        && pt.MetricName == metric 
                        && pt.Date >= startDate)
            .OrderBy(pt => pt.Date)
            .ToListAsync();

        var analysis = AnalyzeTrendData(trends);
        var forecast = GenerateForecast(trends, 30); // 30-day forecast

        return Ok(new
        {
            Metric = metric,
            Period = $"Last {days} days",
            TrendData = trends,
            Analysis = analysis,
            Forecast = forecast,
            Recommendations = GenerateTrendRecommendations(analysis)
        });
    }

    [HttpGet("team/{teamId}")]
    [SwaggerOperation(Summary = "Get team analytics", Description = "Comprehensive team performance analytics and tactical insights")]
    public async Task<ActionResult<object>> GetTeamAnalytics(
        int teamId,
        [FromQuery] string? season = null)
    {
        var teamAnalytics = await _context.TeamAnalytics
            .Where(ta => ta.Id == teamId)
            .Include(ta => ta.TeamPlayers)
            .ThenInclude(tp => tp.Player)
            .Include(ta => ta.MatchAnalyses)
            .FirstOrDefaultAsync();

        if (teamAnalytics == null)
        {
            return NotFound($"No analytics found for team {teamId}");
        }

        var playerContributions = await GetPlayerContributions(teamId);
        var tacticalAnalysis = await GetTacticalAnalysis(teamId);
        var formAnalysis = await GetFormAnalysis(teamId);

        return Ok(new
        {
            TeamAnalytics = teamAnalytics,
            PlayerContributions = playerContributions,
            TacticalInsights = tacticalAnalysis,
            FormAnalysis = formAnalysis,
            Recommendations = GenerateTeamRecommendations(teamAnalytics)
        });
    }

    [HttpGet("compare/players")]
    [SwaggerOperation(Summary = "Compare multiple players", Description = "Side-by-side comparison of player statistics and performance")]
    public async Task<ActionResult<object>> ComparePlayers(
        [FromQuery] int[] playerIds,
        [FromQuery] string? metric = null,
        [FromQuery] string? season = null)
    {
        if (playerIds.Length < 2 || playerIds.Length > 5)
        {
            return BadRequest("Please provide 2-5 player IDs for comparison");
        }

        var comparisons = new List<object>();

        foreach (var playerId in playerIds)
        {
            var analytics = await _context.PlayerAnalytics
                .Where(pa => pa.PlayerId == playerId)
                .Include(pa => pa.Player)
                .OrderByDescending(pa => pa.AnalysisDate)
                .FirstOrDefaultAsync();

            if (analytics != null)
            {
                comparisons.Add(new
                {
                    PlayerId = playerId,
                    PlayerName = analytics.Player.FullName,
                    Position = analytics.Player.Position,
                    Analytics = analytics,
                    NormalizedScores = CalculateNormalizedScores(analytics),
                    StrengthsWeaknesses = IdentifyStrengthsWeaknesses(analytics)
                });
            }
        }

        return Ok(new
        {
            Comparison = comparisons,
            Summary = GenerateComparisonSummary(comparisons),
            Recommendations = GenerateComparisonRecommendations(comparisons)
        });
    }

    [HttpPost("player/{playerId}/analytics")]
    [SwaggerOperation(Summary = "Create player analytics", Description = "Add new analytics data for a player")]
    public async Task<ActionResult<PlayerAnalytics>> CreatePlayerAnalytics(
        int playerId, 
        PlayerAnalytics analytics)
    {
        analytics.PlayerId = playerId;
        analytics.AnalysisDate = DateTime.UtcNow;
        
        // Calculate derived metrics
        analytics.GoalsPerGame = analytics.GamesPlayed > 0 ? (decimal)analytics.Goals / analytics.GamesPlayed : 0;
        analytics.AssistsPerGame = analytics.GamesPlayed > 0 ? (decimal)analytics.Assists / analytics.GamesPlayed : 0;
        analytics.PassAccuracy = analytics.PassesAttempted > 0 ? (decimal)analytics.PassesCompleted / analytics.PassesAttempted * 100 : 0;
        analytics.PlayerEfficiencyRating = CalculateEfficiencyRating(analytics);
        
        _context.PlayerAnalytics.Add(analytics);
        await _context.SaveChangesAsync();

        return CreatedAtAction(nameof(GetPlayerAnalytics), new { playerId }, analytics);
    }

    // Private helper methods
    private object GetPerformanceTrends(List<PlayerAnalytics> analytics)
    {
        // Implementation for trend analysis
        return new
        {
            GoalsTrend = "Improving",
            PassAccuracyTrend = "Stable",
            PhysicalTrend = "Declining"
        };
    }

    private async Task<object> GetPlayerComparison(int playerId, string competition)
    {
        // Get position averages for comparison
        var player = await _context.Players.FindAsync(playerId);
        if (player == null) return null;

        var positionAverages = await _context.PlayerAnalytics
            .Where(pa => pa.Player.Position == player.Position && pa.Competition == competition)
            .GroupBy(pa => pa.Player.Position)
            .Select(g => new
            {
                Position = g.Key,
                AverageGoals = g.Average(pa => pa.Goals),
                AverageAssists = g.Average(pa => pa.Assists),
                AveragePassAccuracy = g.Average(pa => pa.PassAccuracy)
            })
            .FirstOrDefaultAsync();

        return positionAverages;
    }

    private List<string> GenerateKeyInsights(PlayerAnalytics analytics, object trends)
    {
        var insights = new List<string>();
        
        if (analytics.GoalsPerGame > 0.5m)
            insights.Add($"Strong goal scorer with {analytics.GoalsPerGame:F1} goals per game");
        
        if (analytics.PassAccuracy > 85)
            insights.Add($"Excellent passing accuracy at {analytics.PassAccuracy:F1}%");
            
        if (analytics.TackleSuccessRate > 75)
            insights.Add($"Solid defensive presence with {analytics.TackleSuccessRate:F1}% tackle success rate");

        return insights;
    }

    private object AnalyzeTrendData(List<PerformanceTrend> trends)
    {
        if (!trends.Any()) return new { Direction = "No data", Confidence = 0 };

        var recentTrends = trends.TakeLast(5).ToList();
        var isImproving = recentTrends.Count(t => t.TrendDirection == "Improving") > recentTrends.Count / 2;
        
        return new
        {
            Direction = isImproving ? "Improving" : "Declining",
            Confidence = CalculateConfidence(recentTrends),
            Consistency = CalculateConsistency(trends)
        };
    }

    private object GenerateForecast(List<PerformanceTrend> trends, int days)
    {
        // Simple linear projection (in production, use more sophisticated ML models)
        if (trends.Count < 2) return new { Message = "Insufficient data for forecast" };

        var slope = CalculateSlope(trends);
        var lastValue = trends.Last().Value;
        var forecastValue = lastValue + (slope * days);

        return new
        {
            ForecastValue = Math.Max(0, forecastValue),
            Confidence = CalculateForecastConfidence(trends),
            Timeframe = $"{days} days"
        };
    }

    private List<string> GenerateTrendRecommendations(object analysis)
    {
        return new List<string>
        {
            "Focus on consistency in training",
            "Monitor fatigue levels during high-intensity periods",
            "Consider tactical adjustments based on performance trends"
        };
    }

    private async Task<object> GetPlayerContributions(int teamId)
    {
        // Implementation for player contribution analysis
        return new { Message = "Player contribution analysis" };
    }

    private async Task<object> GetTacticalAnalysis(int teamId)
    {
        // Implementation for tactical analysis
        return new { PreferredFormation = "4-3-3", PossessionStyle = "High" };
    }

    private async Task<object> GetFormAnalysis(int teamId)
    {
        // Implementation for form analysis
        return new { LastFiveGames = "W-W-D-L-W", FormRating = 7.5 };
    }

    private List<string> GenerateTeamRecommendations(TeamAnalytics analytics)
    {
        var recommendations = new List<string>();
        
        if (analytics.AveragePossession < 50)
            recommendations.Add("Consider improving ball retention in midfield");
            
        if (analytics.GoalsAgainst > analytics.GoalsFor)
            recommendations.Add("Focus on defensive structure and organization");

        return recommendations;
    }

    private decimal CalculateEfficiencyRating(PlayerAnalytics analytics)
    {
        // Weighted calculation based on multiple metrics
        var offensiveRating = (analytics.Goals * 3 + analytics.Assists * 2) / Math.Max(1, analytics.GamesPlayed);
        var defensiveRating = analytics.TackleSuccessRate / 10;
        var passingRating = analytics.PassAccuracy / 10;
        
        return (offensiveRating + defensiveRating + passingRating) / 3;
    }

    private object CalculateNormalizedScores(PlayerAnalytics analytics)
    {
        // Normalize scores to 0-100 scale for comparison
        return new
        {
            Offensive = Math.Min(100, analytics.GoalsPerGame * 20 + analytics.AssistsPerGame * 15),
            Defensive = Math.Min(100, analytics.TackleSuccessRate),
            Passing = Math.Min(100, analytics.PassAccuracy),
            Physical = Math.Min(100, analytics.DistanceCovered / 100)
        };
    }

    private object IdentifyStrengthsWeaknesses(PlayerAnalytics analytics)
    {
        var strengths = new List<string>();
        var weaknesses = new List<string>();

        if (analytics.PassAccuracy > 85) strengths.Add("Passing");
        else if (analytics.PassAccuracy < 70) weaknesses.Add("Passing");

        if (analytics.TackleSuccessRate > 75) strengths.Add("Defending");
        else if (analytics.TackleSuccessRate < 60) weaknesses.Add("Defending");

        return new { Strengths = strengths, Weaknesses = weaknesses };
    }

    private object GenerateComparisonSummary(List<object> comparisons)
    {
        return new
        {
            BestOverall = "Player with highest combined rating",
            MostConsistent = "Player with lowest variance",
            HighestPotential = "Youngest player with strong metrics"
        };
    }

    private List<string> GenerateComparisonRecommendations(List<object> comparisons)
    {
        return new List<string>
        {
            "Consider positional strengths when making recruitment decisions",
            "Look at age and potential for long-term value",
            "Factor in consistency and injury history"
        };
    }

    private decimal CalculateConfidence(List<PerformanceTrend> trends)
    {
        // Calculate confidence based on data consistency
        return trends.Count > 3 ? 0.8m : 0.5m;
    }

    private decimal CalculateConsistency(List<PerformanceTrend> trends)
    {
        if (!trends.Any()) return 0;
        
        var values = trends.Select(t => t.Value).ToList();
        var mean = values.Average();
        var variance = values.Sum(v => (v - mean) * (v - mean)) / values.Count;
        
        return Math.Max(0, 100 - (decimal)Math.Sqrt((double)variance));
    }

    private decimal CalculateSlope(List<PerformanceTrend> trends)
    {
        if (trends.Count < 2) return 0;
        
        var firstValue = trends.First().Value;
        var lastValue = trends.Last().Value;
        var daysDiff = (trends.Last().Date - trends.First().Date).Days;
        
        return daysDiff > 0 ? (lastValue - firstValue) / daysDiff : 0;
    }

    private decimal CalculateForecastConfidence(List<PerformanceTrend> trends)
    {
        // Higher confidence with more data points and consistent trends
        return Math.Min(0.95m, trends.Count * 0.1m);
    }
}