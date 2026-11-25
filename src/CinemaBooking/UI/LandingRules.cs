// what the landing window leads to (owner: divyesh)
// the landing window is the first window of the program: it shows before the main window and gives one choice back.
// the program only asks this class, so the rules are in one place and can be tested
using CinemaBooking.Models;

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


    // how the landing window ended
    public enum LandingKind
    {
        Closed,
        Guest,
        SignedIn
    }

    // what the landing window gives back to the program: closed, a guest, or someone who signed in on the landing window
    public class LandingOutcome
    {
        public LandingKind Kind { get; }
        public Role Role { get; }
        public Member? Member { get; }
        public string Email { get; }

        private LandingOutcome(LandingKind kind, Role role, Member? member, string email)
        {
            Kind = kind;
            Role = role;
            Member = member;
            Email = email;
        }

        public static LandingOutcome Closed()
        {
            return new LandingOutcome(LandingKind.Closed, Role.Guest, null, "");
        }

        public static LandingOutcome Guest()
        {
            return Closed();
        }

        public static LandingOutcome SignedIn(Role role, Member? member, string email)
        {
            return Closed();
        }
    }

    public static class LandingRules
    {
        // the program opens the main window for a guest and for someone who signed in, never when the window was closed
        public static bool ShouldOpenMain(LandingOutcome outcome)
        {
            return false;
        }

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
