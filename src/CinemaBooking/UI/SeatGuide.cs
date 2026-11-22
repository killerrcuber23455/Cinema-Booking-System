// what the seat map says about a seat and how the arrow keys move between seats (owner: divyesh)
// EMPTY STUB for the red step: it has the real names, but it gives wrong answers
namespace CinemaBooking.UI
{
    // the four arrow keys, kept apart from Windows Forms so the rules can be tested
    public enum SeatMove
    {
        Up,
        Down,
        Left,
        Right
    }

    public static class SeatGuide
    {
        public static string DescribeView(int row_index, int seat_index, int rows, int seats_per_row)
        {
            return "";
        }

        public static void Move(int row_index, int seat_index, SeatMove move, int rows, int seats_per_row,
            out int new_row_index, out int new_seat_index)
        {
            new_row_index = row_index;
            new_seat_index = seat_index;
        }
    }
}
