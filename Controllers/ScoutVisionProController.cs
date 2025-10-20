using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ScoutVision.Core.Models;

namespace ScoutVision.Web.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ScoutVisionProController : ControllerBase
    {
        private readonly InjuryPreventionAnalyzer _injuryAnalyzer;
        private readonly TransferValueCalculator _transferCalculator;
        private readonly BettingDataService _bettingService;
        private readonly IPlayerService _playerService;

        public ScoutVisionProController(
            InjuryPreventionAnalyzer injuryAnalyzer,
            TransferValueCalculator transferCalculator,
            BettingDataService bettingService,
            IPlayerService playerService)
        {
            _injuryAnalyzer = injuryAnalyzer;
            _transferCalculator = transferCalculator;
            _bettingService = bettingService;
            _playerService = playerService;
        }

        // INJURY PREVENTION ENDPOINTS

        [HttpGet("injury-risk/{playerId}")]
        public async Task<ActionResult<InjuryRiskAnalysis>> GetInjuryRisk(int playerId)
        {
            try
            {
                var analysis = await _injuryAnalyzer.AnalyzeInjuryRisk(playerId);
                return Ok(analysis);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error analyzing injury risk: {ex.Message}");
            }
        }

        [HttpGet("injury-alerts/{clubId}")]
        public async Task<ActionResult<List<InjuryRiskAnalysis>>> GetClubInjuryAlerts(int clubId, string riskLevel = "Medium")
        {
            try
            {
                var players = await _playerService.GetClubPlayers(clubId);
                var alerts = new List<InjuryRiskAnalysis>();

                foreach (var player in players)
                {
                    var analysis = await _injuryAnalyzer.AnalyzeInjuryRisk(player.Id);
                    
                    // Filter by risk level
                    if (ShouldIncludeInAlerts(analysis.RiskLevel, riskLevel))
                    {
                        alerts.Add(analysis);
                    }
                }

                return Ok(alerts.OrderByDescending(a => a.InjuryRiskScore));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving injury alerts: {ex.Message}");
            }
        }

        [HttpPost("injury-prevention/batch-analyze")]
        public async Task<ActionResult<List<InjuryRiskAnalysis>>> BatchAnalyzeInjuryRisk([FromBody] List<int> playerIds)
        {
            try
            {
                var analyses = new List<InjuryRiskAnalysis>();
                
                foreach (var playerId in playerIds)
                {
                    var analysis = await _injuryAnalyzer.AnalyzeInjuryRisk(playerId);
                    analyses.Add(analysis);
                }

                return Ok(analyses.OrderByDescending(a => a.InjuryRiskScore));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error in batch injury analysis: {ex.Message}");
            }
        }

        // TRANSFER VALUATION ENDPOINTS

        [HttpGet("transfer-value/{playerId}")]
        public async Task<ActionResult<TransferValuation>> GetTransferValuation(int playerId)
        {
            try
            {
                var valuation = await _transferCalculator.CalculatePlayerValue(playerId);
                return Ok(valuation);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error calculating transfer value: {ex.Message}");
            }
        }

        [HttpGet("transfer-recommendations/{clubId}")]
        public async Task<ActionResult<List<TransferRecommendation>>> GetTransferRecommendations(int clubId)
        {
            try
            {
                var players = await _playerService.GetClubPlayers(clubId);
                var recommendations = new List<TransferRecommendation>();

                foreach (var player in players)
                {
                    var valuation = await _transferCalculator.CalculatePlayerValue(player.Id);
                    
                    recommendations.Add(new TransferRecommendation
                    {
                        Player = player,
                        Valuation = valuation,
                        Priority = CalculateRecommendationPriority(valuation),
                        ActionRequired = DetermineActionRequired(valuation)
                    });
                }

                return Ok(recommendations.OrderByDescending(r => r.Priority));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating transfer recommendations: {ex.Message}");
            }
        }

        [HttpGet("market-opportunities")]
        public async Task<ActionResult<List<MarketOpportunity>>> GetMarketOpportunities(
            string position = null, 
            decimal maxValue = 0, 
            string league = null)
        {
            try
            {
                var allPlayers = await _playerService.GetAvailablePlayers(position, league);
                var opportunities = new List<MarketOpportunity>();

                foreach (var player in allPlayers)
                {
                    var valuation = await _transferCalculator.CalculatePlayerValue(player.Id);
                    
                    // Filter for undervalued players with high transfer probability
                    if (IsMarketOpportunity(valuation, maxValue))
                    {
                        opportunities.Add(new MarketOpportunity
                        {
                            Player = player,
                            EstimatedValue = valuation.EstimatedMarketValue,
                            CurrentPrice = valuation.CurrentContractValue,
                            ValueUpside = valuation.PotentialProfit,
                            TransferProbability = valuation.TransferProbability,
                            Recommendation = valuation.Recommendation
                        });
                    }
                }

                return Ok(opportunities.OrderByDescending(o => o.ValueUpside));
            }
            catch (Exception ex)
            {
                return BadRequest($"Error finding market opportunities: {ex.Message}");
            }
        }

        // BETTING DATA ENDPOINTS

        [HttpGet("live-betting/{matchId}")]
        public async Task<ActionResult<MatchPredictions>> GetLiveBettingData(int matchId)
        {
            try
            {
                var predictions = await _bettingService.GetLiveMatchPredictions(matchId);
                return Ok(predictions);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving live betting data: {ex.Message}");
            }
        }

        [HttpGet("player-probabilities/{playerId}/{matchId}")]
        public async Task<ActionResult<LiveBettingData>> GetPlayerProbabilities(int playerId, int matchId)
        {
            try
            {
                var playerData = await _bettingService.GetLivePlayerData(matchId);
                var specificPlayer = playerData.FirstOrDefault(p => p.PlayerId == playerId);
                
                if (specificPlayer == null)
                    return NotFound($"Player {playerId} not found in match {matchId}");

                return Ok(specificPlayer);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error retrieving player probabilities: {ex.Message}");
            }
        }

        [HttpGet("fantasy-projections/{matchId}")]
        public async Task<ActionResult<List<FantasyProjection>>> GetFantasyProjections(int matchId)
        {
            try
            {
                var playerData = await _bettingService.GetLivePlayerData(matchId);
                var projections = playerData.Select(p => new FantasyProjection
                {
                    PlayerId = p.PlayerId,
                    PlayerName = p.Player.Name,
                    Position = p.Player.Position,
                    ProjectedPoints = p.FantasyPointsProjection,
                    GoalProbability = p.MatchGoalProbability,
                    AssistProbability = p.AssistProbability,
                    CleanSheetProbability = p.CleanSheetProbability,
                    FormRating = p.FormRating,
                    InjuryRisk = p.InjuryRisk
                }).OrderByDescending(p => p.ProjectedPoints);

                return Ok(projections);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating fantasy projections: {ex.Message}");
            }
        }

        // COMBINED INTELLIGENCE ENDPOINTS

        [HttpGet("player-intelligence/{playerId}")]
        public async Task<ActionResult<PlayerIntelligence>> GetCompletePlayerIntelligence(int playerId, int? currentMatchId = null)
        {
            try
            {
                var player = await _playerService.GetPlayerById(playerId);
                var intelligence = new PlayerIntelligence
                {
                    Player = player,
                    GeneratedAt = DateTime.UtcNow
                };

                // Get injury risk analysis
                intelligence.InjuryAnalysis = await _injuryAnalyzer.AnalyzeInjuryRisk(playerId);

                // Get transfer valuation
                intelligence.TransferValuation = await _transferCalculator.CalculatePlayerValue(playerId);

                // Get live betting data if in a match
                if (currentMatchId.HasValue)
                {
                    var liveData = await _bettingService.GetLivePlayerData(currentMatchId.Value);
                    intelligence.LiveBettingData = liveData.FirstOrDefault(p => p.PlayerId == playerId);
                }

                // Generate combined insights
                intelligence.CombinedInsights = GenerateCombinedInsights(intelligence);

                return Ok(intelligence);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating player intelligence: {ex.Message}");
            }
        }

        [HttpGet("club-dashboard/{clubId}")]
        public async Task<ActionResult<ClubDashboard>> GetClubDashboard(int clubId)
        {
            try
            {
                var players = await _playerService.GetClubPlayers(clubId);
                var dashboard = new ClubDashboard
                {
                    ClubId = clubId,
                    GeneratedAt = DateTime.UtcNow,
                    InjuryAlerts = new List<InjuryRiskAnalysis>(),
                    TransferOpportunities = new List<TransferRecommendation>(),
                    PlayerValuations = new List<TransferValuation>()
                };

                foreach (var player in players)
                {
                    // Injury analysis
                    var injuryAnalysis = await _injuryAnalyzer.AnalyzeInjuryRisk(player.Id);
                    if (injuryAnalysis.RiskLevel == "High" || injuryAnalysis.RiskLevel == "Critical")
                    {
                        dashboard.InjuryAlerts.Add(injuryAnalysis);
                    }

                    // Transfer analysis
                    var transferValuation = await _transferCalculator.CalculatePlayerValue(player.Id);
                    dashboard.PlayerValuations.Add(transferValuation);

                    if (transferValuation.Recommendation.Contains("SELL") || 
                        transferValuation.Recommendation.Contains("BUY"))
                    {
                        dashboard.TransferOpportunities.Add(new TransferRecommendation
                        {
                            Player = player,
                            Valuation = transferValuation,
                            Priority = CalculateRecommendationPriority(transferValuation),
                            ActionRequired = DetermineActionRequired(transferValuation)
                        });
                    }
                }

                // Calculate summary statistics
                dashboard.TotalSquadValue = dashboard.PlayerValuations.Sum(v => v.EstimatedMarketValue);
                dashboard.TotalPotentialProfit = dashboard.PlayerValuations.Sum(v => v.PotentialProfit);
                dashboard.HighRiskInjuries = dashboard.InjuryAlerts.Count(a => a.RiskLevel == "High" || a.RiskLevel == "Critical");
                dashboard.ActiveTransferTargets = dashboard.TransferOpportunities.Count;

                return Ok(dashboard);
            }
            catch (Exception ex)
            {
                return BadRequest($"Error generating club dashboard: {ex.Message}");
            }
        }

        // HELPER METHODS

        private bool ShouldIncludeInAlerts(string analysisRiskLevel, string filterRiskLevel)
        {
            var riskLevels = new Dictionary<string, int>
            {
                ["Low"] = 1,
                ["Medium"] = 2,
                ["High"] = 3,
                ["Critical"] = 4
            };

            return riskLevels.GetValueOrDefault(analysisRiskLevel, 0) >= 
                   riskLevels.GetValueOrDefault(filterRiskLevel, 2);
        }

        private int CalculateRecommendationPriority(TransferValuation valuation)
        {
            var priority = 0;
            
            if (valuation.Recommendation.Contains("STRONG")) priority += 5;
            if (valuation.Recommendation.Contains("BUY") || valuation.Recommendation.Contains("SELL")) priority += 3;
            if (valuation.TransferProbability > 0.7) priority += 2;
            if (valuation.PotentialProfit > 5000000) priority += 2; // €5M+ profit potential
            
            return priority;
        }

        private string DetermineActionRequired(TransferValuation valuation)
        {
            if (valuation.Recommendation.Contains("STRONG BUY"))
                return "Immediate negotiation recommended";
            
            if (valuation.Recommendation.Contains("SELL") && valuation.TransferProbability > 0.6)
                return "Consider offers - high transfer probability";
            
            if (valuation.PotentialProfit > 10000000) // €10M+ profit
                return "High-value opportunity - priority action";
            
            if (valuation.OptimalSellWindow.HasValue && 
                valuation.OptimalSellWindow.Value <= DateTime.UtcNow.AddMonths(3))
                return "Optimal sell window approaching";
            
            return "Monitor and evaluate";
        }

        private bool IsMarketOpportunity(TransferValuation valuation, decimal maxValue)
        {
            // Must be undervalued with reasonable transfer probability
            if (valuation.PotentialProfit <= 0 || valuation.TransferProbability < 0.3)
                return false;

            // Check maximum value filter
            if (maxValue > 0 && valuation.EstimatedMarketValue > maxValue)
                return false;

            // Must have buy recommendation or significant upside
            return valuation.Recommendation.Contains("BUY") || 
                   (valuation.PotentialProfit / Math.Max(1, valuation.CurrentContractValue)) > 1.5m;
        }

        private List<string> GenerateCombinedInsights(PlayerIntelligence intelligence)
        {
            var insights = new List<string>();

            // Injury + Transfer insights
            if (intelligence.InjuryAnalysis.RiskLevel == "High" || intelligence.InjuryAnalysis.RiskLevel == "Critical")
            {
                if (intelligence.TransferValuation.Recommendation.Contains("SELL"))
                {
                    insights.Add($"URGENT: High injury risk + sell recommendation - consider immediate transfer to maximize value");
                }
                else
                {
                    insights.Add($"CAUTION: High injury risk detected - value protection measures recommended");
                }
            }

            // Transfer + Performance insights
            if (intelligence.TransferValuation.ValueTrend == "Rising" && 
                intelligence.TransferValuation.TransferProbability > 0.6)
            {
                insights.Add($"OPPORTUNITY: Rising value with high transfer interest - optimal selling conditions");
            }

            // Live performance + Injury insights
            if (intelligence.LiveBettingData != null)
            {
                if (intelligence.LiveBettingData.InjuryRisk > 0.7 && 
                    intelligence.LiveBettingData.CurrentPerformanceScore > 80)
                {
                    insights.Add($"ALERT: Excellent performance but high in-game injury risk - consider substitution");
                }

                if (intelligence.LiveBettingData.FatigueLevel > 0.8)
                {
                    insights.Add($"FATIGUE WARNING: High fatigue level may impact performance and increase injury risk");
                }
            }

            // Value + Performance insights
            var valueToPerformanceRatio = (double)(intelligence.TransferValuation.EstimatedMarketValue / 1000000);
            if (intelligence.LiveBettingData?.CurrentPerformanceScore > 85 && valueToPerformanceRatio < 10)
            {
                insights.Add($"UNDERVALUED STAR: Exceptional performance with modest valuation - prime acquisition target");
            }

            return insights;
        }
    }

    // Supporting models for combined intelligence
    public class PlayerIntelligence
    {
        public Player Player { get; set; }
        public DateTime GeneratedAt { get; set; }
        public InjuryRiskAnalysis InjuryAnalysis { get; set; }
        public TransferValuation TransferValuation { get; set; }
        public LiveBettingData LiveBettingData { get; set; }
        public List<string> CombinedInsights { get; set; } = new List<string>();
    }

    public class ClubDashboard
    {
        public int ClubId { get; set; }
        public DateTime GeneratedAt { get; set; }
        public List<InjuryRiskAnalysis> InjuryAlerts { get; set; }
        public List<TransferRecommendation> TransferOpportunities { get; set; }
        public List<TransferValuation> PlayerValuations { get; set; }
        public decimal TotalSquadValue { get; set; }
        public decimal TotalPotentialProfit { get; set; }
        public int HighRiskInjuries { get; set; }
        public int ActiveTransferTargets { get; set; }
    }

    public class TransferRecommendation
    {
        public Player Player { get; set; }
        public TransferValuation Valuation { get; set; }
        public int Priority { get; set; }
        public string ActionRequired { get; set; }
    }

    public class MarketOpportunity
    {
        public Player Player { get; set; }
        public decimal EstimatedValue { get; set; }
        public decimal CurrentPrice { get; set; }
        public decimal ValueUpside { get; set; }
        public double TransferProbability { get; set; }
        public string Recommendation { get; set; }
    }

    public class FantasyProjection
    {
        public int PlayerId { get; set; }
        public string PlayerName { get; set; }
        public string Position { get; set; }
        public double ProjectedPoints { get; set; }
        public double GoalProbability { get; set; }
        public double AssistProbability { get; set; }
        public double CleanSheetProbability { get; set; }
        public string FormRating { get; set; }
        public double InjuryRisk { get; set; }
    }
}