// the boxes of the "create the first admin" page, checked before the admin account is made (owner: divyesh)
// the email rule lives in Member, the password rule lives in InputCheck,
// this class only trims the email and gives one message for each wrong box
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class FirstAdminInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_EMAIL = 0;
        public const int BOX_PASSWORD = 1;
        public const int BOX_CONFIRM_PASSWORD = 2;
        public const int BOX_COUNT = 3;

        public string Email { get; }
        public string Password { get; }

        private FirstAdminInput(string email, string password)
        {
            Email = email;
            Password = password;
        }

        // one message for each box, in page order, and an empty text for a box that is fine
        // the "do not match" message goes under the confirm box and only when the two passwords are really different
        public static string[] CheckBoxes(string email, string password, string confirm_password)
        {
            string clean_email = (email ?? "").Trim();
            string typed_password = password ?? "";
            string typed_confirm = confirm_password ?? "";
            string[] messages = new string[BOX_COUNT];
            string rule_message;

            messages[BOX_EMAIL] = "";
            if (!Member.IsValidEmail(clean_email, out rule_message))
            {
                messages[BOX_EMAIL] = rule_message.TrimEnd('\n');
            }

            messages[BOX_PASSWORD] = "";
            if (!InputCheck.IsValidPassword(typed_password, out rule_message))
            {
                messages[BOX_PASSWORD] = rule_message.TrimEnd('\n');
            }

            messages[BOX_CONFIRM_PASSWORD] = "";
            if (typed_password != typed_confirm)
            {
                messages[BOX_CONFIRM_PASSWORD] = "passwords do not match";
            }

            return messages;
        }

        // every wrong box is shown, one line each, in page order
        public static bool TryCreate(string email, string password, string confirm_password,
            out FirstAdminInput? input, out string error_message)
        {
            input = null;
            error_message = JoinMessages(CheckBoxes(email, password, confirm_password));

            if (error_message != "")
            {
                return false;
            }

            input = new FirstAdminInput((email ?? "").Trim(), password ?? "");
            return true;
        }

        // puts the messages of the wrong boxes together, one line each
        private static string JoinMessages(string[] messages)
        {
            string all_errors = "";

            for (int box_index = 0; box_index < messages.Length; box_index++)
            {
                if (messages[box_index] != "")
                {
                    all_errors += messages[box_index] + "\n";
                }
            }

            return all_errors.TrimEnd('\n');
        }
    }
}
