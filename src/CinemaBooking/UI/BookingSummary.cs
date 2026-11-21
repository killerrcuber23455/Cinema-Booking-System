// works out the numbers and lines shown on the booking summary page (owner: divyesh)
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class BookingSummary
    {
        // stubs for card 403, the real code comes in the green step
        public BookingSummary(Showing showing, string[] seats, Member? member)
        {
        }

        public int SeatCount
        {
            get { throw new NotImplementedException(); }
        }

        public decimal TicketTotal
        {
            get { throw new NotImplementedException(); }
        }

        public decimal DiscountAmount
        {
            get { throw new NotImplementedException(); }
        }

        public decimal AmountToPay
        {
            get { throw new NotImplementedException(); }
        }

        public int PointsToEarn
        {
            get { throw new NotImplementedException(); }
        }

        public string[] DescribeLines()
        {
            throw new NotImplementedException();
        }
    }
}
