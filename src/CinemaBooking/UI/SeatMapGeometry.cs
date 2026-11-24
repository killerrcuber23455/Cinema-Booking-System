// the sizes and places of the seat map for the space it has: bigger window, bigger seats (owner: divyesh)
// red stub for UI upgrade step 3, the real sums follow in the green commit
using System.Drawing;

namespace CinemaBooking.UI
{
    public class SeatMapGeometry
    {
        // fixed spaces around the seats, in pixels
        public const int SIDE_SPACE = 24;
        public const int ROW_LABEL_WIDTH = 30;
        public const int TOP_ROOM = 96;
        public const int LEGEND_ROOM = 52;
        public const int SEAT_GAP = 8;
        public const int MIN_SEAT_WIDTH = 24;
        public const int MAX_SEAT_WIDTH = 72;

        // a seat is this many percent as high as it is wide
        public const int SEAT_HEIGHT_PERCENT = 85;

        public int SeatWidth { get; private set; }
        public int SeatHeight { get; private set; }
        public int GridLeft { get; private set; }
        public int GridTop { get; private set; }
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        // the geometry for a map that has this much space
        public static SeatMapGeometry Make(int width, int height, int rows, int seats_per_row)
        {
            return new SeatMapGeometry();
        }

        // the place of one seat
        public Rectangle SeatArea(int row_index, int seat_index)
        {
            return Rectangle.Empty;
        }

        // the seat under a point, false for a gap between seats or a point outside the grid
        public bool FindSeat(Point point, int rows, int seats_per_row, out int row_index, out int seat_index)
        {
            row_index = -1;
            seat_index = -1;
            return false;
        }
    }
}
