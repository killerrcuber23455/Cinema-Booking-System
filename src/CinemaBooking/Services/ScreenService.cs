// the rules for listing and adding screens from the admin screens window, card 406 (owner: divyesh)
// the window only calls this class and shows its message. the screens are always read from the database, so the list is never old
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class ScreenService
    {
        public const string LOAD_FAILED_TEXT = "The screens could not be read from the database.";
        public const string SAVE_FAILED_TEXT = "The screen could not be saved to the database, so it was not added.";
        public const string ADMIN_ONLY_TEXT = "only admins can manage screens";

        private readonly IDatabase _database;

        public ScreenService(IDatabase database)
        {
            _database = database;
        }

        // all the screens, lowest id first. only an admin may see them
        public CinemaScreen[] ListScreens(Role role, out string error_message)
        {
            error_message = "";
            return new CinemaScreen[0];
        }

        // adds a screen with the next free id and returns that id, or -1 and says why in error_message
        public int AddScreen(string name, int row_count, int seats_per_row, Role role, out string error_message)
        {
            error_message = "";
            return -1;
        }
    }
}
