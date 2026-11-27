// the text of one member in the Members list, and the hint for the find box (owner: divyesh)
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class MemberLine
    {
        // for example: Ann Lee  -  ann@x.com  -  5712 3456  -  Silver  -  120 points
        public static string Text(Member member)
        {
            string points_text = member.Points == 1 ? "1 point" : member.Points + " points";

            return member.Name
                + "  -  " + member.Email
                + "  -  " + member.Phone
                + "  -  " + member.Tier
                + "  -  " + points_text;
        }

        // an empty find box gets a hint, a box with an email gets no hint (an empty text)
        public static string FindHint(string email)
        {
            return string.IsNullOrWhiteSpace(email) ? "Type an email to find a member" : "";
        }
    }
}
