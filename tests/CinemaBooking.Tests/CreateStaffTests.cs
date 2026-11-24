// create staff tests, written first so they fail until CreateStaff follows the rules (owner: shankar)
// rules: only an admin creates accounts, the saved role is checked too, the new role is staff or admin,
// the same email and password rules as sign up, and an audit row that never holds a password or hash
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class CreateStaffTests
{
    private const string ADMIN_EMAIL = "boss@x.com";
    private const string NEW_EMAIL = "new@x.com";
    private const string PASSWORD = "Secret123";

    private const string NOT_ADMIN_MESSAGE = "only an admin can ceate staff accounts";
    private const string BAD_NEW_ROLE_MESSAGE = "a new account must be staff or admin";
    private const string SHORT_PASSWORD_MESSAGE = "password must be at least 8 characters";
    private const string EMAIL_USED_MESSAGE = "this email is already used";
    private const string AUDIT_STAFF_CREATED = "staff_created";

    private FakeUserRepository _users = new FakeUserRepository();
    private AuthService _service = null!;
    private int _admin_id;

    [TestInitialize]
    public void SetUp()
    {
        _users = new FakeUserRepository();
        _service = new AuthService(_users, new MemberService(new FakeDatabase()));
        _admin_id = _users.SaveUser(ADMIN_EMAIL, "hash", "salt", Role.Admin, null);
    }

    // a refused call must leave no new account behind
    private void AssertNothingSaved(string email)
    {
        Assert.IsNull(_users.FindByEmail(email));
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

    //who can create an acc
    [TestMethod]
    public void CreateStaff_ActingRoleGuest_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Guest, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(NOT_ADMIN_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    [TestMethod]
    public void CreateStaff_ActingRoleCustomer_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Customer, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(NOT_ADMIN_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    [TestMethod]
    public void CreateStaff_ActingRoleStaff_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Staff, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(NOT_ADMIN_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    // the role passed in is only a claim, so the saved role of the acting email is checked as well
    [TestMethod]
    public void CreateStaff_ClaimsAdminButSavedRoleIsStaff_IsRefused()
    {
        _users.SaveUser("clerk@x.com", "hash", "salt", Role.Staff, null);

        bool is_ok = _service.CreateStaff(Role.Admin, "clerk@x.com", NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(NOT_ADMIN_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }
    [TestMethod]
    public void CreateStaff_ClaimsAdminButActingEmailDoesNotExist_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, "nobody@x.com", NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(NOT_ADMIN_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    // the windows never send null, but the service must refuse it instead of crashing
    [TestMethod]
    public void CreateStaff_NullActingEmail_IsRefusedNotCrashed()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, null!, NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(NOT_ADMIN_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    // the permission check comes before the input checks, so a non admin learns nothing about the input rules
    [TestMethod]
    public void CreateStaff_NotAdminAndBadEmail_GivesTheNotAdminMessage()
    {
        bool is_ok = _service.CreateStaff(Role.Staff, ADMIN_EMAIL, "not-an-email", Role.Staff, "short", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(NOT_ADMIN_MESSAGE, error_message);
    }

    // ---- which roles may be created ----

    [TestMethod]
    public void CreateStaff_NewRoleGuest_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Guest, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(BAD_NEW_ROLE_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    [TestMethod]
    public void CreateStaff_NewRoleCustomer_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Customer, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(BAD_NEW_ROLE_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    // ---- the same input rules as sign up ----

    [TestMethod]
    public void CreateStaff_BadEmail_IsRefusedWithTheMemberEmailMessage()
    {
        Member.IsValidEmail("not-an-email", out string expected_message);

        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, "not-an-email", Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
        AssertNothingSaved("not-an-email");
    }

    [TestMethod]
    public void CreateStaff_NullEmail_IsRefusedNotCrashed()
    {
        Member.IsValidEmail("", out string expected_message);

        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, null!, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(expected_message, error_message);
    }

    [TestMethod]
    public void CreateStaff_PasswordUnderEightCharacters_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, "Abc1234", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    [TestMethod]
    public void CreateStaff_PasswordOfOnlySpaces_IsRefused()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, "        ", out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    [TestMethod]
    public void CreateStaff_NullPassword_IsRefusedNotCrashed()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, null!, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(SHORT_PASSWORD_MESSAGE, error_message);
        AssertNothingSaved(NEW_EMAIL);
    }

    [TestMethod]
    public void CreateStaff_EmailAlreadyUsed_IsRefusedWithAClearMessage()
    {
        _users.SaveUser(NEW_EMAIL, "hash", "salt", Role.Customer, null);

        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(EMAIL_USED_MESSAGE, error_message);
        Assert.AreEqual(Role.Customer, _users.FindByEmail(NEW_EMAIL)!.Role);
    }

    [TestMethod]
    public void CreateStaff_SameEmailInOtherCase_IsTheSameEmail()
    {
        _users.SaveUser("New@x.com", "hash", "salt", Role.Customer, null);

        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(EMAIL_USED_MESSAGE, error_message);
    }

    // ---- a good call ----

    [TestMethod]
    public void CreateStaff_AdminCreatesStaff_SavesAStaffUserWithNoMember()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        UserRecord? user = _users.FindByEmail(NEW_EMAIL);
        Assert.IsNotNull(user);
        Assert.AreEqual(Role.Staff, user.Role);
        Assert.IsNull(user.MemberId);
    }

    [TestMethod]
    public void CreateStaff_AdminCreatesAdmin_SavesAnAdminUser()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Admin, PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        UserRecord? user = _users.FindByEmail(NEW_EMAIL);
        Assert.IsNotNull(user);
        Assert.AreEqual(Role.Admin, user.Role);
    }

    [TestMethod]
    public void CreateStaff_Password_IsStoredAsASaltedHash()
    {
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out _);

        UserRecord? user = _users.FindByEmail(NEW_EMAIL);
        Assert.IsNotNull(user);
        Assert.AreNotEqual(PASSWORD, user.PasswordHash);
        Assert.IsTrue(PasswordHasher.Verify(PASSWORD, user.PasswordHash, user.Salt));
    }

    [TestMethod]
    public void CreateStaff_ThenLogin_WorksWithTheChosenRole()
    {
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out _);

        bool is_ok = _service.Login(NEW_EMAIL, PASSWORD, out Role role, out Member? member, out _);

        Assert.IsTrue(is_ok);
        Assert.AreEqual(Role.Staff, role);
        Assert.IsNull(member);
    }

    [TestMethod]
    public void CreateStaff_EmailWithSpacesAround_IsTrimmedAndSaved()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, "  new@x.com  ", Role.Staff, PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(_users.FindByEmail(NEW_EMAIL));
    }

    [TestMethod]
    public void CreateStaff_PasswordWithSpacesAtStartAndEnd_IsKeptAsTyped()
    {
        string spaced_password = "  Secret123  ";

        bool is_created = _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, spaced_password, out _);
        bool is_ok_as_typed = _service.Login(NEW_EMAIL, spaced_password, out _, out _, out _);
        bool is_ok_trimmed = _service.Login(NEW_EMAIL, "Secret123", out _, out _, out _);

        Assert.IsTrue(is_created);
        Assert.IsTrue(is_ok_as_typed);
        Assert.IsFalse(is_ok_trimmed);
    }

    // the acting email is looked up the same way as at login, so capitals and spaces do not matter
    [TestMethod]
    public void CreateStaff_ActingEmailInOtherCaseWithSpaces_StillFindsTheAdmin()
    {
        bool is_ok = _service.CreateStaff(Role.Admin, "  BOSS@x.com ", NEW_EMAIL, Role.Staff, PASSWORD, out string error_message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", error_message);
    }

    // ---- the audit log ----

    [TestMethod]
    public void CreateStaff_Success_WritesOneStaffCreatedRowWithTheActingAdminId()
    {
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out _);

        List<AuditRow> rows = AuditRowsFor(AUDIT_STAFF_CREATED);
        Assert.AreEqual(1, rows.Count);
        Assert.AreEqual(_admin_id, rows[0].UserId);
    }

    [TestMethod]
    public void CreateStaff_AuditDetails_HoldTheNewEmailAndRole()
    {
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Admin, PASSWORD, out _);

        AuditRow row = AuditRowsFor(AUDIT_STAFF_CREATED)[0];
        StringAssert.Contains(row.Details, NEW_EMAIL);
        StringAssert.Contains(row.Details, "Admin");
    }

    [TestMethod]
    public void CreateStaff_AuditRows_NeverHoldThePasswordOrTheHash()
    {
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out _);

        UserRecord? user = _users.FindByEmail(NEW_EMAIL);
        Assert.IsNotNull(user);
        for (int index = 0; index < _users.AuditRows.Count; index++)
        {
            string details = _users.AuditRows[index].Details;
            Assert.IsFalse(details.Contains(PASSWORD));
            Assert.IsFalse(details.Contains(user.PasswordHash));
            Assert.IsFalse(details.Contains(user.Salt));
        }
    }

    [TestMethod]
    public void CreateStaff_Refused_WritesNoStaffCreatedRow()
    {
        _service.CreateStaff(Role.Staff, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, PASSWORD, out _);
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Customer, PASSWORD, out _);
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, "not-an-email", Role.Staff, PASSWORD, out _);
        _service.CreateStaff(Role.Admin, ADMIN_EMAIL, NEW_EMAIL, Role.Staff, "short", out _);

        Assert.AreEqual(0, AuditRowsFor(AUDIT_STAFF_CREATED).Count);
    }
}