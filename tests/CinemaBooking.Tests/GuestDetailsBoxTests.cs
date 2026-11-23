// tests for the one message per box of the guest details (owner: divyesh)
// written before the code for card 434, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class GuestDetailsBoxTests
{
    [TestMethod]
    public void CheckBoxes_GoodDetails_AllMessagesAreEmpty()
    {
        string[] messages = GuestDetails.CheckBoxes("Anna Smith", "anna@example.com", "57123456");

        Assert.AreEqual(GuestDetails.BOX_COUNT, messages.Length);
        Assert.AreEqual("", messages[GuestDetails.BOX_NAME]);
        Assert.AreEqual("", messages[GuestDetails.BOX_EMAIL]);
        Assert.AreEqual("", messages[GuestDetails.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_EmptyName_OnlyTheNameBoxHasAMessage()
    {
        string[] messages = GuestDetails.CheckBoxes("", "anna@example.com", "57123456");

        Booking.IsValidGuestName(null, "", out string expected);
        Assert.AreEqual(expected.TrimEnd('\n'), messages[GuestDetails.BOX_NAME]);
        Assert.AreEqual("", messages[GuestDetails.BOX_EMAIL]);
        Assert.AreEqual("", messages[GuestDetails.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_BadEmail_OnlyTheEmailBoxHasAMessage()
    {
        string[] messages = GuestDetails.CheckBoxes("Anna Smith", "not an email", "57123456");

        Assert.AreEqual("", messages[GuestDetails.BOX_NAME]);
        Assert.AreNotEqual("", messages[GuestDetails.BOX_EMAIL]);
        Assert.AreEqual("", messages[GuestDetails.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_BadPhone_OnlyThePhoneBoxHasAMessage()
    {
        string[] messages = GuestDetails.CheckBoxes("Anna Smith", "anna@example.com", "abc");

        Assert.AreEqual("", messages[GuestDetails.BOX_NAME]);
        Assert.AreEqual("", messages[GuestDetails.BOX_EMAIL]);
        Assert.AreNotEqual("", messages[GuestDetails.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_SpacesAroundGoodText_AreIgnored()
    {
        string[] messages = GuestDetails.CheckBoxes("  Anna Smith ", " anna@example.com ", " 57123456 ");

        Assert.AreEqual("", messages[GuestDetails.BOX_NAME]);
        Assert.AreEqual("", messages[GuestDetails.BOX_EMAIL]);
        Assert.AreEqual("", messages[GuestDetails.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_AllNull_EveryBoxHasAMessageAndNothingCrashes()
    {
        string[] messages = GuestDetails.CheckBoxes(null!, null!, null!);

        Assert.AreNotEqual("", messages[GuestDetails.BOX_NAME]);
        Assert.AreNotEqual("", messages[GuestDetails.BOX_EMAIL]);
        Assert.AreNotEqual("", messages[GuestDetails.BOX_PHONE]);
    }

    [TestMethod]
    public void TryCreate_TwoBadBoxes_JoinsTheirMessagesInPageOrder()
    {
        bool is_good = GuestDetails.TryCreate("", "anna@example.com", "abc", out _, out string error_message);

        string[] box_messages = GuestDetails.CheckBoxes("", "anna@example.com", "abc");
        Assert.IsFalse(is_good);
        Assert.AreEqual(box_messages[GuestDetails.BOX_NAME] + "\n" + box_messages[GuestDetails.BOX_PHONE], error_message);
    }
}
