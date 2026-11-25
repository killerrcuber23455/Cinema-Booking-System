// the two tabs of the page a guest gets from "Sign in to earn points" (owner: divyesh)
namespace CinemaBooking.UI
{
    public enum AuthTab
    {
        SignIn,
        CreateAccount
    }

    public static class GuestSignInRules
    {
        // the tabs from left to right
        public static AuthTab[] Tabs()
        {
            return new AuthTab[] { AuthTab.SignIn, AuthTab.CreateAccount };
        }

        // the words on a tab
        public static string TabLabel(AuthTab tab)
        {
            return tab == AuthTab.SignIn ? "Sign in" : "Create account";
        }
    }
}
