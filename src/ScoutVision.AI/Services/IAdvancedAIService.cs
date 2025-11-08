using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace ScoutVision.AI.Services;

/// <summary>
/// Advanced AI service for multi-sport support and real-time analysis
/// </summary>
public interface IAdvancedAIService
{
    // Real-time video processing
    Task<StreamAnalysisSession> StartLiveAnalysisAsync(string streamUrl, SportType sport);
    Task<LiveAnalysisResult> GetLiveAnalysisResultAsync(string sessionId);
    Task StopLiveAnalysisAsync(string sessionId);
    
    // Multi-sport support
    Task<PlayerAnalysis> AnalyzePlayerAsync(int playerId, SportType sport, AnalysisOptions options);
    Task<TeamAnalysis> AnalyzeTeamAsync(int teamId, SportType sport, AnalysisOptions options);
    Task<MatchAnalysis> AnalyzeMatchAsync(int matchId, SportType sport, AnalysisOptions options);
    
    // Tactical pattern recognition
    Task<List<TacticalPattern>> DetectTacticalPatternsAsync(int matchId, SportType sport);
    Task<FormationAnalysis> AnalyzeFormationAsync(int teamId, int matchId);
    Task<PressingAnalysis> AnalyzePressingAsync(int teamId, int matchId);
    Task<PassingNetwork> GeneratePassingNetworkAsync(int teamId, int matchId);
    
    // Enhanced injury prediction
    Task<InjuryRiskAssessment> AssessInjuryRiskAsync(int playerId, BiomechanicalData data);
    Task<FatigueAnalysis> AnalyzeFatigueAsync(int playerId, WorkloadData workload);
    Task<List<InjuryPrediction>> PredictInjuriesAsync(int teamId, int daysAhead = 30);
    
    // Advanced performance metrics
    Task<PerformanceProjection> ProjectPerformanceAsync(int playerId, int weeksAhead = 12);
    Task<PlayerComparison> ComparePlayersAsync(List<int> playerIds, SportType sport);
    Task<PeerBenchmark> BenchmarkPlayerAsync(int playerId, string position, string league);
}

public enum SportType
{
    Football,
    Basketball,
    AmericanFootball,
    Rugby,
    Hockey,
    Baseball,
    Cricket,
    Volleyball,
    Handball
}

public class StreamAnalysisSession
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string StreamUrl { get; set; } = string.Empty;
    public SportType Sport { get; set; }
    public SessionStatus Status { get; set; } = SessionStatus.Active;
    public DateTime StartedAt { get; set; } = DateTime.UtcNow;
    public DateTime? EndedAt { get; set; }
    public Dictionary<string, object> Metadata { get; set; } = new();
}

public enum SessionStatus
{
    Active,
    Paused,
    Stopped,
    Error
}

public class LiveAnalysisResult
{
    public string SessionId { get; set; } = string.Empty;
    public int CurrentMinute { get; set; }
    public List<DetectedEvent> RecentEvents { get; set; } = new();
    public LivePlayerTracking PlayerTracking { get; set; } = new();
    public LiveTeamStats TeamStats { get; set; } = new();
    public List<Highlight> AutoDetectedHighlights { get; set; } = new();
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class DetectedEvent
{
    public string Type { get; set; } = string.Empty;
    public int Minute { get; set; }
    public int Second { get; set; }
    public int? PlayerId { get; set; }
    public string? PlayerName { get; set; }
    public double Confidence { get; set; }
    public Dictionary<string, object> Details { get; set; } = new();
}

public class LivePlayerTracking
{
    public List<PlayerPosition> Positions { get; set; } = new();
    public Dictionary<int, PlayerMovementMetrics> MovementMetrics { get; set; } = new();
}

public class PlayerPosition
{
    public int PlayerId { get; set; }
    public double X { get; set; }
    public double Y { get; set; }
    public double Speed { get; set; }
    public DateTime Timestamp { get; set; }
}

public class PlayerMovementMetrics
{
    public double DistanceCovered { get; set; }
    public double TopSpeed { get; set; }
    public double AverageSpeed { get; set; }
    public int Sprints { get; set; }
    public int HighIntensityRuns { get; set; }
}

public class LiveTeamStats
{
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public Dictionary<string, double> HomeStats { get; set; } = new();
    public Dictionary<string, double> AwayStats { get; set; } = new();
}

public class Highlight
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Type { get; set; } = string.Empty;
    public int StartMinute { get; set; }
    public int StartSecond { get; set; }
    public int DurationSeconds { get; set; }
    public double Importance { get; set; }
    public string Description { get; set; } = string.Empty;
}

public class AnalysisOptions
{
    public string? Season { get; set; }
    public string? Competition { get; set; }
    public DateTime? FromDate { get; set; }
    public DateTime? ToDate { get; set; }
    public bool IncludeAdvancedMetrics { get; set; } = true;
    public bool IncludeTacticalAnalysis { get; set; } = true;
    public bool IncludePhysicalMetrics { get; set; } = true;
    public List<string>? CustomMetrics { get; set; }
}

public class PlayerAnalysis
{
    public int PlayerId { get; set; }
    public SportType Sport { get; set; }
    public Dictionary<string, double> CoreMetrics { get; set; } = new();
    public Dictionary<string, double> AdvancedMetrics { get; set; } = new();
    public TacticalProfile TacticalProfile { get; set; } = new();
    public PhysicalProfile PhysicalProfile { get; set; } = new();
    public List<StrengthWeakness> Strengths { get; set; } = new();
    public List<StrengthWeakness> Weaknesses { get; set; } = new();
    public PerformanceTrend Trend { get; set; } = new();
}

public class TacticalProfile
{
    public string PreferredPosition { get; set; } = string.Empty;
    public List<string> AlternativePositions { get; set; } = new();
    public string PlayingStyle { get; set; } = string.Empty;
    public Dictionary<string, double> TacticalAttributes { get; set; } = new();
    public List<string> TacticalStrengths { get; set; } = new();
}

public class PhysicalProfile
{
    public double TopSpeed { get; set; }
    public double AverageSpeed { get; set; }
    public double Acceleration { get; set; }
    public double Stamina { get; set; }
    public double Agility { get; set; }
    public double JumpHeight { get; set; }
    public Dictionary<string, double> CustomPhysicalMetrics { get; set; } = new();
}

public class StrengthWeakness
{
    public string Category { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Score { get; set; }
    public string Evidence { get; set; } = string.Empty;
}

public class PerformanceTrend
{
    public TrendDirection Direction { get; set; }
    public double ChangePercentage { get; set; }
    public List<PerformanceDataPoint> DataPoints { get; set; } = new();
}

public enum TrendDirection
{
    Improving,
    Stable,
    Declining
}

public class PerformanceDataPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string? Context { get; set; }
}

public class TeamAnalysis
{
    public int TeamId { get; set; }
    public SportType Sport { get; set; }
    public Dictionary<string, double> TeamMetrics { get; set; } = new();
    public FormationAnalysis PreferredFormation { get; set; } = new();
    public List<TacticalPattern> TacticalPatterns { get; set; } = new();
    public TeamChemistry Chemistry { get; set; } = new();
    public List<KeyPlayer> KeyPlayers { get; set; } = new();
}

public class FormationAnalysis
{
    public string Formation { get; set; } = string.Empty;
    public double UsagePercentage { get; set; }
    public double SuccessRate { get; set; }
    public List<string> AlternativeFormations { get; set; } = new();
    public Dictionary<string, PlayerRole> PlayerRoles { get; set; } = new();
}

public class PlayerRole
{
    public int PlayerId { get; set; }
    public string Position { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public double EffectivenessScore { get; set; }
}

public class TacticalPattern
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public PatternType Type { get; set; }
    public double Frequency { get; set; }
    public double SuccessRate { get; set; }
    public List<int> InvolvedPlayers { get; set; } = new();
    public string? VideoClipUrl { get; set; }
}

public enum PatternType
{
    BuildUpPlay,
    Pressing,
    Transition,
    SetPiece,
    DefensiveShape,
    AttackingMovement,
    CounterAttack
}

public class TeamChemistry
{
    public double OverallScore { get; set; }
    public Dictionary<string, double> PartnershipScores { get; set; } = new();
    public List<PlayerPartnership> StrongPartnerships { get; set; } = new();
    public PassingNetwork PassingNetwork { get; set; } = new();
}

public class PlayerPartnership
{
    public int Player1Id { get; set; }
    public int Player2Id { get; set; }
    public double ChemistryScore { get; set; }
    public int Interactions { get; set; }
    public double SuccessRate { get; set; }
}

public class PassingNetwork
{
    public List<PassingConnection> Connections { get; set; } = new();
    public Dictionary<int, PassingNode> Nodes { get; set; } = new();
}

public class PassingConnection
{
    public int FromPlayerId { get; set; }
    public int ToPlayerId { get; set; }
    public int PassCount { get; set; }
    public double CompletionRate { get; set; }
    public double AverageDistance { get; set; }
}

public class PassingNode
{
    public int PlayerId { get; set; }
    public int TotalPasses { get; set; }
    public int PassesReceived { get; set; }
    public double CompletionRate { get; set; }
    public double Centrality { get; set; }
}

public class KeyPlayer
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public double ImportanceScore { get; set; }
    public string Reason { get; set; } = string.Empty;
}

public class MatchAnalysis
{
    public int MatchId { get; set; }
    public SportType Sport { get; set; }
    public int HomeTeamId { get; set; }
    public int AwayTeamId { get; set; }
    public Dictionary<string, double> HomeTeamStats { get; set; } = new();
    public Dictionary<string, double> AwayTeamStats { get; set; } = new();
    public List<KeyMoment> KeyMoments { get; set; } = new();
    public List<TacticalInsight> TacticalInsights { get; set; } = new();
    public MomentumAnalysis Momentum { get; set; } = new();
}

public class KeyMoment
{
    public int Minute { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public double Impact { get; set; }
    public int? PlayerId { get; set; }
}

public class TacticalInsight
{
    public string Category { get; set; } = string.Empty;
    public string Insight { get; set; } = string.Empty;
    public double Confidence { get; set; }
    public string? Evidence { get; set; }
}

public class MomentumAnalysis
{
    public List<MomentumPoint> Timeline { get; set; } = new();
    public int CurrentMomentumTeamId { get; set; }
    public List<MomentumShift> Shifts { get; set; } = new();
}

public class MomentumPoint
{
    public int Minute { get; set; }
    public double HomeTeamMomentum { get; set; }
    public double AwayTeamMomentum { get; set; }
}

public class MomentumShift
{
    public int Minute { get; set; }
    public string Trigger { get; set; } = string.Empty;
    public double MagnitudeChange { get; set; }
}

public class PressingAnalysis
{
    public int TeamId { get; set; }
    public double PressingIntensity { get; set; }
    public double PPDA { get; set; } // Passes Per Defensive Action
    public List<PressingZone> PressingZones { get; set; } = new();
    public double SuccessRate { get; set; }
    public List<PressingTrigger> Triggers { get; set; } = new();
}

public class PressingZone
{
    public string Zone { get; set; } = string.Empty;
    public double Intensity { get; set; }
    public int Occurrences { get; set; }
    public double SuccessRate { get; set; }
}

public class PressingTrigger
{
    public string Trigger { get; set; } = string.Empty;
    public int Frequency { get; set; }
    public double SuccessRate { get; set; }
}

public class BiomechanicalData
{
    public int PlayerId { get; set; }
    public DateTime Timestamp { get; set; }
    public Dictionary<string, JointAngle> JointAngles { get; set; } = new();
    public Dictionary<string, double> ForceMetrics { get; set; } = new();
    public GaitAnalysis Gait { get; set; } = new();
    public List<MovementAsymmetry> Asymmetries { get; set; } = new();
}

public class JointAngle
{
    public string Joint { get; set; } = string.Empty;
    public double Angle { get; set; }
    public double RangeOfMotion { get; set; }
    public bool IsAbnormal { get; set; }
}

public class GaitAnalysis
{
    public double StrideLength { get; set; }
    public double Cadence { get; set; }
    public double GroundContactTime { get; set; }
    public double FlightTime { get; set; }
    public List<string> Abnormalities { get; set; } = new();
}

public class MovementAsymmetry
{
    public string Type { get; set; } = string.Empty;
    public double Percentage { get; set; }
    public string AffectedSide { get; set; } = string.Empty;
    public RiskLevel RiskLevel { get; set; }
}

public enum RiskLevel
{
    Low,
    Moderate,
    High,
    Critical
}

public class WorkloadData
{
    public int PlayerId { get; set; }
    public List<WorkloadSession> Sessions { get; set; } = new();
    public double AcuteWorkload { get; set; }
    public double ChronicWorkload { get; set; }
    public double ACWR { get; set; } // Acute:Chronic Workload Ratio
}

public class WorkloadSession
{
    public DateTime Date { get; set; }
    public int DurationMinutes { get; set; }
    public double Distance { get; set; }
    public double HighSpeedDistance { get; set; }
    public int Sprints { get; set; }
    public double RPE { get; set; } // Rating of Perceived Exertion
    public int TrainingLoad { get; set; }
}

public class InjuryRiskAssessment
{
    public int PlayerId { get; set; }
    public double OverallRisk { get; set; }
    public RiskLevel RiskLevel { get; set; }
    public List<SpecificInjuryRisk> SpecificRisks { get; set; } = new();
    public List<string> RiskFactors { get; set; } = new();
    public List<string> Recommendations { get; set; } = new();
    public DateTime AssessedAt { get; set; } = DateTime.UtcNow;
}

public class SpecificInjuryRisk
{
    public string InjuryType { get; set; } = string.Empty;
    public string BodyPart { get; set; } = string.Empty;
    public double Probability { get; set; }
    public int EstimatedDaysOut { get; set; }
    public List<string> Contributors { get; set; } = new();
}

public class FatigueAnalysis
{
    public int PlayerId { get; set; }
    public double FatigueScore { get; set; }
    public FatigueLevel Level { get; set; }
    public int RecommendedRestDays { get; set; }
    public Dictionary<string, double> FatigueIndicators { get; set; } = new();
    public RecoveryRecommendation Recovery { get; set; } = new();
}

public enum FatigueLevel
{
    Fresh,
    Mild,
    Moderate,
    High,
    Severe
}

public class RecoveryRecommendation
{
    public int RestDays { get; set; }
    public List<string> Activities { get; set; } = new();
    public Dictionary<string, string> Interventions { get; set; } = new();
}

public class InjuryPrediction
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string InjuryType { get; set; } = string.Empty;
    public double Probability { get; set; }
    public DateTime PredictedDate { get; set; }
    public int DaysUntil { get; set; }
    public RiskLevel RiskLevel { get; set; }
}

public class PerformanceProjection
{
    public int PlayerId { get; set; }
    public List<ProjectedMetric> Projections { get; set; } = new();
    public double ConfidenceScore { get; set; }
    public List<string> Assumptions { get; set; } = new();
}

public class ProjectedMetric
{
    public string MetricName { get; set; } = string.Empty;
    public double CurrentValue { get; set; }
    public double ProjectedValue { get; set; }
    public double ChangePercentage { get; set; }
    public DateTime ProjectionDate { get; set; }
}

public class PlayerComparison
{
    public List<int> PlayerIds { get; set; } = new();
    public SportType Sport { get; set; }
    public Dictionary<string, Dictionary<int, double>> Metrics { get; set; } = new();
    public List<ComparisonInsight> Insights { get; set; } = new();
    public string? RadarChartUrl { get; set; }
}

public class ComparisonInsight
{
    public string Category { get; set; } = string.Empty;
    public string Insight { get; set; } = string.Empty;
    public List<int> InvolvedPlayers { get; set; } = new();
}

public class PeerBenchmark
{
    public int PlayerId { get; set; }
    public string Position { get; set; } = string.Empty;
    public string League { get; set; } = string.Empty;
    public Dictionary<string, PercentileScore> Percentiles { get; set; } = new();
    public double OverallPercentile { get; set; }
    public List<string> AboveAverageMetrics { get; set; } = new();
    public List<string> BelowAverageMetrics { get; set; } = new();
}

public class PercentileScore
{
    public string Metric { get; set; } = string.Empty;
    public double Value { get; set; }
    public double Percentile { get; set; }
    public double LeagueAverage { get; set; }
    public double LeagueMedian { get; set; }
}

