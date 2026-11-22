// the booking summary page: a ticket with the film, the showing, the chosen seats, the price and the points (owner: divyesh)
// a guest also types a name, email and phone here, and can choose to sign in instead
// it is a page shown inside the main window. it only shows the lines made by BookingSummary, reads the boxes and
// the clicks, the rules are in GuestDetails and the booking itself is done later by the booking service
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class BookingSummaryForm : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int TICKET_TOP = 56;
        private const int PAGE_WIDTH = 600;
        private const int BOX_LEFT = 120;
        private const int BOX_WIDTH = 300;
        private const int BOX_ROW_HEIGHT = 40;

        private readonly Film _film;
        private readonly Showing _showing;
        private readonly string[] _seats;
        private readonly BookingSummary _summary;

        // null means a guest, a guest types details here, a member already has them
        private readonly Member? _member;

        // called when the user clicks back to the seats, the main window decides what to show next
        private readonly Action _go_back;

        // called when a guest clicks sign in
        private readonly Action _sign_in;

        // called when the details are good and the user clicks confirm, the details are null for a member
        private readonly Action<GuestDetails?> _confirm_booking;

        private readonly TextBox txtGuestName = new TextBox();
        private readonly TextBox txtGuestEmail = new TextBox();
        private readonly TextBox txtGuestPhone = new TextBox();
        private readonly Label lblMessage = new Label();

        public BookingSummaryForm(Film film, Showing showing, string[] seats, Member? member,
            Action go_back, Action sign_in, Action<GuestDetails?> confirm_booking)
        {
            _film = film;
            _showing = showing;
            _seats = seats;
            _member = member;
            _summary = new BookingSummary(showing, seats, member);
            _go_back = go_back;
            _sign_in = sign_in;
            _confirm_booking = confirm_booking;

            BuildPage();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Button btnBack = FormParts.MakeButton("< Back to seats", UiTheme.CARD);
            btnBack.SetBounds(PAGE_PADDING - UiTheme.GLOW_SPREAD, PAGE_PADDING - UiTheme.GLOW_SPREAD, 150, 42);
            btnBack.Click += (sender, e) => _go_back();
            Controls.Add(btnBack);

            int next_top = BuildTicket();

            if (_member == null)
            {
                next_top = BuildGuestBoxes(next_top);
            }

            // the message sits under the confirm button and can have up to three lines, one for each wrong box
            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(PAGE_PADDING, next_top + 52, PAGE_WIDTH, 80);

            Button btnConfirm = FormParts.MakeButton("Confirm booking", UiTheme.ACCENT_RED);
            btnConfirm.SetBounds(PAGE_PADDING + PAGE_WIDTH - 208 + UiTheme.GLOW_SPREAD, next_top - UiTheme.GLOW_SPREAD, 208, 48);
            btnConfirm.Click += (sender, e) => ClickConfirm();

            Controls.Add(lblMessage);
            Controls.Add(btnConfirm);
        }

        // the ticket with the film, the showing and the lines made by BookingSummary, so this page has no price rules
        // returns the top of the space under the ticket
        private int BuildTicket()
        {
            string[] lines = _summary.DescribeLines();

            TicketStub ticket = new TicketStub(_film.Title, ShowingLabel.Describe(_showing), lines, _seats);
            ticket.SetBounds(PAGE_PADDING, TICKET_TOP, PAGE_WIDTH, TicketStub.HeightFor(lines.Length));
            Controls.Add(ticket);

            return ticket.Bottom + 24;
        }

        // the three boxes a guest fills in and the sign in button, returns the top of the space under them
        private int BuildGuestBoxes(int top)
        {
            Label lblHeading = new Label();
            lblHeading.Text = "Your details";
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.SetBounds(PAGE_PADDING, top, 200, 26);
            Controls.Add(lblHeading);

            Button btnSignIn = FormParts.MakeButton("Sign in to earn points", UiTheme.CARD);
            btnSignIn.SetBounds(PAGE_PADDING + PAGE_WIDTH - 208 + UiTheme.GLOW_SPREAD, top - 8, 208, 42);
            btnSignIn.Click += (sender, e) => _sign_in();
            Controls.Add(btnSignIn);

            int box_top = top + 40;
            AddBoxRow("Name", txtGuestName, box_top);
            AddBoxRow("Email", txtGuestEmail, box_top + BOX_ROW_HEIGHT);
            AddBoxRow("Phone", txtGuestPhone, box_top + (2 * BOX_ROW_HEIGHT));

            return box_top + (3 * BOX_ROW_HEIGHT) + 12;
        }

        private void AddBoxRow(string label_text, TextBox box, int top)
        {
            Label lblName = new Label();
            lblName.Text = label_text;
            lblName.ForeColor = UiTheme.TEXT_MUTED;
            lblName.SetBounds(PAGE_PADDING, top + 4, BOX_LEFT - PAGE_PADDING, 24);

            box.BackColor = UiTheme.INPUT;
            box.ForeColor = UiTheme.TEXT;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.SetBounds(BOX_LEFT, top, BOX_WIDTH, 28);

            Controls.Add(lblName);
            Controls.Add(box);
            FocusGlow.AddTo(this, box);
        }

        // a guest must give good details first, a member goes straight on
        private void ClickConfirm()
        {
            if (_member != null)
            {
                _confirm_booking(null);
                return;
            }

            if (!GuestDetails.TryCreate(txtGuestName.Text, txtGuestEmail.Text, txtGuestPhone.Text,
                out GuestDetails? details, out string error_message))
            {
                lblMessage.Text = error_message;
                return;
            }

            lblMessage.Text = "";
            _confirm_booking(details);
        }
    }
}
