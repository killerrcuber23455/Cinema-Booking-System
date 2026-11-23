// tests for when the landing page is shown, card 345 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class LandingRulesTests
{
    [TestMethod]
    public void ShouldShow_GuestWhoHasNotChosenYet_ReturnsTrue()
    {
        Assert.IsTrue(LandingRules.ShouldShow(Role.Guest, false));
    }

    [TestMethod]
    public void ShouldShow_GuestWhoAlreadyChose_ReturnsFalse()
    {
        Assert.IsFalse(LandingRules.ShouldShow(Role.Guest, true));
    }

    [TestMethod]
    public void ShouldShow_SignedInCustomer_ReturnsFalse()
    {
        Assert.IsFalse(LandingRules.ShouldShow(Role.Customer, false));
    }

    [TestMethod]
    public void ShouldShow_Staff_ReturnsFalse()
    {
        Assert.IsFalse(LandingRules.ShouldShow(Role.Staff, false));
    }

    [TestMethod]
    public void ShouldShow_Admin_ReturnsFalse()
    {
        Assert.IsFalse(LandingRules.ShouldShow(Role.Admin, false));
    }

    [TestMethod]
    public void ShouldShow_TheStartingRoleSeesItOnFirstOpen()
    {
        Assert.IsTrue(LandingRules.ShouldShow(MenuRules.STARTING_ROLE, false));
    }
}
