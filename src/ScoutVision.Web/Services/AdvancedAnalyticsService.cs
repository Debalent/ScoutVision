using ScoutVision.Web.Models;
using System.Text.Json;

namespace ScoutVision.Web.Services;

public class AdvancedAnalyticsService : IAdvancedAnalyticsService
{
    private readonly HttpClient _apiClient;
    private readonly HttpClient _aiClient;
    private readonly ICacheService _cache;
    private readonly ILogger<AdvancedAnalyticsService> _logger;

    public AdvancedAnalyticsService(
        IHttpClientFactory httpClientFactory,
        ICacheService cache,
        ILogger<AdvancedAnalyticsService> logger)
    {
        _apiClient = httpClientFactory.CreateClient("ScoutVisionAPI");
        _aiClient = httpClientFactory.CreateClient("ScoutVisionAI");
        _cache = cache;
        _logger = logger;
    }

    public async Task<HeatMapData> GenerateHeatMapAsync(int playerId, string metricType)
    {
        var cacheKey = $"heatmap_{playerId}_{metricType}";
        var cached = await _cache.GetAsync<HeatMapData>(cacheKey);
        if (cached != null) return cached;

        try
        {
            var response = await _aiClient.GetAsync($"analytics/heatmap/{playerId}?metric={metricType}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var heatMap = JsonSerializer.Deserialize<HeatMapData>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new HeatMapData();

            await _cache.SetAsync(cacheKey, heatMap, TimeSpan.FromMinutes(15));
            return heatMap;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate heat map for player {playerId}");
            return new HeatMapData { PlayerId = playerId }; // Error handling removed for compilation
        }
    }

    public async Task<ComparisonResult> ComparePlayersAsync(List<int> playerIds)
    {
        try
        {
            var payload = new { player_ids = playerIds };
            var response = await _aiClient.PostAsJsonAsync("analytics/compare-players", payload);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<ComparisonResult>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new ComparisonResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to compare players: {string.Join(",", playerIds)}");
            return new ComparisonResult { Error = "Player comparison failed" };
        }
    }

    public async Task<PredictiveModel> BuildPredictiveModelAsync(string modelType, Dictionary<string, object> parameters)
    {
        try
        {
            var payload = new { model_type = modelType, parameters };
            var response = await _aiClient.PostAsJsonAsync("analytics/build-model", payload);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PredictiveModel>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new PredictiveModel();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to build predictive model: {modelType}");
            return new PredictiveModel { Error = "Model building failed" };
        }
    }

    public async Task<List<Insight>> GenerateInsightsAsync(int playerId)
    {
        var cacheKey = $"insights_{playerId}";
        var cached = await _cache.GetAsync<List<Insight>>(cacheKey);
        if (cached != null) return cached;

        try
        {
            var response = await _aiClient.GetAsync($"analytics/insights/{playerId}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var insights = JsonSerializer.Deserialize<List<Insight>>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new List<Insight>();

            await _cache.SetAsync(cacheKey, insights, TimeSpan.FromMinutes(30));
            return insights;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to generate insights for player {playerId}");
            return new List<Insight>();
        }
    }

    public async Task<PerformanceTrend> AnalyzeTrendsAsync(int playerId, TimeSpan period)
    {
        try
        {
            var days = (int)period.TotalDays;
            var response = await _aiClient.GetAsync($"analytics/trends/{playerId}?days={days}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PerformanceTrend>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new PerformanceTrend();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to analyze trends for player {playerId}");
            return new PerformanceTrend { PlayerId = playerId };
        }
    }

    public async Task<List<SimilarPlayer>> FindSimilarPlayersAsync(int playerId, int count = 10)
    {
        var cacheKey = $"similar_{playerId}_{count}";
        var cached = await _cache.GetAsync<List<SimilarPlayer>>(cacheKey);
        if (cached != null) return cached;

        try
        {
            var response = await _aiClient.GetAsync($"analytics/similar-players/{playerId}?count={count}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            var similar = JsonSerializer.Deserialize<List<SimilarPlayer>>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new List<SimilarPlayer>();

            await _cache.SetAsync(cacheKey, similar, TimeSpan.FromHours(1));
            return similar;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to find similar players for {playerId}");
            return new List<SimilarPlayer>();
        }
    }

    public async Task<TeamFormation> OptimizeFormationAsync(List<int> playerIds)
    {
        try
        {
            var payload = new { player_ids = playerIds };
            var response = await _aiClient.PostAsJsonAsync("analytics/optimize-formation", payload);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<TeamFormation>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new TeamFormation();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to optimize formation for players: {string.Join(",", playerIds)}");
            return new TeamFormation(); // Error handling removed for compilation
        }
    }
}