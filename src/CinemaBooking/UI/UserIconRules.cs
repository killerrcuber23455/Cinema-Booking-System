// what a click on the user box at the bottom of the side menu does (owner: divyesh)
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public enum UserIconAction
    {
        SignIn,
        OpenSettings
    }

    public static class UserIconRules
    {
        // a guest is sent to sign in, everyone who is signed in gets the settings page
        public static UserIconAction ActionFor(Role role)
        {
            if (!Enum.IsDefined<Role>(role))
            {
                return UserIconAction.SignIn;
            }

            return role == Role.Guest ? UserIconAction.SignIn : UserIconAction.OpenSettings;
        }
    }
}
