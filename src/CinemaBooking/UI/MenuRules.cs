// which buttons of the side menu each role sees (owner: divyesh)
// the main window only asks this class, so the role rules for the menu are in one place and can be tested
using System;
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
        // a guest can look at films, find a booking and sign in
        private static readonly MenuItem[] GUEST_ITEMS =
        {
            MenuItem.Films, MenuItem.CancelBooking, MenuItem.Account
        };

        // a customer sees their own bookings
        private static readonly MenuItem[] CUSTOMER_ITEMS =
        {
            MenuItem.Films, MenuItem.MyBookings, MenuItem.Account
        };

        // staff run the cinema desk: showings, booking for a customer, cancelling and members
        private static readonly MenuItem[] STAFF_ITEMS =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.BookSeats,
            MenuItem.CancelBooking, MenuItem.Members, MenuItem.Account
        };

        // an admin sees everything staff see, and also the admin page
        private static readonly MenuItem[] ADMIN_ITEMS =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.BookSeats,
            MenuItem.CancelBooking, MenuItem.Members, MenuItem.Admin, MenuItem.Account
        };

        // true when this role may see this menu button
        public static bool IsVisible(MenuItem item, Role role)
        {
            return Array.IndexOf(ItemsFor(role), item) >= 0;
        }

        // the buttons this role sees, in the order they appear
        // a role that is not known is treated like a guest, so it never sees more than a guest
        public static MenuItem[] ItemsFor(Role role)
        {
            if (!Enum.IsDefined<Role>(role))
            {
                return (MenuItem[])GUEST_ITEMS.Clone();
            }

            switch (role)
            {
                case Role.Customer:
                    return (MenuItem[])CUSTOMER_ITEMS.Clone();
                case Role.Staff:
                    return (MenuItem[])STAFF_ITEMS.Clone();
                case Role.Admin:
                    return (MenuItem[])ADMIN_ITEMS.Clone();
                default:
                    return (MenuItem[])GUEST_ITEMS.Clone();
            }
        }

        // the text of the account button: "Sign in" for a guest, "My account" for everyone else
        public static string AccountLabel(Role role)
        {
            if (!Enum.IsDefined<Role>(role) || role == Role.Guest)
            {
                return "Sign in";
            }

            return "My account";
        }
    }
}
