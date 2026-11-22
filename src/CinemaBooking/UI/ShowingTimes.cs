// short texts about a showing for the film page: how soon it starts and how full it is (owner: divyesh)
// kept out of the form so they can be tested without opening a window. "now" is given so tests can fix the clock
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class ShowingTimes
    {
        private const int MINUTES_PER_HOUR = 60;

        // only showings later today get a countdown, other days give an empty text
        // the minutes are rounded up so 30 seconds left never shows as 0 min
        public static string StartsIn(DateTime starts_at, DateTime now)
        {
            if (starts_at <= now)
            {
                return "started";
            }

            if (starts_at.Date != now.Date)
            {
                return "";
            }

            int minutes_left = (int)Math.Ceiling((starts_at - now).TotalMinutes);
            int hours = minutes_left / MINUTES_PER_HOUR;
            int minutes = minutes_left % MINUTES_PER_HOUR;

            if (hours == 0)
            {
                return "starts in " + minutes + " min";
            }

            if (minutes == 0)
            {
                return "starts in " + hours + " h";
            }

            return "starts in " + hours + " h " + minutes + " min";
        }

        // taken seats out of all the seats, from 0 (empty) to 1 (sold out)
        public static double FillFraction(Showing showing)
        {
            int all_seats = showing.SeatGrid.Rows * showing.SeatGrid.SeatsPerRow;

            if (all_seats <= 0)
            {
                return 0;
            }

            return (double)showing.SeatGrid.ListTakenSeats().Length / all_seats;
        }
    }
}
