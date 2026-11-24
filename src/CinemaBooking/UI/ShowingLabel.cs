// builds the one line of text shown for each showing on the film detail page (owner: divyesh)
// it is kept out of the form so it can be tested without opening a window
using System;
using System.Globalization;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class ShowingLabel
    {
        // example: Fri 20 Nov 2026 · 18:30 · Screen 2 · Rs 250.00 · 96 seats left
        public static string Describe(Showing showing)
        {
            // invariant culture so the line reads the same on every computer
            CultureInfo culture = CultureInfo.InvariantCulture;

            string date_text = showing.StartsAt.ToString("ddd d MMM yyyy", culture);
            string time_text = showing.StartsAt.ToString("HH:mm", culture);
            string price_text = showing.Price.ToString("0.00", culture);

            return date_text + " · " + time_text + " · Screen " + showing.ScreenId
                + " · Rs " + price_text + " · " + DescribeSeatsLeft(showing);
        }

        // free seats are all the seats minus the taken ones
        // public since UI upgrade step 3, the showing cards on the film page show the same words
        public static string DescribeSeatsLeft(Showing showing)
        {
            int all_seats = showing.SeatGrid.Rows * showing.SeatGrid.SeatsPerRow;
            int seats_left = all_seats - showing.SeatGrid.ListTakenSeats().Length;

            if (seats_left <= 0)
            {
                return "sold out";
            }

            return seats_left == 1 ? "1 seat left" : seats_left + " seats left";
        }
    }
}
