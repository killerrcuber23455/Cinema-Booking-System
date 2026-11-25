// the words of the staff accounts page that can be tested without a window (owner: divyesh)
// the role list offers the roles StaffInput allows, the page turns the chosen text back into a role,
// and the success message is made here so the page only shows it
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class StaffAccountText
    {
        // the texts of the role list, in the order of StaffInput.ChoosableRoles
        public static string[] RoleOptions()
        {
            string[] options = new string[StaffInput.ChoosableRoles.Length];

            for (int role_index = 0; role_index < options.Length; role_index++)
            {
                options[role_index] = StaffInput.ChoosableRoles[role_index].ToString();
            }

            return options;
        }

        // the role for the text chosen in the role list, null when the text is empty or not a role the list offers
        // spaces around the text and capital letters do not matter, a number like "2" is not a role
        public static Role? ParseRole(string? text)
        {
            string clean_text = (text ?? "").Trim();

            for (int role_index = 0; role_index < StaffInput.ChoosableRoles.Length; role_index++)
            {
                Role role = StaffInput.ChoosableRoles[role_index];

                if (string.Equals(role.ToString(), clean_text, StringComparison.OrdinalIgnoreCase))
                {
                    return role;
                }
            }

            return null;
        }

        // the line shown after an account is made, for example "Staff account created for ann@x.com"
        public static string SuccessMessage(string email, Role role)
        {
            return role + " account created for " + email;
        }
    }
}
