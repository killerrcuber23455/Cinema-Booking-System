// login tests, written first so they fail until Login checks the password (owner: shankar)
// rules from card 400: a good login gives the role and member, a wrong email or password gets the same message
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class LoginTests
{
    private const string WRONG_LOGIN_MESSAGE = "email or password is wrong";
    private const string PASSWORD = "Secret123";

    private FakeUserRepository _users = new FakeUserRepository();
    private FakeDatabase _database = new FakeDatabase();
    private AuthService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _users = new FakeUserRepository();
        _database = new FakeDatabase();
        _service = new AuthService(_users, new MemberService(_database));
    }

    // saves a user with a real salted hash, so Login checks it the way it will in the program
    private int AddUser(string email, Role role, int? member_id)
    {
        string salt = PasswordHasher.GenerateSalt();
        string hash = PasswordHasher.HashPassword(PASSWORD, salt);
        return _users.SaveUser(email, hash, salt, role, member_id);
    }

    private int AddMember(string email)
    {
        Member member = new Member(0, "Ann Lee", email, "0771234567", Tier.Standard, 0, 0, DateTime.Now);
        return _database.SaveMember(member);
    }

    [TestMethod]
    public void Login_CorrectPasswordCustomer_ReturnsTrueWithRoleAndMember()
    {
        int member_id = AddMember("ann@x.com");
        AddUser("ann@x.com", Role.Customer, member_id);

        bool is_ok = _service.Login("ann@x.com", PASSWORD, out Role role, out Member? member, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(Role.Customer, role);
        Assert.AreEqual(member_id, member?.Id);
        Assert.AreEqual("", error_message);
    }

    [TestMethod]
    public void Login_CorrectPasswordStaff_ReturnsTrueWithRoleAndNoMember()
    {
        AddUser("staff@x.com", Role.Staff, null);

        bool is_ok = _service.Login("staff@x.com", PASSWORD, out Role role, out Member? member, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(Role.Staff, role);
        Assert.IsNull(member);
        Assert.AreEqual("", error_message);
    }

    [TestMethod]
    public void Login_UnknownEmail_IsRefusedWithWrongLoginMessage()
    {
        bool is_ok = _service.Login("nobody@x.com", PASSWORD, out Role role, out Member? member, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(Role.Guest, role);
        Assert.IsNull(member);
        Assert.AreEqual(WRONG_LOGIN_MESSAGE, error_message);
    }

    [TestMethod]
    public void Login_WrongPassword_IsRefusedWithTheSameMessage()
    {
        AddUser("ann@x.com", Role.Customer, null);

        bool is_ok = _service.Login("ann@x.com", "Wrong999", out Role role, out Member? member, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(Role.Guest, role);
        Assert.IsNull(member);
        Assert.AreEqual(WRONG_LOGIN_MESSAGE, error_message);
    }

    [TestMethod]
    public void Login_EmptyPassword_IsRefusedWithTheSameMessage()
    {
        AddUser("ann@x.com", Role.Customer, null);

        bool is_ok = _service.Login("ann@x.com", "", out Role role, out Member? member, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(WRONG_LOGIN_MESSAGE, error_message);
    }
}
