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
        // this is only a stub for the red step, it finds nothing
        public static Booking? Find(IDatabase database, string? reference, string? guest_email, out string error_message)
        {
            error_message = "";
            return null;
        }
    }
}
