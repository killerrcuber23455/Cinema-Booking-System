// the boxes of the "create staff or admin account" page, checked before the account is made (owner: divyesh)
// the email rule lives in Member, the password rule lives in InputCheck,
// this class only trims the email, checks the role and gives one message for each wrong box
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class StaffInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_EMAIL = 0;
        public const int BOX_ROLE = 1;
        public const int BOX_PASSWORD = 2;
        public const int BOX_CONFIRM_PASSWORD = 3;
        public const int BOX_COUNT = 4;

        private const string ROLE_MESSAGE = "role must be Staff or Admin";

        // the roles the role list on the page offers (a customer or a guest is made by the sign up page)
        public static readonly Role[] ChoosableRoles = new Role[] { Role.Staff, Role.Admin };

        public string Email { get; }
        public Role NewRole { get; }
        public string Password { get; }

        private StaffInput(string email, Role new_role, string password)
        {
            Email = email;
            NewRole = new_role;
            Password = password;
        }

        // one message for each box, in page order, and an empty text for a box that is fine
        // the "do not match" message goes under the confirm box and only when the two passwords are really different
        public static string[] CheckBoxes(string email, Role? role, string password, string confirm_password)
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

            messages[BOX_ROLE] = "";
            if (role == null || Array.IndexOf(ChoosableRoles, role.Value) < 0)
            {
                messages[BOX_ROLE] = ROLE_MESSAGE;
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

        // spaces around the email are removed, the password is kept exactly as typed
        // every wrong box is shown, one line each, in page order
        public static bool TryCreate(string email, Role? role, string password, string confirm_password,
            out StaffInput? input, out string error_message)
        {
            input = null;
            error_message = JoinMessages(CheckBoxes(email, role, password, confirm_password));

            if (error_message != "")
            {
                return false;
            }

            input = new StaffInput((email ?? "").Trim(), role!.Value, password ?? "");
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
