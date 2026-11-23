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
        // shown above the boxes when staff or admin come in from the landing window
        public const string STAFF_NOTICE = "Staff and admin: sign in with your work email and password";

        // closing the landing window without a choice ends the program, every real choice opens the main window
        public static bool ShouldOpenMain(LandingChoice choice)
        {
            return choice != LandingChoice.None;
        }

        // the guest starts on the films, the others start on the page they asked for
        public static FirstPage FirstPageFor(LandingChoice choice)
        {
            switch (choice)
            {
                case LandingChoice.LogIn:
                    return FirstPage.Login;
                case LandingChoice.SignUp:
                    return FirstPage.SignUp;
                case LandingChoice.Staff:
                    return FirstPage.StaffLogin;
                default:
                    return FirstPage.Films;
            }
        }
    }
}
