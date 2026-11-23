// find member tests, written first so they fail until the code follows the rules (owner: shankar)
// rules: only staff and admin can look a member up, the email is trimmed and matched ignoring case
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class FindMemberTests
{
    private const string PHONE = "07123456789";

    private const string NOT_FOUND_MESSAGE = "no member found with this email";

    private MemberService _service = null!;

    [TestInitialize]
    public void SetUp()
    {
        _service = new MemberService(new FakeDatabase());
        _service.RegisterMember("Ann Lee", "ann@x.com", PHONE, out _);
        _service.RegisterMember("Bob Ray", "bob@x.com", PHONE, out _);
    }

    [TestMethod]
    public void FindMember_AdminWithExistingEmail_ReturnsTheMember()
    {
        Member? found = _service.FindMember(Role.Admin, "ann@x.com", out string error_message);

        Assert.IsNotNull(found);
        Assert.AreEqual("Ann Lee", found.Name);
        Assert.AreEqual("ann@x.com", found.Email);
        Assert.AreEqual("", error_message);
    }

    [TestMethod]
    public void FindMember_StaffWithExistingEmail_ReturnsTheMember()
    {
        Member? found = _service.FindMember(Role.Staff, "ann@x.com", out string error_message);

        Assert.IsNotNull(found);
        Assert.AreEqual("Ann Lee", found.Name);
        Assert.AreEqual("", error_message);
    }

    // the refusal message is the same one GetAllMembers gives, so the two stay consistent
    [TestMethod]
    public void FindMember_Customer_IsRefused()
    {
        _service.GetAllMembers(Role.Customer, out string expected_message);

        Member? found = _service.FindMember(Role.Customer, "ann@x.com", out string error_message);

        Assert.IsNull(found);
        Assert.AreEqual(expected_message, error_message);
    }

    // guests are refused too, with the same message
    [TestMethod]
    public void FindMember_Guest_IsRefused()
    {
        _service.GetAllMembers(Role.Guest, out string expected_message);

        Member? found = _service.FindMember(Role.Guest, "ann@x.com", out string error_message);

        Assert.IsNull(found);
        Assert.AreEqual(expected_message, error_message);
    }

    [TestMethod]
    public void FindMember_EmailInOtherCase_ReturnsTheMember()
    {
        Member? found = _service.FindMember(Role.Admin, "ANN@X.COM", out string error_message);

        Assert.IsNotNull(found);
        Assert.AreEqual("Ann Lee", found.Name);
        Assert.AreEqual("", error_message);
    }

    [TestMethod]
    public void FindMember_EmailWithSpacesAround_ReturnsTheMember()
    {
        Member? found = _service.FindMember(Role.Admin, "  ann@x.com  ", out string error_message);

        Assert.IsNotNull(found);
        Assert.AreEqual("Ann Lee", found.Name);
        Assert.AreEqual("", error_message);
    }

    [TestMethod]
    public void FindMember_UnknownEmail_ReturnsNullWithMessage()
    {
        Member? found = _service.FindMember(Role.Admin, "nobody@x.com", out string error_message);

        Assert.IsNull(found);
        Assert.AreEqual(NOT_FOUND_MESSAGE, error_message);
    }

    // an empty email never matches anything, even when members exist
    [TestMethod]
    public void FindMember_EmptyOrSpacesEmail_NeverMatches()
    {
        Member? found_empty = _service.FindMember(Role.Admin, "", out string empty_message);
        Member? found_spaces = _service.FindMember(Role.Admin, "   ", out string spaces_message);

        Assert.IsNull(found_empty);
        Assert.AreEqual(NOT_FOUND_MESSAGE, empty_message);
        Assert.IsNull(found_spaces);
        Assert.AreEqual(NOT_FOUND_MESSAGE, spaces_message);
    }

    [TestMethod]
    public void FindMember_NullEmail_ReturnsNullNotCrashed()
    {
        Member? found = _service.FindMember(Role.Admin, null!, out string error_message);

        Assert.IsNull(found);
        Assert.AreEqual(NOT_FOUND_MESSAGE, error_message);
    }

    [TestMethod]
    public void FindMember_SeveralMembers_ReturnsTheRightOne()
    {
        Member? found = _service.FindMember(Role.Admin, "bob@x.com", out _);

        Assert.IsNotNull(found);
        Assert.AreEqual("Bob Ray", found.Name);
    }

    // only the whole email matches, a part of it does not
    [TestMethod]
    public void FindMember_PartOfAnEmail_DoesNotMatch()
    {
        Member? found = _service.FindMember(Role.Admin, "ann@x", out string error_message);

        Assert.IsNull(found);
        Assert.AreEqual(NOT_FOUND_MESSAGE, error_message);
    }
}
