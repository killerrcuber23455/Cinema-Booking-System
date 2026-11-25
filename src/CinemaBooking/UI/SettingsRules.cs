// the rules for the settings page, the page the user box at the bottom of the side menu opens (owner: divyesh)
// which parts of the page a role sees, and the words over each part. the page itself only draws what these rules say.
// STUB for the red step: nothing is shown yet, so the tests fail first
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
            return new SettingsSection[0];
        }

        // only an admin may add another staff or admin account
        public static bool CanAddAccounts(Role role)
        {
            return false;
        }

        // the words over a part of the page
        public static string TitleFor(SettingsSection section)
        {
            return "";
        }
    }
}
