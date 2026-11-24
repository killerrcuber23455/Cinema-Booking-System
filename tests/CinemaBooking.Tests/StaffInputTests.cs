// tests for the boxes of the "create staff or admin account" page (owner: divyesh)
// written before the code for card 436, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class StaffInputTests
{
    private static bool Create(out StaffInput? input, out string error_message,
        string email = "clerk@cinema.com", Role? role = Role.Staff, string password = "secret123", string confirm_password = "secret123")
    {
        return StaffInput.TryCreate(email, role, password, confirm_password, out input, out error_message);
    }

    [TestMethod]
    public void TryCreate_GoodValues_ReturnsTrueWithNoMessage()
    {
        bool is_good = Create(out StaffInput? input, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(input);
    }

    [TestMethod]
    public void TryCreate_GoodValues_KeepsEmailRoleAndPassword()
    {
        Create(out StaffInput? input, out string error_message);

        Assert.AreEqual("clerk@cinema.com", input!.Email);
        Assert.AreEqual(Role.Staff, input.NewRole);
        Assert.AreEqual("secret123", input.Password);
    }

    [TestMethod]
    public void TryCreate_SpacesAroundEmail_AreRemoved()
    {
        Create(out StaffInput? input, out string error_message, email: " clerk@cinema.com ");

        Assert.AreEqual("clerk@cinema.com", input!.Email);
    }

    [TestMethod]
    public void TryCreate_AdminRole_IsAccepted()
    {
        bool is_good = Create(out StaffInput? input, out string error_message, role: Role.Admin);

        Assert.IsTrue(is_good);
        Assert.AreEqual(Role.Admin, input!.NewRole);
    }

    [TestMethod]
    public void TryCreate_BadEmail_SaysEmail()
    {
        bool is_good = Create(out StaffInput? input, out string error_message, email: "clerk");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "email");
    }

    [TestMethod]
    public void TryCreate_NoRoleChosen_SaysRole()
    {
        bool is_good = Create(out StaffInput? input, out string error_message, role: null);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "role must be Staff or Admin");
    }

    [TestMethod]
    public void TryCreate_CustomerRole_IsRefused()
    {
        bool is_good = Create(out StaffInput? input, out string error_message, role: Role.Customer);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "role must be Staff or Admin");
    }

    [TestMethod]
    public void TryCreate_GuestRole_IsRefused()
    {
        bool is_good = Create(out StaffInput? input, out string error_message, role: Role.Guest);

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "role must be Staff or Admin");
    }

    [TestMethod]
    public void TryCreate_ShortPassword_SaysEightCharacters()
    {
        bool is_good = Create(out StaffInput? input, out string error_message, password: "abc", confirm_password: "abc");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "password must be at least 8 characters");
    }

    [TestMethod]
    public void TryCreate_PasswordsDifferent_SaysTheyDoNotMatch()
    {
        bool is_good = Create(out StaffInput? input, out string error_message, confirm_password: "secret124");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "passwords do not match");
    }

    [TestMethod]
    public void TryCreate_EverythingEmpty_ShowsEmailRoleAndPassword()
    {
        Create(out StaffInput? input, out string error_message, email: "", role: null, password: "", confirm_password: "");

        StringAssert.Contains(error_message, "email");
        StringAssert.Contains(error_message, "role must be Staff or Admin");
        StringAssert.Contains(error_message, "password must be at least 8 characters");
        Assert.AreEqual(3, error_message.Split('\n').Length);
    }

    [TestMethod]
    public void TryCreate_BadValues_GiveNoInput()
    {
        Create(out StaffInput? input, out string error_message, email: "");

        Assert.IsNull(input);
    }

    [TestMethod]
    public void TryCreate_NullText_IsRefusedWithoutCrashing()
    {
        bool is_good = StaffInput.TryCreate(null!, null, null!, null!, out StaffInput? input, out string error_message);

        Assert.IsFalse(is_good);
        Assert.IsNull(input);
    }

    // the role list on the page only offers the two roles an admin can create
    [TestMethod]
    public void ChoosableRoles_AreStaffAndAdminOnly()
    {
        CollectionAssert.AreEqual(new Role[] { Role.Staff, Role.Admin }, StaffInput.ChoosableRoles);
    }

    // the message of each box, for the page to show under the box
    [TestMethod]
    public void CheckBoxes_GoodValues_EveryBoxIsEmptyText()
    {
        string[] messages = StaffInput.CheckBoxes("clerk@cinema.com", Role.Staff, "secret123", "secret123");

        Assert.AreEqual(StaffInput.BOX_COUNT, messages.Length);
        for (int box_index = 0; box_index < messages.Length; box_index++)
        {
            Assert.AreEqual("", messages[box_index]);
        }
    }

    [TestMethod]
    public void CheckBoxes_BadEmail_TheMessageIsUnderTheEmailBox()
    {
        string[] messages = StaffInput.CheckBoxes("clerk", Role.Staff, "secret123", "secret123");

        StringAssert.Contains(messages[StaffInput.BOX_EMAIL], "email");
        Assert.AreEqual("", messages[StaffInput.BOX_ROLE]);
        Assert.AreEqual("", messages[StaffInput.BOX_PASSWORD]);
    }

    [TestMethod]
    public void CheckBoxes_NoRole_TheMessageIsUnderTheRoleBox()
    {
        string[] messages = StaffInput.CheckBoxes("clerk@cinema.com", null, "secret123", "secret123");

        StringAssert.Contains(messages[StaffInput.BOX_ROLE], "role must be Staff or Admin");
        Assert.AreEqual("", messages[StaffInput.BOX_EMAIL]);
    }

    [TestMethod]
    public void CheckBoxes_ShortAndDifferent_PasswordBoxAndConfirmBoxEachHaveTheirOwn()
    {
        string[] messages = StaffInput.CheckBoxes("clerk@cinema.com", Role.Staff, "abc", "abd");

        StringAssert.Contains(messages[StaffInput.BOX_PASSWORD], "password must be at least 8 characters");
        StringAssert.Contains(messages[StaffInput.BOX_CONFIRM_PASSWORD], "passwords do not match");
    }

    [TestMethod]
    public void CheckBoxes_NullText_DoesNotCrash()
    {
        string[] messages = StaffInput.CheckBoxes(null!, null, null!, null!);

        Assert.AreEqual(4, messages.Length);
        Assert.AreNotEqual("", messages[StaffInput.BOX_EMAIL]);
    }
}
