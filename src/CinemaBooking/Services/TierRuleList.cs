// the tier rules as a list for the read-only tier rules window (owner: divyesh)
// the numbers are not written here: they come from LoyaltyService, so the window always shows the rules the bookings use
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
        // red stubs: wrong values on purpose. the real code comes in the green step
        public static TierRuleRow[] Rows()
        {
            return new TierRuleRow[0];
        }

        public static string PointsRateText()
        {
            return "";
        }
    }
}
