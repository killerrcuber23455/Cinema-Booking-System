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
    public void ItemsFor_Guest_IsFilmsAndCancelBooking()
    {
        MenuItem[] expected = { MenuItem.Films, MenuItem.CancelBooking };

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
    public void ItemsFor_Customer_IsFilmsAndMyBookings()
    {
        MenuItem[] expected = { MenuItem.Films, MenuItem.MyBookings };

        CollectionAssert.AreEqual(expected, MenuRules.ItemsFor(Role.Customer));
    }

    [TestMethod]
    public void IsVisible_Customer_CannotSeeAdminMembersOrShowings()
    {
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Admin, Role.Customer));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Members, Role.Customer));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Showings, Role.Customer));
    }

    // staff

    [TestMethod]
    public void ItemsFor_Staff_HasFourButtonsAndNoAdmin()
    {
        MenuItem[] expected =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.CancelBooking, MenuItem.Members
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
    public void ItemsFor_Admin_HasSixButtonsAndNoStaffAccountsPage()
    {
        MenuItem[] expected =
        {
            MenuItem.Films, MenuItem.Showings, MenuItem.CancelBooking, MenuItem.Members,
            MenuItem.TierRules, MenuItem.Admin
        };

        CollectionAssert.AreEqual(expected, MenuRules.ItemsFor(Role.Admin));
    }

    // rules for every role

    [TestMethod]
    public void IsVisible_EveryRole_SeesFilms()
    {
        foreach (Role role in ALL_ROLES)
        {
            Assert.IsTrue(MenuRules.IsVisible(MenuItem.Films, role), role.ToString());
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
    public void MenuItem_HasNoStaffAccountsPage()
    {
        Assert.IsFalse(Enum.GetNames<MenuItem>().Contains("StaffAccounts"));
    }

    [TestMethod]
    public void ItemsFor_EveryRole_StartsWithFilms()
    {
        foreach (Role role in ALL_ROLES)
        {
            MenuItem[] items = MenuRules.ItemsFor(role);

            Assert.AreEqual(MenuItem.Films, items[0], role.ToString());
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

    // the menu has no sign in and no account button any more, the user box at the bottom does that

    [TestMethod]
    public void MenuItem_HasNoAccountAndNoBookSeatsButton()
    {
        string[] names = Enum.GetNames<MenuItem>();

        Assert.IsFalse(names.Contains("Account"));
        Assert.IsFalse(names.Contains("BookSeats"));
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
