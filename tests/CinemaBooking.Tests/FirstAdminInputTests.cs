// tests for the boxes of the "create the first admin" page (owner: divyesh)
// written before the code for card 335, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class FirstAdminInputTests
{
    private static bool Create(out FirstAdminInput? input, out string error_message,
        string email = "admin@cinema.com", string password = "secret123", string confirm_password = "secret123")
    {
        return FirstAdminInput.TryCreate(email, password, confirm_password, out input, out error_message);
    }

    [TestMethod]
    public void TryCreate_GoodValues_ReturnsTrueWithNoMessage()
    {
        bool is_good = Create(out FirstAdminInput? input, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(input);
    }

    [TestMethod]
    public void TryCreate_GoodValues_KeepsEmailAndPassword()
    {
        Create(out FirstAdminInput? input, out string error_message);

        Assert.AreEqual("admin@cinema.com", input!.Email);
        Assert.AreEqual("secret123", input.Password);
    }

    [TestMethod]
    public void TryCreate_SpacesAroundEmail_AreRemoved()
    {
        Create(out FirstAdminInput? input, out string error_message, email: " admin@cinema.com ");

        Assert.AreEqual("admin@cinema.com", input!.Email);
    }

    [TestMethod]
    public void TryCreate_BadEmail_SaysEmail()
    {
        bool is_good = Create(out FirstAdminInput? input, out string error_message, email: "admin");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "email");
    }

    [TestMethod]
    public void TryCreate_ShortPassword_SaysEightCharacters()
    {
        bool is_good = Create(out FirstAdminInput? input, out string error_message, password: "abc", confirm_password: "abc");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "password must be at least 8 characters");
    }

    [TestMethod]
    public void TryCreate_PasswordsDifferent_SaysTheyDoNotMatch()
    {
        bool is_good = Create(out FirstAdminInput? input, out string error_message, confirm_password: "secret124");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "passwords do not match");
    }

    [TestMethod]
    public void TryCreate_EverythingEmpty_ShowsEmailAndPassword()
    {
        Create(out FirstAdminInput? input, out string error_message, email: "", password: "", confirm_password: "");

        StringAssert.Contains(error_message, "email");
        StringAssert.Contains(error_message, "password must be at least 8 characters");
        Assert.AreEqual(2, error_message.Split('\n').Length);
    }

    [TestMethod]
    public void TryCreate_ShortAndDifferent_ShowsBothPasswordLines()
    {
        Create(out FirstAdminInput? input, out string error_message, password: "abc", confirm_password: "abd");

        StringAssert.Contains(error_message, "password must be at least 8 characters");
        StringAssert.Contains(error_message, "passwords do not match");
    }

    [TestMethod]
    public void TryCreate_BadValues_GiveNoInput()
    {
        Create(out FirstAdminInput? input, out string error_message, email: "");

        Assert.IsNull(input);
    }

    [TestMethod]
    public void TryCreate_NullText_IsRefusedWithoutCrashing()
    {
        bool is_good = FirstAdminInput.TryCreate(null!, null!, null!, out FirstAdminInput? input, out string error_message);

        Assert.IsFalse(is_good);
        Assert.IsNull(input);
    }

    // the message of each box, for the page to show under the box
    [TestMethod]
    public void CheckBoxes_GoodValues_EveryBoxIsEmptyText()
    {
        string[] messages = FirstAdminInput.CheckBoxes("admin@cinema.com", "secret123", "secret123");

        Assert.AreEqual(FirstAdminInput.BOX_COUNT, messages.Length);
        for (int box_index = 0; box_index < messages.Length; box_index++)
        {
            Assert.AreEqual("", messages[box_index]);
        }
    }

    [TestMethod]
    public void CheckBoxes_BadEmail_TheMessageIsUnderTheEmailBox()
    {
        string[] messages = FirstAdminInput.CheckBoxes("admin", "secret123", "secret123");

        StringAssert.Contains(messages[FirstAdminInput.BOX_EMAIL], "email");
        Assert.AreEqual("", messages[FirstAdminInput.BOX_PASSWORD]);
    }

    [TestMethod]
    public void CheckBoxes_ShortAndDifferent_PasswordBoxAndConfirmBoxEachHaveTheirOwn()
    {
        string[] messages = FirstAdminInput.CheckBoxes("admin@cinema.com", "abc", "abd");

        StringAssert.Contains(messages[FirstAdminInput.BOX_PASSWORD], "password must be at least 8 characters");
        StringAssert.Contains(messages[FirstAdminInput.BOX_CONFIRM_PASSWORD], "passwords do not match");
    }

    [TestMethod]
    public void CheckBoxes_NullText_DoesNotCrash()
    {
        string[] messages = FirstAdminInput.CheckBoxes(null!, null!, null!);

        Assert.AreEqual(3, messages.Length);
        Assert.AreNotEqual("", messages[FirstAdminInput.BOX_EMAIL]);
    }
}
