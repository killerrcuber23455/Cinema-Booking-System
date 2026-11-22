// first admin tests, written first so they fail until the code follows the rules (owner: shankar)
// rules only while no admin exists, a valid email, a password of at least 8 characters, stored as a salted hash
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class FirstAdminTests
{
    private const string EMAIL = "boss@x.com";
    private const string PASSWORD = "Secret123";

    private const string SHORT_PASSWORD_MESSAGE = "password must be at least 8 characters";

    private FakeUserRepository _users = new FakeUserRepository();
    private AuthService _service = null;

    [TestInitialize]
    public void SetUp()
    {
        _users = new FakeUserRepository();
        _service = new AuthService(_users, new MemberService(new FakeDatabase()));
    }

    [TestMethod]
    public void HasAnyAdmin_NoUsers_ReturnsFalse()
    {
        Assert.IsFalse(_service.HasAnyAdmin());
    }

    [TestMethod]
    public void HasAnyAdmin_OnlyACustomer_ReturnsFalse()
    {
        _users.SaveUser("ann@x.com", "hash", "salt", Role.Customer, null);
        Assert.IsFalse(_service.HasAnyAdmin());
    }
    [TestMethod]
    public void HasAnyAdmin_AnAdminExists_ReturnsTrue()
    {
        _users.SaveUser("boss@x.com", "hash", "salt", Role.Admin, null);

        Assert.IsTrue(_service.HasAnyAdmin());
    }

    [TestMethod]
    public void CreateFirstAdmin_NoAdminYet_SavesAnAdminUserWithNoMember()
    {
        bool is_ok = _service.CreateFirstAdmin(EMAIL, PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        UserRecord? user = _users.FindByEmail(EMAIL);
        Assert.IsNotNull(user);
        Assert.AreEqual(Role.Admin, user.Role);
        Assert.IsNull(user.MemberId);
    }

    [TestMethod]
    public void CreateFirstAdmin_Password_IsStoredAsASaltedHash()
    {
        _service.CreateFirstAdmin(EMAIL, PASSWORD, out _);

        UserRecord? user = _users.FindByEmail(EMAIL);
        Assert.IsNotNull(user);
        Assert.AreNotEqual(PASSWORD, user.PasswordHash);
        Assert.IsTrue(PasswordHasher.Verify(PASSWORD, user.PasswordHash, user.Salt));
    }

    [TestMethod]
    public void CreateFirstAdmin_ThenLogin_WorksAsAdmin()
    {
        _service.CreateFirstAdmin(EMAIL, PASSWORD, out _);

        bool is_ok = _service.Login(EMAIL, PASSWORD, out Role role, out Member? member, out _);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(Role.Admin, role);
        Assert.IsNull(member);
    }

    [TestMethod]
    public void CreateFirstAdmin_AnAdminExists_IsRefused()
    {
        _users.SaveUser("first@x.com", "hash", "salt", Role.Admin, null);

        bool is_ok = _service.CreateFirstAdmin(EMAIL, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual("an admin already exists", error_message);
        Assert.IsNull(_users.FindByEmail(EMAIL));
    }

    [TestMethod]
    public void CreateFirstAdmin_BadEmail_IsRefusedWithTheMemberEmailMessage()
    {
        Member.IsValidEmail("not-an-email", out string expected_message);

        bool is_ok = _service.CreateFirstAdmin("not-an-email", PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
        Assert.IsNull(_users.FindByEmail("not-an-email"));
    }

    [TestMethod]
    public void CreateFirstAdmin_EmptyPassword_IsRefused()
    {
        bool is_ok = _service.CreateFirstAdmin(EMAIL, "", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        Assert.IsNull(_users.FindByEmail(EMAIL));
    }

    [TestMethod]
    public void CreateFirstAdmin_PasswordUnderEightCharacters_IsRefused()
    {
        bool is_ok = _service.CreateFirstAdmin(EMAIL, "Abc1234", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        Assert.IsNull(_users.FindByEmail(EMAIL));
    }

    // Login refuses a password of only spaces, so an account with one could never log in
    [TestMethod]
    public void CreateFirstAdmin_PasswordOfOnlySpaces_IsRefused()
    {
        bool is_ok = _service.CreateFirstAdmin(EMAIL, "        ", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        Assert.IsNull(_users.FindByEmail(EMAIL));
    }

    [TestMethod]
    public void CreateFirstAdmin_EmailAlreadyUsedByACustomer_IsRefused()
    {
        _users.SaveUser(EMAIL, "hash", "salt", Role.Customer, null);

        bool is_ok = _service.CreateFirstAdmin(EMAIL, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual("this email is already used", error_message);
        Assert.IsFalse(_service.HasAnyAdmin());
    }

    [TestMethod]
    public void CreateFirstAdmin_SameEmailInOtherCase_IsTheSameEmail()
    {
        _users.SaveUser("Boss@x.com", "hash", "salt", Role.Customer, null);

        bool is_ok = _service.CreateFirstAdmin("boss@x.com", PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual("this email is already used", error_message);
    }

    [TestMethod]
    public void CreateFirstAdmin_EmailWithSpacesAround_IsTrimmedAndSaved()
    {
        bool is_ok = _service.CreateFirstAdmin("  boss@x.com  ", PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(_users.FindByEmail(EMAIL));
    }

    [TestMethod]
    public void CreateFirstAdmin_PasswordWithSpacesAtStartAndEnd_IsKeptAsTyped()
    {
        string spaced_password = "  Secret123  ";

        bool is_created = _service.CreateFirstAdmin(EMAIL, spaced_password, out _);
        bool is_ok_as_typed = _service.Login(EMAIL, spaced_password, out _, out _, out _);
        bool is_ok_trimmed = _service.Login(EMAIL, "Secret123", out _, out _, out _);

        Assert.IsTrue(is_created);
        Assert.IsTrue(is_ok_as_typed);
        Assert.IsFalse(is_ok_trimmed);
    }

    // the windows never send null, but the service must refuse it instead of crashing
    [TestMethod]
    public void CreateFirstAdmin_NullEmail_IsRefusedNotCrashed()
    {
        Member.IsValidEmail("", out string expected_message);

        bool is_ok = _service.CreateFirstAdmin(null!, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
    }

    // this one passes at once because IsValidPassword already handles null, it guards that
    [TestMethod]
    public void CreateFirstAdmin_NullPassword_IsRefusedNotCrashed()
    {
        bool is_ok = _service.CreateFirstAdmin(EMAIL, null!, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
    }
}