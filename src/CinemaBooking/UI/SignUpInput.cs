// the boxes of the member sign up page, checked before the account is made (owner: divyesh)
// the name, email and phone rules live in Member, the password rule lives in InputCheck,
// this class only trims the text and gives one message for each wrong box
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class SignUpInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_NAME = 0;
        public const int BOX_EMAIL = 1;
        public const int BOX_PHONE = 2;
        public const int BOX_PASSWORD = 3;
        public const int BOX_CONFIRM_PASSWORD = 4;
        public const int BOX_COUNT = 5;

        public string Name { get; }
        public string Email { get; }
        public string Phone { get; }
        public string Password { get; }

        private SignUpInput(string name, string email, string phone, string password)
        {
            Name = name;
            Email = email;
            Phone = phone;
            Password = password;
        }

        // one message for each box, in page order, and an empty text for a box that is fine
        // the "do not match" message goes under the confirm box and only when the two passwords are really different
        public static string[] CheckBoxes(string name, string email, string phone, string password, string confirm_password)
        {
            string clean_name = (name ?? "").Trim();
            string clean_email = (email ?? "").Trim();
            string clean_phone = (phone ?? "").Trim();
            string typed_password = password ?? "";
            string typed_confirm = confirm_password ?? "";
            string[] messages = new string[BOX_COUNT];
            string rule_message;

            messages[BOX_NAME] = "";
            if (!Member.IsValidName(clean_name, out rule_message))
            {
                messages[BOX_NAME] = rule_message.TrimEnd('\n');
            }

            messages[BOX_EMAIL] = "";
            if (!Member.IsValidEmail(clean_email, out rule_message))
            {
                messages[BOX_EMAIL] = rule_message.TrimEnd('\n');
            }

            messages[BOX_PHONE] = "";
            if (!Member.IsValidPhone(clean_phone, out rule_message))
            {
                messages[BOX_PHONE] = rule_message.TrimEnd('\n');
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

        // spaces around name, email and phone are removed, the passwords are kept exactly as typed
        // every wrong box is shown, one line each, in page order
        public static bool TryCreate(string name, string email, string phone, string password,
            string confirm_password, out SignUpInput? input, out string error_message)
        {
            input = null;
            error_message = JoinMessages(CheckBoxes(name, email, phone, password, confirm_password));

            if (error_message != "")
            {
                return false;
            }

            input = new SignUpInput((name ?? "").Trim(), (email ?? "").Trim(), (phone ?? "").Trim(), password ?? "");
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
