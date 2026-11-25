// the sizes of the landing window and its card, so a form never needs a scroll bar (owner: divyesh)
// the card is made as tall as its form needs. the window is made big enough for the tallest form.
// STUB for the red step: the heights are not worked out yet, so the tests fail first
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

        // the height of the card for a form with this many box rows
        public static int CardHeightForForm(int row_count)
        {
            return 0;
        }

        // true when the card for this many rows fits inside the window, so the form needs no scroll bar
        public static bool FormFitsWithoutScrolling(int row_count)
        {
            return false;
        }
    }
}
