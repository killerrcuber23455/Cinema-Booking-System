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

        private readonly IDatabase _database;
        private readonly ShowingAdminService _showing_admin_service;
        private readonly ShowingTable _showing_table;

        public ShowingStore(IDatabase database, ShowingAdminService showing_admin_service, ShowingTable showing_table)
        {
            _database = database;
            _showing_admin_service = showing_admin_service;
            _showing_table = showing_table;
        }

        // red stub: says yes and does nothing, on purpose. the real code comes in the green step
        public bool AddShowing(int showing_id, int film_id, int screen_id, DateTime starts_at, decimal price,
            Role role, out string error_message)
        {
            error_message = "";
            return true;
        }
    }
}
