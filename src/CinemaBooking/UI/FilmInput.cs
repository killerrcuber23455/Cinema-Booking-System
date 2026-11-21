// turns what the admin typed into a film, or says what is wrong in plain words (owner: divyesh)
// the numbers are read with InputCheck, then the film rules in Film check the values,
// so the admin page shows the same words as the rest of the program
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class FilmInput
    {
        public static bool TryRead(string id_text, string title, string genre, string year_text,
            string duration_text, string age_rating_text, string description, string poster_file,
            out Film? film, out string error_message)
        {
            film = null;

            // the numbers are read in page order, the first one that cannot be read is the one shown
            if (!InputCheck.TryReadWholeNumber(id_text, "id", out int id, out error_message)
                || !InputCheck.TryReadWholeNumber(year_text, "year", out int year, out error_message)
                || !InputCheck.TryReadWholeNumber(duration_text, "duration", out int duration_minutes, out error_message)
                || !InputCheck.TryReadWholeNumber(age_rating_text, "age rating", out int age_rating, out error_message))
            {
                return false;
            }

            // spaces around the text are removed, a missing value counts as empty
            Film new_film = new Film(
                id,
                (title ?? "").Trim(),
                (genre ?? "").Trim(),
                year,
                duration_minutes,
                age_rating,
                (description ?? "").Trim(),
                (poster_file ?? "").Trim());

            // Film gathers every broken rule into one message
            if (!new_film.IsValid(out error_message))
            {
                return false;
            }

            film = new_film;
            error_message = "";
            return true;
        }
    }
}
