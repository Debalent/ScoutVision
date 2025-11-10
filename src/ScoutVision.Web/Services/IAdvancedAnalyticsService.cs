using ScoutVision.Web.Models;

namespace ScoutVision.Web.Services;

public interface IAdvancedAnalyticsService
{
    Task<HeatMapData> GenerateHeatMapAsync(int playerId, string metricType);
    Task<ComparisonResult> ComparePlayersAsync(List<int> playerIds);
    Task<PredictiveModel> BuildPredictiveModelAsync(string modelType, Dictionary<string, object> parameters);
    Task<List<Insight>> GenerateInsightsAsync(int playerId);
    Task<PerformanceTrend> AnalyzeTrendsAsync(int playerId, TimeSpan period);
    Task<List<SimilarPlayer>> FindSimilarPlayersAsync(int playerId, int count = 10);
    Task<TeamFormation> OptimizeFormationAsync(List<int> playerIds);
}