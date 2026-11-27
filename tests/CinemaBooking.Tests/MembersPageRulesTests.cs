// tests for the small rules of the Members page, card 342 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class MembersPageRulesTests
{
    private static Member Make(int points, Tier tier = Tier.Standard)
    {
        return new Member(7, "Ann Lee", "ann@x.com", "5712 3456", tier, points, points, new DateTime(2026, 11, 1));
    }

    // the line of one member in the list

    [TestMethod]
    public void Line_ShowsNameEmailPhoneTierAndPoints()
    {
        string text = MemberLine.Text(Make(120, Tier.Silver));

        StringAssert.Contains(text, "Ann Lee");
        StringAssert.Contains(text, "ann@x.com");
        StringAssert.Contains(text, "5712 3456");
        StringAssert.Contains(text, "Silver");
        StringAssert.Contains(text, "120 points");
    }

    [TestMethod]
    public void Line_OnePoint_SaysPointNotPoints()
    {
        string text = MemberLine.Text(Make(1));

        StringAssert.Contains(text, "1 point");
        Assert.DoesNotContain("1 points", text);
    }

    [TestMethod]
    public void Line_NoPoints_SaysZeroPoints()
    {
        StringAssert.Contains(MemberLine.Text(Make(0)), "0 points");
    }

    // the three boxes of the register form, one message for each box

    [TestMethod]
    public void CheckBoxes_AllGood_GiveNoMessages()
    {
        string[] messages = MemberInput.CheckBoxes("Ann Lee", "ann@x.com", "5712 3456");

        Assert.HasCount(MemberInput.BOX_COUNT, messages);
        Assert.AreEqual("", messages[MemberInput.BOX_NAME]);
        Assert.AreEqual("", messages[MemberInput.BOX_EMAIL]);
        Assert.AreEqual("", messages[MemberInput.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_EmailWithSpacesAround_IsAccepted()
    {
        string[] messages = MemberInput.CheckBoxes("Ann Lee", "  ann@x.com  ", "5712 3456");

        Assert.AreEqual("", messages[MemberInput.BOX_EMAIL]);
    }

    [TestMethod]
    public void CheckBoxes_EmptyName_OnlyTheNameBoxIsWrong()
    {
        string[] messages = MemberInput.CheckBoxes("", "ann@x.com", "5712 3456");

        Assert.AreNotEqual("", messages[MemberInput.BOX_NAME]);
        Assert.AreEqual("", messages[MemberInput.BOX_EMAIL]);
        Assert.AreEqual("", messages[MemberInput.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_BadEmail_OnlyTheEmailBoxIsWrong()
    {
        string[] messages = MemberInput.CheckBoxes("Ann Lee", "not an email", "5712 3456");

        Assert.AreEqual("", messages[MemberInput.BOX_NAME]);
        Assert.AreNotEqual("", messages[MemberInput.BOX_EMAIL]);
        Assert.AreEqual("", messages[MemberInput.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_BadPhone_OnlyThePhoneBoxIsWrong()
    {
        string[] messages = MemberInput.CheckBoxes("Ann Lee", "ann@x.com", "abc");

        Assert.AreEqual("", messages[MemberInput.BOX_NAME]);
        Assert.AreEqual("", messages[MemberInput.BOX_EMAIL]);
        Assert.AreNotEqual("", messages[MemberInput.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_Nulls_AreTreatedAsEmpty()
    {
        string[] messages = MemberInput.CheckBoxes(null!, null!, null!);

        Assert.AreNotEqual("", messages[MemberInput.BOX_NAME]);
        Assert.AreNotEqual("", messages[MemberInput.BOX_EMAIL]);
        Assert.AreNotEqual("", messages[MemberInput.BOX_PHONE]);
    }

    [TestMethod]
    public void CheckBoxes_MessagesHaveNoLineBreakAtTheEnd()
    {
        string[] messages = MemberInput.CheckBoxes("", "", "");

        for (int index = 0; index < messages.Length; index++)
        {
            Assert.IsFalse(messages[index].EndsWith("\n"));
        }
    }

    // the page that opens from the menu

    [TestMethod]
    public void FindText_EmptyEmail_AsksForAnEmail()
    {
        Assert.AreEqual("Type an email to find a member", MemberLine.FindHint(""));
        Assert.AreEqual("Type an email to find a member", MemberLine.FindHint("   "));
        Assert.AreEqual("", MemberLine.FindHint("ann@x.com"));
    }
}
