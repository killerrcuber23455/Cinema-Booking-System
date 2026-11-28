// works out the numbers and lines shown on the booking summary page (owner: divyesh)
// the discount and the points come from LoyaltyService, so the page shows the same numbers the booking will use
using System;
using System.Globalization;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class BookingSummary
    {
        private readonly Showing _showing;
        private readonly string[] _seats;

        // null means a guest, a guest has no tier and earns no points
        private readonly Member? _member;

        public BookingSummary(Showing showing, string[] seats, Member? member)
        {
            _showing = showing;
            _seats = seats;
            _member = member;
        }

        public int SeatCount
        {
            get { return _seats.Length; }
        }

        // the price of all the seats before any discount
        public decimal TicketTotal
        {
            get { return _showing.Price * _seats.Length; }
        }

        // a guest pays the standard price
        private Tier Tier
        {
            get { return _member == null ? Tier.Standard : _member.Tier; }
        }

        public decimal AmountToPay
        {
            get { return LoyaltyService.ApplyTierDiscount(TicketTotal, Tier); }
        }

        public decimal DiscountAmount
        {
            get { return TicketTotal - AmountToPay; }
        }

        // points are earned on the amount paid, a guest earns none
        public int PointsToEarn
        {
            get { return _member == null ? 0 : LoyaltyService.PointsEarned(AmountToPay, Tier); }
        }

        // the lines the page shows, one under the other
        // the discount line only appears when there is a discount
        public string[] DescribeLines()
        {
            bool has_discount = DiscountAmount > 0m;
            string[] lines = new string[has_discount ? 5 : 4];
            int next_line = 0;

            lines[next_line++] = "Seats: " + string.Join(", ", _seats);
            lines[next_line++] = "Tickets: " + SeatCount + " x " + Money(_showing.Price) + " = " + Money(TicketTotal);

            if (has_discount)
            {
                string percent = LoyaltyService.TierDiscountPercent(Tier).ToString("0", CultureInfo.InvariantCulture);
                lines[next_line++] = Tier + " discount (" + percent + "%): -" + Money(DiscountAmount);
            }

            lines[next_line++] = "To pay: " + Money(AmountToPay);
            lines[next_line++] = _member == null
                ? "Points to earn: none for guests, sign in to earn points"
                : "Points to earn: " + PointsToEarn;

            return lines;
        }

        // an amount like Rs 250.00, with the same look on every computer
        private static string Money(decimal amount)
        {
            return "Rs " + amount.ToString("0.00", CultureInfo.InvariantCulture);
        }
    }
}
