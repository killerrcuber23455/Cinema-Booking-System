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

        // stub for now, the real rules come with the 406 green step
        public static bool IsValidId(int id, out string error_message)
        {
            error_message = "";
            return true;
        }

        // stub for now
        public static bool IsValidName(string name, out string error_message)
        {
            error_message = "";
            return true;
        }

        // stub for now
        public static bool IsValidRowCount(int row_count, out string error_message)
        {
            error_message = "";
            return true;
        }

        // stub for now
        public static bool IsValidSeatsPerRow(int seats_per_row, out string error_message)
        {
            error_message = "";
            return true;
        }

        // stub for now
        public bool IsValid(out string errors)
        {
            errors = "";
            return true;
        }
    }
}