// the text of one tier rule line for the read-only tier rules window (owner: divyesh)
using System.Globalization;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public static class TierRuleLine
    {
        // for example "Silver  -  from 100 lifetime points  -  5% off tickets  -  earns 1.25x points"
        public static string Text(TierRuleRow row)
        {
            string discount = row.DiscountPercent == 0m
                ? "no discount"
                : Number(row.DiscountPercent) + "% off tickets";

            return row.Tier + "  -  from " + row.FromPoints + " lifetime points  -  " + discount
                + "  -  earns " + Number(row.PointsMultiplier) + "x points";
        }

        // 5 stays 5, 1.25 stays 1.25, no trailing zeros
        private static string Number(decimal value)
        {
            return value.ToString("0.##", CultureInfo.InvariantCulture);
        }
    }
}
