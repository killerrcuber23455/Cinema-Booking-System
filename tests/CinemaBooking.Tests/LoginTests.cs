// login tests, written first so they fail until Login checks the password and the lock-out (owner: shankar)
// rules from card 400, role and member, the same message for a wrong email or password, 3 wrong tries lock for 15 minutes
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;
using CinemaBooking.Data;

namespace CinemaBooking.Tests;

[TestClass]
public class LoginTests
{
    private const string WRONG_LOGIN_MESSAGE = "email or password is wrong";
    private const string PASSWORD = "Secret123";
    private const int MAX_TRIES = 3;
    private const int LOCK_MINUTES = 15;

    // the tests move this clock instead of waiting 15 real minutes
    private DateTime _clock_time = new DateTime(2026, 11, 22, 10, 0, 0);

    private FakeUserRepository _users = new FakeUserRepository();
    private FakeDatabase _database = new FakeDatabase();
    private AuthService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _users = new FakeUserRepository();
        _database = new FakeDatabase();
        _clock_time = new DateTime(2026, 11, 22, 10, 0, 0);
        _service = new AuthService(_users, new MemberService(_database), () => _clock_time);
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

    // makes wrong password tries, the same way the window would
    private void WrongTries(string email, int how_many)
    {
        for (int count = 0; count < how_many; count++)
        {
            _service.Login(email, "Wrong999", out _, out _, out _);
        }
    }

    // reads the saved user back, so a test can check the counter and the lock
    private UserRecord GetUser(string email)
    {
        return _users.FindByEmail(email)!;
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

    [TestMethod]
    public void Login_TwoWrongPasswords_DoesNotLockTheAccount()
    {
        AddUser("ann@x.com", Role.Customer, null);

        WrongTries("ann@x.com", MAX_TRIES - 1);

        Assert.AreEqual(MAX_TRIES - 1, GetUser("ann@x.com").FailedAttempts);
        Assert.IsNull(GetUser("ann@x.com").LockedUntil);
    }

    [TestMethod]
    public void Login_ThirdWrongPassword_LocksTheAccountForFifteenMinutes()
    {
        AddUser("ann@x.com", Role.Customer, null);

        WrongTries("ann@x.com", MAX_TRIES);

        Assert.AreEqual(MAX_TRIES, GetUser("ann@x.com").FailedAttempts);
        Assert.AreEqual(_clock_time.AddMinutes(LOCK_MINUTES), GetUser("ann@x.com").LockedUntil);
    }

    [TestMethod]
    public void Login_AccountLocked_IsRefusedEvenWithCorrectPassword()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES);

        bool is_ok = _service.Login("ann@x.com", PASSWORD, out Role role, out Member? member, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(Role.Guest, role);
        Assert.IsNull(member);
        StringAssert.Contains(error_message, "locked");
    }

    [TestMethod]
    public void Login_AccountLocked_MessageSaysForHowLong()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES);

        _service.Login("ann@x.com", PASSWORD, out _, out _, out string first_message);
        _clock_time = _clock_time.AddMinutes(10);
        _service.Login("ann@x.com", PASSWORD, out _, out _, out string second_message);

        Assert.IsTrue(first_message.EndsWith("in 15 minutes"));
        Assert.IsTrue(second_message.EndsWith("in 5 minutes"));
    }

    [TestMethod]
    public void Login_AccountLockedWithOneMinuteLeft_SaysOneMinute()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES);
        _clock_time = _clock_time.AddMinutes(LOCK_MINUTES - 1);

        _service.Login("ann@x.com", PASSWORD, out _, out _, out string error_message);

        Assert.IsTrue(error_message.EndsWith("in 1 minute"));
    }

    [TestMethod]
    public void Login_WrongPasswordWhileLocked_DoesNotExtendTheLock()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES);
        DateTime? first_lock = GetUser("ann@x.com").LockedUntil;
        Assert.IsNotNull(first_lock);
        _clock_time = _clock_time.AddMinutes(5);

        bool is_ok = _service.Login("ann@x.com", "Wrong999", out _, out _, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.IsTrue(error_message.EndsWith("in 10 minutes"));
        Assert.AreEqual(first_lock, GetUser("ann@x.com").LockedUntil);
        Assert.AreEqual(MAX_TRIES, GetUser("ann@x.com").FailedAttempts);
    }

    [TestMethod]
    public void Login_CorrectPasswordAfterWrongTries_ResetsCounterToZero()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES - 1);
        Assert.AreEqual(MAX_TRIES - 1, GetUser("ann@x.com").FailedAttempts);

        bool is_ok = _service.Login("ann@x.com", PASSWORD, out _, out _, out _);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(0, GetUser("ann@x.com").FailedAttempts);
    }

    [TestMethod]
    public void Login_CorrectPasswordAfterLockEnded_WorksAndResetsCounter()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES);
        Assert.IsNotNull(GetUser("ann@x.com").LockedUntil);
        _clock_time = _clock_time.AddMinutes(LOCK_MINUTES + 1);

        bool is_ok = _service.Login("ann@x.com", PASSWORD, out _, out _, out _);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(0, GetUser("ann@x.com").FailedAttempts);
        Assert.IsNull(GetUser("ann@x.com").LockedUntil);
    }

    [TestMethod]
    public void Login_WrongPasswordAfterLockEnded_CountsAsFirstTry()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES);
        _clock_time = _clock_time.AddMinutes(LOCK_MINUTES + 1);

        WrongTries("ann@x.com", 1);

        Assert.AreEqual(1, GetUser("ann@x.com").FailedAttempts);
        Assert.IsNull(GetUser("ann@x.com").LockedUntil);
    }

    [TestMethod]
    public void Login_ThirdWrongPassword_ReturnsTheLockedMessage()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES - 1);

        bool is_ok = _service.Login("ann@x.com", "Wrong999", out Role role, out Member? member, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(Role.Guest, role);
        Assert.IsNull(member);
        StringAssert.Contains(error_message, "locked");
        Assert.IsTrue(error_message.EndsWith("in 15 minutes"));
    }

    [TestMethod]
    public void Login_ClockExactlyAtLockEnd_AccountIsOpen()
    {
        AddUser("ann@x.com", Role.Customer, null);
        WrongTries("ann@x.com", MAX_TRIES);
        DateTime lock_end = GetUser("ann@x.com").LockedUntil ?? DateTime.MinValue;
        Assert.AreNotEqual(DateTime.MinValue, lock_end);

        _clock_time = lock_end.AddSeconds(-1);
        bool is_ok_before = _service.Login("ann@x.com", PASSWORD, out _, out _, out _);
        _clock_time = lock_end;
        bool is_ok_at_end = _service.Login("ann@x.com", PASSWORD, out _, out _, out _);

        Assert.IsFalse(is_ok_before);
        Assert.IsTrue(is_ok_at_end);
    }
}
