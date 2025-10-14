namespace ScoutVision.Core.Enums;

public enum PlayerStatus
{
    Active,
    Inactive,
    Injured,
    Retired,
    Transferred,
    OnLoan
}

public enum ScoutingPriority
{
    Low = 1,
    Medium = 2,
    High = 3,
    Critical = 4
}

public enum VideoAnalysisType
{
    GameFootage,
    TrainingSession,
    SkillDemonstration,
    Interview,
    Highlight
}

public enum PerformanceMetricType
{
    Physical,
    Technical,
    Tactical,
    Mental,
    Statistical
}

public enum PredictionConfidence
{
    VeryLow,
    Low,
    Medium,
    High,
    VeryHigh
}

public enum MindsetCategory
{
    Leadership,
    Resilience,
    TeamWork,
    Discipline,
    Motivation,
    Adaptability,
    PressureHandling
}

public enum VideoQuality
{
    SD480p,
    HD720p,
    HD1080p,
    UHD4K
}

public enum StatBookType
{
    LeagueStats,
    PlayerStats,
    TeamStats,
    MatchStats,
    SeasonSummary,
    AdvancedAnalytics,
    PerformanceReport
}

public enum HeatMapType
{
    Positioning,
    Movement,
    Touches,
    Passes,
    Defensive,
    Offensive,
    Overall
}