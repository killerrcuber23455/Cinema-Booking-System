// who sees the "Load film file" button on the films page, and what an empty films page says (owner: divyesh)
// the films page only asks this class, so the role rule is in one place and can be tested
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class FilmPageRules
    {
        // the words that tell staff what to click when there are no films yet
        private const string STAFF_EMPTY_TEXT = "no films yet, click \"Load film file\" and choose the film file";

        // true when this role may load the film file
        // this is only a stub for the red step, it says yes to everyone
        public static bool CanLoadFilms(Role role)
        {
            return true;
        }

        // the text shown on the films page when there are no films
        // this is only a stub for the red step, it gives staff text to everyone
        public static string EmptyFilmsText(Role role)
        {
            return STAFF_EMPTY_TEXT;
        }
    }
}
