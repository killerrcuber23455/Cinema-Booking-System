// what the landing window leads to (owner: divyesh)
// the landing window is the first window of the program: it shows before the main window and gives one choice back.
// the program only asks this class, so the rules are in one place and can be tested
namespace CinemaBooking.UI
{
    // what the user pressed on the landing window, None means the window was closed without a choice
    public enum LandingChoice
    {
        None,
        Guest,
        LogIn,
        SignUp,
        Staff
    }

    // the page the main window opens first
    public enum FirstPage
    {
        Films,
        Login,
        SignUp,
        StaffLogin
    }

    public static class LandingRules
    {
        // red stubs: wrong values on purpose. the real code comes in the green step
        public const string STAFF_NOTICE = "";

        public static bool ShouldOpenMain(LandingChoice choice)
        {
            return true;
        }

        public static FirstPage FirstPageFor(LandingChoice choice)
        {
            return FirstPage.Films;
        }
    }
}
