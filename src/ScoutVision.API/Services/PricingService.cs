using System;

namespace ScoutVision.API.Services
{
    public static class PricingService
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
    }
}
