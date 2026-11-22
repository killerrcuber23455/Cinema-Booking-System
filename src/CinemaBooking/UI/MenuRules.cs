// empty stub so the tests compile and fail first (red), the real code comes in the green commit (owner: divyesh)
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    // the buttons of the side menu
    public enum MenuItem
    {
        Films,
        Showings,
        BookSeats,
        MyBookings,
        CancelBooking,
        Members,
        Admin,
        Account
    }

    public static class MenuRules
    {
        // true when this role may see this menu button
        public static bool IsVisible(MenuItem item, Role role)
        {
            return false;
        }

        // the buttons this role sees, in the order they appear
        public static MenuItem[] ItemsFor(Role role)
        {
            return new MenuItem[0];
        }

        // the text of the account button: "Sign in" for a guest, "My account" for everyone else
        public static string AccountLabel(Role role)
        {
            return "";
        }
    }
}
