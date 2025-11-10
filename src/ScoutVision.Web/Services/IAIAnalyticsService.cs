using ScoutVision.Web.Models;

namespace ScoutVision.Web.Services;

public interface IAIAnalyticsService
{
    Task<PlayerPrediction> GetTalentPredictionAsync(int playerId);
    Task<InjuryRiskAssessment> GetInjuryRiskAsync(int playerId);
    Task<VideoAnalysisResult> AnalyzeVideoAsync(string videoPath);
    Task<PerformanceInsights> GetPerformanceInsightsAsync(int playerId, DateTime fromDate, DateTime toDate);
    Task<List<TacticalPattern>> DetectTacticalPatternsAsync(int matchId);
}