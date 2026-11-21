// temporary demo showings so the film page and seat map can be seen before the database is connected (owner: divyesh)
// only used in a debug build, the real showings will come from the database
using System;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking
{
    internal static class DemoShowings
    {
        // film ids 1 to 5 are the first films in data/films_sample.csv
        public static void Load(ShowingTable showing_table)
        {
            DateTime tomorrow = DateTime.Today.AddDays(1);

            showing_table.CreateShowing(1, 1, 1, tomorrow.AddHours(14), 200m, 8, 12);
            showing_table.CreateShowing(2, 1, 2, tomorrow.AddHours(18).AddMinutes(30), 250m, 8, 12);
            showing_table.CreateShowing(3, 1, 1, tomorrow.AddDays(1).AddHours(20), 250m, 8, 12);
            showing_table.CreateShowing(4, 2, 1, tomorrow.AddHours(16), 200m, 8, 12);
            showing_table.CreateShowing(5, 3, 2, tomorrow.AddHours(21), 250m, 8, 12);
            showing_table.CreateShowing(6, 4, 1, tomorrow.AddHours(19), 250m, 8, 12);
            showing_table.CreateShowing(7, 5, 2, tomorrow.AddHours(20), 300m, 8, 12);

            // a few seats already taken on showing 2 so the seat map shows the three colours
            Showing? busy_showing = showing_table.FindShowing(2);
            if (busy_showing != null)
            {
                busy_showing.SeatGrid.BookSeats(new string[] { "C5", "C6", "D5", "D6", "F1" });
            }
        }
    }
}
