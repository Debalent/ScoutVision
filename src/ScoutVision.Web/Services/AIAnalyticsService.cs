using ScoutVision.Web.Models;
using System.Text.Json;

namespace ScoutVision.Web.Services;

public class AIAnalyticsService : IAIAnalyticsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<AIAnalyticsService> _logger;

    public AIAnalyticsService(IHttpClientFactory httpClientFactory, ILogger<AIAnalyticsService> logger)
    {
        _httpClient = httpClientFactory.CreateClient("ScoutVisionAI");
        _logger = logger;
    }

    public async Task<PlayerPrediction> GetTalentPredictionAsync(int playerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"ai/predict/talent/{playerId}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PlayerPrediction>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new PlayerPrediction();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get talent prediction for player {playerId}");
            return new PlayerPrediction { PlayerId = playerId, Error = "Prediction service unavailable" };
        }
    }

    public async Task<InjuryRiskAssessment> GetInjuryRiskAsync(int playerId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"ai/injury-risk/{playerId}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<InjuryRiskAssessment>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new InjuryRiskAssessment();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get injury risk for player {playerId}");
            return new InjuryRiskAssessment { PlayerId = playerId, RiskLevel = "Unknown" };
        }
    }

    public async Task<VideoAnalysisResult> AnalyzeVideoAsync(string videoPath)
    {
        try
        {
            var payload = new { video_path = videoPath };
            var response = await _httpClient.PostAsJsonAsync("ai/analyze-video", payload);
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<VideoAnalysisResult>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new VideoAnalysisResult();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to analyze video: {videoPath}");
            return new VideoAnalysisResult { Error = "Video analysis failed" };
        }
    }

    public async Task<PerformanceInsights> GetPerformanceInsightsAsync(int playerId, DateTime fromDate, DateTime toDate)
    {
        try
        {
            var response = await _httpClient.GetAsync($"ai/performance-insights/{playerId}?from={fromDate:yyyy-MM-dd}&to={toDate:yyyy-MM-dd}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<PerformanceInsights>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new PerformanceInsights();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to get performance insights for player {playerId}");
            return new PerformanceInsights { PlayerId = playerId };
        }
    }

    public async Task<List<TacticalPattern>> DetectTacticalPatternsAsync(int matchId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"ai/tactical-patterns/{matchId}");
            response.EnsureSuccessStatusCode();
            
            var json = await response.Content.ReadAsStringAsync();
            return JsonSerializer.Deserialize<List<TacticalPattern>>(json, new JsonSerializerOptions
            {
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            }) ?? new List<TacticalPattern>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Failed to detect tactical patterns for match {matchId}");
            return new List<TacticalPattern>();
        }
    }
}