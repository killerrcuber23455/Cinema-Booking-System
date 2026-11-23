// the bookings of the signed in member, newest first, for the My bookings page (owner: divyesh)
// the page holds no rules and no database, it asks this class and shows the result and the message
using CinemaBooking.Data;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public static class MyBookingList
    {
        public const string SIGN_IN_TEXT = "Sign in as a customer to see your bookings";
        public const string NONE_TEXT = "You have no bookings yet";
        public const string DATABASE_DOWN_TEXT = "Could not reach the database, so your bookings cannot be shown now";

        // red stub: gives back nothing and no message, on purpose. the real code comes in the green step
        public static Booking[] ForMember(IDatabase database, int? member_id, out string message)
        {
            message = "";
            return new Booking[0];
        }
    }
}
