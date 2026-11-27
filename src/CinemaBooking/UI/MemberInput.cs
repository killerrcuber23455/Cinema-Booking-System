// the three boxes of the "register a member" part of the Members page, checked before the member is registered (owner: divyesh)
// the rules live in Member (name, email, phone), this class only trims and gives one message for each wrong box
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class MemberInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_NAME = 0;
        public const int BOX_EMAIL = 1;
        public const int BOX_PHONE = 2;
        public const int BOX_COUNT = 3;

        // one message for each box, in page order, and an empty text for a box that is fine
        public static string[] CheckBoxes(string name, string email, string phone)
        {
            string[] messages = new string[BOX_COUNT];
            string rule_message;

            messages[BOX_NAME] = "";
            if (!Member.IsValidName((name ?? "").Trim(), out rule_message))
            {
                messages[BOX_NAME] = rule_message.TrimEnd('\n');
            }

            messages[BOX_EMAIL] = "";
            if (!Member.IsValidEmail((email ?? "").Trim(), out rule_message))
            {
                messages[BOX_EMAIL] = rule_message.TrimEnd('\n');
            }

            messages[BOX_PHONE] = "";
            if (!Member.IsValidPhone((phone ?? "").Trim(), out rule_message))
            {
                messages[BOX_PHONE] = rule_message.TrimEnd('\n');
            }

            return messages;
        }
    }
}
