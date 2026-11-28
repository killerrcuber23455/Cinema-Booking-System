// the text of one screen in the admin screens list (owner: divyesh)
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class ScreenLine
    {
        // for example: Screen 1  -  8 rows x 12 seats  -  96 seats
        public static string Text(CinemaScreen screen)
        {
            return screen.Name
                + "  -  " + Plural(screen.RowCount, "row")
                + " x " + Plural(screen.SeatsPerRow, "seat")
                + "  -  " + Plural(screen.RowCount * screen.SeatsPerRow, "seat");
        }

        // 1 row, 8 rows
        private static string Plural(int count, string word)
        {
            return count == 1 ? "1 " + word : count + " " + word + "s";
        }
    }
}
