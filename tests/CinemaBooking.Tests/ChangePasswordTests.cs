// change password tests, written first so they fail until ChangePassword follows the rules (owner: shankar)
// rules: only a signed-in user (claimed and saved role not guest), the current password must be right,
// the new one follows the sign up rule and must be different, a failed save changes nothing,
// and the audit log never holds a password or hash
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;
using System.Runtime.CompilerServices;

namespace CinemaBooking.Tests;

[TestClass]
public class ChangePasswordTests
{
    private const string EMAIL = "ann@x.com";
    private const string OLD_PASSWORD = "Secret123";
    private const string NEW_PASSWORD = "Better456";

    // the words the settings page shows, kept here so a change of wording is one line
    private const string SIGN_IN_MESSAGE = "sign in to change your password";
    private const string WRONG_OLD_MESSAGE = "your current password is wrong";
    private const string SHORT_PASSWORD_MESSAGE = "password must be at least 8 characters";
    private const string SAME_PASSWORD_MESSAGE = "the new password must be different";
    private const string SAVE_FAILED_MESSAGE = "the password could not be saved";
    private const string AUDIT_PASSWORD_CHANGED = "password_changed";

    private FakeUserRepository _users = new FakeUserRepository();
    private AuthService _service = null!;
    private int _user_id;
    private string _saved_hash = "";
    private string _saved_salt = "";

    // a real customer with a real salted hash, so Verify and Login work like in the app
    [TestInitialize]
    public void SetUp()
    {
        _users = new FakeUserRepository();
        _service = new AuthService(_users, new MemberService(new FakeDatabase()));
        _saved_salt = PasswordHasher.GenerateSalt();
        _saved_hash = PasswordHasher.HashPassword(OLD_PASSWORD, _saved_salt);
        _user_id = _users.SaveUser(EMAIL, _saved_hash, _saved_salt, Role.Customer, null);
    }

    private UserRecord GetSavedUser()
    {
        UserRecord? user = _users.FindByEmail(EMAIL);
        Assert.IsNotNull(user);
        return user;
    }

    // a refused call must leave the saved hash and salt exactly as they were
    private void AssertPasswordUnchanged()
    {
        UserRecord user = GetSavedUser();
        Assert.AreEqual(_saved_hash, user.PasswordHash);
        Assert.AreEqual(_saved_salt, user.Salt);
    }

    // the audit rows of one action, so a test can look at what was written
    private List<AuditRow> AuditRowsFor(string action)
    {
        List<AuditRow> found = new List<AuditRow>();
        for (int index = 0; index < _users.AuditRows.Count; index++)
        {
            if (_users.AuditRows[index].Action == action)
            {
                found.Add(_users.AuditRows[index]);
            }
        }
        return found;
    }

    //who can change a password
    [TestMethod]
    public void ChangePassword_ActingRoleGuest_IsRefused()
    {
        bool is_ok = _service.ChangePassword(Role.Guest, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SIGN_IN_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    // a number that is not one of the four roles, the service must refuse it instead of trusting it
    [TestMethod]
    public void ChangePassword_UnknownRole_IsRefused()
    {
        bool is_ok = _service.ChangePassword((Role)99, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SIGN_IN_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    [TestMethod]
    public void ChangePassword_EmailNotFound_IsRefused()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, "nobody@x.com", OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SIGN_IN_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    // the windows never send null, but the service must refuse it instead of crashing
    [TestMethod]
    public void ChangePassword_NullEmail_IsRefusedNotCrashed()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, null!, OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SIGN_IN_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    // the role passed in is only a claim, so the saved role is checked as well (same idea as CreateStaff)
    [TestMethod]
    public void ChangePassword_ClaimsCustomerButSavedRoleIsGuest_IsRefused()
    {
        string guest_salt = PasswordHasher.GenerateSalt();
        string guest_hash = PasswordHasher.HashPassword(OLD_PASSWORD, guest_salt);
        _users.SaveUser("guest@x.com", guest_hash, guest_salt, Role.Guest, null);

        bool is_ok = _service.ChangePassword(Role.Customer, "guest@x.com", OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SIGN_IN_MESSAGE, error_message);
        Assert.AreEqual(guest_hash, _users.FindByEmail("guest@x.com")!.PasswordHash);
    }

    // the sign in check comes first, so a guest learns nothing about the password
    [TestMethod]
    public void ChangePassword_GuestWithWrongOldPassword_GivesTheSignInMessage()
    {
        bool is_ok = _service.ChangePassword(Role.Guest, EMAIL, "wrong-password", NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SIGN_IN_MESSAGE, error_message);
    }

    // staff and admin change their own password the same way as a customer
    [TestMethod]
    public void ChangePassword_StaffUser_CanChangeTheirOwnPassword()
    {
        string staff_salt = PasswordHasher.GenerateSalt();
        _users.SaveUser("clerk@x.com", PasswordHasher.HashPassword(OLD_PASSWORD, staff_salt), staff_salt, Role.Staff, null);

        bool is_ok = _service.ChangePassword(Role.Staff, "clerk@x.com", OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
    }

    // the current password

    [TestMethod]
    public void ChangePassword_WrongOldPassword_IsRefused()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, "Wrong1234", NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(WRONG_OLD_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    [TestMethod]
    public void ChangePassword_EmptyOldPassword_IsRefused()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, "", NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(WRONG_OLD_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    [TestMethod]
    public void ChangePassword_NullOldPassword_IsRefusedNotCrashed()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, null!, NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(WRONG_OLD_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    // the old password is checked before the new one, so a wrong old password always gets the same answer
    [TestMethod]
    public void ChangePassword_WrongOldAndShortNew_GivesTheWrongOldMessage()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, "Wrong1234", "short", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(WRONG_OLD_MESSAGE, error_message);
    }

    // the new password

    [TestMethod]
    public void ChangePassword_NewPasswordUnderEightCharacters_IsRefused()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, "Abc1234", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    [TestMethod]
    public void ChangePassword_NewPasswordOfOnlySpaces_IsRefused()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, "        ", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    [TestMethod]
    public void ChangePassword_NullNewPassword_IsRefusedNotCrashed()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, null!, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    [TestMethod]
    public void ChangePassword_NewSameAsOld_IsRefused()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, OLD_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SAME_PASSWORD_MESSAGE, error_message);
        AssertPasswordUnchanged();
    }

    // passwords are case sensitive, so a change of case is a different password
    [TestMethod]
    public void ChangePassword_NewDiffersOnlyInCase_IsAllowed()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, "secret123", out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
    }

    // a failed save

    [TestMethod]
    public void ChangePassword_UpdatePasswordFails_IsRefusedAndNothingChanges()
    {
        _users.FailUpdatePassword = true;

        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SAVE_FAILED_MESSAGE, error_message);
        AssertPasswordUnchanged();
        Assert.AreEqual(0, AuditRowsFor(AUDIT_PASSWORD_CHANGED).Count);
    }

    // a good call

    [TestMethod]
    public void ChangePassword_Success_SavesANewHashAndANewSalt()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        UserRecord user = GetSavedUser();
        Assert.AreNotEqual(_saved_hash, user.PasswordHash);
        Assert.AreNotEqual(_saved_salt, user.Salt);
    }

    [TestMethod]
    public void ChangePassword_Success_StoresTheNewPasswordAsASaltedHash()
    {
        _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out _);

        UserRecord user = GetSavedUser();
        Assert.AreNotEqual(NEW_PASSWORD, user.PasswordHash);
        Assert.IsTrue(PasswordHasher.Verify(NEW_PASSWORD, user.PasswordHash, user.Salt));
    }

    [TestMethod]
    public void ChangePassword_ThenLogin_WorksWithTheNewPassword()
    {
        _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out _);

        bool is_ok = _service.Login(EMAIL, NEW_PASSWORD, out Role role, out _, out _);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(Role.Customer, role);
    }

    [TestMethod]
    public void ChangePassword_ThenLogin_RefusesTheOldPassword()
    {
        bool is_changed = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out _);

        bool is_ok = _service.Login(EMAIL, OLD_PASSWORD, out _, out _, out _);

        Assert.IsTrue(is_changed);
        Assert.IsFalse(is_ok);
    }

    // the acting email is looked up the same way as at login, so capitals and spaces do not matter
    [TestMethod]
    public void ChangePassword_EmailInOtherCaseWithSpaces_StillFindsTheUser()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, "  ANN@x.com ", OLD_PASSWORD, NEW_PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
    }

    //the audit log

    // exactly one row, so the service and the repository never both write it
    [TestMethod]
    public void ChangePassword_Success_WritesOnePasswordChangedRowWithTheUserId()
    {
        _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out _);

        List<AuditRow> rows = AuditRowsFor(AUDIT_PASSWORD_CHANGED);
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual(_user_id, rows[0].UserId);
    }

    [TestMethod]
    public void ChangePassword_AuditRows_NeverHoldAPasswordOrAHash()
    {
        bool is_ok = _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out _);

        Assert.IsTrue(is_ok);
        UserRecord user = GetSavedUser();
        for (int index = 0; index < _users.AuditRows.Count; index++)
        {
            string details = _users.AuditRows[index].Details;
            Assert.IsFalse(details.Contains(OLD_PASSWORD));
            Assert.IsFalse(details.Contains(NEW_PASSWORD));
            Assert.IsFalse(details.Contains(user.PasswordHash));
            Assert.IsFalse(details.Contains(user.Salt));
            Assert.IsFalse(details.Contains(_saved_hash));
            Assert.IsFalse(details.Contains(_saved_salt));
        }
    }

    // four refusals then one good change, so only the good change may leave a row
    [TestMethod]
    public void ChangePassword_Refused_WritesNoPasswordChangedRow()
    {
        _service.ChangePassword(Role.Guest, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out _);
        _service.ChangePassword(Role.Customer, EMAIL, "Wrong1234", NEW_PASSWORD, out _);
        _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, "short", out _);
        _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, OLD_PASSWORD, out _);

        _service.ChangePassword(Role.Customer, EMAIL, OLD_PASSWORD, NEW_PASSWORD, out _);

        Assert.AreEqual(1, AuditRowsFor(AUDIT_PASSWORD_CHANGED).Count);
    }
}
