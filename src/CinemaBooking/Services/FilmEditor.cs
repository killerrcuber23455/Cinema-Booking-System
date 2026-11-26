// the rule for editing a film: only an admin, the film must exist, the new details must be valid (owner: divyesh)
// FilmService only has add and remove, so an edit is a remove and an add that puts the old film back if it fails
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public static class FilmEditor
    {
        public const string NOT_ADMIN_TEXT = "Only an admin can edit a film";
        public const string NO_FILM_GIVEN_TEXT = "No film was given";

        public static string NotFoundText(int film_id)
        {
            return "No film with id " + film_id + " was found";
        }

        // replaces the film that has the same id with the edited film, otherwise false with the reason
        public static bool Update(FilmService film_service, Film? edited_film, Role role, out string error_message)
        {
            if (role != Role.Admin)
            {
                error_message = NOT_ADMIN_TEXT;
                return false;
            }

            if (edited_film == null)
            {
                error_message = NO_FILM_GIVEN_TEXT;
                return false;
            }

            // the new details are checked first, so a bad edit never touches the old film
            string film_errors;

            if (!edited_film.IsValid(out film_errors))
            {
                error_message = film_errors;
                return false;
            }

            Film? old_film = film_service.FindById(edited_film.Id);

            if (old_film == null)
            {
                error_message = NotFoundText(edited_film.Id);
                return false;
            }

            if (!film_service.RemoveFilm(old_film.Id, role, out error_message))
            {
                return false;
            }

            if (!film_service.AddFilm(edited_film, role, out error_message))
            {
                // give the old film back so nothing is lost
                string ignored;
                film_service.AddFilm(old_film, role, out ignored);
                return false;
            }

            error_message = "";
            return true;
        }
    }
}
