// the name, email and phone of a guest booking, checked before the booking is made (owner: divyesh)
// the rules live in Booking, this class only trims the text and gathers every message, one line per wrong box
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class GuestDetails
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_NAME = 0;
        public const int BOX_EMAIL = 1;
        public const int BOX_PHONE = 2;
        public const int BOX_COUNT = 3;

        public string Name { get; }
        public string Email { get; }
        public string Phone { get; }

        private GuestDetails(string name, string email, string phone)
        {
            Name = name;
            Email = email;
            Phone = phone;
        }

        // one message for each box, in page order, and an empty text for a box that is fine
        // the page shows each message under its box. a missing value counts as empty
        public static string[] CheckBoxes(string name, string email, string phone)
        {
            string clean_name = (name ?? "").Trim();
            string clean_email = (email ?? "").Trim();
            string clean_phone = (phone ?? "").Trim();
            string[] messages = new string[BOX_COUNT];
            string rule_message;

            // null as member id means a guest, so the guest rules are checked
            messages[BOX_NAME] = "";
            if (!Booking.IsValidGuestName(null, clean_name, out rule_message))
            {
                messages[BOX_NAME] = rule_message.TrimEnd('\n');
            }

            messages[BOX_EMAIL] = "";
            if (!Booking.IsValidGuestEmail(null, clean_email, out rule_message))
            {
                messages[BOX_EMAIL] = rule_message.TrimEnd('\n');
            }

            messages[BOX_PHONE] = "";
            if (!Booking.IsValidGuestPhone(null, clean_phone, out rule_message))
            {
                messages[BOX_PHONE] = rule_message.TrimEnd('\n');
            }

            return messages;
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
            string all_errors = "";
            string[] box_messages = CheckBoxes(clean_name, clean_email, clean_phone);
            for (int index = 0; index < box_messages.Length; index++)
            {
                if (box_messages[index] != "")
                {
                    all_errors += box_messages[index] + "\n";
                }
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
