// the sizes of the landing window and its card, so a form never needs a scroll bar (owner: divyesh)
// the card is made as tall as its form needs. the window is made big enough for the tallest form (the sign up form).
namespace CinemaBooking.UI
{
    public static class LandingLayout
    {
        public const int WINDOW_WIDTH = 1000;
        public const int WINDOW_HEIGHT = 800;

        // the card of the first view (logo, one line and the buttons)
        public const int HOME_CARD_HEIGHT = 440;

        // a form starts under the logo and keeps a little space at the bottom of the card
        public const int FORM_TOP = 96;
        public const int FORM_BOTTOM_PADDING = 16;

        // the free space the window keeps above and below the card
        public const int WINDOW_MARGIN = 24;

        // the height the form itself needs: its title, rows, button and the room under them (FormParts works it out)
        public static int FormHeight(int row_count)
        {
            return FormParts.HeightForRows(row_count);
        }

        // the height of the card for a form with this many box rows
        public static int CardHeightForForm(int row_count)
        {
            return FORM_TOP + FormHeight(row_count) + FORM_BOTTOM_PADDING;
        }

        // true when the card for this many rows fits inside the window, so the form needs no scroll bar
        public static bool FormFitsWithoutScrolling(int row_count)
        {
            return CardHeightForForm(row_count) + (2 * WINDOW_MARGIN) <= WINDOW_HEIGHT;
        }
    }
}
