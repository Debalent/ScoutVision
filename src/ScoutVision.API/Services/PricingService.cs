using System;
using System.Collections.Generic;
using System.Linq;

namespace ScoutVision.API.Services
{
    public class PricingService
    {
        public static decimal GetPerSeatPrice(int userCount)
        {
            if (userCount <= 5) return 50;
            if (userCount <= 20) return 40;
            return 30;
        }

        public static decimal GetAnnualDiscount(decimal monthlyPrice)
        {
            return monthlyPrice * 12 * 0.85m;
        }

        public static PricingCalculation CalculatePricing(PricingRequest request)
        {
            var basePricePerUser = GetPerSeatPrice(request.UserCount);
            var baseSubtotal = basePricePerUser * request.UserCount;
            
            // Calculate addon costs
            var addonCost = request.SelectedAddons?.Sum(addon => 
                GetAddonPrice(addon) * request.UserCount) ?? 0;
            
            var monthlySubtotal = baseSubtotal + addonCost;
            
            // Apply annual discount if applicable
            var totalBeforeTax = request.BillingCycle == "annual" 
                ? monthlySubtotal * 12 * 0.85m 
                : monthlySubtotal;
            
            // Calculate tax
            var taxRate = GetTaxRate(request.Region, request.CustomerType);
            var taxAmount = totalBeforeTax * taxRate;
            
            var finalTotal = totalBeforeTax + taxAmount;
            
            return new PricingCalculation
            {
                BasePrice = basePricePerUser,
                UserCount = request.UserCount,
                MonthlySubtotal = monthlySubtotal,
                AddonCosts = request.SelectedAddons?.ToDictionary(
                    addon => addon, 
                    addon => GetAddonPrice(addon) * request.UserCount) ?? new Dictionary<string, decimal>(),
                AnnualDiscount = request.BillingCycle == "annual" ? monthlySubtotal * 12 * 0.15m : 0,
                TaxRate = taxRate,
                TaxAmount = taxAmount,
                Total = finalTotal,
                BillingCycle = request.BillingCycle,
                Region = request.Region,
                EffectiveMonthlyRate = request.BillingCycle == "annual" ? finalTotal / 12 : finalTotal
            };
        }

        private static decimal GetAddonPrice(string addonName)
        {
            return addonName.ToLower() switch
            {
                "advanced analytics" => 10m,
                "transfer engine" => 15m,
                "injury tracking" => 8m,
                "video analysis" => 12m,
                "api access" => 20m,
                _ => 0m
            };
        }

        private static decimal GetTaxRate(string region, string customerType = "business")
        {
            return region switch
            {
                "US" => 0.08m, // Average US sales tax
                "EU" => customerType == "business" ? 0.0m : 0.20m, // VAT or reverse charge
                "UK" => customerType == "business" ? 0.0m : 0.20m, // VAT or reverse charge
                "CA" => 0.13m, // HST
                "AU" => 0.10m, // GST
                _ => 0.05m // Default for other countries
            };
        }

        public static List<PricingTier> GetPricingTiers()
        {
            return new List<PricingTier>
            {
                new()
                {
                    Name = "Starter",
                    MinUsers = 1,
                    MaxUsers = 5,
                    PricePerUser = 50m,
                    Features = new[]
                    {
                        "Basic scouting tools",
                        "Player database (up to 1,000 players)",
                        "Standard reports",
                        "Email support",
                        "Mobile app access"
                    },
                    RecommendedFor = "Small clubs, youth academies"
                },
                new()
                {
                    Name = "Professional",
                    MinUsers = 6,
                    MaxUsers = 20,
                    PricePerUser = 40m,
                    Features = new[]
                    {
                        "Full scouting suite",
                        "Advanced analytics engine",
                        "Unlimited player database",
                        "Custom report builder",
                        "Video integration",
                        "API access",
                        "Priority support",
                        "Data export tools"
                    },
                    RecommendedFor = "Professional clubs, agencies"
                },
                new()
                {
                    Name = "Enterprise",
                    MinUsers = 21,
                    MaxUsers = int.MaxValue,
                    PricePerUser = 30m,
                    Features = new[]
                    {
                        "All Professional features",
                        "White-label options",
                        "Custom integrations",
                        "Dedicated account manager",
                        "24/7 phone support",
                        "On-premise deployment option",
                        "Advanced security features",
                        "Custom training & onboarding"
                    },
                    RecommendedFor = "Large organizations, federations"
                }
            };
        }
    }

    public class PricingRequest
    {
        public int UserCount { get; set; }
        public string BillingCycle { get; set; } = "monthly";
        public string Region { get; set; } = "US";
        public string CustomerType { get; set; } = "business";
        public List<string>? SelectedAddons { get; set; }
    }

    public class PricingCalculation
    {
        public decimal BasePrice { get; set; }
        public int UserCount { get; set; }
        public decimal MonthlySubtotal { get; set; }
        public Dictionary<string, decimal> AddonCosts { get; set; } = new();
        public decimal AnnualDiscount { get; set; }
        public decimal TaxRate { get; set; }
        public decimal TaxAmount { get; set; }
        public decimal Total { get; set; }
        public string BillingCycle { get; set; } = "";
        public string Region { get; set; } = "";
        public decimal EffectiveMonthlyRate { get; set; }
        public decimal AnnualSavings => AnnualDiscount;
        public string RecommendedTier => GetRecommendedTier();

        private string GetRecommendedTier()
        {
            if (UserCount <= 5) return "Starter";
            if (UserCount <= 20) return "Professional";
            return "Enterprise";
        }
    }

    public class PricingTier
    {
        public string Name { get; set; } = "";
        public int MinUsers { get; set; }
        public int MaxUsers { get; set; }
        public decimal PricePerUser { get; set; }
        public string[] Features { get; set; } = Array.Empty<string>();
        public string RecommendedFor { get; set; } = "";
    }
}
