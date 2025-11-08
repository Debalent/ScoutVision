namespace ScoutVision.Infrastructure.Services;

/// <summary>
/// Interface for Injury Prevention Intelligence Module
/// </summary>
public interface IInjuryPrevention
{
    Task<InjuryRiskAnalysis> AnalyzePlayerRiskAsync(string playerId, CancellationToken cancellationToken = default);
    Task<ClubInjuryReport> GenerateClubReportAsync(string clubId, CancellationToken cancellationToken = default);
    Task<List<InjuryAlert>> GetActiveAlertsAsync(string clubId, CancellationToken cancellationToken = default);
}

public class InjuryRiskAnalysis
{
    public string PlayerId { get; set; }
    public int RiskScore { get; set; } // 0-100
    public string RiskLevel { get; set; } // "Low", "Moderate", "High", "Critical"
    public Dictionary<string, int> RiskFactors { get; set; }
    public string PrimaryRiskType { get; set; }
    public List<string> Recommendations { get; set; }
    public DateTime AnalyzedAt { get; set; }
}

public class InjuryAlert
{
    public string AlertId { get; set; }
    public string PlayerId { get; set; }
    public int RiskScore { get; set; }
    public string AlertType { get; set; }
    public string Message { get; set; }
    public DateTime AlertTime { get; set; }
    public bool IsAcknowledged { get; set; }
}

public class ClubInjuryReport
{
    public string ClubId { get; set; }
    public DateTime ReportDate { get; set; }
    public int TotalPlayers { get; set; }
    public int PlayersAtRisk { get; set; }
    public decimal RiskPercentage { get; set; }
    public List<InjuryRiskAnalysis> PlayerRisks { get; set; }
    public List<string> RecommendedActions { get; set; }
}

/// <summary>
/// Interface for Transfer Valuation Intelligence Module
/// </summary>
public interface ITransferValuation
{
    Task<PlayerTransferValuation> GetPlayerValuationAsync(string playerId, CancellationToken cancellationToken = default);
    Task<List<ComparablePlayer>> GetComparablePlaysAsync(string playerId, int limit = 5, CancellationToken cancellationToken = default);
    Task<TransferMarketAnalysis> AnalyzeMarketAsync(string position, string league = null, CancellationToken cancellationToken = default);
}

public class PlayerTransferValuation
{
    public string PlayerId { get; set; }
    public decimal EstimatedValue { get; set; }
    public decimal ValueRange_Low { get; set; }
    public decimal ValueRange_High { get; set; }
    public string BuySellHoldRecommendation { get; set; }
    public decimal TransferProbability { get; set; }
    public List<ComparablePlayer> ComparablePlayers { get; set; }
    public List<InterestingClub> PotentialBuyers { get; set; }
    public DateTime ValuationDate { get; set; }
}

public class ComparablePlayer
{
    public string PlayerId { get; set; }
    public string PlayerName { get; set; }
    public string Position { get; set; }
    public int Age { get; set; }
    public string Club { get; set; }
    public decimal TransferValue { get; set; }
    public double SimilarityScore { get; set; } // 0-1
}

public class InterestingClub
{
    public string ClubId { get; set; }
    public string ClubName { get; set; }
    public string League { get; set; }
    public double InterestProbability { get; set; }
    public decimal EstimatedOfferValue { get; set; }
}

public class TransferMarketAnalysis
{
    public string Position { get; set; }
    public string League { get; set; }
    public decimal AverageValue { get; set; }
    public decimal MedianValue { get; set; }
    public List<ValueTrend> ValueTrends { get; set; }
    public int PlayersAnalyzed { get; set; }
}

public class ValueTrend
{
    public string Metric { get; set; }
    public decimal Change { get; set; }
    public DateTime Date { get; set; }
}