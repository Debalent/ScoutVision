using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ScoutVision.Core.Models
{
    public class TransferValuation
    {
        public int Id { get; set; }
        public int PlayerId { get; set; }
        public Player Player { get; set; }
        public DateTime ValuationDate { get; set; }
        public decimal EstimatedMarketValue { get; set; }
        public decimal CurrentContractValue { get; set; }
        public string ValueTrend { get; set; } // Rising, Stable, Declining
        public double TransferProbability { get; set; } // 0-1 scale
        public string Recommendation { get; set; } // Buy, Sell, Hold, Monitor
        public List<string> ValueDrivers { get; set; } = new List<string>();
        public List<TransferComparable> Comparables { get; set; } = new List<TransferComparable>();
        public DateTime? OptimalSellWindow { get; set; }
        public decimal PotentialProfit { get; set; }
    }

    public class TransferComparable
    {
        public string PlayerName { get; set; }
        public decimal TransferFee { get; set; }
        public DateTime TransferDate { get; set; }
        public double SimilarityScore { get; set; }
        public string Club { get; set; }
        public int Age { get; set; }
        public string Position { get; set; }
    }

    public class MarketTrend
    {
        public string Position { get; set; }
        public string AgeGroup { get; set; }
        public decimal AverageValue { get; set; }
        public double ValueGrowth { get; set; } // Yearly percentage change
        public int TransactionVolume { get; set; }
        public string MarketSentiment { get; set; }
    }

    public class TransferValueCalculator
    {
        private readonly IPerformanceService _performance;
        private readonly IMarketDataService _marketData;
        private readonly IPlayerAnalyticsService _analytics;

        public TransferValueCalculator(IPerformanceService performance, 
            IMarketDataService marketData, IPlayerAnalyticsService analytics)
        {
            _performance = performance;
            _marketData = marketData;
            _analytics = analytics;
        }

        public async Task<TransferValuation> CalculatePlayerValue(int playerId)
        {
            var player = await _performance.GetPlayerById(playerId);
            var performanceMetrics = await _performance.GetComprehensiveMetrics(playerId, 24); // Last 2 years
            var comparables = await FindComparablePlayers(player);
            var marketTrends = await _marketData.GetPositionMarketTrends(player.Position);

            var baseValue = await CalculateBaseMarketValue(player, performanceMetrics);
            var adjustedValue = ApplyMarketAdjustments(baseValue, marketTrends, comparables);
            var finalValue = ApplyPerformanceTrend(adjustedValue, performanceMetrics);

            var transferProbability = CalculateTransferProbability(player, performanceMetrics);
            var recommendation = GenerateRecommendation(finalValue, player.CurrentValue, transferProbability, performanceMetrics);
            var valueDrivers = IdentifyValueDrivers(player, performanceMetrics, comparables);

            return new TransferValuation
            {
                PlayerId = playerId,
                Player = player,
                ValuationDate = DateTime.UtcNow,
                EstimatedMarketValue = finalValue,
                CurrentContractValue = player.CurrentContractValue,
                ValueTrend = DetermineValueTrend(performanceMetrics),
                TransferProbability = transferProbability,
                Recommendation = recommendation,
                ValueDrivers = valueDrivers,
                Comparables = comparables.Take(5).ToList(),
                OptimalSellWindow = CalculateOptimalSellWindow(performanceMetrics, player.Age),
                PotentialProfit = finalValue - player.CurrentContractValue
            };
        }

        private async Task<decimal> CalculateBaseMarketValue(Player player, PerformanceMetrics metrics)
        {
            decimal baseValue = 0;

            // Performance-based valuation (60% of base value)
            var performanceScore = CalculatePerformanceScore(metrics);
            var positionMultiplier = GetPositionMultiplier(player.Position);
            var performanceValue = performanceScore * positionMultiplier * 100000; // Base €100k per performance point

            // Age adjustment (20% of base value)
            var ageMultiplier = CalculateAgeMultiplier(player.Age, player.Position);
            var ageAdjustedValue = performanceValue * ageMultiplier;

            // League and club prestige (15% of base value)
            var leagueMultiplier = await GetLeagueMultiplier(player.CurrentClub);
            var prestigeAdjustedValue = ageAdjustedValue * leagueMultiplier;

            // International experience (5% of base value)
            var internationalMultiplier = CalculateInternationalMultiplier(player.InternationalCaps);
            
            baseValue = prestigeAdjustedValue * internationalMultiplier;

            return Math.Max(50000, baseValue); // Minimum €50k value
        }

        private decimal ApplyMarketAdjustments(decimal baseValue, MarketTrend trends, List<TransferComparable> comparables)
        {
            decimal adjustedValue = baseValue;

            // Market trend adjustment
            if (trends.ValueGrowth > 0.1) // 10%+ growth in position market
                adjustedValue *= 1.15m;
            else if (trends.ValueGrowth < -0.1) // 10%+ decline
                adjustedValue *= 0.85m;

            // Supply and demand adjustment
            if (trends.TransactionVolume < 10) // Low supply = higher prices
                adjustedValue *= 1.1m;
            else if (trends.TransactionVolume > 50) // High supply = lower prices
                adjustedValue *= 0.9m;

            // Comparable player adjustment
            if (comparables.Any())
            {
                var avgComparableValue = comparables.Average(c => (decimal)c.TransferFee);
                var similarityWeightedValue = comparables
                    .Sum(c => (decimal)c.TransferFee * (decimal)c.SimilarityScore) / 
                    comparables.Sum(c => (decimal)c.SimilarityScore);

                // Blend base calculation with market comparables (70% base, 30% comparables)
                adjustedValue = (adjustedValue * 0.7m) + (similarityWeightedValue * 0.3m);
            }

            return adjustedValue;
        }

        private decimal ApplyPerformanceTrend(decimal baseValue, PerformanceMetrics metrics)
        {
            var trendMultiplier = 1.0m;

            // Recent form trend (last 6 months vs previous 6 months)
            if (metrics.RecentFormTrend > 0.15) // 15%+ improvement
                trendMultiplier *= 1.25m;
            else if (metrics.RecentFormTrend > 0.05) // 5-15% improvement
                trendMultiplier *= 1.1m;
            else if (metrics.RecentFormTrend < -0.15) // 15%+ decline
                trendMultiplier *= 0.75m;
            else if (metrics.RecentFormTrend < -0.05) // 5-15% decline
                trendMultiplier *= 0.9m;

            // Injury history impact
            if (metrics.InjuryDaysLast12Months > 90) // 3+ months injured
                trendMultiplier *= 0.8m;
            else if (metrics.InjuryDaysLast12Months > 30) // 1-3 months injured
                trendMultiplier *= 0.9m;

            // Consistency factor
            if (metrics.ConsistencyScore > 0.9) // Very consistent
                trendMultiplier *= 1.1m;
            else if (metrics.ConsistencyScore < 0.6) // Inconsistent
                trendMultiplier *= 0.85m;

            return baseValue * trendMultiplier;
        }

        private double CalculateTransferProbability(Player player, PerformanceMetrics metrics)
        {
            double probability = 0.1; // Base 10% chance

            // Contract situation
            if (player.ContractMonthsRemaining <= 6)
                probability += 0.4; // Free agent soon
            else if (player.ContractMonthsRemaining <= 18)
                probability += 0.2; // Contract expiring
            else if (player.ContractMonthsRemaining >= 48)
                probability -= 0.1; // Long-term contract

            // Performance trend
            if (metrics.RecentFormTrend > 0.15)
                probability += 0.15; // Hot streak attracts buyers
            else if (metrics.RecentFormTrend < -0.15)
                probability += 0.1; // Club may sell declining player

            // Age factor
            if (player.Age >= 30)
                probability += 0.1; // Older players more likely to move
            else if (player.Age <= 23)
                probability += 0.05; // Young talent attracts interest

            // Playing time
            if (metrics.MinutesPlayedPercentage < 0.5)
                probability += 0.2; // Not getting playing time
            else if (metrics.MinutesPlayedPercentage > 0.9)
                probability -= 0.05; // Key player, less likely to leave

            // Club financial situation (would need external data)
            // probability += GetClubFinancialPressure(player.CurrentClub);

            return Math.Min(0.95, Math.Max(0.01, probability));
        }

        private async Task<List<TransferComparable>> FindComparablePlayers(Player player)
        {
            var allTransfers = await _marketData.GetRecentTransfers(24); // Last 2 years
            var comparables = new List<TransferComparable>();

            foreach (var transfer in allTransfers)
            {
                var similarity = CalculatePlayerSimilarity(player, transfer);
                if (similarity > 0.6) // 60%+ similarity threshold
                {
                    comparables.Add(new TransferComparable
                    {
                        PlayerName = transfer.PlayerName,
                        TransferFee = transfer.Fee,
                        TransferDate = transfer.Date,
                        SimilarityScore = similarity,
                        Club = transfer.ToClub,
                        Age = transfer.PlayerAge,
                        Position = transfer.Position
                    });
                }
            }

            return comparables.OrderByDescending(c => c.SimilarityScore).ToList();
        }

        private double CalculatePlayerSimilarity(Player player1, TransferRecord transfer)
        {
            double similarity = 0;
            double maxScore = 0;

            // Position similarity (25% weight)
            if (player1.Position == transfer.Position)
                similarity += 25;
            else if (IsCompatiblePosition(player1.Position, transfer.Position))
                similarity += 15;
            maxScore += 25;

            // Age similarity (20% weight)
            var ageDifference = Math.Abs(player1.Age - transfer.PlayerAge);
            var ageScore = Math.Max(0, 20 - (ageDifference * 2));
            similarity += ageScore;
            maxScore += 20;

            // League level similarity (15% weight)
            if (GetLeagueTier(player1.CurrentLeague) == GetLeagueTier(transfer.FromLeague))
                similarity += 15;
            else if (Math.Abs(GetLeagueTier(player1.CurrentLeague) - GetLeagueTier(transfer.FromLeague)) == 1)
                similarity += 10;
            maxScore += 15;

            // Performance level similarity (25% weight)
            // This would compare performance metrics if available
            similarity += 15; // Placeholder - would need transfer player's performance data
            maxScore += 25;

            // Market timing (15% weight)
            var monthsSinceTransfer = (DateTime.UtcNow - transfer.Date).Days / 30.0;
            var timingScore = Math.Max(0, 15 - monthsSinceTransfer);
            similarity += timingScore;
            maxScore += 15;

            return similarity / maxScore;
        }

        private double CalculatePerformanceScore(PerformanceMetrics metrics)
        {
            // Composite performance score from 0-100
            var score = 0.0;

            // Goal/assist contribution (30%)
            score += (metrics.GoalsPerGame * 10 + metrics.AssistsPerGame * 7) * 0.3;

            // Key actions per game (25%)
            score += (metrics.KeyPassesPerGame * 2 + metrics.TacklesPerGame * 1.5 + 
                     metrics.InterceptionsPerGame * 1.5) * 0.25;

            // Efficiency metrics (25%)
            score += (metrics.PassAccuracy + metrics.ShotAccuracy + metrics.CrossAccuracy) / 3 * 0.25;

            // Physical performance (20%)
            score += (metrics.DistanceCoveredPerGame / 100 + metrics.SprintsPerGame) * 0.2;

            return Math.Min(100, Math.Max(0, score));
        }

        private decimal GetPositionMultiplier(string position)
        {
            return position switch
            {
                "Striker" => 1.4m,
                "Attacking Midfielder" => 1.3m,
                "Winger" => 1.2m,
                "Central Midfielder" => 1.0m,
                "Defender" => 0.8m,
                "Goalkeeper" => 0.7m,
                _ => 1.0m
            };
        }

        private double CalculateAgeMultiplier(int age, string position)
        {
            // Peak ages vary by position
            var peakAge = position switch
            {
                "Striker" => 28,
                "Attacking Midfielder" => 27,
                "Winger" => 26,
                "Central Midfielder" => 29,
                "Defender" => 30,
                "Goalkeeper" => 32,
                _ => 28
            };

            if (age <= peakAge)
            {
                // Young players with potential
                if (age <= 21) return 1.2; // High potential premium
                if (age <= 25) return 1.1; // Good potential
                return 1.0; // At or approaching peak
            }
            else
            {
                // Post-peak decline
                var yearsOverPeak = age - peakAge;
                return Math.Max(0.3, 1.0 - (yearsOverPeak * 0.08)); // 8% decline per year after peak
            }
        }

        private async Task<decimal> GetLeagueMultiplier(string club)
        {
            // This would look up league prestige ratings
            // Placeholder implementation
            var league = await _marketData.GetLeagueForClub(club);
            return league switch
            {
                "Premier League" => 1.4m,
                "La Liga" => 1.3m,
                "Bundesliga" => 1.2m,
                "Serie A" => 1.2m,
                "Ligue 1" => 1.1m,
                _ => 1.0m
            };
        }

        private double CalculateInternationalMultiplier(int caps)
        {
            if (caps >= 50) return 1.15;
            if (caps >= 20) return 1.1;
            if (caps >= 5) return 1.05;
            return 1.0;
        }

        private List<string> IdentifyValueDrivers(Player player, PerformanceMetrics metrics, List<TransferComparable> comparables)
        {
            var drivers = new List<string>();

            if (metrics.RecentFormTrend > 0.15)
                drivers.Add("Excellent recent form (+15% performance improvement)");

            if (player.Age <= 23)
                drivers.Add("Young age with high potential");

            if (metrics.ConsistencyScore > 0.9)
                drivers.Add("Exceptional consistency in performance");

            if (metrics.GoalsPerGame > 0.7)
                drivers.Add("High goal-scoring rate");

            if (metrics.AssistsPerGame > 0.5)
                drivers.Add("Creative playmaker with high assist rate");

            if (player.ContractMonthsRemaining <= 18)
                drivers.Add("Contract situation creates urgency");

            if (comparables.Any() && comparables.First().TransferFee > player.CurrentContractValue * 2)
                drivers.Add("Recent comparable transfers at premium prices");

            if (metrics.InjuryDaysLast12Months == 0)
                drivers.Add("Clean injury record");

            return drivers;
        }

        private string DetermineValueTrend(PerformanceMetrics metrics)
        {
            if (metrics.RecentFormTrend > 0.1) return "Rising";
            if (metrics.RecentFormTrend < -0.1) return "Declining";
            return "Stable";
        }

        private string GenerateRecommendation(decimal estimatedValue, decimal currentValue, 
            double transferProbability, PerformanceMetrics metrics)
        {
            var valueRatio = estimatedValue / Math.Max(1, currentValue);
            
            if (valueRatio > 2.0 && transferProbability > 0.6)
                return "STRONG BUY - Undervalued with high transfer probability";
            
            if (valueRatio > 1.5 && transferProbability > 0.4)
                return "BUY - Good value opportunity";
            
            if (valueRatio < 0.7 && transferProbability > 0.5)
                return "SELL - Overvalued with transfer interest";
            
            if (metrics.RecentFormTrend < -0.15)
                return "SELL - Declining performance trend";
            
            if (valueRatio > 0.8 && valueRatio < 1.2)
                return "HOLD - Fair value, monitor performance";
            
            return "MONITOR - Mixed signals, watch closely";
        }

        private DateTime? CalculateOptimalSellWindow(PerformanceMetrics metrics, int age)
        {
            // If player is in good form and at peak age
            if (metrics.RecentFormTrend > 0.1 && age >= 26 && age <= 29)
            {
                return DateTime.UtcNow.AddMonths(6); // Sell at end of season
            }
            
            // If player is young with rising value
            if (age <= 23 && metrics.RecentFormTrend > 0.05)
            {
                return DateTime.UtcNow.AddMonths(18); // Wait for further development
            }
            
            // If player is aging
            if (age >= 30)
            {
                return DateTime.UtcNow.AddMonths(3); // Sell soon before further decline
            }
            
            return null; // No optimal window identified
        }

        private bool IsCompatiblePosition(string pos1, string pos2)
        {
            var compatiblePositions = new Dictionary<string, List<string>>
            {
                ["Striker"] = new List<string> { "Attacking Midfielder", "Winger" },
                ["Winger"] = new List<string> { "Attacking Midfielder", "Striker" },
                ["Attacking Midfielder"] = new List<string> { "Central Midfielder", "Striker", "Winger" },
                ["Central Midfielder"] = new List<string> { "Attacking Midfielder", "Defensive Midfielder" },
                ["Defensive Midfielder"] = new List<string> { "Central Midfielder", "Defender" }
            };

            return compatiblePositions.ContainsKey(pos1) && 
                   compatiblePositions[pos1].Contains(pos2);
        }

        private int GetLeagueTier(string league)
        {
            return league switch
            {
                "Premier League" => 1,
                "La Liga" => 1,
                "Bundesliga" => 1,
                "Serie A" => 1,
                "Ligue 1" => 2,
                "Eredivisie" => 2,
                "Portuguese Liga" => 2,
                _ => 3
            };
        }
    }

    // Supporting models
    public class TransferRecord
    {
        public string PlayerName { get; set; }
        public decimal Fee { get; set; }
        public DateTime Date { get; set; }
        public string FromClub { get; set; }
        public string ToClub { get; set; }
        public string FromLeague { get; set; }
        public string ToLeague { get; set; }
        public int PlayerAge { get; set; }
        public string Position { get; set; }
    }
}