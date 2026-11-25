// what the landing window leads to (owner: divyesh)
// the landing window is the first window of the program: it shows before the main window. the user can continue
// as a guest, or sign in, sign up or set up the first admin inside the landing window itself. it gives back one outcome.
// the program only asks this class, so the rules are in one place and can be tested
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    // the button pressed on the landing window, it tells the window which form to open
    // None means nothing is pressed
    public enum LandingChoice
    {
        None,
        Guest,
        LogIn,
        SignUp,
        Staff
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

        // the window was closed without a choice
        public static LandingOutcome Closed()
        {
            return new LandingOutcome(LandingKind.Closed, Role.Guest, null, "");
        }

        // the user continues as a guest
        public static LandingOutcome Guest()
        {
            return new LandingOutcome(LandingKind.Guest, Role.Guest, null, "");
        }

        // the user signed in, the member is null for staff and admin
        public static LandingOutcome SignedIn(Role role, Member? member, string email)
        {
            return new LandingOutcome(LandingKind.SignedIn, role, member, email);
        }
    }

    public static class LandingRules
    {
        // the program opens the main window for a guest and for someone who signed in, never when the window was closed
        public static bool ShouldOpenMain(LandingOutcome outcome)
        {
            return outcome.Kind != LandingKind.Closed;
        }
    }
}
