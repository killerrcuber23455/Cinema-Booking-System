// the rules for adding and removing showings from the admin page (owner: divyesh)
// the page only calls this class and shows its message. the checks are in the order the admin would fix them
using System.Globalization;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class ShowingAdminService
    {
        // every showing has the same seat grid, 8 rows of 12 seats
        private const int ROWS = 8;
        private const int SEATS_PER_ROW = 12;

        private const string START_FORMAT = "yyyy-MM-dd HH:mm";

        // the same table the booking service and the showing service use
        private readonly ShowingTable _showing_table;

        // tells if a film id exists, so a showing can not be added for a film that is not there
        private readonly Func<int, bool> _film_exists;

        public ShowingAdminService(ShowingTable showing_table, Func<int, bool> film_exists)
        {
            _showing_table = showing_table;
            _film_exists = film_exists;
        }

        // adds a showing with a new empty seat grid. returns false and says why if a rule is broken
        public bool AddShowing(int showing_id, int film_id, int screen_id, DateTime starts_at, decimal price,
            Role role, out string error_message)
        {
            if (!CanChangeShowings(role, out error_message))
            {
                return false;
            }

            if (!_film_exists(film_id))
            {
                error_message = "film " + film_id + " does not exist";
                return false;
            }

            if (_showing_table.FindShowing(showing_id) != null)
            {
                error_message = "showing id " + showing_id + " is already used";
                return false;
            }

            // a screen can only show one film at a time, so the same start on the same screen is a clash
            Showing[] all_showings = _showing_table.ListAll();
            for (int showing_index = 0; showing_index < all_showings.Length; showing_index++)
            {
                if (all_showings[showing_index].ScreenId == screen_id
                    && all_showings[showing_index].StartsAt == starts_at)
                {
                    error_message = "screen " + screen_id + " already has a showing at "
                        + starts_at.ToString(START_FORMAT, CultureInfo.InvariantCulture);
                    return false;
                }
            }

            _showing_table.CreateShowing(showing_id, film_id, screen_id, starts_at, price, ROWS, SEATS_PER_ROW);
            error_message = "";
            return true;
        }

        // removes a showing that has no booked seats. returns false and says why if it can not be removed
        public bool RemoveShowing(int showing_id, Role role, out string error_message)
        {
            if (!CanChangeShowings(role, out error_message))
            {
                return false;
            }

            Showing? showing = _showing_table.FindShowing(showing_id);
            if (showing == null)
            {
                error_message = "showing id " + showing_id + " was not found";
                return false;
            }

            // customers have paid for these seats, so a showing with booked seats stays
            if (showing.SeatGrid.ListTakenSeats().Length > 0)
            {
                error_message = "showing id " + showing_id + " has booked seats and cannot be removed";
                return false;
            }

            _showing_table.RemoveShowing(showing_id);
            error_message = "";
            return true;
        }

        // all the showings, earliest first
        public Showing[] ListShowings()
        {
            Showing[] found = _showing_table.ListAll();

            // insertion sort by start time, the same way the showing service sorts
            for (int sorted_end = 1; sorted_end < found.Length; sorted_end++)
            {
                Showing current = found[sorted_end];
                int compare_index = sorted_end - 1;

                while (compare_index >= 0 && found[compare_index].StartsAt > current.StartsAt)
                {
                    found[compare_index + 1] = found[compare_index];
                    compare_index--;
                }
                found[compare_index + 1] = current;
            }

            return found;
        }

        // only staff and admins change showings
        private static bool CanChangeShowings(Role role, out string error_message)
        {
            if (role == Role.Staff || role == Role.Admin)
            {
                error_message = "";
                return true;
            }

            error_message = "only staff and admins can change showings";
            return false;
        }
    }
}
