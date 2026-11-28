// one cinema screen: its name and its size, rows A to Z and seats 1 to 30 (owner: kushal)
// the checks are in the IsValid methods like Film and Booking, the constructor does not throw.
// the class is called CinemaScreen because Screen is already a class in Windows Forms.
namespace CinemaBooking.Models
{
    public class CinemaScreen
    {
        // validation rules constant values
        public const int MAX_NAME_LENGTH = 50;
        public const int MAX_ROWS = 26;
        public const int MAX_SEATS_PER_ROW = 30;

        // properties of screen
        public int Id { get; set; }
        public string Name { get; set; }
        public int RowCount { get; set; }
        public int SeatsPerRow { get; set; }

        // constructor
        public CinemaScreen(int id, string name, int row_count, int seats_per_row)
        {
            Id = id;
            Name = name;
            RowCount = row_count;
            SeatsPerRow = seats_per_row;
        }

        // validation methods, each returns false with a reason
        public static bool IsValidId(int id, out string error_message)
        {
            if (id <= 0)
            {
                error_message = "screen id must be a positive number";
                return false;
            }

            error_message = "";
            return true;
        }

        public static bool IsValidName(string name, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            {
                error_message = "name cannot be empty and must be " + MAX_NAME_LENGTH + " characters or fewer";
                return false;
            }

            error_message = "";
            return true;
        }

        public static bool IsValidRowCount(int row_count, out string error_message)
        {
            if (row_count < 1 || row_count > MAX_ROWS)
            {
                error_message = "rows must be between 1 and " + MAX_ROWS + " (seat letters A to Z)";
                return false;
            }

            error_message = "";
            return true;
        }

        public static bool IsValidSeatsPerRow(int seats_per_row, out string error_message)
        {
            if (seats_per_row < 1 || seats_per_row > MAX_SEATS_PER_ROW)
            {
                error_message = "seats per row must be between 1 and " + MAX_SEATS_PER_ROW;
                return false;
            }

            error_message = "";
            return true;
        }

        // checks every field and puts all the problems in one text, empty if the screen is fine
        public bool IsValid(out string errors)
        {
            errors = "";
            string error_message;

            if (!IsValidId(Id, out error_message))
            {
                errors = AddError(errors, error_message);
            }

            if (!IsValidName(Name, out error_message))
            {
                errors = AddError(errors, error_message);
            }

            if (!IsValidRowCount(RowCount, out error_message))
            {
                errors = AddError(errors, error_message);
            }

            if (!IsValidSeatsPerRow(SeatsPerRow, out error_message))
            {
                errors = AddError(errors, error_message);
            }

            return errors == "";
        }

        // adds one problem to the text of problems
        private static string AddError(string errors, string new_error)
        {
            if (errors == "")
            {
                return new_error;
            }

            return errors + "; " + new_error;
        }
    }
}