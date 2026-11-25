// what happens when a guest signs in or signs up in the middle of a booking (owner: divyesh)
// the guest keeps the film, the showing and the seats: after the sign in the booking summary opens again, now as a member.
// STUB for the red step: nothing is kept yet, so the tests fail first
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public enum ResumePage
    {
        Films,
        BookingSummary
    }

    public static class ResumeRules
    {
        // the page the main window shows after a sign in made from the booking summary
        public static ResumePage PageAfterSignIn(Role role, Member? member, bool has_pending_booking)
        {
            return ResumePage.Films;
        }

        // the small message after the sign in
        public static string SignedInNotice(ResumePage page, string email)
        {
            return "";
        }
    }
}
