// the boxes of the "create staff or admin account" page, checked before the account is made (owner: divyesh)
// the email rule lives in Member, the password rule lives in InputCheck,
// this class only trims the email, checks the role and gives one message for each wrong box
// card 436: this is the red stub, it does nothing yet, so the tests fail first
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

        // the roles the role list on the page offers (a customer or a guest is made by the sign up page)
        public static readonly Role[] ChoosableRoles = new Role[0];

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
        public static string[] CheckBoxes(string email, Role? role, string password, string confirm_password)
        {
            string[] messages = new string[BOX_COUNT];

            for (int box_index = 0; box_index < BOX_COUNT; box_index++)
            {
                messages[box_index] = "";
            }

            return messages;
        }

        // every wrong box is shown, one line each, in page order
        public static bool TryCreate(string email, Role? role, string password, string confirm_password,
            out StaffInput? input, out string error_message)
        {
            input = null;
            error_message = "";
            return false;
        }
    }
}
