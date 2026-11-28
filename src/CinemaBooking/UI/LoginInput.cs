// the email and password typed on the login page, checked before the login is tried (owner: divyesh)
// the email rule lives in Member. the password only has to be typed, because the 8 character rule is for
// new passwords and an old account must still be able to try. the login itself is done by the login service
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class LoginInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_EMAIL = 0;
        public const int BOX_PASSWORD = 1;
        public const int BOX_COUNT = 2;

        public string Email { get; }
        public string Password { get; }

        private LoginInput(string email, string password)
        {
            Email = email;
            Password = password;
        }

        // one message for each box, in page order, and an empty text for a box that is fine
        // the page shows each message under its box. a missing value counts as empty
        public static string[] CheckBoxes(string email, string password)
        {
            string clean_email = (email ?? "").Trim();
            string typed_password = password ?? "";
            string[] messages = new string[BOX_COUNT];
            string rule_message;

            messages[BOX_EMAIL] = "";
            if (!Member.IsValidEmail(clean_email, out rule_message))
            {
                messages[BOX_EMAIL] = rule_message.TrimEnd('\n');
            }

            messages[BOX_PASSWORD] = "";
            if (typed_password == "")
            {
                messages[BOX_PASSWORD] = "password cannot be empty";
            }

            return messages;
        }

        // spaces around the email are removed, the password is kept exactly as typed
        // every wrong box is shown, one line each, in page order
        public static bool TryCreate(string email, string password, out LoginInput? input, out string error_message)
        {
            input = null;
            error_message = JoinMessages(CheckBoxes(email, password));

            if (error_message != "")
            {
                return false;
            }

            input = new LoginInput((email ?? "").Trim(), password ?? "");
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
