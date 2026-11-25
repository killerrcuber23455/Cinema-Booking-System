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
        // a guest can only go to the sign in, a customer sees the account and the bookings, staff and admin the account
        // everyone who is signed in can sign out. a role that is not known is treated like a guest
        public static UserMenuItem[] ItemsFor(Role role)
        {
            if (!Enum.IsDefined<Role>(role))
            {
                return new UserMenuItem[] { UserMenuItem.SignIn };
            }

            switch (role)
            {
                case Role.Customer:
                    return new UserMenuItem[] { UserMenuItem.AccountDetails, UserMenuItem.MyBookings, UserMenuItem.SignOut };
                case Role.Staff:
                case Role.Admin:
                    return new UserMenuItem[] { UserMenuItem.AccountDetails, UserMenuItem.SignOut };
                default:
                    return new UserMenuItem[] { UserMenuItem.SignIn };
            }
        }

        // the text of an entry
        public static string LabelFor(UserMenuItem item)
        {
            switch (item)
            {
                case UserMenuItem.AccountDetails:
                    return "Account details";
                case UserMenuItem.MyBookings:
                    return "My bookings";
                case UserMenuItem.SignOut:
                    return "Sign out";
                default:
                    return "Sign in";
            }
        }
    }
}
