// tests for the menu of the user box at the bottom of the side menu (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class UserMenuRulesTests
{
    private static readonly Role[] ALL_ROLES = { Role.Guest, Role.Customer, Role.Staff, Role.Admin };

    [TestMethod]
    public void ItemsFor_Guest_IsOnlySignIn()
    {
        UserMenuItem[] expected = { UserMenuItem.SignIn };

        CollectionAssert.AreEqual(expected, UserMenuRules.ItemsFor(Role.Guest));
    }

    [TestMethod]
    public void ItemsFor_Customer_IsAccountDetailsMyBookingsAndSignOut()
    {
        UserMenuItem[] expected = { UserMenuItem.AccountDetails, UserMenuItem.MyBookings, UserMenuItem.SignOut };

        CollectionAssert.AreEqual(expected, UserMenuRules.ItemsFor(Role.Customer));
    }

    [TestMethod]
    public void ItemsFor_Staff_IsAccountDetailsAndSignOut()
    {
        UserMenuItem[] expected = { UserMenuItem.AccountDetails, UserMenuItem.SignOut };

        CollectionAssert.AreEqual(expected, UserMenuRules.ItemsFor(Role.Staff));
    }

    [TestMethod]
    public void ItemsFor_Admin_IsTheSameAsStaff()
    {
        CollectionAssert.AreEqual(UserMenuRules.ItemsFor(Role.Staff), UserMenuRules.ItemsFor(Role.Admin));
    }

    [TestMethod]
    public void ItemsFor_UnknownRole_IsTreatedLikeAGuest()
    {
        CollectionAssert.AreEqual(UserMenuRules.ItemsFor(Role.Guest), UserMenuRules.ItemsFor((Role)99));
    }

    [TestMethod]
    public void ItemsFor_OnlySignedInRolesCanSignOut()
    {
        foreach (Role role in ALL_ROLES)
        {
            bool has_sign_out = Array.IndexOf(UserMenuRules.ItemsFor(role), UserMenuItem.SignOut) >= 0;

            Assert.AreEqual(role != Role.Guest, has_sign_out, role.ToString());
        }
    }

    [TestMethod]
    public void LabelFor_EveryEntry_HasItsOwnText()
    {
        UserMenuItem[] all_items = Enum.GetValues<UserMenuItem>();
        string[] labels = all_items.Select(UserMenuRules.LabelFor).ToArray();

        foreach (string label in labels)
        {
            Assert.AreNotEqual("", label);
        }

        Assert.AreEqual(labels.Length, labels.Distinct().Count());
    }

    [TestMethod]
    public void LabelFor_GivesTheTextsOfTheMenu()
    {
        Assert.AreEqual("Sign in", UserMenuRules.LabelFor(UserMenuItem.SignIn));
        Assert.AreEqual("Account details", UserMenuRules.LabelFor(UserMenuItem.AccountDetails));
        Assert.AreEqual("My bookings", UserMenuRules.LabelFor(UserMenuItem.MyBookings));
        Assert.AreEqual("Sign out", UserMenuRules.LabelFor(UserMenuItem.SignOut));
    }
}
