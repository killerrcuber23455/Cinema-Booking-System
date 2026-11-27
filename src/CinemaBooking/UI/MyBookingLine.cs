// one line of text for a booking in the My bookings list (owner: divyesh)
using System.Globalization;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class MyBookingLine
    {
        // only a booking that is still confirmed can be cancelled
        public static bool CanCancel(Booking booking)
        {
            return booking.Status == BookingStatus.Confirmed;
        }

        // for example: Heat · Tue 1 Dec 2026 · 18:30  -  seats C5, C6  -  Rs 300.00  -  Confirmed  -  ABC123
        // a cancelled booking has no seats left, so it says "seats freed" and not an empty list
        public static string Text(Booking booking, string showing_text)
        {
            string seat_text = booking.Seats.Length == 0 ? "seats freed" : "seats " + string.Join(", ", booking.Seats);

            return showing_text
                + "  -  " + seat_text
                + "  -  Rs " + booking.Total.ToString("0.00", CultureInfo.InvariantCulture)
                + "  -  " + booking.Status
                + "  -  " + booking.Reference;
        }
    }
}
