// the page shown after a booking is made: a ticket with the reference the guest needs to find the booking later (owner: divyesh)
// the page only shows the result, the booking itself was made by BookingService
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class BookingConfirmedForm : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int PAGE_WIDTH = 620;

        private readonly Booking _booking;
        private readonly Film _film;
        private readonly Showing _showing;
        private readonly Action _back_to_films;

        public BookingConfirmedForm(Booking booking, Film film, Showing showing, Action back_to_films)
        {
            _booking = booking;
            _film = film;
            _showing = showing;
            _back_to_films = back_to_films;

            BuildPage();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "Booking confirmed";
            lblHeading.SetBounds(PAGE_PADDING, PAGE_PADDING, PAGE_WIDTH, 26);
            Controls.Add(lblHeading);

            // a guest needs the reference and the email to find or cancel the booking later
            Label lblReference = new Label();
            lblReference.ForeColor = UiTheme.TEXT_MUTED;
            lblReference.Text = _booking.IsGuest
                ? "Keep your reference " + _booking.Reference + ". You need it with your email to find or cancel the booking."
                : "Your reference is " + _booking.Reference + ". Your bookings are kept on your account.";
            lblReference.SetBounds(PAGE_PADDING, 38, PAGE_WIDTH, 40);
            Controls.Add(lblReference);

            string[] lines = new string[]
            {
                "Reference: " + _booking.Reference,
                "Total paid: Rs " + _booking.Total.ToString("0.00", CultureInfo.InvariantCulture),
                "Points earned: " + _booking.PointsEarned
            };

            TicketStub ticket = new TicketStub(_film.Title, ShowingLabel.Describe(_showing), lines, _booking.Seats);
            ticket.SetBounds(PAGE_PADDING, 90, PAGE_WIDTH, TicketStub.HeightFor(lines.Length));
            Controls.Add(ticket);

            Button btnBack = FormParts.MakeButton("Back to films", UiTheme.ACCENT_RED);
            btnBack.SetBounds(PAGE_PADDING, ticket.Bottom + 24, 170, 40);
            btnBack.Click += (sender, e) => _back_to_films();
            Controls.Add(btnBack);
        }
    }
}
