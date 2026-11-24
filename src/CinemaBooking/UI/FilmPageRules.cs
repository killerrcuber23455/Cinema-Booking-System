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

        // the films shown big at the top of the films page: the newest first, the same year in title order (UI upgrade step 2)
        // the list it is given is not changed, the films page still shows it in its own order
        public static Film[] FeaturedFilms(Film[] films, int max_count)
        {
            if (films == null || max_count <= 0)
            {
                return new Film[0];
            }

            Film[] ordered = new Film[films.Length];
            Array.Copy(films, ordered, films.Length);

            // insertion sort: the list is a handful of films, and equal films keep their place
            for (int next_index = 1; next_index < ordered.Length; next_index++)
            {
                Film moving = ordered[next_index];
                int place = next_index - 1;

                while (place >= 0 && ComesBefore(moving, ordered[place]))
                {
                    ordered[place + 1] = ordered[place];
                    place--;
                }

                ordered[place + 1] = moving;
            }

            int keep_count = Math.Min(max_count, ordered.Length);
            Film[] featured = new Film[keep_count];
            Array.Copy(ordered, featured, keep_count);
            return featured;
        }

        // a newer film goes first, in the same year the title decides (A before Z, capitals do not matter)
        private static bool ComesBefore(Film first, Film second)
        {
            if (first.Year != second.Year)
            {
                return first.Year > second.Year;
            }

            return string.Compare(first.Title, second.Title, StringComparison.OrdinalIgnoreCase) < 0;
        }
    }
}
