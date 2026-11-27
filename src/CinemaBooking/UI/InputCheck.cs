// turns what the user typed into numbers and seat codes, and says what is wrong in plain words (owner: divyesh)
// forms call these before they call a service, the rules for names, emails and phones stay in the models
using System;
using System.Globalization;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class InputCheck
    {
        // the minimum password length is a confirmed decision in docs/design/validation-rules.md
        private const int MIN_PASSWORD_LENGTH = 8;

        // money has cents, so two decimals at most
        private const int MAX_DECIMALS = 2;

        // reads a whole number from a text box, for ids and counts
        public static bool TryReadWholeNumber(string text, string field_name, out int value, out string error_message)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                error_message = field_name + " cannot be empty";
                return false;
            }

            // the integer style allows spaces and a sign, but no decimal point and no thousands comma
            if (!int.TryParse(text, NumberStyles.Integer, CultureInfo.InvariantCulture, out int number))
            {
                error_message = field_name + " must be a whole number";
                return false;
            }

            value = number;
            error_message = "";
            return true;
        }

        // reads an amount like 12.50 from a text box
        // a comma is refused, because 12,50 would be read as 1250 in some settings
        public static bool TryReadMoney(string text, string field_name, out decimal value, out string error_message)
        {
            value = 0;

            if (string.IsNullOrWhiteSpace(text))
            {
                error_message = field_name + " cannot be empty";
                return false;
            }

            NumberStyles money_style = NumberStyles.AllowLeadingWhite | NumberStyles.AllowTrailingWhite
                | NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint;

            if (!decimal.TryParse(text, money_style, CultureInfo.InvariantCulture, out decimal amount))
            {
                error_message = field_name + " must be an amount like 12.50";
                return false;
            }

            // 12.505 is refused, it is not rounded behind the user's back
            if (decimal.Round(amount, MAX_DECIMALS) != amount)
            {
                error_message = field_name + " can have two decimals at most";
                return false;
            }

            value = amount;
            error_message = "";
            return true;
        }

        // turns "c5, a12" into the seat codes C5 and A12, then the booking rules check them
        public static bool TryReadSeatList(string text, out string[] seats, out string error_message)
        {
            char[] separators = new char[] { ',', ';', ' ' };
            string[] parts = (text ?? "").Split(separators, StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

            seats = new string[parts.Length];
            for (int part_index = 0; part_index < parts.Length; part_index++)
            {
                seats[part_index] = parts[part_index].ToUpperInvariant();
            }

            // the seat rules and their messages live in Booking, so the form shows the same words
            return Booking.IsValidSeats(seats, out error_message);
        }

        // a password needs at least 8 characters and cannot be only spaces
        public static bool IsValidPassword(string password, out string error_message)
        {
            if (password == null || password.Length < MIN_PASSWORD_LENGTH)
            {
                error_message = "password must be at least " + MIN_PASSWORD_LENGTH + " characters";
                return false;
            }

            // long enough but only spaces, so saying "at least 8 characters" would confuse the person
            if (string.IsNullOrWhiteSpace(password))
            {
                error_message = "password cannot be only spaces";
                return false;
            }

            error_message = "";
            return true;
        }
    }
}
