// who sees the "Load film file" button on the films page, and what an empty films page says (owner: divyesh)
// the films page only asks this class, so the role rule is in one place and can be tested
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class FilmPageRules
    {
        // the words that tell staff what to click when there are no films yet
        private const string STAFF_EMPTY_TEXT = "no films yet, click \"Load film file\" and choose the film file";

        // everyone else is only told that films will come
        private const string OTHER_EMPTY_TEXT = "no films yet, please come back soon";

        // true when this role may load the film file
        // only staff and admin change films, a role that is not known is treated like a guest
        public static bool CanLoadFilms(Role role)
        {
            if (!Enum.IsDefined<Role>(role))
            {
                return false;
            }

            return role == Role.Staff || role == Role.Admin;
        }

        // the text shown on the films page when there are no films
        // a guest must not be told to click a button that they cannot see
        public static string EmptyFilmsText(Role role)
        {
            if (CanLoadFilms(role))
            {
                return STAFF_EMPTY_TEXT;
            }

            return OTHER_EMPTY_TEXT;
        }
    }
}
