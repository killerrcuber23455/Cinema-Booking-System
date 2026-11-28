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
            if (role != Role.Admin)
            {
                error_message = ADMIN_ONLY_TEXT;
                return new CinemaScreen[0];
            }

            HashTable<int, CinemaScreen> screen_table = new HashTable<int, CinemaScreen>();

            try
            {
                _database.LoadScreens(screen_table);
            }
            catch (Exception)
            {
                error_message = LOAD_FAILED_TEXT;
                return new CinemaScreen[0];
            }

            CinemaScreen[] found = screen_table.ListAll();

            // insertion sort by id, the same way the showing service sorts
            for (int sorted_end = 1; sorted_end < found.Length; sorted_end++)
            {
                CinemaScreen current = found[sorted_end];
                int compare_index = sorted_end - 1;

                while (compare_index >= 0 && found[compare_index].Id > current.Id)
                {
                    found[compare_index + 1] = found[compare_index];
                    compare_index--;
                }
                found[compare_index + 1] = current;
            }

            error_message = "";
            return found;
        }

        // adds a screen with the next free id and returns that id, or -1 and says why in error_message
        public int AddScreen(string name, int row_count, int seats_per_row, Role role, out string error_message)
        {
            if (role != Role.Admin)
            {
                error_message = ADMIN_ONLY_TEXT;
                return -1;
            }

            // the list is read fresh, so the new id and the name check use what the database holds now
            CinemaScreen[] existing = ListScreens(role, out error_message);
            if (error_message != "")
            {
                return -1;
            }

            int next_id = 1;
            for (int screen_index = 0; screen_index < existing.Length; screen_index++)
            {
                if (existing[screen_index].Id >= next_id)
                {
                    next_id = existing[screen_index].Id + 1;
                }
            }

            string clean_name = (name ?? "").Trim();
            CinemaScreen screen = new CinemaScreen(next_id, clean_name, row_count, seats_per_row);

            if (!screen.IsValid(out string rule_errors))
            {
                error_message = rule_errors.TrimEnd('\n');
                return -1;
            }

            // the database ignores upper and lower case in names, so the check does too
            for (int screen_index = 0; screen_index < existing.Length; screen_index++)
            {
                if (string.Equals(existing[screen_index].Name, clean_name, StringComparison.OrdinalIgnoreCase))
                {
                    error_message = "a screen named " + existing[screen_index].Name + " already exists";
                    return -1;
                }
            }

            bool is_saved;
            try
            {
                is_saved = _database.SaveScreen(screen);
            }
            catch (Exception)
            {
                is_saved = false;
            }

            if (!is_saved)
            {
                error_message = SAVE_FAILED_TEXT;
                return -1;
            }

            error_message = "";
            return next_id;
        }
    }
}
