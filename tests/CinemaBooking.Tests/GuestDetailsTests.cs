// tests for the guest name, email and phone read from the booking summary page (owner: divyesh)
// written before the code for card 403, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class GuestDetailsTests
{
    // good details
    [TestMethod]
    public void TryCreate_GoodDetails_ReturnsTrueWithNoMessage()
    {
        bool is_good = GuestDetails.TryCreate("Anna Smith", "anna@example.com", "57123456", out GuestDetails? details, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(details);
    }

    [TestMethod]
    public void TryCreate_GoodDetails_KeepsTheValues()
    {
        GuestDetails.TryCreate("Anna Smith", "anna@example.com", "57123456", out GuestDetails? details, out string error_message);

        Assert.AreEqual("Anna Smith", details!.Name);
        Assert.AreEqual("anna@example.com", details.Email);
        Assert.AreEqual("57123456", details.Phone);
    }

    [TestMethod]
    public void TryCreate_SpacesAroundTheText_AreRemoved()
    {
        GuestDetails.TryCreate("  Anna Smith ", " anna@example.com  ", " 57123456 ", out GuestDetails? details, out string error_message);

        Assert.AreEqual("Anna Smith", details!.Name);
        Assert.AreEqual("anna@example.com", details.Email);
        Assert.AreEqual("57123456", details.Phone);
    }

    [TestMethod]
    public void TryCreate_PhoneWithPlusAndSpaces_IsAccepted()
    {
        bool is_good = GuestDetails.TryCreate("Anna Smith", "anna@example.com", "+230 5712 3456", out GuestDetails? details, out string error_message);

        Assert.IsTrue(is_good);
    }

    // bad name
    [TestMethod]
    public void TryCreate_EmptyName_ReturnsFalseAndSaysName()
    {
        bool is_good = GuestDetails.TryCreate("", "anna@example.com", "57123456", out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "name");
    }

    [TestMethod]
    public void TryCreate_NameOfOnlySpaces_ReturnsFalse()
    {
        bool is_good = GuestDetails.TryCreate("     ", "anna@example.com", "57123456", out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
    }

    // bad email
    [TestMethod]
    public void TryCreate_EmptyEmail_ReturnsFalseAndSaysEmail()
    {
        bool is_good = GuestDetails.TryCreate("Anna Smith", "", "57123456", out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "email");
    }

    [TestMethod]
    public void TryCreate_EmailWithoutAt_ReturnsFalse()
    {
        bool is_good = GuestDetails.TryCreate("Anna Smith", "anna.example.com", "57123456", out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "name@site.com");
    }

    // bad phone
    [TestMethod]
    public void TryCreate_PhoneWithLetters_ReturnsFalseAndSaysPhone()
    {
        bool is_good = GuestDetails.TryCreate("Anna Smith", "anna@example.com", "57abc456", out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "phone");
    }

    [TestMethod]
    public void TryCreate_PhoneTooShort_ReturnsFalse()
    {
        bool is_good = GuestDetails.TryCreate("Anna Smith", "anna@example.com", "123", out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
    }

    // what comes back when it fails
    [TestMethod]
    public void TryCreate_BadDetails_GivesNoDetails()
    {
        GuestDetails.TryCreate("", "anna@example.com", "57123456", out GuestDetails? details, out string error_message);

        Assert.IsNull(details);
    }

    [TestMethod]
    public void TryCreate_SeveralWrong_SaysTheFirstOneInPageOrder()
    {
        // the page order is name, email, phone, so the user fixes the top box first
        GuestDetails.TryCreate("", "wrong", "abc", out GuestDetails? details, out string error_message);

        StringAssert.Contains(error_message, "name");
    }

    [TestMethod]
    public void TryCreate_NullText_IsRefusedWithoutCrashing()
    {
        bool is_good = GuestDetails.TryCreate(null!, null!, null!, out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
    }
}
