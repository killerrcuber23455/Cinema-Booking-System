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
        MyBookings,
        CancelBooking,
        Members,
        TierRules,
        Admin,
        Screens
    }

    public static class MenuRules
    {
        // the role the program starts with, before anyone signs in
        public const Role STARTING_ROLE = Role.Guest;

        // a guest can look at films and find a booking, signing in is on the landing window and in the user box
        private static readonly MenuItem[] GUEST_ITEMS =
        {
            MenuItem.Films, MenuItem.CancelBooking
        };

        // a customer sees their own bookings, the account is in the user box at the bottom
        private static readonly MenuItem[] CUSTOMER_ITEMS =
        {
            MenuItem.Films, MenuItem.MyBookings
        };

        // staff run the cinema desk: showings, cancelling and members
        private static readonly MenuItem[] STAFF_ITEMS =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.CancelBooking, MenuItem.Members
        };

        // an admin sees everything staff see, and also the tier rules, the staff accounts page and the admin page
        private static readonly MenuItem[] ADMIN_ITEMS =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.CancelBooking, MenuItem.Members,
            MenuItem.TierRules, MenuItem.Admin
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
    }
}
