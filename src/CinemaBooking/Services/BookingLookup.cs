// finds a guest's booking from the reference and the email they used (owner: divyesh)
// a wrong email and an unknown reference give the same message, so nobody can use this to find out which references exist
using System;
using CinemaBooking.Data;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public static class BookingLookup
    {
        public const string EMAIL_TEXT = "Enter the email used for the booking";
        public const string NOT_FOUND_TEXT = "No booking found with this reference and email";

        // gives back the guest booking, or null with the reason
        // a cancelled booking is still found, so the page can say it is cancelled
        public static Booking? Find(IDatabase database, string? reference, string? guest_email, out string error_message)
        {
            if (!Booking.IsValidReference(reference ?? "", out error_message))
            {
                return null;
            }

            if (string.IsNullOrWhiteSpace(guest_email))
            {
                error_message = EMAIL_TEXT;
                return null;
            }

            Booking? booking = database.FindBooking(reference!.Trim());

            // a member booking is never shown here, members use My bookings
            if (booking == null
                || !booking.IsGuest
                || !string.Equals(booking.GuestEmail, guest_email.Trim(), StringComparison.OrdinalIgnoreCase))
            {
                error_message = NOT_FOUND_TEXT;
                return null;
            }

            error_message = "";
            return booking;
        }
    }
}
