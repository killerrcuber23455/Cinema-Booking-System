// tests for what the landing window leads to, card 345 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class LandingRulesTests
{
    [TestMethod]
    public void ShouldOpenMain_WindowClosedWithoutAChoice_ReturnsFalse()
    {
        Assert.IsFalse(LandingRules.ShouldOpenMain(LandingChoice.None));
    }

    [TestMethod]
    public void ShouldOpenMain_EveryRealChoice_ReturnsTrue()
    {
        Assert.IsTrue(LandingRules.ShouldOpenMain(LandingChoice.Guest));
        Assert.IsTrue(LandingRules.ShouldOpenMain(LandingChoice.LogIn));
        Assert.IsTrue(LandingRules.ShouldOpenMain(LandingChoice.SignUp));
        Assert.IsTrue(LandingRules.ShouldOpenMain(LandingChoice.Staff));
    }

    [TestMethod]
    public void FirstPageFor_Guest_IsTheFilms()
    {
        Assert.AreEqual(FirstPage.Films, LandingRules.FirstPageFor(LandingChoice.Guest));
    }

    [TestMethod]
    public void FirstPageFor_LogIn_IsTheLoginPage()
    {
        Assert.AreEqual(FirstPage.Login, LandingRules.FirstPageFor(LandingChoice.LogIn));
    }

    [TestMethod]
    public void FirstPageFor_SignUp_IsTheSignUpPage()
    {
        Assert.AreEqual(FirstPage.SignUp, LandingRules.FirstPageFor(LandingChoice.SignUp));
    }

    [TestMethod]
    public void FirstPageFor_Staff_IsTheStaffLoginPage()
    {
        Assert.AreEqual(FirstPage.StaffLogin, LandingRules.FirstPageFor(LandingChoice.Staff));
    }

    [TestMethod]
    public void StaffNotice_HasATextForTheStaffLoginPage()
    {
        Assert.AreNotEqual("", LandingRules.STAFF_NOTICE);
    }
}
