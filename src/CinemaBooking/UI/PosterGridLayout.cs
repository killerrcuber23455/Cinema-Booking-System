// where each poster card goes in the films grid, and which card is under the mouse (owner: divyesh)
// the grid adds columns when the window gets wider, so a maximised window shows more films, not bigger gaps
using System;
using System.Drawing;

namespace CinemaBooking.UI
{
    public static class PosterGridLayout
    {
        // how many cards fit in one row: as many as fit at the smallest card width, never fewer than one
        public static int ColumnsFor(int available_width, int min_card_width, int gap)
        {
            if (available_width <= 0 || min_card_width <= 0)
            {
                return 1;
            }

            // n cards need n widths and n - 1 gaps, adding one gap to the width makes the sum simple
            int columns = (available_width + gap) / (min_card_width + gap);
            return Math.Max(1, columns);
        }

        // the cards share the row, so they grow to fill it
        public static int CardWidth(int available_width, int columns, int gap)
        {
            int safe_columns = Math.Max(1, columns);
            int width = (available_width - (gap * (safe_columns - 1))) / safe_columns;
            return Math.Max(0, width);
        }

        // film posters are 2 wide for 3 high
        public static int PosterHeight(int card_width)
        {
            return card_width * 3 / 2;
        }

        public static int RowsFor(int film_count, int columns)
        {
            if (film_count <= 0)
            {
                return 0;
            }

            int safe_columns = Math.Max(1, columns);
            return (film_count + safe_columns - 1) / safe_columns;
        }

        // the place of card number index, counted from the top left of the grid
        public static Rectangle CardBounds(int index, int columns, int card_width, int card_height, int gap)
        {
            int safe_columns = Math.Max(1, columns);
            int row = index / safe_columns;
            int column = index % safe_columns;

            return new Rectangle(column * (card_width + gap), row * (card_height + gap), card_width, card_height);
        }

        // the card under a point of the grid, or -1 for a gap or a place with no card
        public static int IndexAt(Point point, int film_count, int columns, int card_width, int card_height, int gap)
        {
            if (point.X < 0 || point.Y < 0 || card_width <= 0 || card_height <= 0)
            {
                return -1;
            }

            int column = point.X / (card_width + gap);
            int row = point.Y / (card_height + gap);

            // the point is in the gap on the right of a card or under it
            bool is_in_gap = point.X - (column * (card_width + gap)) >= card_width
                || point.Y - (row * (card_height + gap)) >= card_height;

            if (is_in_gap || column >= Math.Max(1, columns))
            {
                return -1;
            }

            int index = (row * Math.Max(1, columns)) + column;
            return index < film_count ? index : -1;
        }

        // the height of all the rows with the gaps between them
        public static int TotalHeight(int film_count, int columns, int card_height, int gap)
        {
            int rows = RowsFor(film_count, columns);

            if (rows == 0)
            {
                return 0;
            }

            return (rows * card_height) + ((rows - 1) * gap);
        }
    }
}
