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
            string[] messages = new string[BOX_COUNT];
            string rule_message;

            messages[BOX_NAME] = "";
            if (!CinemaScreen.IsValidName((name ?? "").Trim(), out rule_message))
            {
                messages[BOX_NAME] = rule_message.TrimEnd('\n');
            }

            messages[BOX_ROWS] = "";
            if (!int.TryParse((rows_text ?? "").Trim(), out int row_count))
            {
                messages[BOX_ROWS] = "rows must be a whole number";
            }
            else if (!CinemaScreen.IsValidRowCount(row_count, out rule_message))
            {
                messages[BOX_ROWS] = rule_message.TrimEnd('\n');
            }

            messages[BOX_SEATS] = "";
            if (!int.TryParse((seats_text ?? "").Trim(), out int seats_per_row))
            {
                messages[BOX_SEATS] = "seats per row must be a whole number";
            }
            else if (!CinemaScreen.IsValidSeatsPerRow(seats_per_row, out rule_message))
            {
                messages[BOX_SEATS] = rule_message.TrimEnd('\n');
            }

            return messages;
        }

        // the whole number in a box that CheckBoxes accepted, 0 if it is not a whole number
        public static int ReadNumber(string text)
        {
            return int.TryParse((text ?? "").Trim(), out int number) ? number : 0;
        }
    }
}
