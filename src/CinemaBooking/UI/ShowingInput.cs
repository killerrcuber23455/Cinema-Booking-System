// the boxes of the admin showings page, checked before a showing is added (owner: divyesh)
// EMPTY STUB for the red step: it has the real names, but it does not check anything yet
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

        // one message for each box, in page order. "now" is given so the tests can fix the clock
        public static string[] CheckBoxes(string id_text, string film_text, string screen_text,
            string starts_text, string price_text, DateTime now)
        {
            string[] messages = new string[BOX_COUNT];

            for (int box_index = 0; box_index < BOX_COUNT; box_index++)
            {
                messages[box_index] = "";
            }

            return messages;
        }

        public static bool TryCreate(string id_text, string film_text, string screen_text,
            string starts_text, string price_text, DateTime now,
            out ShowingInput? input, out string error_message)
        {
            input = null;
            error_message = "showing input is not built yet";
            return false;
        }
    }
}
