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

    // several wrong at once: the user sees every problem together, one message per line
    [TestMethod]
    public void TryCreate_SeveralWrong_ShowsAllMessagesAtOnce()
    {
        GuestDetails.TryCreate("", "wrong", "abc", out GuestDetails? details, out string error_message);

        StringAssert.Contains(error_message, "name");
        StringAssert.Contains(error_message, "email");
        StringAssert.Contains(error_message, "phone");
    }

    [TestMethod]
    public void TryCreate_AllThreeEmpty_ShowsThreeLines()
    {
        GuestDetails.TryCreate("", "", "", out GuestDetails? details, out string error_message);

        string[] lines = error_message.Split('\n');

        Assert.AreEqual(3, lines.Length);
    }

    [TestMethod]
    public void TryCreate_SeveralWrong_MessagesAreInPageOrder()
    {
        // the page order is name, email, phone, so the lines follow the boxes from the top
        GuestDetails.TryCreate("", "", "", out GuestDetails? details, out string error_message);

        int name_place = error_message.IndexOf("name");
        int email_place = error_message.IndexOf("email");
        int phone_place = error_message.IndexOf("phone");

        Assert.IsTrue(name_place >= 0 && name_place < email_place);
        Assert.IsTrue(email_place < phone_place);
    }

    [TestMethod]
    public void TryCreate_OnlyTheEmailWrong_ShowsOnlyOneLine()
    {
        GuestDetails.TryCreate("Anna Smith", "wrong", "57123456", out GuestDetails? details, out string error_message);

        Assert.AreEqual(1, error_message.Split('\n').Length);
        StringAssert.Contains(error_message, "email");
    }

    [TestMethod]
    public void TryCreate_NullText_IsRefusedWithoutCrashing()
    {
        bool is_good = GuestDetails.TryCreate(null!, null!, null!, out GuestDetails? details, out string error_message);

        Assert.IsFalse(is_good);
    }
}
