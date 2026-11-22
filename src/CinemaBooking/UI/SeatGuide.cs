// what the seat map says about a seat and how the arrow keys move between seats (owner: divyesh)
// kept out of the seat map so the words and the moves can be tested without opening a window
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
        // the first two rows are near the screen, the last two are at the back
        private const int EDGE_ROWS = 2;

        // a row is cut in three: left side, centre and right side
        private const int ROW_PARTS = 3;

        // example: Row C, seat 6: centre, best view in the room
        // an empty text for a seat that is not in the grid
        public static string DescribeView(int row_index, int seat_index, int rows, int seats_per_row)
        {
            if (row_index < 0 || row_index >= rows || seat_index < 0 || seat_index >= seats_per_row)
            {
                return "";
            }

            int side_width = seats_per_row / ROW_PARTS;
            string side = "centre";

            if (seat_index < side_width)
            {
                side = "left side";
            }
            else if (seat_index >= seats_per_row - side_width)
            {
                side = "right side";
            }

            string view;
            if (row_index < EDGE_ROWS)
            {
                view = "close to the screen, look up a little";
            }
            else if (row_index >= rows - EDGE_ROWS)
            {
                view = "at the back, the whole screen in view";
            }
            else if (side == "centre")
            {
                view = "best view in the room";
            }
            else
            {
                view = "good view";
            }

            string row_letter = ((char)('A' + row_index)).ToString();
            return "Row " + row_letter + ", seat " + (seat_index + 1) + ": " + side + ", " + view;
        }

        // one step in the arrow's direction, it stops at the edge of the grid instead of wrapping round
        public static void Move(int row_index, int seat_index, SeatMove move, int rows, int seats_per_row,
            out int new_row_index, out int new_seat_index)
        {
            new_row_index = row_index;
            new_seat_index = seat_index;

            switch (move)
            {
                case SeatMove.Up:
                    new_row_index = row_index - 1;
                    break;
                case SeatMove.Down:
                    new_row_index = row_index + 1;
                    break;
                case SeatMove.Left:
                    new_seat_index = seat_index - 1;
                    break;
                case SeatMove.Right:
                    new_seat_index = seat_index + 1;
                    break;
            }

            new_row_index = Math.Max(0, Math.Min(rows - 1, new_row_index));
            new_seat_index = Math.Max(0, Math.Min(seats_per_row - 1, new_seat_index));
        }
    }
}
