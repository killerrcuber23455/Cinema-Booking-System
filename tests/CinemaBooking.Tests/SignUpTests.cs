// sign up tests, written first so they fail until the code follows the rules (owner: shankar)
// rules: a customer account and a member are made together, or neither is made
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class SignUpTests
{
    private const string NAME = "Ann Lee";
    private const string EMAIL = "ann@x.com";
    private const string PHONE = "07123456789";
    private const string PASSWORD = "Secret123";
    private const string SHORT_PASSWORD_MESSAGE = "password must be at least 8 characters";
    private const string EMAIL_USED_MESSAGE = "this email is already used";

    private FakeUserRepository _users = new FakeUserRepository();
    private FakeDatabase _database = new FakeDatabase();
    private MemberService _members = null!;
    private AuthService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _users = new FakeUserRepository();
        _database = new FakeDatabase();
        _members = new MemberService(_database);
        _service = new AuthService(_users, _members);
    }

    // asks a separate MemberService what it says about these details, so the tests use its exact words
    private static string ExpectedMemberError(string name, string email, string phone)
    {
        new MemberService(new FakeDatabase()).RegisterMember(name, email, phone, out string message);
        return message;
    }

    // checking that a refused sign-up left nothing behind in either place.
    private void AssertNothingSaved(string email)
    {
        Assert.IsNull(_users.FindByEmail(email));
        Assert.AreEqual(0, _database.GetAllMembers().Count());
    }

    [TestMethod]
    public void SignUp_ValidDetails_SavesCustomerUserAndOneMember()
    {
        bool is_ok = _service.SignUp(NAME, EMAIL, PHONE, PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        UserRecord? user = _users.FindByEmail(EMAIL);
        Assert.IsNotNull(user);
        Assert.AreEqual(Role.Customer, user.Role);
        Assert.IsNotNull(user.MemberId);
        Assert.AreEqual(1, _database.GetAllMembers().Count());
    }

    [TestMethod]
    public void SignUp_Password_IsStoredAsASaltedHash()
    {
        _service.SignUp(NAME, EMAIL, PHONE, PASSWORD, out _);

        UserRecord? user = _users.FindByEmail(EMAIL);
        Assert.IsNotNull(user);
        Assert.AreNotEqual(PASSWORD, user.PasswordHash);
        Assert.IsTrue(PasswordHasher.Verify(PASSWORD, user.PasswordHash, user.Salt));
    }

    [TestMethod]
    public void SignUp_ThenLogin_WorksAsCustomerWithTheMember()
    {
        _service.SignUp(NAME, EMAIL, PHONE, PASSWORD, out _);

        bool is_ok = _service.Login(EMAIL, PASSWORD, out Role role, out Member? member, out _);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(Role.Customer, role);
        Assert.IsNotNull(member);
        Assert.AreEqual(NAME, member.Name);
    }

    [TestMethod]
    public void SignUp_BadEmail_IsRefusedAndNothingIsSaved()
    {
        string expected_message = ExpectedMemberError(NAME, "not-an-email", PHONE);

        bool is_ok = _service.SignUp(NAME, "not-an-email", PHONE, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
        AssertNothingSaved("not-an-email");
    }

    [TestMethod]
    public void SignUp_EmptyName_IsRefusedAndNothingIsSaved()
    {
        string expected_message = ExpectedMemberError("", EMAIL, PHONE);

        bool is_ok = _service.SignUp("", EMAIL, PHONE, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
        AssertNothingSaved(EMAIL);
    }

    [TestMethod]
    public void SignUp_BadPhone_IsRefusedAndNothingIsSaved()
    {
        string expected_message = ExpectedMemberError(NAME, EMAIL, "abc");

        bool is_ok = _service.SignUp(NAME, EMAIL, "abc", PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
        AssertNothingSaved(EMAIL);
    }

    // the member must not be left behind when the password is refused
    [TestMethod]
    public void SignUp_ShortPassword_IsRefusedAndNoMemberIsLeftBehind()
    {
        bool is_ok = _service.SignUp(NAME, EMAIL, PHONE, "Abc1234", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertNothingSaved(EMAIL);
    }

    [TestMethod]
    public void SignUp_PasswordOfOnlySpaces_IsRefused()
    {
        bool is_ok = _service.SignUp(NAME, EMAIL, PHONE, "        ", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertNothingSaved(EMAIL);
    }

    // an account already has this email, so no new member may be made
    [TestMethod]
    public void SignUp_EmailUsedByAnAccount_IsRefusedAndNoMemberIsMade()
    {
        _users.SaveUser(EMAIL, "hash", "salt", Role.Admin, null);

        bool is_ok = _service.SignUp(NAME, EMAIL, PHONE, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(EMAIL_USED_MESSAGE, error_message);
        Assert.AreEqual(0, _database.GetAllMembers().Count());
    }

    // a member already has this email but no account, so no account may be made
    [TestMethod]
    public void SignUp_EmailUsedByAMemberWithoutAccount_IsRefusedAndNoAccountIsMade()
    {
        _members.RegisterMember("Old Ann", EMAIL, PHONE, out _);

        bool is_ok = _service.SignUp(NAME, EMAIL, PHONE, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(EMAIL_USED_MESSAGE, error_message);
        Assert.IsNull(_users.FindByEmail(EMAIL));
        Assert.AreEqual(1, _database.GetAllMembers().Count());
    }

    [TestMethod]
    public void SignUp_SameEmailInOtherCase_IsTheSameEmail()
    {
        _users.SaveUser("Ann@X.com", "hash", "salt", Role.Customer, null);

        bool is_ok = _service.SignUp(NAME, "ann@x.com", PHONE, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(EMAIL_USED_MESSAGE, error_message);
        Assert.AreEqual(0, _database.GetAllMembers().Count());
    }

    [TestMethod]
    public void SignUp_EmailWithSpacesAround_IsTrimmedAndSaved()
    {
        bool is_ok = _service.SignUp(NAME, "  ann@x.com  ", PHONE, PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(_users.FindByEmail(EMAIL));
    }

    [TestMethod]
    public void SignUp_PasswordWithSpacesAtStartAndEnd_IsKeptAsTyped()
    {
        string spaced_password = "  Secret123  ";

        bool is_created = _service.SignUp(NAME, EMAIL, PHONE, spaced_password, out _);
        bool is_ok_as_typed = _service.Login(EMAIL, spaced_password, out _, out _, out _);
        bool is_ok_trimmed = _service.Login(EMAIL, "Secret123", out _, out _, out _);

        Assert.IsTrue(is_created);
        Assert.IsTrue(is_ok_as_typed);
        Assert.IsFalse(is_ok_trimmed);
    }

    [TestMethod]
    public void SignUp_NullEmail_IsRefusedNotCrashed()
    {
        string expected_message = ExpectedMemberError(NAME, "", PHONE);

        bool is_ok = _service.SignUp(NAME, null!, PHONE, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
        Assert.AreEqual(0, _database.GetAllMembers().Count());
    }
}