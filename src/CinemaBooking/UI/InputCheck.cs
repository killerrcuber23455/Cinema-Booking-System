// turns what the user typed into numbers and seat codes, and says what is wrong in plain words (owner: divyesh)
// forms call these before they call a service, the rules for names, emails and phones stay in the models
using System;

namespace CinemaBooking.UI
{
    public static class InputCheck
    {
        // stubs for card 343, the real code comes in the green step
        public static bool TryReadWholeNumber(string text, string field_name, out int value, out string error_message)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadMoney(string text, string field_name, out decimal value, out string error_message)
        {
            throw new NotImplementedException();
        }

        public static bool TryReadSeatList(string text, out string[] seats, out string error_message)
        {
            throw new NotImplementedException();
        }

        public static bool IsValidPassword(string password, out string error_message)
        {
            throw new NotImplementedException();
        }
    }
}
