// tests for the good messages of the sign in form (owner: divyesh)
// written before the code for card 445, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class AuthTextTests
{
    [TestMethod]
    public void AccountCreatedNotice_NamesTheEmailAndSaysToSignIn()
    {
        Assert.AreEqual("Account created for ann@x.com, sign in now", AuthText.AccountCreatedNotice("ann@x.com"));
    }

    [TestMethod]
    public void AdminCreatedNotice_NamesTheEmailAndSaysToSignIn()
    {
        Assert.AreEqual("Admin account created for ann@x.com, sign in now", AuthText.AdminCreatedNotice("ann@x.com"));
    }
}
