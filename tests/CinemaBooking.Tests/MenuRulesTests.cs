// tests for which side menu buttons each role sees (owner: divyesh)
// written before the code for card 346, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class MenuRulesTests
{
    private static readonly Role[] ALL_ROLES = { Role.Guest, Role.Customer, Role.Staff, Role.Admin };

    // guest

    [TestMethod]
    public void ItemsFor_Guest_IsFilmsCancelBookingAndAccount()
    {
        MenuItem[] expected = { MenuItem.Films, MenuItem.CancelBooking, MenuItem.Account };

        CollectionAssert.AreEqual(expected, MenuRules.ItemsFor(Role.Guest));
    }

    [TestMethod]
    public void IsVisible_Guest_CannotSeeAdminMembersOrMyBookings()
    {
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Admin, Role.Guest));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Members, Role.Guest));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.MyBookings, Role.Guest));
    }

    // customer

    [TestMethod]
    public void ItemsFor_Customer_IsFilmsMyBookingsAndAccount()
    {
        MenuItem[] expected = { MenuItem.Films, MenuItem.MyBookings, MenuItem.Account };

        CollectionAssert.AreEqual(expected, MenuRules.ItemsFor(Role.Customer));
    }

    [TestMethod]
    public void IsVisible_Customer_CannotSeeAdminMembersShowingsOrBookSeats()
    {
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Admin, Role.Customer));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Members, Role.Customer));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Showings, Role.Customer));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.BookSeats, Role.Customer));
    }

    // staff

    [TestMethod]
    public void ItemsFor_Staff_HasSixButtonsAndNoAdmin()
    {
        MenuItem[] expected =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.BookSeats,
            MenuItem.CancelBooking, MenuItem.Members, MenuItem.Account
        };

        CollectionAssert.AreEqual(expected, MenuRules.ItemsFor(Role.Staff));
    }

    [TestMethod]
    public void IsVisible_Staff_CannotSeeAdminOrMyBookings()
    {
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Admin, Role.Staff));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.MyBookings, Role.Staff));
    }

    // admin

    [TestMethod]
    public void ItemsFor_Admin_HasNineButtonsWithStaffAccountsBetweenTierRulesAndAdmin()
    {
        MenuItem[] expected =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.BookSeats,
            MenuItem.CancelBooking, MenuItem.Members, MenuItem.TierRules, MenuItem.StaffAccounts,
            MenuItem.Admin, MenuItem.Account
        };

        CollectionAssert.AreEqual(expected, MenuRules.ItemsFor(Role.Admin));
    }

    // rules for every role

    [TestMethod]
    public void IsVisible_EveryRole_SeesFilmsAndAccount()
    {
        foreach (Role role in ALL_ROLES)
        {
            Assert.IsTrue(MenuRules.IsVisible(MenuItem.Films, role), role.ToString());
            Assert.IsTrue(MenuRules.IsVisible(MenuItem.Account, role), role.ToString());
        }
    }

    [TestMethod]
    public void IsVisible_OnlyAdminSeesTheAdminButton()
    {
        foreach (Role role in ALL_ROLES)
        {
            Assert.AreEqual(role == Role.Admin, MenuRules.IsVisible(MenuItem.Admin, role), role.ToString());
        }
    }

    [TestMethod]
    public void IsVisible_OnlyAdminSeesTheTierRulesButton()
    {
        foreach (Role role in ALL_ROLES)
        {
            Assert.AreEqual(role == Role.Admin, MenuRules.IsVisible(MenuItem.TierRules, role), role.ToString());
        }
    }

    [TestMethod]
    public void IsVisible_OnlyAdminSeesTheStaffAccountsButton()
    {
        foreach (Role role in ALL_ROLES)
        {
            Assert.AreEqual(role == Role.Admin, MenuRules.IsVisible(MenuItem.StaffAccounts, role), role.ToString());
        }
    }

    [TestMethod]
    public void ItemsFor_EveryRole_StartsWithFilmsAndEndsWithAccount()
    {
        foreach (Role role in ALL_ROLES)
        {
            MenuItem[] items = MenuRules.ItemsFor(role);

            Assert.AreEqual(MenuItem.Films, items[0], role.ToString());
            Assert.AreEqual(MenuItem.Account, items[items.Length - 1], role.ToString());
        }
    }

    [TestMethod]
    public void ItemsFor_AgreesWithIsVisible_ForEveryRoleAndButton()
    {
        foreach (Role role in ALL_ROLES)
        {
            MenuItem[] items = MenuRules.ItemsFor(role);

            foreach (MenuItem item in Enum.GetValues<MenuItem>())
            {
                Assert.AreEqual(Array.IndexOf(items, item) >= 0, MenuRules.IsVisible(item, role),
                    role + " " + item);
            }
        }
    }

    [TestMethod]
    public void ItemsFor_NoButtonAppearsTwice()
    {
        foreach (Role role in ALL_ROLES)
        {
            MenuItem[] items = MenuRules.ItemsFor(role);

            Assert.AreEqual(items.Length, items.Distinct().Count(), role.ToString());
        }
    }

    [TestMethod]
    public void ItemsFor_UnknownRole_IsTreatedLikeAGuest()
    {
        CollectionAssert.AreEqual(MenuRules.ItemsFor(Role.Guest), MenuRules.ItemsFor((Role)99));
    }

    // the account button text

    [TestMethod]
    public void AccountLabel_Guest_IsSignIn()
    {
        Assert.AreEqual("Sign in", MenuRules.AccountLabel(Role.Guest));
    }

    [TestMethod]
    public void AccountLabel_SignedInRoles_AreMyAccount()
    {
        Assert.AreEqual("My account", MenuRules.AccountLabel(Role.Customer));
        Assert.AreEqual("My account", MenuRules.AccountLabel(Role.Staff));
        Assert.AreEqual("My account", MenuRules.AccountLabel(Role.Admin));
    }

    // the role at start

    [TestMethod]
    public void StartingRole_IsGuest()
    {
        Assert.AreEqual(Role.Guest, MenuRules.STARTING_ROLE);
    }

    [TestMethod]
    public void ItemsFor_StartingRole_DoesNotShowAdminOrMembers()
    {
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Admin, MenuRules.STARTING_ROLE));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Members, MenuRules.STARTING_ROLE));
    }
}
