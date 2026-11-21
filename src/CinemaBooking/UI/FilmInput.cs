// turns what the admin typed into a film, or says what is wrong in plain words (owner: divyesh)
using System;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class FilmInput
    {
        // stub for card 351, the real code comes in the green step
        public static bool TryRead(string id_text, string title, string genre, string year_text,
            string duration_text, string age_rating_text, string description, string poster_file,
            out Film? film, out string error_message)
        {
            throw new NotImplementedException();
        }
    }
}
