// the tier rules as a list for the read-only tier rules window (owner: divyesh)
// the numbers are not written here: they come from LoyaltyService, so the window always shows the rules the bookings use
using System.Globalization;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    // one line of the tier rules table
    public class TierRuleRow
    {
        public Tier Tier { get; }
        public int FromPoints { get; }
        public decimal DiscountPercent { get; }
        public decimal PointsMultiplier { get; }

        public TierRuleRow(Tier tier, int from_points, decimal discount_percent, decimal points_multiplier)
        {
            Tier = tier;
            FromPoints = from_points;
            DiscountPercent = discount_percent;
            PointsMultiplier = points_multiplier;
        }
    }

    public static class TierRuleList
    {
        // the three tiers in order, every number is asked from LoyaltyService
        public static TierRuleRow[] Rows()
        {
            Tier[] tiers = { Tier.Standard, Tier.Silver, Tier.Gold };
            TierRuleRow[] rows = new TierRuleRow[tiers.Length];

            for (int tier_index = 0; tier_index < tiers.Length; tier_index++)
            {
                Tier tier = tiers[tier_index];
                rows[tier_index] = new TierRuleRow(
                    tier,
                    LoyaltyService.PointsNeededFor(tier),
                    LoyaltyService.TierDiscountPercent(tier),
                    LoyaltyService.PointsMultiplier(tier));
            }

            return rows;
        }

        // the points rate as one sentence
        public static string PointsRateText()
        {
            return "1 point for every Rs " + LoyaltyService.RupeesPerPoint.ToString("0.##", CultureInfo.InvariantCulture)
                + " paid, counted on the amount after the tier discount";
        }
    }
}
