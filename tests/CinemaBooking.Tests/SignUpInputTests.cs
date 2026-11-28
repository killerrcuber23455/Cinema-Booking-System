// tests for the boxes of the member sign up page (owner: divyesh)
// written before the code for card 345, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SignUpInputTests
{
    // a good set of values, each test changes only the one it is about
    private static bool Create(out SignUpInput? input, out string error_message,
        string name = "Anna Smith", string email = "anna@site.com", string phone = "5712 3456",
        string password = "secret123", string confirm_password = "secret123")
    {
        return SignUpInput.TryCreate(name, email, phone, password, confirm_password, out input, out error_message);
    }

    [TestMethod]
    public void TryCreate_GoodValues_ReturnsTrueWithNoMessage()
    {
        bool is_good = Create(out SignUpInput? input, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(input);
    }

    [TestMethod]
    public void TryCreate_GoodValues_KeepsEveryField()
    {
        Create(out SignUpInput? input, out string error_message);

        Assert.AreEqual("Anna Smith", input!.Name);
        Assert.AreEqual("anna@site.com", input.Email);
        Assert.AreEqual("5712 3456", input.Phone);
        Assert.AreEqual("secret123", input.Password);
    }

    [TestMethod]
    public void TryCreate_SpacesAroundNameEmailPhone_AreRemoved()
    {
        Create(out SignUpInput? input, out string error_message,
            name: "  Anna Smith ", email: " anna@site.com ", phone: " 5712 3456 ");

        Assert.AreEqual("Anna Smith", input!.Name);
        Assert.AreEqual("anna@site.com", input.Email);
        Assert.AreEqual("5712 3456", input.Phone);
    }

    [TestMethod]
    public void TryCreate_PasswordIsKeptExactly()
    {
        Create(out SignUpInput? input, out string error_message, password: " secret123", confirm_password: " secret123");

        Assert.AreEqual(" secret123", input!.Password);
    }

    [TestMethod]
    public void TryCreate_EmptyName_SaysName()
    {
        bool is_good = Create(out SignUpInput? input, out string error_message, name: "");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "name");
    }

    [TestMethod]
    public void TryCreate_BadEmail_SaysEmail()
    {
        bool is_good = Create(out SignUpInput? input, out string error_message, email: "anna.site.com");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "email");
    }

    [TestMethod]
    public void TryCreate_BadPhone_SaysPhone()
    {
        bool is_good = Create(out SignUpInput? input, out string error_message, phone: "12");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "phone");
    }

    [TestMethod]
    public void TryCreate_ShortPassword_SaysEightCharacters()
    {
        bool is_good = Create(out SignUpInput? input, out string error_message, password: "abc", confirm_password: "abc");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "password must be at least 8 characters");
    }

    [TestMethod]
    public void TryCreate_PasswordsDifferent_SaysTheyDoNotMatch()
    {
        bool is_good = Create(out SignUpInput? input, out string error_message, confirm_password: "secret124");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "passwords do not match");
    }

    [TestMethod]
    public void TryCreate_PasswordsDifferentByOneCapital_DoNotMatch()
    {
        bool is_good = Create(out SignUpInput? input, out string error_message, password: "Secret123", confirm_password: "secret123");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "passwords do not match");
    }

    [TestMethod]
    public void TryCreate_EverythingEmpty_ShowsEveryBox()
    {
        Create(out SignUpInput? input, out string error_message, name: "", email: "", phone: "",
            password: "", confirm_password: "");

        StringAssert.Contains(error_message, "name");
        StringAssert.Contains(error_message, "email");
        StringAssert.Contains(error_message, "phone");
        StringAssert.Contains(error_message, "password must be at least 8 characters");
    }

    [TestMethod]
    public void TryCreate_EverythingEmpty_GivesOneLinePerWrongBox()
    {
        // two empty passwords are equal, so there is no "do not match" line, name, email, phone and password are four lines
        Create(out SignUpInput? input, out string error_message, name: "", email: "", phone: "",
            password: "", confirm_password: "");

        Assert.AreEqual(4, error_message.Split('\n').Length);
    }

    [TestMethod]
    public void TryCreate_MessagesAreInPageOrder()
    {
        Create(out SignUpInput? input, out string error_message, name: "", email: "", phone: "",
            password: "abc", confirm_password: "abd");

        int name_place = error_message.IndexOf("name");
        int email_place = error_message.IndexOf("email");
        int phone_place = error_message.IndexOf("phone");
        int password_place = error_message.IndexOf("password must be at least 8 characters");
        int match_place = error_message.IndexOf("passwords do not match");

        Assert.IsTrue(name_place < email_place);
        Assert.IsTrue(email_place < phone_place);
        Assert.IsTrue(phone_place < password_place);
        Assert.IsTrue(password_place < match_place);
    }

    [TestMethod]
    public void TryCreate_OnlyTheEmailWrong_ShowsOnlyOneLine()
    {
        Create(out SignUpInput? input, out string error_message, email: "anna.site.com");

        Assert.AreEqual(1, error_message.Split('\n').Length);
    }

    [TestMethod]
    public void TryCreate_BadValues_GiveNoInput()
    {
        Create(out SignUpInput? input, out string error_message, name: "");

        Assert.IsNull(input);
    }

    [TestMethod]
    public void TryCreate_NullText_IsRefusedWithoutCrashing()
    {
        bool is_good = SignUpInput.TryCreate(null!, null!, null!, null!, null!, out SignUpInput? input, out string error_message);

        Assert.IsFalse(is_good);
        Assert.IsNull(input);
    }

    // the message of each box, for the page to show under the box
    [TestMethod]
    public void CheckBoxes_GoodValues_EveryBoxIsEmptyText()
    {
        string[] messages = SignUpInput.CheckBoxes("Anna Smith", "anna@site.com", "5712 3456", "secret123", "secret123");

        Assert.AreEqual(SignUpInput.BOX_COUNT, messages.Length);
        for (int box_index = 0; box_index < messages.Length; box_index++)
        {
            Assert.AreEqual("", messages[box_index]);
        }
    }

    [TestMethod]
    public void CheckBoxes_EverythingEmpty_EachMessageIsInItsOwnBox()
    {
        string[] messages = SignUpInput.CheckBoxes("", "", "", "", "");

        StringAssert.Contains(messages[SignUpInput.BOX_NAME], "name");
        StringAssert.Contains(messages[SignUpInput.BOX_EMAIL], "email");
        StringAssert.Contains(messages[SignUpInput.BOX_PHONE], "phone");
        StringAssert.Contains(messages[SignUpInput.BOX_PASSWORD], "password must be at least 8 characters");
    }

    [TestMethod]
    public void CheckBoxes_PasswordsDifferent_TheMessageIsUnderTheConfirmBox()
    {
        string[] messages = SignUpInput.CheckBoxes("Anna Smith", "anna@site.com", "5712 3456", "secret123", "secret124");

        StringAssert.Contains(messages[SignUpInput.BOX_CONFIRM_PASSWORD], "passwords do not match");
        Assert.AreEqual("", messages[SignUpInput.BOX_PASSWORD]);
    }

    [TestMethod]
    public void CheckBoxes_ShortAndDifferent_PasswordBoxAndConfirmBoxEachHaveTheirOwn()
    {
        string[] messages = SignUpInput.CheckBoxes("Anna Smith", "anna@site.com", "5712 3456", "abc", "abd");

        StringAssert.Contains(messages[SignUpInput.BOX_PASSWORD], "password must be at least 8 characters");
        StringAssert.Contains(messages[SignUpInput.BOX_CONFIRM_PASSWORD], "passwords do not match");
    }

    [TestMethod]
    public void CheckBoxes_OnlyTheEmailWrong_OnlyTheEmailBoxHasAMessage()
    {
        string[] messages = SignUpInput.CheckBoxes("Anna Smith", "anna.site.com", "5712 3456", "secret123", "secret123");

        for (int box_index = 0; box_index < messages.Length; box_index++)
        {
            if (box_index == SignUpInput.BOX_EMAIL)
            {
                Assert.AreNotEqual("", messages[box_index]);
            }
            else
            {
                Assert.AreEqual("", messages[box_index]);
            }
        }
    }

    [TestMethod]
    public void CheckBoxes_NullText_DoesNotCrash()
    {
        string[] messages = SignUpInput.CheckBoxes(null!, null!, null!, null!, null!);

        Assert.AreEqual(5, messages.Length);
        Assert.AreNotEqual("", messages[SignUpInput.BOX_NAME]);
    }
}
