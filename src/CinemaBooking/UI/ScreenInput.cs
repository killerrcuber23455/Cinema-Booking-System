// the three boxes of the "add a screen" part of the admin screens window, checked before the screen is added (owner: divyesh)
// the limits live in CinemaScreen, this class only reads the numbers and gives one message for each wrong box
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class ScreenInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_NAME = 0;
        public const int BOX_ROWS = 1;
        public const int BOX_SEATS = 2;
        public const int BOX_COUNT = 3;

        // one message for each box, in page order, and an empty text for a box that is fine
        public static string[] CheckBoxes(string name, string rows_text, string seats_text)
        {
            return new string[BOX_COUNT];
        }

        // the whole number in a box that CheckBoxes accepted, 0 if it is not a whole number
        public static int ReadNumber(string text)
        {
            return -1;
        }
    }
}
