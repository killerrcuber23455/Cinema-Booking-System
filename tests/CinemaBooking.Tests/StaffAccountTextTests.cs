// tests for the words of the staff accounts page (owner: divyesh)
// written before the code for card 436, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class StaffAccountTextTests
{
    // the role list

    [TestMethod]
    public void RoleOptions_AreStaffThenAdmin()
    {
        string[] expected = { "Staff", "Admin" };

        CollectionAssert.AreEqual(expected, StaffAccountText.RoleOptions());
    }

    // the text chosen in the role list

    [TestMethod]
    public void ParseRole_Staff_ReturnsStaff()
    {
        Assert.AreEqual(Role.Staff, StaffAccountText.ParseRole("Staff"));
    }

    [TestMethod]
    public void ParseRole_Admin_ReturnsAdmin()
    {
        Assert.AreEqual(Role.Admin, StaffAccountText.ParseRole("Admin"));
    }

    [TestMethod]
    public void ParseRole_NullOrEmptyText_ReturnsNull()
    {
        Assert.IsNull(StaffAccountText.ParseRole(null));
        Assert.IsNull(StaffAccountText.ParseRole(""));
        Assert.IsNull(StaffAccountText.ParseRole("   "));
    }

    [TestMethod]
    public void ParseRole_CustomerGuestOrANumber_ReturnsNull()
    {
        Assert.IsNull(StaffAccountText.ParseRole("Customer"));
        Assert.IsNull(StaffAccountText.ParseRole("Guest"));
        Assert.IsNull(StaffAccountText.ParseRole("2"));
    }

    [TestMethod]
    public void ParseRole_IgnoresCapitalLettersAndSpaces()
    {
        Assert.AreEqual(Role.Admin, StaffAccountText.ParseRole("  admin "));
        Assert.AreEqual(Role.Staff, StaffAccountText.ParseRole("STAFF"));
    }

    // the message after an account is made

    [TestMethod]
    public void SuccessMessage_Staff_NamesTheRoleAndTheEmail()
    {
        Assert.AreEqual("Staff account created for ann@x.com", StaffAccountText.SuccessMessage("ann@x.com", Role.Staff));
    }

    [TestMethod]
    public void SuccessMessage_Admin_NamesTheRoleAndTheEmail()
    {
        Assert.AreEqual("Admin account created for bob@x.com", StaffAccountText.SuccessMessage("bob@x.com", Role.Admin));
    }
}
