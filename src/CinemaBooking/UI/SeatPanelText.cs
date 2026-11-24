// the words of the summary panel next to the seat map: tickets, price of one and the total (owner: divyesh)
// invariant culture, so the amounts read the same on every computer
using System.Globalization;

namespace CinemaBooking.UI
{
    public static class SeatPanelText
    {
        public static string TicketsText(int seat_count)
        {
            if (seat_count <= 0)
            {
                return "No seats chosen yet";
            }

            return seat_count == 1 ? "1 ticket" : seat_count + " tickets";
        }

        public static string PriceEachText(decimal price)
        {
            return "Rs " + price.ToString("0.00", CultureInfo.InvariantCulture) + " each";
        }

        // the total of the tickets before any loyalty discount, the booking summary page works that out
        public static string TotalText(int seat_count, decimal price)
        {
            decimal total = Math.Max(0, seat_count) * price;
            return "Rs " + total.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
