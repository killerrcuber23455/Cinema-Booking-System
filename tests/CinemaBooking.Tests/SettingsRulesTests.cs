// tests for the settings page rules (owner: divyesh)
// written before the code for card 445, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SettingsRulesTests
{
    [TestMethod]
    public void SectionsFor_GuestAndUnknownRole_HaveNoSettings()
    {
        Assert.AreEqual(0, SettingsRules.SectionsFor(Role.Guest).Length);
        Assert.AreEqual(0, SettingsRules.SectionsFor((Role)99).Length);
    }

    [TestMethod]
    public void SectionsFor_Customer_IsAccountPasswordBookingsAndSignOut()
    {
        SettingsSection[] expected =
        {
            SettingsSection.Account, SettingsSection.Password, SettingsSection.Bookings, SettingsSection.SignOut
        };

        CollectionAssert.AreEqual(expected, SettingsRules.SectionsFor(Role.Customer));
    }

    [TestMethod]
    public void SectionsFor_Staff_IsAccountPasswordAndSignOut()
    {
        SettingsSection[] expected = { SettingsSection.Account, SettingsSection.Password, SettingsSection.SignOut };

        CollectionAssert.AreEqual(expected, SettingsRules.SectionsFor(Role.Staff));
    }

    [TestMethod]
    public void SectionsFor_Admin_HasAddAccountBetweenPasswordAndSignOut()
    {
        SettingsSection[] expected =
        {
            SettingsSection.Account, SettingsSection.Password, SettingsSection.AddAccount, SettingsSection.SignOut
        };

        CollectionAssert.AreEqual(expected, SettingsRules.SectionsFor(Role.Admin));
    }

    [TestMethod]
    public void CanAddAccounts_OnlyAdmin()
    {
        Assert.IsTrue(SettingsRules.CanAddAccounts(Role.Admin));
        Assert.IsFalse(SettingsRules.CanAddAccounts(Role.Staff));
        Assert.IsFalse(SettingsRules.CanAddAccounts(Role.Customer));
        Assert.IsFalse(SettingsRules.CanAddAccounts(Role.Guest));
        Assert.IsFalse(SettingsRules.CanAddAccounts((Role)99));
    }

    [TestMethod]
    public void SectionsFor_AddAccountAppearsExactlyWhenCanAddAccounts()
    {
        foreach (Role role in new Role[] { Role.Guest, Role.Customer, Role.Staff, Role.Admin })
        {
            bool has_part = Array.IndexOf(SettingsRules.SectionsFor(role), SettingsSection.AddAccount) >= 0;

            Assert.AreEqual(SettingsRules.CanAddAccounts(role), has_part, role.ToString());
        }
    }

    [TestMethod]
    public void SectionsFor_EverySignedInRole_EndsWithSignOut()
    {
        foreach (Role role in new Role[] { Role.Customer, Role.Staff, Role.Admin })
        {
            SettingsSection[] sections = SettingsRules.SectionsFor(role);

            Assert.IsTrue(sections.Length > 0, role.ToString());
            Assert.AreEqual(SettingsSection.SignOut, sections[sections.Length - 1], role.ToString());
        }
    }

    [TestMethod]
    public void TitleFor_EveryPart_HasItsOwnWords()
    {
        SettingsSection[] all_sections = Enum.GetValues<SettingsSection>();
        string[] titles = all_sections.Select(SettingsRules.TitleFor).ToArray();

        Assert.IsTrue(titles.All(title => title != ""));
        Assert.AreEqual(titles.Length, titles.Distinct().Count());
        Assert.AreEqual("Account details", SettingsRules.TitleFor(SettingsSection.Account));
        Assert.AreEqual("Change password", SettingsRules.TitleFor(SettingsSection.Password));
        Assert.AreEqual("My bookings", SettingsRules.TitleFor(SettingsSection.Bookings));
        Assert.AreEqual("Add an account", SettingsRules.TitleFor(SettingsSection.AddAccount));
        Assert.AreEqual("Sign out", SettingsRules.TitleFor(SettingsSection.SignOut));
    }
}
