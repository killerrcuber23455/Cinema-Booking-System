// the sizes and places of the seat map for the space it has: bigger window, bigger seats (owner: divyesh)
// the seat map draws and reads clicks with these numbers, so the sums are here where they can be tested
using System;
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

        private const int PERCENT = 100;

        public int SeatWidth { get; private set; }
        public int SeatHeight { get; private set; }
        public int GridLeft { get; private set; }
        public int GridTop { get; private set; }
        public int GridWidth { get; private set; }
        public int GridHeight { get; private set; }

        // the geometry for a map that has this much space: the seats are as big as the width and the height allow,
        // never smaller than the minimum and never bigger than the maximum
        public static SeatMapGeometry Make(int width, int height, int rows, int seats_per_row)
        {
            int safe_rows = Math.Max(1, rows);
            int safe_seats = Math.Max(1, seats_per_row);

            int room_for_width = (width - (2 * SIDE_SPACE) - ROW_LABEL_WIDTH - ((safe_seats - 1) * SEAT_GAP)) / safe_seats;
            int room_for_height = (height - TOP_ROOM - LEGEND_ROOM - ((safe_rows - 1) * SEAT_GAP)) / safe_rows;
            int width_from_height = room_for_height * PERCENT / SEAT_HEIGHT_PERCENT;

            SeatMapGeometry geometry = new SeatMapGeometry();
            geometry.SeatWidth = Math.Max(MIN_SEAT_WIDTH, Math.Min(MAX_SEAT_WIDTH, Math.Min(room_for_width, width_from_height)));
            geometry.SeatHeight = geometry.SeatWidth * SEAT_HEIGHT_PERCENT / PERCENT;
            geometry.GridWidth = (safe_seats * geometry.SeatWidth) + ((safe_seats - 1) * SEAT_GAP);
            geometry.GridHeight = (safe_rows * geometry.SeatHeight) + ((safe_rows - 1) * SEAT_GAP);

            // the row letters and the seats are centred together, and never go under the side space
            geometry.GridLeft = Math.Max(SIDE_SPACE + ROW_LABEL_WIDTH, (width - geometry.GridWidth + ROW_LABEL_WIDTH) / 2);
            geometry.GridTop = TOP_ROOM;
            return geometry;
        }

        // the place of one seat
        public Rectangle SeatArea(int row_index, int seat_index)
        {
            return new Rectangle(GridLeft + (seat_index * (SeatWidth + SEAT_GAP)),
                GridTop + (row_index * (SeatHeight + SEAT_GAP)), SeatWidth, SeatHeight);
        }

        // the seat under a point, false for a gap between seats or a point outside the grid
        public bool FindSeat(Point point, int rows, int seats_per_row, out int row_index, out int seat_index)
        {
            row_index = -1;
            seat_index = -1;

            if (point.X < GridLeft || point.Y < GridTop)
            {
                return false;
            }

            int column = (point.X - GridLeft) / (SeatWidth + SEAT_GAP);
            int row = (point.Y - GridTop) / (SeatHeight + SEAT_GAP);

            if (column >= seats_per_row || row >= rows || !SeatArea(row, column).Contains(point))
            {
                return false;
            }

            row_index = row;
            seat_index = column;
            return true;
        }
    }
}
