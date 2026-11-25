// the "sign in as" choice of the landing window (owner: divyesh)
// a customer, a staff member and an admin all sign in with an email and a password, the choice on the page
// must fit the real role of the account. the page only asks this class, so the rule is in one place and can be tested
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    // what the user says they are when they sign in
    public enum SignInAs
    {
        Customer,
        Staff,
        Admin
    }

    public static class SignInRules
    {
        // the choices the page offers, in the order of the list
        public static SignInAs[] Choices()
        {
            return new SignInAs[0];
        }

        // true when an account with this real role may sign in with this choice
        public static bool IsAllowed(SignInAs choice, Role actual_role)
        {
            return false;
        }

        // the sentence shown when the choice does not fit the account, empty when it fits
        public static string RefusalMessage(SignInAs choice, Role actual_role)
        {
            return "";
        }

        // the choice the list starts on for the button pressed on the landing window
        public static SignInAs StartingChoice(LandingChoice pressed)
        {
            return SignInAs.Customer;
        }
    }
}
