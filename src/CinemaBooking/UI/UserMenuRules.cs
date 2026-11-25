// the menu that opens when the user box at the bottom of the side menu is clicked (owner: divyesh)
// the main window only asks this class which entries each role gets and what they say, so the rules can be tested
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    // the entries of the user menu
    public enum UserMenuItem
    {
        SignIn,
        AccountDetails,
        MyBookings,
        SignOut
    }

    public static class UserMenuRules
    {
        // the entries this role sees, in the order they appear
        public static UserMenuItem[] ItemsFor(Role role)
        {
            return new UserMenuItem[0];
        }

        // the text of an entry
        public static string LabelFor(UserMenuItem item)
        {
            return "";
        }
    }
}
