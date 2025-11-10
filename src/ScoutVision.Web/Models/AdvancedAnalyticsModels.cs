namespace ScoutVision.Web.Models;

public class HeatMapData
{
    public int PlayerId { get; set; }
    public string MetricType { get; set; } = "";
    public List<HeatMapPoint> Points { get; set; } = new();
    public Dictionary<string, double> Statistics { get; set; } = new();
    public string? Error { get; set; }
}

public class HeatMapPoint
{
    public double X { get; set; }
    public double Y { get; set; }
    public double Intensity { get; set; }
    public string? Label { get; set; }
}

public class ComparisonResult
{
    public List<int> PlayerIds { get; set; } = new();
    public Dictionary<string, List<double>> Metrics { get; set; } = new();
    public List<ComparisonInsight> Insights { get; set; } = new();
    public string? Error { get; set; }
}

public class ComparisonInsight
{
    public string Category { get; set; } = "";
    public string Description { get; set; } = "";
    public double Significance { get; set; }
}

public class PredictiveModel
{
    public string ModelType { get; set; } = "";
    public string ModelId { get; set; } = "";
    public double Accuracy { get; set; }
    public Dictionary<string, double> FeatureImportance { get; set; } = new();
    public List<string> Predictions { get; set; } = new();
    public string? Error { get; set; }
}

public class Insight
{
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public double Confidence { get; set; }
    public string Priority { get; set; } = "Medium";
    public List<string> SupportingData { get; set; } = new();
}

public class PerformanceTrend
{
    public int PlayerId { get; set; }
    public Dictionary<string, List<TrendPoint>> Trends { get; set; } = new();
    public double OverallTrend { get; set; }
    public List<string> TrendInsights { get; set; } = new();
}

public class TrendPoint
{
    public DateTime Date { get; set; }
    public double Value { get; set; }
    public string? Context { get; set; }
}

public class SimilarPlayer
{
    public int PlayerId { get; set; }
    public string Name { get; set; } = "";
    public double SimilarityScore { get; set; }
    public List<string> SimilarAttributes { get; set; } = new();
    public string Position { get; set; } = "";
}

public class TeamFormation
{
    public string Formation { get; set; } = "";
    public Dictionary<string, int> PlayerPositions { get; set; } = new();
    public double OverallRating { get; set; }
    public List<string> Strengths { get; set; } = new();
    public List<string> Weaknesses { get; set; } = new();
    public string? Error { get; set; }
}

public class PlayerPrediction
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public Dictionary<string, double> FuturePredictions { get; set; } = new();
    public double PredictionConfidence { get; set; }
    public string PredictionType { get; set; } = "";
    public DateTime PredictionDate { get; set; }
    public List<string> KeyFactors { get; set; } = new();
    public string? Error { get; set; }
}

public class InjuryRiskAssessment
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public double InjuryRiskScore { get; set; }
    public string RiskLevel { get; set; } = "";
    public List<string> RiskFactors { get; set; } = new();
    public Dictionary<string, double> BodyPartRisks { get; set; } = new();
    public List<string> RecommendedActions { get; set; } = new();
    public DateTime AssessmentDate { get; set; }
}

public class VideoAnalysisResult
{
    public string VideoId { get; set; } = "";
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public Dictionary<string, double> TechnicalSkills { get; set; } = new();
    public List<string> KeyMoments { get; set; } = new();
    public double OverallPerformanceScore { get; set; }
    public List<string> AreasForImprovement { get; set; } = new();
    public DateTime AnalysisDate { get; set; }
    public string? Error { get; set; }
}

public class PerformanceInsights
{
    public int PlayerId { get; set; }
    public string PlayerName { get; set; } = "";
    public Dictionary<string, double> PerformanceMetrics { get; set; } = new();
    public List<string> Strengths { get; set; } = new();
    public List<string> WeaknessAreas { get; set; } = new();
    public double OverallRating { get; set; }
    public List<string> RecommendedTraining { get; set; } = new();
    public DateTime InsightDate { get; set; }
}

public class TacticalPattern
{
    public string PatternId { get; set; } = "";
    public string PatternType { get; set; } = "";
    public string Description { get; set; } = "";
    public double Frequency { get; set; }
    public double Effectiveness { get; set; }
    public List<int> InvolvedPlayers { get; set; } = new();
    public string Formation { get; set; } = "";
    public List<string> Situations { get; set; } = new();
}