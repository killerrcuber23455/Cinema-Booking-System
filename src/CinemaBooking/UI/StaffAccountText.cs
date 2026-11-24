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
            return new string[0];
        }

        // the role for the text chosen in the role list, null when the text is empty or not a role the list offers
        public static Role? ParseRole(string? text)
        {
            return null;
        }

        // the line shown after an account is made, for example "Staff account created for ann@x.com"
        public static string SuccessMessage(string email, Role role)
        {
            return "";
        }
    }
}
