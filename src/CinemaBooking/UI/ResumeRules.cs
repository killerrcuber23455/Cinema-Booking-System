// what happens when a guest signs in or signs up in the middle of a booking (owner: divyesh)
// the guest keeps the film, the showing and the seats: after the sign in the booking summary opens again, now as a member
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
        // the page the main window shows after a sign in made from the booking summary.
        // only a customer who is a member can go on with the booking, staff and admin do not book seats
        public static ResumePage PageAfterSignIn(Role role, Member? member, bool has_pending_booking)
        {
            bool is_member = role == Role.Customer && member != null;

            return is_member && has_pending_booking ? ResumePage.BookingSummary : ResumePage.Films;
        }

        // the small message after the sign in
        public static string SignedInNotice(ResumePage page, string email)
        {
            if (page == ResumePage.BookingSummary)
            {
                return "Signed in as " + email + ", your seats are kept";
            }

            return "Signed in as " + email;
        }
    }
}
