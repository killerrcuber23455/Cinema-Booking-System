// the name, email and phone of a guest booking, checked before the booking is made (owner: divyesh)
// the rules live in Booking, this class only trims the text and shows the first message in page order
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class GuestDetails
    {
        public string Name { get; }
        public string Email { get; }
        public string Phone { get; }

        private GuestDetails(string name, string email, string phone)
        {
            Name = name;
            Email = email;
            Phone = phone;
        }

        // spaces around the text are removed first, so " Anna " and "Anna" are the same
        // a missing value counts as empty, so the rule shows its normal message
        public static bool TryCreate(string name, string email, string phone, out GuestDetails? details, out string error_message)
        {
            string clean_name = (name ?? "").Trim();
            string clean_email = (email ?? "").Trim();
            string clean_phone = (phone ?? "").Trim();
            details = null;

            // null as member id means a guest, so the guest rules are checked
            if (!Booking.IsValidGuestName(null, clean_name, out error_message))
            {
                return false;
            }

            if (!Booking.IsValidGuestEmail(null, clean_email, out error_message))
            {
                return false;
            }

            if (!Booking.IsValidGuestPhone(null, clean_phone, out error_message))
            {
                return false;
            }

            details = new GuestDetails(clean_name, clean_email, clean_phone);
            return true;
        }
    }
}
