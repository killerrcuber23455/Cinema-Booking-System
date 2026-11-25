// tests for what happens after a guest signs in during a booking (owner: divyesh)
// written before the code for card 445, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class ResumeRulesTests
{
    private static Member MakeMember()
    {
        return new Member(1, "Ann", "ann@x.com", "5712345", Tier.Standard, 0, 0, new DateTime(2026, 11, 1));
    }

    [TestMethod]
    public void PageAfterSignIn_CustomerWithAPendingBooking_OpensTheSummaryAgain()
    {
        Assert.AreEqual(ResumePage.BookingSummary, ResumeRules.PageAfterSignIn(Role.Customer, MakeMember(), true));
    }

    [TestMethod]
    public void PageAfterSignIn_CustomerWithNoPendingBooking_OpensFilms()
    {
        Assert.AreEqual(ResumePage.Films, ResumeRules.PageAfterSignIn(Role.Customer, MakeMember(), false));
    }

    [TestMethod]
    public void PageAfterSignIn_Staff_OpensFilmsEvenWithAPendingBooking()
    {
        Assert.AreEqual(ResumePage.Films, ResumeRules.PageAfterSignIn(Role.Staff, null, true));
    }

    [TestMethod]
    public void PageAfterSignIn_Admin_OpensFilmsEvenWithAPendingBooking()
    {
        Assert.AreEqual(ResumePage.Films, ResumeRules.PageAfterSignIn(Role.Admin, null, true));
    }

    [TestMethod]
    public void PageAfterSignIn_CustomerWithoutAMember_OpensFilms()
    {
        Assert.AreEqual(ResumePage.Films, ResumeRules.PageAfterSignIn(Role.Customer, null, true));
    }

    [TestMethod]
    public void SignedInNotice_ForTheSummary_SaysTheSeatsAreKept()
    {
        Assert.AreEqual("Signed in as ann@x.com, your seats are kept", ResumeRules.SignedInNotice(ResumePage.BookingSummary, "ann@x.com"));
    }

    [TestMethod]
    public void SignedInNotice_ForFilms_OnlySaysWhoSignedIn()
    {
        Assert.AreEqual("Signed in as ann@x.com", ResumeRules.SignedInNotice(ResumePage.Films, "ann@x.com"));
    }
}
