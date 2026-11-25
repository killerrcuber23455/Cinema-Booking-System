// tests for the "sign in as" choice of the landing window (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SignInRulesTests
{
    // the list

    [TestMethod]
    public void Choices_AreCustomerStaffAdmin()
    {
        SignInAs[] expected = { SignInAs.Customer, SignInAs.Staff, SignInAs.Admin };

        CollectionAssert.AreEqual(expected, SignInRules.Choices());
    }

    // which choice fits which account

    [TestMethod]
    public void IsAllowed_CustomerAccount_OnlyWithCustomer()
    {
        Assert.IsTrue(SignInRules.IsAllowed(SignInAs.Customer, Role.Customer));
        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Staff, Role.Customer));
        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Admin, Role.Customer));
    }

    [TestMethod]
    public void IsAllowed_StaffAccount_OnlyWithStaff()
    {
        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Customer, Role.Staff));
        Assert.IsTrue(SignInRules.IsAllowed(SignInAs.Staff, Role.Staff));
        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Admin, Role.Staff));
    }

    [TestMethod]
    public void IsAllowed_AdminAccount_WithStaffOrAdminButNotCustomer()
    {
        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Customer, Role.Admin));
        Assert.IsTrue(SignInRules.IsAllowed(SignInAs.Staff, Role.Admin));
        Assert.IsTrue(SignInRules.IsAllowed(SignInAs.Admin, Role.Admin));
    }

    [TestMethod]
    public void IsAllowed_GuestOrUnknownRole_IsNeverAllowed()
    {
        foreach (SignInAs choice in SignInRules.Choices())
        {
            Assert.IsFalse(SignInRules.IsAllowed(choice, Role.Guest), choice.ToString());
            Assert.IsFalse(SignInRules.IsAllowed(choice, (Role)99), choice.ToString());
        }

        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Customer, Role.Guest));
        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Staff, Role.Guest));
        Assert.IsFalse(SignInRules.IsAllowed(SignInAs.Admin, Role.Guest));
    }

    // the sentence shown when it does not fit

    [TestMethod]
    public void RefusalMessage_WhenTheChoiceFits_IsEmpty()
    {
        Assert.AreEqual("", SignInRules.RefusalMessage(SignInAs.Customer, Role.Customer));
        Assert.AreEqual("", SignInRules.RefusalMessage(SignInAs.Staff, Role.Staff));
        Assert.AreEqual("", SignInRules.RefusalMessage(SignInAs.Staff, Role.Admin));
        Assert.AreEqual("", SignInRules.RefusalMessage(SignInAs.Admin, Role.Admin));
    }

    [TestMethod]
    public void RefusalMessage_CustomerAccountWithStaff_NamesCustomer()
    {
        Assert.AreEqual("this is a customer account, choose Customer",
            SignInRules.RefusalMessage(SignInAs.Staff, Role.Customer));
    }

    [TestMethod]
    public void RefusalMessage_StaffAccountWithAdmin_NamesStaff()
    {
        Assert.AreEqual("this is a staff account, choose Staff",
            SignInRules.RefusalMessage(SignInAs.Admin, Role.Staff));
    }

    [TestMethod]
    public void RefusalMessage_AdminAccountWithCustomer_NamesStaffOrAdmin()
    {
        Assert.AreEqual("this is an admin account, choose Staff or Admin",
            SignInRules.RefusalMessage(SignInAs.Customer, Role.Admin));
    }

    // the choice the list starts on

    [TestMethod]
    public void StartingChoice_StaffButton_IsStaff()
    {
        Assert.AreEqual(SignInAs.Staff, SignInRules.StartingChoice(LandingChoice.Staff));
    }

    [TestMethod]
    public void StartingChoice_AnyOtherButton_IsCustomer()
    {
        Assert.AreEqual(SignInAs.Customer, SignInRules.StartingChoice(LandingChoice.LogIn));
        Assert.AreEqual(SignInAs.Customer, SignInRules.StartingChoice(LandingChoice.SignUp));
        Assert.AreEqual(SignInAs.Customer, SignInRules.StartingChoice(LandingChoice.Guest));
        Assert.AreEqual(SignInAs.Customer, SignInRules.StartingChoice(LandingChoice.None));
    }
}
