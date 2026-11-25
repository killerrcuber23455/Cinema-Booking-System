// tests for what a click on the user box does (owner: divyesh)
// written before the code for card 445, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class UserIconRulesTests
{
    [TestMethod]
    public void ActionFor_Guest_IsSignIn()
    {
        Assert.AreEqual(UserIconAction.SignIn, UserIconRules.ActionFor(Role.Guest));
        Assert.AreEqual(UserIconAction.SignIn, UserIconRules.ActionFor((Role)99));
    }

    [TestMethod]
    public void ActionFor_Customer_OpensSettings()
    {
        Assert.AreEqual(UserIconAction.OpenSettings, UserIconRules.ActionFor(Role.Customer));
    }

    [TestMethod]
    public void ActionFor_Staff_OpensSettings()
    {
        Assert.AreEqual(UserIconAction.OpenSettings, UserIconRules.ActionFor(Role.Staff));
    }

    [TestMethod]
    public void ActionFor_Admin_OpensSettings()
    {
        Assert.AreEqual(UserIconAction.OpenSettings, UserIconRules.ActionFor(Role.Admin));
    }
}
