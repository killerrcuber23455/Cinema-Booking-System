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
        // this is only a stub for the red step, it says yes and changes nothing
        public static bool Update(FilmService film_service, Film? edited_film, Role role, out string error_message)
        {
            error_message = "";
            return true;
        }
    }
}
