// the rules for adding and removing showings from the admin page (owner: divyesh)
// EMPTY STUB for the red step: it has the real names, but it does not do anything yet
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class ShowingAdminService
    {
        // the same table the booking service and the showing service use
        private readonly ShowingTable _showing_table;

        // tells if a film id exists, so a showing can not be added for a film that is not there
        private readonly Func<int, bool> _film_exists;

        public ShowingAdminService(ShowingTable showing_table, Func<int, bool> film_exists)
        {
            _showing_table = showing_table;
            _film_exists = film_exists;
        }

        public bool AddShowing(int showing_id, int film_id, int screen_id, DateTime starts_at, decimal price,
            Role role, out string error_message)
        {
            error_message = "adding a showing is not built yet";
            return false;
        }

        public bool RemoveShowing(int showing_id, Role role, out string error_message)
        {
            error_message = "removing a showing is not built yet";
            return false;
        }

        // all the showings, earliest first
        public Showing[] ListShowings()
        {
            return new Showing[0];
        }
    }
}
