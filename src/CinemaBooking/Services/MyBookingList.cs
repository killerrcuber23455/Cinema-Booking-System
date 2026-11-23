// the bookings of the signed in member, newest first, for the My bookings page (owner: divyesh)
// the page holds no rules and no database, it asks this class and shows the result and the message
using System;
using CinemaBooking.Data;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public static class MyBookingList
    {
        public const string SIGN_IN_TEXT = "Sign in as a customer to see your bookings";
        public const string NONE_TEXT = "You have no bookings yet";
        public const string DATABASE_DOWN_TEXT = "Could not reach the database, so your bookings cannot be shown now";

        // gives the bookings of the member, newest first, and an empty message when there are some.
        // when there is nothing to show the list is empty and the message says why
        public static Booking[] ForMember(IDatabase database, int? member_id, out string message)
        {
            message = "";

            if (member_id == null)
            {
                message = SIGN_IN_TEXT;
                return new Booking[0];
            }

            Booking[] found;
            try
            {
                found = database.GetBookingsForMember(member_id.Value);
            }
            catch (Exception)
            {
                message = DATABASE_DOWN_TEXT;
                return new Booking[0];
            }

            if (found.Length == 0)
            {
                message = NONE_TEXT;
                return found;
            }

            // insertion sort by the time of booking, newest first, the same way the other lists are sorted
            for (int sorted_end = 1; sorted_end < found.Length; sorted_end++)
            {
                Booking current = found[sorted_end];
                int compare_index = sorted_end - 1;

                while (compare_index >= 0 && found[compare_index].CreatedAt < current.CreatedAt)
                {
                    found[compare_index + 1] = found[compare_index];
                    compare_index--;
                }

                found[compare_index + 1] = current;
            }

            return found;
        }
    }
}
