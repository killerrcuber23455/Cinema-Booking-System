// the name, email and phone of a guest booking, checked before the booking is made (owner: divyesh)
// the rules live in Booking, this class only trims the text and gathers every message, one line per wrong box
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

            // every wrong box is added, in page order, so the user sees all the problems together
            // null as member id means a guest, so the guest rules are checked
            string all_errors = "";
            string rule_message;

            if (!Booking.IsValidGuestName(null, clean_name, out rule_message))
            {
                all_errors += rule_message + "\n";
            }

            if (!Booking.IsValidGuestEmail(null, clean_email, out rule_message))
            {
                all_errors += rule_message + "\n";
            }

            if (!Booking.IsValidGuestPhone(null, clean_phone, out rule_message))
            {
                all_errors += rule_message + "\n";
            }

            if (all_errors != "")
            {
                error_message = all_errors.TrimEnd('\n');
                return false;
            }

            details = new GuestDetails(clean_name, clean_email, clean_phone);
            error_message = "";
            return true;
        }
    }
}
