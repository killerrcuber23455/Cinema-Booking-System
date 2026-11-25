// the good messages of the sign in and sign up forms, in one place so the landing window and the booking page say the same (owner: divyesh)
namespace CinemaBooking.UI
{
    public static class AuthText
    {
        // shown on the sign in form after an account was made
        public static string AccountCreatedNotice(string email)
        {
            return "Account created for " + email + ", sign in now";
        }

        // shown on the sign in form after the first admin account was made
        public static string AdminCreatedNotice(string email)
        {
            return "Admin account created for " + email + ", sign in now";
        }
    }
}
