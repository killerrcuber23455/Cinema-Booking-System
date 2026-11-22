// the boxes of the admin showings page, checked before a showing is added (owner: divyesh)
// each box gives its own message, so the page can show it under that box. the numbers and the price are
// read with InputCheck, so the words are the same as on the other pages. the seat grid is always 8 x 12,
// so it has no boxes
using System.Globalization;

namespace CinemaBooking.UI
{
    public class ShowingInput
    {
        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_ID = 0;
        public const int BOX_FILM = 1;
        public const int BOX_SCREEN = 2;
        public const int BOX_STARTS = 3;
        public const int BOX_PRICE = 4;
        public const int BOX_COUNT = 5;

        // the start is typed like 2026-12-01 18:30
        public const string START_FORMAT = "yyyy-MM-dd HH:mm";
        private const string START_EXAMPLE = "2026-12-01 18:30";

        private const decimal MAX_PRICE = 1000m;

        public int Id { get; }
        public int FilmId { get; }
        public int ScreenId { get; }
        public DateTime StartsAt { get; }
        public decimal Price { get; }

        private ShowingInput(int id, int film_id, int screen_id, DateTime starts_at, decimal price)
        {
            Id = id;
            FilmId = film_id;
            ScreenId = screen_id;
            StartsAt = starts_at;
            Price = price;
        }

        // one message for each box, in page order, and an empty text for a box that is fine
        // "now" is given so the tests can fix the clock. a missing value counts as empty
        public static string[] CheckBoxes(string id_text, string film_text, string screen_text,
            string starts_text, string price_text, DateTime now)
        {
            string[] messages = new string[BOX_COUNT];

            messages[BOX_ID] = CheckPositiveNumber(id_text, "showing id", out int unused_id);
            messages[BOX_FILM] = CheckPositiveNumber(film_text, "film id", out int unused_film);
            messages[BOX_SCREEN] = CheckPositiveNumber(screen_text, "screen id", out int unused_screen);
            messages[BOX_STARTS] = CheckStart(starts_text, now, out DateTime unused_start);
            messages[BOX_PRICE] = CheckPrice(price_text, out decimal unused_price);

            return messages;
        }

        // spaces around the values are removed. every wrong box is shown, one line each, in page order
        public static bool TryCreate(string id_text, string film_text, string screen_text,
            string starts_text, string price_text, DateTime now,
            out ShowingInput? input, out string error_message)
        {
            input = null;

            string[] messages = new string[BOX_COUNT];
            messages[BOX_ID] = CheckPositiveNumber(id_text, "showing id", out int id);
            messages[BOX_FILM] = CheckPositiveNumber(film_text, "film id", out int film_id);
            messages[BOX_SCREEN] = CheckPositiveNumber(screen_text, "screen id", out int screen_id);
            messages[BOX_STARTS] = CheckStart(starts_text, now, out DateTime starts_at);
            messages[BOX_PRICE] = CheckPrice(price_text, out decimal price);

            error_message = JoinMessages(messages);
            if (error_message != "")
            {
                return false;
            }

            input = new ShowingInput(id, film_id, screen_id, starts_at, price);
            return true;
        }

        // an id box: a whole number above 0
        private static string CheckPositiveNumber(string text, string field_name, out int number)
        {
            if (!InputCheck.TryReadWholeNumber(text ?? "", field_name, out number, out string message))
            {
                return message.TrimEnd('\n');
            }

            if (number <= 0)
            {
                return field_name + " must be above 0";
            }

            return "";
        }

        // the start box: the right look, and a time that has not passed yet
        private static string CheckStart(string text, DateTime now, out DateTime starts_at)
        {
            starts_at = DateTime.MinValue;
            string clean_text = (text ?? "").Trim();

            if (clean_text == "")
            {
                return "start cannot be empty";
            }

            if (!DateTime.TryParseExact(clean_text, START_FORMAT, CultureInfo.InvariantCulture,
                DateTimeStyles.None, out DateTime typed_time))
            {
                return "start must look like " + START_EXAMPLE;
            }

            // exactly now is already too late
            if (typed_time <= now)
            {
                return "start must be in the future";
            }

            starts_at = typed_time;
            return "";
        }

        // the price box: an amount above 0 and not more than the top price
        private static string CheckPrice(string text, out decimal price)
        {
            if (!InputCheck.TryReadMoney(text ?? "", "price", out price, out string message))
            {
                return message.TrimEnd('\n');
            }

            if (price <= 0)
            {
                return "price must be above 0";
            }

            if (price > MAX_PRICE)
            {
                return "price must be 1000 or less";
            }

            return "";
        }

        // puts the messages of the wrong boxes together, one line each
        private static string JoinMessages(string[] messages)
        {
            string all_errors = "";

            for (int box_index = 0; box_index < messages.Length; box_index++)
            {
                if (messages[box_index] != "")
                {
                    all_errors += messages[box_index] + "\n";
                }
            }

            return all_errors.TrimEnd('\n');
        }
    }
}
