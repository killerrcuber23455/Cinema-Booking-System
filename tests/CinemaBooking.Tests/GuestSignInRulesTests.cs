// tests for the two tabs of the guest sign in page (owner: divyesh)
// written before the code for card 445, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class GuestSignInRulesTests
{
    [TestMethod]
    public void Tabs_AreSignInThenCreateAccount()
    {
        AuthTab[] expected = { AuthTab.SignIn, AuthTab.CreateAccount };

        CollectionAssert.AreEqual(expected, GuestSignInRules.Tabs());
    }

    [TestMethod]
    public void TabLabel_EachTabHasItsOwnWords()
    {
        Assert.AreEqual("Sign in", GuestSignInRules.TabLabel(AuthTab.SignIn));
        Assert.AreEqual("Create account", GuestSignInRules.TabLabel(AuthTab.CreateAccount));
    }
}
