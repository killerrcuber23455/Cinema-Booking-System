// turns what the admin typed into a film, or says what is wrong in plain words (owner: divyesh)
// every box is checked, so the admin sees all the wrong boxes at once and not only the first one.
// the numbers are read with InputCheck, the film rules in Film check the values,
// so the admin page shows the same words as the rest of the program
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class FilmInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_ID = 0;
        public const int BOX_TITLE = 1;
        public const int BOX_GENRE = 2;
        public const int BOX_YEAR = 3;
        public const int BOX_DURATION = 4;
        public const int BOX_AGE_RATING = 5;
        public const int BOX_DESCRIPTION = 6;
        public const int BOX_POSTER = 7;
        public const int BOX_COUNT = 8;

        // red stub: gives a wrong value on purpose, the real code comes in the green step
        public static string[] CheckBoxes(string id_text, string title, string genre, string year_text,
            string duration_text, string age_rating_text, string description, string poster_file)
        {
            string[] messages = new string[BOX_COUNT];
            for (int index = 0; index < BOX_COUNT; index++)
            {
                messages[index] = "stub";
            }
            return messages;
        }

        public static bool TryRead(string id_text, string title, string genre, string year_text,
            string duration_text, string age_rating_text, string description, string poster_file,
            out Film? film, out string error_message)
        {
            film = null;

            // spaces around the text are removed, a missing value counts as empty
            string clean_title = (title ?? "").Trim();
            string clean_genre = (genre ?? "").Trim();
            string clean_description = (description ?? "").Trim();
            string clean_poster = (poster_file ?? "").Trim();

            // one line for each wrong box, in the order the boxes are on the page
            string all_errors = "";
            string message;

            int id = 0;
            if (!InputCheck.TryReadWholeNumber(id_text, "id", out id, out message)
                || !Film.IsValidId(id, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            if (!Film.IsValidTitle(clean_title, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            if (!Film.IsValidGenre(clean_genre, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            int year = 0;
            if (!InputCheck.TryReadWholeNumber(year_text, "year", out year, out message)
                || !Film.IsValidYear(year, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            int duration_minutes = 0;
            if (!InputCheck.TryReadWholeNumber(duration_text, "duration", out duration_minutes, out message)
                || !Film.IsValidDuration(duration_minutes, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            int age_rating = 0;
            if (!InputCheck.TryReadWholeNumber(age_rating_text, "age rating", out age_rating, out message)
                || !Film.IsValidAgeRating(age_rating, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            if (!Film.IsValidDescription(clean_description, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            if (!Film.IsValidPosterFile(clean_poster, out message))
            {
                all_errors += message.TrimEnd('\n') + "\n";
            }

            if (all_errors != "")
            {
                error_message = all_errors.TrimEnd('\n');
                return false;
            }

            film = new Film(id, clean_title, clean_genre, year, duration_minutes, age_rating,
                clean_description, clean_poster);
            error_message = "";
            return true;
        }
    }
}
