// the rules for the settings page, the page the user box at the bottom of the side menu opens (owner: divyesh)
// which parts of the page a role sees, and the words over each part. the page itself only draws what these rules say.
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public enum SettingsSection
    {
        Account,
        Password,
        Bookings,
        AddAccount,
        SignOut
    }

    public static class SettingsRules
    {
        // the title of the page in the main window
        public const string PAGE_TITLE = "Settings";

        // the parts a role sees, top to bottom. a guest has no settings, the user box sends a guest to sign in
        public static SettingsSection[] SectionsFor(Role role)
        {
            if (!Enum.IsDefined<Role>(role))
            {
                return new SettingsSection[0];
            }

            switch (role)
            {
                case Role.Customer:
                    return new SettingsSection[]
                    {
                        SettingsSection.Account, SettingsSection.Password, SettingsSection.Bookings, SettingsSection.SignOut
                    };
                case Role.Staff:
                    return new SettingsSection[]
                    {
                        SettingsSection.Account, SettingsSection.Password, SettingsSection.SignOut
                    };
                case Role.Admin:
                    return new SettingsSection[]
                    {
                        SettingsSection.Account, SettingsSection.Password, SettingsSection.AddAccount, SettingsSection.SignOut
                    };
                default:
                    return new SettingsSection[0];
            }
        }

        // only an admin may add another staff or admin account
        public static bool CanAddAccounts(Role role)
        {
            return role == Role.Admin;
        }

        // the words over a part of the page
        public static string TitleFor(SettingsSection section)
        {
            switch (section)
            {
                case SettingsSection.Account:
                    return "Account details";
                case SettingsSection.Password:
                    return "Change password";
                case SettingsSection.Bookings:
                    return "My bookings";
                case SettingsSection.AddAccount:
                    return "Add an account";
                default:
                    return "Sign out";
            }
        }
    }
}
