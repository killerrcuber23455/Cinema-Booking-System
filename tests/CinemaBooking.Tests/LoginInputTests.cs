// tests for the email and password boxes of the login page (owner: divyesh)
// written before the code for card 401, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class LoginInputTests
{
    [TestMethod]
    public void TryCreate_GoodValues_ReturnsTrueWithNoMessage()
    {
        bool is_good = LoginInput.TryCreate("anna@site.com", "secret123", out LoginInput? input, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(input);
    }

    [TestMethod]
    public void TryCreate_SpacesAroundEmail_AreRemoved()
    {
        LoginInput.TryCreate("  anna@site.com  ", "secret123", out LoginInput? input, out string error_message);

        Assert.AreEqual("anna@site.com", input!.Email);
    }

    [TestMethod]
    public void TryCreate_PasswordIsKeptExactly_SpacesIncluded()
    {
        LoginInput.TryCreate("anna@site.com", " secret123 ", out LoginInput? input, out string error_message);

        Assert.AreEqual(" secret123 ", input!.Password);
    }

    [TestMethod]
    public void TryCreate_ShortPassword_IsStillAllowed()
    {
        // the length rule is for new passwords, login only needs something typed
        bool is_good = LoginInput.TryCreate("anna@site.com", "abc", out LoginInput? input, out string error_message);

        Assert.IsTrue(is_good);
    }

    [TestMethod]
    public void TryCreate_EmptyEmail_SaysEmail()
    {
        bool is_good = LoginInput.TryCreate("", "secret123", out LoginInput? input, out string error_message);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "email");
    }

    [TestMethod]
    public void TryCreate_EmailWithoutAtSign_SaysEmail()
    {
        bool is_good = LoginInput.TryCreate("anna.site.com", "secret123", out LoginInput? input, out string error_message);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "email");
    }

    [TestMethod]
    public void TryCreate_EmptyPassword_SaysPassword()
    {
        bool is_good = LoginInput.TryCreate("anna@site.com", "", out LoginInput? input, out string error_message);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "password cannot be empty");
    }

    [TestMethod]
    public void TryCreate_BothEmpty_ShowsTwoLines()
    {
        LoginInput.TryCreate("", "", out LoginInput? input, out string error_message);

        Assert.AreEqual(2, error_message.Split('\n').Length);
        StringAssert.Contains(error_message, "email");
        StringAssert.Contains(error_message, "password cannot be empty");
    }

    [TestMethod]
    public void TryCreate_MessagesAreInPageOrder()
    {
        LoginInput.TryCreate("", "", out LoginInput? input, out string error_message);

        Assert.IsTrue(error_message.IndexOf("email") < error_message.IndexOf("password"));
    }

    [TestMethod]
    public void TryCreate_OnlyThePasswordWrong_ShowsOnlyOneLine()
    {
        LoginInput.TryCreate("anna@site.com", "", out LoginInput? input, out string error_message);

        Assert.AreEqual(1, error_message.Split('\n').Length);
    }

    [TestMethod]
    public void TryCreate_BadValues_GiveNoInput()
    {
        LoginInput.TryCreate("", "", out LoginInput? input, out string error_message);

        Assert.IsNull(input);
    }

    [TestMethod]
    public void TryCreate_NullText_IsRefusedWithoutCrashing()
    {
        bool is_good = LoginInput.TryCreate(null!, null!, out LoginInput? input, out string error_message);

        Assert.IsFalse(is_good);
        Assert.IsNull(input);
    }

    // the message of each box, for the page to show under the box
    [TestMethod]
    public void CheckBoxes_GoodValues_EveryBoxIsEmptyText()
    {
        string[] messages = LoginInput.CheckBoxes("anna@site.com", "secret123");

        Assert.AreEqual(LoginInput.BOX_COUNT, messages.Length);
        Assert.AreEqual("", messages[LoginInput.BOX_EMAIL]);
        Assert.AreEqual("", messages[LoginInput.BOX_PASSWORD]);
    }

    [TestMethod]
    public void CheckBoxes_BothEmpty_EachMessageIsInItsOwnBox()
    {
        string[] messages = LoginInput.CheckBoxes("", "");

        StringAssert.Contains(messages[LoginInput.BOX_EMAIL], "email");
        StringAssert.Contains(messages[LoginInput.BOX_PASSWORD], "password cannot be empty");
    }

    [TestMethod]
    public void CheckBoxes_OnlyThePasswordWrong_EmailBoxStaysEmptyText()
    {
        string[] messages = LoginInput.CheckBoxes("anna@site.com", "");

        Assert.AreEqual("", messages[LoginInput.BOX_EMAIL]);
        Assert.AreNotEqual("", messages[LoginInput.BOX_PASSWORD]);
    }

    [TestMethod]
    public void CheckBoxes_NullText_DoesNotCrash()
    {
        string[] messages = LoginInput.CheckBoxes(null!, null!);

        Assert.AreEqual(2, messages.Length);
        Assert.AreNotEqual("", messages[LoginInput.BOX_EMAIL]);
    }
}
