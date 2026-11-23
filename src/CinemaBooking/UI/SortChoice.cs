// the choices of the sort dropdown on the films page and the sort key each one means (owner: divyesh)
namespace CinemaBooking.UI
{
    public static class SortChoice
    {
        public const string TITLE_LABEL = "Title (A to Z)";
        public const string YEAR_LABEL = "Year (oldest first)";
        public const string RATING_LABEL = "Age rating (lowest first)";

        // the labels in the order they appear in the dropdown
        public static readonly string[] LABELS = { TITLE_LABEL, YEAR_LABEL, RATING_LABEL };

        // the sort key FilmService and SortHelper understand: "title", "year" or "rating"
        // anything unknown, empty or null gives "title"
        public static string KeyFor(string? label)
        {
            if (label == YEAR_LABEL)
            {
                return "year";
            }

            if (label == RATING_LABEL)
            {
                return "rating";
            }

            return "title";
        }
    }
}
