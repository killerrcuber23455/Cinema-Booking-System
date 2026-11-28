// adds a showing from the admin page and saves it to the database (owner: divyesh)
// ShowingAdminService keeps the rules and the in-memory table, this class also saves to IDatabase
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class ShowingStore
    {
        public const string SAVE_FAILED_TEXT = "The showing could not be saved to the database (the id or the screen time may already be used), so it was not added.";
        public const string REMOVE_FAILED_TEXT = "The showing could not be removed from the database (it may have bookings, even cancelled ones), so it was kept.";

        private readonly IDatabase _database;
        private readonly ShowingAdminService _showing_admin_service;
        private readonly ShowingTable _showing_table;

        public ShowingStore(IDatabase database, ShowingAdminService showing_admin_service, ShowingTable showing_table)
        {
            _database = database;
            _showing_admin_service = showing_admin_service;
            _showing_table = showing_table;
        }

        // removes a showing (staff or admin) from the program and from the database.
        // if the database refuses (for example a cancelled booking still uses the showing, or the database is down)
        // the showing is put back, so the program and the database say the same
        public bool RemoveShowing(int showing_id, Role role, out string error_message)
        {
            Showing? showing = _showing_table.FindShowing(showing_id);

            // ShowingAdminService checks the role, that the showing exists and that it has no booked seats first
            if (!_showing_admin_service.RemoveShowing(showing_id, role, out error_message))
            {
                return false;
            }

            bool is_deleted;
            try
            {
                is_deleted = _database.DeleteShowing(showing_id);
            }
            catch (Exception)
            {
                is_deleted = false;
            }

            if (!is_deleted)
            {
                // the database did not forget it, so the program must keep it too
                if (showing != null)
                {
                    _showing_table.AddShowing(showing);
                }

                error_message = REMOVE_FAILED_TEXT;
                return false;
            }

            error_message = "";
            return true;
        }

        // adds a showing (staff or admin) to the program and saves it to the database.
        // if the database refuses (id or screen time already used, or the database is down) the showing is taken back out
        public bool AddShowing(int showing_id, int film_id, int screen_id, DateTime starts_at, decimal price,
            Role role, out string error_message)
        {
            // ShowingAdminService checks the role, the film, the id and the clash first, nothing is saved if it refuses
            if (!_showing_admin_service.AddShowing(showing_id, film_id, screen_id, starts_at, price, role, out error_message))
            {
                return false;
            }

            Showing? added_showing = _showing_table.FindShowing(showing_id);

            bool is_saved;
            try
            {
                is_saved = added_showing != null && _database.SaveShowing(added_showing);
            }
            catch (Exception)
            {
                is_saved = false;
            }

            if (!is_saved)
            {
                // the database did not keep it, so the program must not show it either
                _showing_admin_service.RemoveShowing(showing_id, Role.Admin, out _);
                error_message = SAVE_FAILED_TEXT;
                return false;
            }

            error_message = "";
            return true;
        }
    }
}