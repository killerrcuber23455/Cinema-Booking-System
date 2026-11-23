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

        // one message for each box and an empty text for a box that is fine
        // the page shows each message under its box. a missing value counts as empty
        public static string[] CheckBoxes(string id_text, string title, string genre, string year_text,
            string duration_text, string age_rating_text, string description, string poster_file)
        {
            string clean_title = (title ?? "").Trim();
            string clean_genre = (genre ?? "").Trim();
            string clean_description = (description ?? "").Trim();
            string clean_poster = (poster_file ?? "").Trim();

            string[] messages = new string[BOX_COUNT];
            for (int index = 0; index < BOX_COUNT; index++)
            {
                messages[index] = "";
            }

            string message;

            int id = 0;
            if (!InputCheck.TryReadWholeNumber(id_text ?? "", "id", out id, out message)
                || !Film.IsValidId(id, out message))
            {
                messages[BOX_ID] = message.TrimEnd('\n');
            }

            if (!Film.IsValidTitle(clean_title, out message))
            {
                messages[BOX_TITLE] = message.TrimEnd('\n');
            }

            if (!Film.IsValidGenre(clean_genre, out message))
            {
                messages[BOX_GENRE] = message.TrimEnd('\n');
            }

            int year = 0;
            if (!InputCheck.TryReadWholeNumber(year_text ?? "", "year", out year, out message)
                || !Film.IsValidYear(year, out message))
            {
                messages[BOX_YEAR] = message.TrimEnd('\n');
            }

            int duration_minutes = 0;
            if (!InputCheck.TryReadWholeNumber(duration_text ?? "", "duration", out duration_minutes, out message)
                || !Film.IsValidDuration(duration_minutes, out message))
            {
                messages[BOX_DURATION] = message.TrimEnd('\n');
            }

            int age_rating = 0;
            if (!InputCheck.TryReadWholeNumber(age_rating_text ?? "", "age rating", out age_rating, out message)
                || !Film.IsValidAgeRating(age_rating, out message))
            {
                messages[BOX_AGE_RATING] = message.TrimEnd('\n');
            }

            if (!Film.IsValidDescription(clean_description, out message))
            {
                messages[BOX_DESCRIPTION] = message.TrimEnd('\n');
            }

            if (!Film.IsValidPosterFile(clean_poster, out message))
            {
                messages[BOX_POSTER] = message.TrimEnd('\n');
            }

            return messages;
        }

        public static bool TryRead(string id_text, string title, string genre, string year_text,
            string duration_text, string age_rating_text, string description, string poster_file,
            out Film? film, out string error_message)
        {
            film = null;

            // one line for each wrong box, in the order the boxes are on the page
            string all_errors = "";
            string[] box_messages = CheckBoxes(id_text, title, genre, year_text, duration_text,
                age_rating_text, description, poster_file);
            for (int index = 0; index < box_messages.Length; index++)
            {
                if (box_messages[index] != "")
                {
                    all_errors += box_messages[index] + "\n";
                }
            }

            if (all_errors != "")
            {
                error_message = all_errors.TrimEnd('\n');
                return false;
            }

            // every box is good, so the numbers can be read again without a message
            string ignored;
            int id;
            int year;
            int duration_minutes;
            int age_rating;
            InputCheck.TryReadWholeNumber(id_text, "id", out id, out ignored);
            InputCheck.TryReadWholeNumber(year_text, "year", out year, out ignored);
            InputCheck.TryReadWholeNumber(duration_text, "duration", out duration_minutes, out ignored);
            InputCheck.TryReadWholeNumber(age_rating_text, "age rating", out age_rating, out ignored);

            film = new Film(id, (title ?? "").Trim(), (genre ?? "").Trim(), year, duration_minutes, age_rating,
                (description ?? "").Trim(), (poster_file ?? "").Trim());
            error_message = "";
            return true;
        }
    }
}
