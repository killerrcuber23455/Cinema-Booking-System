//loyalty number rules: points, tiers, discount, using points
using System;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public static class LoyaltyService
    {
        //every loyalty number lives here
        private const decimal SILVER_DISCOUNT_PERCENT = 5m;
        private const decimal GOLD_DISCOUNT_PERCENT = 10m;
        private const decimal SILVER_MULTIPLIER = 1.25m;
        private const decimal GOLD_MULTIPLIER = 1.5m;
        private const decimal RUPEES_PER_POINT = 10m;
        private const decimal PERCENT_BASE = 100m;
        private const int SILVER_FROM_POINTS = 100;
        private const int GOLD_FROM_POINTS = 300;

        //percent taken off the final ticket according to tier
        public static decimal TierDiscountPercent(Tier tier)
        {
            if (tier == Tier.Gold) return GOLD_DISCOUNT_PERCENT;
            if (tier == Tier.Silver) return SILVER_DISCOUNT_PERCENT;
            return 0m;
        }

        //how many points the tier earns
        public static decimal PointsMultiplier(Tier tier)
        {
            if (tier == Tier.Gold) return GOLD_MULTIPLIER;
            if (tier == Tier.Silver) return SILVER_MULTIPLIER;
            return 1m;
        }

        //final amount left to pay
        public static decimal ApplyTierDiscount(decimal total, Tier tier)
        {
            decimal discount = total * TierDiscountPercent(tier) / PERCENT_BASE;
            return Math.Round(total - discount, 2);
        }

        //points earned on the amount actually paid, rounded down to a whole number
        public static int PointsEarned(decimal amount_paid, Tier tier)
        {
            decimal normal_points = amount_paid / RUPEES_PER_POINT;
            decimal multiplied_points = normal_points * PointsMultiplier(tier);
            return (int)Math.Floor(multiplied_points);
        }

        //the tier a member reaches from all points he ever earned
        public static Tier TierForLifetimePoints(int lifetime_points)
        {
            if (lifetime_points >= GOLD_FROM_POINTS) return Tier.Gold;
            if (lifetime_points >= SILVER_FROM_POINTS) return Tier.Silver;
            return Tier.Standard;
        }
    }
}
