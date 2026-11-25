// tests for what the landing window leads to (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
// since the sign in moved into the landing window the window gives back an outcome, not only a button
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class LandingRulesTests
{
    [TestMethod]
    public void ShouldOpenMain_WindowClosedWithoutAChoice_ReturnsFalse()
    {
        Assert.IsFalse(LandingRules.ShouldOpenMain(LandingOutcome.Closed()));
    }

    [TestMethod]
    public void ShouldOpenMain_Guest_ReturnsTrue()
    {
        Assert.IsTrue(LandingRules.ShouldOpenMain(LandingOutcome.Guest()));
    }

    [TestMethod]
    public void ShouldOpenMain_SignedIn_ReturnsTrue()
    {
        Assert.IsTrue(LandingRules.ShouldOpenMain(LandingOutcome.SignedIn(Role.Customer, null, "ann@x.com")));
    }

    [TestMethod]
    public void Closed_HasTheClosedKind()
    {
        Assert.AreEqual(LandingKind.Closed, LandingOutcome.Closed().Kind);
    }

    [TestMethod]
    public void Guest_IsAGuestWithNoMemberAndNoEmail()
    {
        LandingOutcome outcome = LandingOutcome.Guest();

        Assert.AreEqual(LandingKind.Guest, outcome.Kind);
        Assert.AreEqual(Role.Guest, outcome.Role);
        Assert.IsNull(outcome.Member);
        Assert.AreEqual("", outcome.Email);
    }

    [TestMethod]
    public void SignedIn_KeepsTheRoleAndTheEmail()
    {
        LandingOutcome outcome = LandingOutcome.SignedIn(Role.Staff, null, "desk@cinema.com");

        Assert.AreEqual(LandingKind.SignedIn, outcome.Kind);
        Assert.AreEqual(Role.Staff, outcome.Role);
        Assert.IsNull(outcome.Member);
        Assert.AreEqual("desk@cinema.com", outcome.Email);
    }
}
