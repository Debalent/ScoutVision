using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScoutVision.Core.Models
{
    public class LiveBettingData
    {
        public int Id { get; set; }
        public int MatchId { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public DateTime Timestamp { get; set; }
        public int MinutePlayed { get; set; }
        
        // Goal Probabilities
        public double NextGoalProbability { get; set; }
        public double MatchGoalProbability { get; set; }
        public double HatTrickProbability { get; set; }
        
        // Card Probabilities
        public double YellowCardProbability { get; set; }
        public double RedCardProbability { get; set; }
        
        // Performance Predictions
        public double AssistProbability { get; set; }
        public double CleanSheetProbability { get; set; }
        public double ManOfMatchProbability { get; set; }
        
        // Fantasy Metrics
        public double FantasyPointsProjection { get; set; }
        public string FormRating { get; set; }
        public double InjuryRisk { get; set; }
        
        // Live Performance Data
        public double CurrentPerformanceScore { get; set; }
        public int TouchesInBox { get; set; }
        public int ShotsOnTarget { get; set; }
        public int KeyPasses { get; set; }
        public double FatigueLevel { get; set; }
    }

    public class MatchPredictions
    {
        public int MatchId { get; set; }
        public DateTime MatchTime { get; set; }
        public string HomeTeam { get; set; }
        public string AwayTeam { get; set; }
        public int CurrentMinute { get; set; }
        public int HomeScore { get; set; }
        public int AwayScore { get; set; }
        
        // Match Outcome Probabilities
        public double HomeWinProbability { get; set; }
        public double DrawProbability { get; set; }
        public double AwayWinProbability { get; set; }
        
        // Goal Predictions
        public double Over25GoalsProbability { get; set; }
        public double BothTeamsToScoreProbability { get; set; }
        public double NextGoalTeamProbability { get; set; } // 0 = Home, 1 = Away
        
        // Time-based Predictions
        public double GoalInNext15MinProbability { get; set; }
        public double RedCardProbability { get; set; }
        public double PenaltyProbability { get; set; }
        
        // Advanced Metrics
        public double MomentumScore { get; set; } // -100 to +100 (negative = away momentum)
        public string GameState { get; set; } // "Balanced", "Home Dominant", "Away Pressing", etc.
        public List<LiveBettingData> PlayerData { get; set; } = new List<LiveBettingData>();
    }

    public class BettingDataService
    {
        private readonly IVideoAnalysisService _videoAnalysis;
        private readonly IPerformanceService _performance;
        private readonly IMatchDataService _matchData;
        private readonly IMLPredictionService _mlService;

        public BettingDataService(IVideoAnalysisService videoAnalysis, 
            IPerformanceService performance, IMatchDataService matchData, IMLPredictionService mlService)
        {
            _videoAnalysis = videoAnalysis;
            _performance = performance;
            _matchData = matchData;
            _mlService = mlService;
        }

        public async Task<MatchPredictions> GetLiveMatchPredictions(int matchId)
        {
            var match = await _matchData.GetLiveMatch(matchId);
            var homeTeamStats = await _performance.GetLiveTeamStats(match.HomeTeamId);
            var awayTeamStats = await _performance.GetLiveTeamStats(match.AwayTeamId);
            var playerData = await GetLivePlayerData(matchId);

            var predictions = new MatchPredictions
            {
                MatchId = matchId,
                MatchTime = match.MatchTime,
                HomeTeam = match.HomeTeam,
                AwayTeam = match.AwayTeam,
                CurrentMinute = match.CurrentMinute,
                HomeScore = match.HomeScore,
                AwayScore = match.AwayScore,
                PlayerData = playerData
            };

            // Calculate match outcome probabilities
            await CalculateMatchOutcomeProbabilities(predictions, homeTeamStats, awayTeamStats);
            
            // Calculate goal predictions
            await CalculateGoalPredictions(predictions, homeTeamStats, awayTeamStats);
            
            // Calculate advanced metrics
            await CalculateAdvancedMetrics(predictions, homeTeamStats, awayTeamStats);

            return predictions;
        }

        public async Task<List<LiveBettingData>> GetLivePlayerData(int matchId)
        {
            var players = await _matchData.GetMatchPlayers(matchId);
            var playerBettingData = new List<LiveBettingData>();

            foreach (var player in players)
            {
                var liveStats = await _performance.GetLivePlayerStats(player.Id, matchId);
                var videoAnalysis = await _videoAnalysis.GetRealTimePlayerAnalysis(player.Id, matchId);
                var predictions = await CalculatePlayerProbabilities(player, liveStats, videoAnalysis);

                playerBettingData.Add(predictions);
            }

            return playerBettingData;
        }

        private async Task<LiveBettingData> CalculatePlayerProbabilities(Player player, 
            LivePlayerStats stats, RealTimeAnalysis videoAnalysis)
        {
            var historicalData = await _performance.GetPlayerHistory(player.Id, 10); // Last 10 matches
            var seasonStats = await _performance.GetSeasonStats(player.Id);
            
            var bettingData = new LiveBettingData
            {
                PlayerId = player.Id,
                Player = player,
                Timestamp = DateTime.UtcNow,
                MinutePlayed = stats.MinutesPlayed,
                CurrentPerformanceScore = CalculateCurrentPerformance(stats, videoAnalysis),
                TouchesInBox = stats.TouchesInPenaltyArea,
                ShotsOnTarget = stats.ShotsOnTarget,
                KeyPasses = stats.KeyPasses,
                FatigueLevel = videoAnalysis.FatigueLevel
            };

            // Calculate goal probabilities
            bettingData.NextGoalProbability = await CalculateNextGoalProbability(player, stats, historicalData);
            bettingData.MatchGoalProbability = await CalculateMatchGoalProbability(player, stats, historicalData, bettingData.NextGoalProbability);
            bettingData.HatTrickProbability = await CalculateHatTrickProbability(player, stats, historicalData);

            // Calculate card probabilities
            bettingData.YellowCardProbability = await CalculateYellowCardProbability(player, stats, historicalData);
            bettingData.RedCardProbability = await CalculateRedCardProbability(player, stats, historicalData);

            // Calculate other event probabilities
            bettingData.AssistProbability = await CalculateAssistProbability(player, stats, historicalData);
            bettingData.CleanSheetProbability = await CalculateCleanSheetProbability(player, stats, historicalData);
            bettingData.ManOfMatchProbability = await CalculateManOfMatchProbability(player, stats, bettingData.CurrentPerformanceScore);

            // Fantasy metrics
            bettingData.FantasyPointsProjection = CalculateFantasyProjection(bettingData, seasonStats);
            bettingData.FormRating = DetermineFormRating(historicalData, stats);
            bettingData.InjuryRisk = videoAnalysis.InjuryRisk;

            return bettingData;
        }

        private async Task<double> CalculateNextGoalProbability(Player player, LivePlayerStats stats, List<MatchHistory> history)
        {
            double baseProbability = 0.05; // 5% base chance per 10-minute period

            // Historical goal rate adjustment
            var avgGoalsPerMatch = history.Average(h => h.Goals);
            var goalRateMultiplier = Math.Min(3.0, avgGoalsPerMatch * 2);
            baseProbability *= goalRateMultiplier;

            // Position adjustment
            var positionMultiplier = player.Position switch
            {
                "Striker" => 2.0,
                "Attacking Midfielder" => 1.5,
                "Winger" => 1.3,
                "Central Midfielder" => 0.8,
                "Defender" => 0.3,
                "Goalkeeper" => 0.1,
                _ => 1.0
            };
            baseProbability *= positionMultiplier;

            // Current match performance adjustment
            if (stats.ShotsOnTarget > 2) baseProbability *= 1.5;
            if (stats.TouchesInPenaltyArea > 5) baseProbability *= 1.3;
            if (stats.KeyPasses > 3) baseProbability *= 1.2;

            // Fatigue adjustment (tired players less likely to score)
            var videoAnalysis = await _videoAnalysis.GetRealTimePlayerAnalysis(player.Id, stats.MatchId);
            if (videoAnalysis.FatigueLevel > 0.7) baseProbability *= 0.8;

            // Time adjustment (goals more likely in certain periods)
            if (stats.MinutesPlayed >= 70 && stats.MinutesPlayed <= 90) baseProbability *= 1.2; // Late goals
            if (stats.MinutesPlayed <= 15) baseProbability *= 1.1; // Early goals

            return Math.Min(0.8, Math.Max(0.01, baseProbability));
        }

        private async Task<double> CalculateMatchGoalProbability(Player player, LivePlayerStats stats, 
            List<MatchHistory> history, double nextGoalProb)
        {
            var remainingTime = 90 - stats.MinutesPlayed;
            var timeSegments = Math.Max(1, remainingTime / 10); // 10-minute segments

            // Probability of scoring at least once in remaining time
            var noGoalProbability = Math.Pow(1 - nextGoalProb, timeSegments);
            var matchGoalProbability = 1 - noGoalProbability;

            // Adjust for current goals scored
            if (stats.Goals >= 1)
            {
                matchGoalProbability = 1.0; // Already scored
            }

            return Math.Min(0.95, Math.Max(0.01, matchGoalProbability));
        }

        private async Task<double> CalculateHatTrickProbability(Player player, LivePlayerStats stats, List<MatchHistory> history)
        {
            if (stats.Goals < 2) return 0.0; // Need at least 2 goals for hat-trick

            var hatTrickRate = history.Count(h => h.Goals >= 3) / (double)Math.Max(1, history.Count);
            var baseProbability = hatTrickRate * 0.5; // Base from historical rate

            // Boost if player is having exceptional game
            if (stats.ShotsOnTarget >= 4) baseProbability *= 2.0;
            if (stats.TouchesInPenaltyArea >= 8) baseProbability *= 1.5;

            // Time remaining factor
            var remainingTime = 90 - stats.MinutesPlayed;
            if (remainingTime < 10) baseProbability *= 0.5; // Less time remaining

            return Math.Min(0.6, Math.Max(0.01, baseProbability));
        }

        private async Task<double> CalculateYellowCardProbability(Player player, LivePlayerStats stats, List<MatchHistory> history)
        {
            if (stats.YellowCards >= 1) return 0.0; // Already has yellow card

            var avgYellowsPerMatch = history.Average(h => h.YellowCards);
            var baseProbability = avgYellowsPerMatch * 0.15; // Base rate

            // Defensive players more likely to get cards
            var positionMultiplier = player.Position switch
            {
                "Defender" => 1.5,
                "Defensive Midfielder" => 1.3,
                "Central Midfielder" => 1.1,
                "Striker" => 0.8,
                "Goalkeeper" => 0.3,
                _ => 1.0
            };
            baseProbability *= positionMultiplier;

            // Aggressive play indicators
            if (stats.Fouls > 2) baseProbability *= 1.8;
            if (stats.Tackles > 4) baseProbability *= 1.2;

            // Match intensity (close games = more cards)
            var scoreDifference = Math.Abs(stats.TeamScore - stats.OpponentScore);
            if (scoreDifference <= 1) baseProbability *= 1.3;

            // Time factor (more cards in second half)
            if (stats.MinutesPlayed > 45) baseProbability *= 1.2;

            return Math.Min(0.4, Math.Max(0.01, baseProbability));
        }

        private async Task<double> CalculateRedCardProbability(Player player, LivePlayerStats stats, List<MatchHistory> history)
        {
            if (stats.RedCards >= 1) return 0.0; // Already sent off

            var avgRedsPerMatch = history.Average(h => h.RedCards);
            var baseProbability = avgRedsPerMatch * 0.05; // Very low base rate

            // Second yellow card scenario
            if (stats.YellowCards >= 1)
            {
                baseProbability = await CalculateYellowCardProbability(player, stats, history) * 0.3;
            }

            // Aggressive behavior indicators
            if (stats.Fouls > 4) baseProbability *= 3.0;
            if (stats.YellowCards >= 1 && stats.Fouls > 2) baseProbability *= 2.0;

            // Pressure situations (late in close games)
            var scoreDifference = Math.Abs(stats.TeamScore - stats.OpponentScore);
            if (stats.MinutesPlayed > 70 && scoreDifference <= 1) baseProbability *= 1.5;

            return Math.Min(0.15, Math.Max(0.001, baseProbability));
        }

        private async Task<double> CalculateAssistProbability(Player player, LivePlayerStats stats, List<MatchHistory> history)
        {
            var avgAssistsPerMatch = history.Average(h => h.Assists);
            var baseProbability = avgAssistsPerMatch * 0.2;

            // Creative players more likely to assist
            var positionMultiplier = player.Position switch
            {
                "Attacking Midfielder" => 2.0,
                "Central Midfielder" => 1.5,
                "Winger" => 1.8,
                "Striker" => 1.2,
                "Defender" => 0.4,
                "Goalkeeper" => 0.1,
                _ => 1.0
            };
            baseProbability *= positionMultiplier;

            // Current performance indicators
            if (stats.KeyPasses > 3) baseProbability *= 2.0;
            if (stats.Crosses > 2) baseProbability *= 1.5;
            if (stats.PassAccuracy > 0.85) baseProbability *= 1.3;

            return Math.Min(0.7, Math.Max(0.01, baseProbability));
        }

        private async Task<double> CalculateCleanSheetProbability(Player player, LivePlayerStats stats, List<MatchHistory> history)
        {
            if (!IsDefensivePlayer(player.Position)) return 0.0;
            if (stats.OpponentScore > 0) return 0.0; // Already conceded

            var cleanSheetRate = history.Count(h => h.OpponentGoals == 0) / (double)Math.Max(1, history.Count);
            var baseProbability = cleanSheetRate;

            // Defensive performance adjustments
            if (stats.Tackles > 3) baseProbability *= 1.2;
            if (stats.Interceptions > 2) baseProbability *= 1.1;
            if (stats.ClearancesSuccessful > 4) baseProbability *= 1.1;

            // Time remaining factor
            var remainingTime = 90 - stats.MinutesPlayed;
            if (remainingTime < 15) baseProbability *= 1.3; // Close to clean sheet

            return Math.Min(0.9, Math.Max(0.05, baseProbability));
        }

        private async Task<double> CalculateManOfMatchProbability(Player player, LivePlayerStats stats, double currentPerformance)
        {
            var baseProbability = 1.0 / 22.0; // 1 in 22 players

            // Performance-based adjustment
            var performanceMultiplier = Math.Max(0.1, currentPerformance / 50.0); // Normalize to ~2x multiplier for great performance
            baseProbability *= performanceMultiplier;

            // Goal bonus
            if (stats.Goals > 0) baseProbability *= (1 + stats.Goals * 2);

            // Assist bonus
            if (stats.Assists > 0) baseProbability *= (1 + stats.Assists * 1.5);

            // Clean sheet bonus for defensive players
            if (IsDefensivePlayer(player.Position) && stats.OpponentScore == 0)
                baseProbability *= 2.0;

            // Goalkeeper bonus for saves
            if (player.Position == "Goalkeeper" && stats.Saves > 3)
                baseProbability *= 3.0;

            return Math.Min(0.8, Math.Max(0.01, baseProbability));
        }

        private double CalculateFantasyProjection(LiveBettingData data, SeasonStats seasonStats)
        {
            double projection = 2.0; // Base points for playing

            // Goals (6 points each for forwards, 5 for midfielders, 6 for defenders/goalkeepers)
            var goalPoints = data.Player.Position switch
            {
                "Striker" => data.MatchGoalProbability * 4,
                "Attacking Midfielder" => data.MatchGoalProbability * 5,
                "Central Midfielder" => data.MatchGoalProbability * 5,
                "Defender" => data.MatchGoalProbability * 6,
                "Goalkeeper" => data.MatchGoalProbability * 6,
                _ => data.MatchGoalProbability * 4
            };

            // Assists (3 points each)
            var assistPoints = data.AssistProbability * 3;

            // Clean sheet bonus (4 points for defenders/goalkeepers)
            var cleanSheetPoints = IsDefensivePlayer(data.Player.Position) ? data.CleanSheetProbability * 4 : 0;

            // Card deductions (-1 yellow, -3 red)
            var cardDeduction = (data.YellowCardProbability * -1) + (data.RedCardProbability * -3);

            projection += goalPoints + assistPoints + cleanSheetPoints + cardDeduction;

            return Math.Max(0, projection);
        }

        private string DetermineFormRating(List<MatchHistory> history, LivePlayerStats currentStats)
        {
            var recentPerformance = history.Take(5).Average(h => h.PerformanceRating);
            var currentNormalizedPerformance = currentStats.PerformanceRating;

            var combinedRating = (recentPerformance * 0.7) + (currentNormalizedPerformance * 0.3);

            return combinedRating switch
            {
                >= 8.0 => "Excellent",
                >= 7.0 => "Very Good",
                >= 6.0 => "Good",
                >= 5.0 => "Average",
                >= 4.0 => "Below Average",
                _ => "Poor"
            };
        }

        private double CalculateCurrentPerformance(LivePlayerStats stats, RealTimeAnalysis videoAnalysis)
        {
            double performance = 50; // Base performance score

            // Positive contributions
            performance += stats.Goals * 15;
            performance += stats.Assists * 10;
            performance += stats.KeyPasses * 2;
            performance += stats.ShotsOnTarget * 3;
            performance += stats.Tackles * 1.5;
            performance += stats.Interceptions * 1.5;
            performance += (stats.PassAccuracy - 0.7) * 20; // Bonus for >70% accuracy

            // Negative contributions
            performance -= stats.Fouls * 1;
            performance -= stats.YellowCards * 5;
            performance -= stats.RedCards * 20;
            performance -= (0.3 - stats.PassAccuracy) * 30; // Penalty for <70% accuracy

            // Video analysis factors
            performance += (videoAnalysis.PositionalAwareness - 0.5) * 20;
            performance += (videoAnalysis.WorkRate - 0.5) * 15;
            performance -= videoAnalysis.FatigueLevel * 10;

            return Math.Min(100, Math.Max(0, performance));
        }

        private bool IsDefensivePlayer(string position)
        {
            return position == "Defender" || position == "Goalkeeper" || position == "Defensive Midfielder";
        }

        private async Task CalculateMatchOutcomeProbabilities(MatchPredictions predictions, 
            LiveTeamStats homeStats, LiveTeamStats awayStats)
        {
            // This would use ML models trained on historical match data
            // Placeholder implementation using current match state

            var homeStrength = CalculateTeamStrength(homeStats);
            var awayStrength = CalculateTeamStrength(awayStats);
            var totalStrength = homeStrength + awayStrength;

            // Adjust for current score and time
            var scoreDifference = predictions.HomeScore - predictions.AwayScore;
            var timeRemaining = 90 - predictions.CurrentMinute;

            predictions.HomeWinProbability = (homeStrength / totalStrength) + (scoreDifference * 0.1) + (timeRemaining < 30 ? scoreDifference * 0.05 : 0);
            predictions.AwayWinProbability = (awayStrength / totalStrength) - (scoreDifference * 0.1) - (timeRemaining < 30 ? scoreDifference * 0.05 : 0);
            predictions.DrawProbability = 1 - predictions.HomeWinProbability - predictions.AwayWinProbability;

            // Normalize probabilities
            var total = predictions.HomeWinProbability + predictions.DrawProbability + predictions.AwayWinProbability;
            predictions.HomeWinProbability /= total;
            predictions.DrawProbability /= total;
            predictions.AwayWinProbability /= total;
        }

        private async Task CalculateGoalPredictions(MatchPredictions predictions, 
            LiveTeamStats homeStats, LiveTeamStats awayStats)
        {
            var totalGoals = predictions.HomeScore + predictions.AwayScore;
            var averageGoalsPerMatch = 2.7; // League average
            var timeElapsed = predictions.CurrentMinute / 90.0;
            var expectedGoalsAtThisPoint = averageGoalsPerMatch * timeElapsed;

            // Goals trend
            var goalsTrend = totalGoals - expectedGoalsAtThisPoint;

            predictions.Over25GoalsProbability = totalGoals >= 3 ? 1.0 : 
                Math.Max(0.1, Math.Min(0.9, 0.4 + (goalsTrend * 0.2)));

            predictions.BothTeamsToScoreProbability = (predictions.HomeScore > 0 && predictions.AwayScore > 0) ? 1.0 :
                Math.Max(0.2, Math.Min(0.8, 0.55 + (Math.Min(predictions.HomeScore, predictions.AwayScore) * 0.1)));

            // Next goal team probability based on current momentum
            var homeMomentum = CalculateTeamMomentum(homeStats);
            var awayMomentum = CalculateTeamMomentum(awayStats);
            var totalMomentum = homeMomentum + awayMomentum;

            predictions.NextGoalTeamProbability = totalMomentum > 0 ? homeMomentum / totalMomentum : 0.5;
        }

        private async Task CalculateAdvancedMetrics(MatchPredictions predictions, 
            LiveTeamStats homeStats, LiveTeamStats awayStats)
        {
            // Momentum calculation (-100 to +100)
            var homeMomentum = CalculateTeamMomentum(homeStats);
            var awayMomentum = CalculateTeamMomentum(awayStats);
            predictions.MomentumScore = ((homeMomentum - awayMomentum) / (homeMomentum + awayMomentum)) * 100;

            // Game state determination
            predictions.GameState = Math.Abs(predictions.MomentumScore) switch
            {
                >= 60 => predictions.MomentumScore > 0 ? "Home Dominant" : "Away Dominant",
                >= 30 => predictions.MomentumScore > 0 ? "Home Pressing" : "Away Pressing",
                _ => "Balanced"
            };

            // Event probabilities
            predictions.GoalInNext15MinProbability = CalculateShortTermGoalProbability(homeStats, awayStats);
            predictions.RedCardProbability = CalculateMatchRedCardProbability(homeStats, awayStats);
            predictions.PenaltyProbability = CalculateMatchPenaltyProbability(homeStats, awayStats);
        }

        private double CalculateTeamStrength(LiveTeamStats stats)
        {
            return stats.ShotsOnTarget * 2 + stats.Possession * 0.5 + stats.PassAccuracy * 0.3 + 
                   stats.Tackles * 0.5 + stats.Corners * 0.3;
        }

        private double CalculateTeamMomentum(LiveTeamStats stats)
        {
            return stats.RecentShots * 3 + stats.RecentPossession * 2 + stats.RecentCorners * 2 + 
                   stats.RecentFoulsWon - stats.RecentFoulsConceded;
        }

        private double CalculateShortTermGoalProbability(LiveTeamStats homeStats, LiveTeamStats awayStats)
        {
            var combinedAttackingThreat = (homeStats.ShotsOnTarget + awayStats.ShotsOnTarget) * 0.1;
            return Math.Min(0.4, Math.Max(0.05, combinedAttackingThreat));
        }

        private double CalculateMatchRedCardProbability(LiveTeamStats homeStats, LiveTeamStats awayStats)
        {
            var totalFouls = homeStats.Fouls + awayStats.Fouls;
            var totalCards = homeStats.YellowCards + awayStats.YellowCards;
            var aggression = (totalFouls * 0.01) + (totalCards * 0.03);
            return Math.Min(0.25, Math.Max(0.01, aggression));
        }

        private double CalculateMatchPenaltyProbability(LiveTeamStats homeStats, LiveTeamStats awayStats)
        {
            var boxTouches = homeStats.TouchesInOpponentBox + awayStats.TouchesInOpponentBox;
            var pressure = boxTouches * 0.005;
            return Math.Min(0.15, Math.Max(0.01, pressure));
        }
    }

    // API Controller for external consumption
    public class BettingApiController : Controller
    {
        private readonly BettingDataService _bettingService;

        public BettingApiController(BettingDataService bettingService)
        {
            _bettingService = bettingService;
        }

        [HttpGet("api/betting/match/{matchId}/predictions")]
        public async Task<ActionResult<MatchPredictions>> GetMatchPredictions(int matchId, string apiKey)
        {
            if (!await ValidateApiKey(apiKey))
                return Unauthorized();

            var predictions = await _bettingService.GetLiveMatchPredictions(matchId);
            return Ok(predictions);
        }

        [HttpGet("api/betting/player/{playerId}/probabilities")]
        public async Task<ActionResult<LiveBettingData>> GetPlayerProbabilities(int playerId, int matchId, string apiKey)
        {
            if (!await ValidateApiKey(apiKey))
                return Unauthorized();

            var playerData = await _bettingService.GetLivePlayerData(matchId);
            var specificPlayer = playerData.FirstOrDefault(p => p.PlayerId == playerId);
            
            return specificPlayer != null ? Ok(specificPlayer) : NotFound();
        }

        [HttpGet("api/betting/live-feed")]
        public async Task<ActionResult<List<MatchPredictions>>> GetLiveFeed(string apiKey, string league = null)
        {
            if (!await ValidateApiKey(apiKey))
                return Unauthorized();

            // Return live data for all current matches
            var liveMatches = await GetActiveMatches(league);
            var predictions = new List<MatchPredictions>();

            foreach (var match in liveMatches)
            {
                var prediction = await _bettingService.GetLiveMatchPredictions(match.Id);
                predictions.Add(prediction);
            }

            return Ok(predictions);
        }

        private async Task<bool> ValidateApiKey(string apiKey)
        {
            // Implement API key validation logic
            return !string.IsNullOrEmpty(apiKey);
        }

        private async Task<List<Match>> GetActiveMatches(string league)
        {
            // Implementation to get currently active matches
            return new List<Match>();
        }
    }

    // Supporting models for live data
    public class LivePlayerStats
    {
        public int PlayerId { get; set; }
        public int MatchId { get; set; }
        public int MinutesPlayed { get; set; }
        public int Goals { get; set; }
        public int Assists { get; set; }
        public int Shots { get; set; }
        public int ShotsOnTarget { get; set; }
        public int KeyPasses { get; set; }
        public int Passes { get; set; }
        public double PassAccuracy { get; set; }
        public int Touches { get; set; }
        public int TouchesInPenaltyArea { get; set; }
        public int Tackles { get; set; }
        public int Interceptions { get; set; }
        public int Fouls { get; set; }
        public int YellowCards { get; set; }
        public int RedCards { get; set; }
        public int Crosses { get; set; }
        public int ClearancesSuccessful { get; set; }
        public int Saves { get; set; } // For goalkeepers
        public int TeamScore { get; set; }
        public int OpponentScore { get; set; }
        public double PerformanceRating { get; set; }
    }

    public class LiveTeamStats
    {
        public int Shots { get; set; }
        public int ShotsOnTarget { get; set; }
        public double Possession { get; set; }
        public double PassAccuracy { get; set; }
        public int Tackles { get; set; }
        public int Corners { get; set; }
        public int Fouls { get; set; }
        public int YellowCards { get; set; }
        public int TouchesInOpponentBox { get; set; }
        
        // Recent period stats (last 15 minutes)
        public int RecentShots { get; set; }
        public double RecentPossession { get; set; }
        public int RecentCorners { get; set; }
        public int RecentFoulsWon { get; set; }
        public int RecentFoulsConceded { get; set; }
    }

    public class RealTimeAnalysis
    {
        public int PlayerId { get; set; }
        public double FatigueLevel { get; set; }
        public double PositionalAwareness { get; set; }
        public double WorkRate { get; set; }
        public double InjuryRisk { get; set; }
    }
}