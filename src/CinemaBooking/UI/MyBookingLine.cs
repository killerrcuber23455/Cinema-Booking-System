// one line of text for a booking in the My bookings list (owner: divyesh)
using System.Globalization;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class MyBookingLine
    {
        // for example: Heat · Tue 1 Dec 2026 · 18:30  -  seats C5, C6  -  Rs 300.00  -  Confirmed  -  ABC123
        public static string Text(Booking booking, string showing_text)
        {
            return showing_text
                + "  -  seats " + string.Join(", ", booking.Seats)
                + "  -  Rs " + booking.Total.ToString("0.00", CultureInfo.InvariantCulture)
                + "  -  " + booking.Status
                + "  -  " + booking.Reference;
        }
    }
}
