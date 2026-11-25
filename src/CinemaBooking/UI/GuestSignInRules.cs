// the two tabs of the page a guest gets from "Sign in to earn points" (owner: divyesh)
// STUB for the red step: the tabs have no words yet, so the tests fail first
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
            return new AuthTab[0];
        }

        // the words on a tab
        public static string TabLabel(AuthTab tab)
        {
            return "";
        }
    }
}
