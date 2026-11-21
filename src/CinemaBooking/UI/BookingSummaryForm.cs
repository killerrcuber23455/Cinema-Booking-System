// the booking summary page: shows the film, the showing, the chosen seats, the price and the points (owner: divyesh)
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
        private const int LINE_HEIGHT = 30;
        private const int LINES_TOP = 130;
        private const int PAGE_WIDTH = 560;
        private const int BOX_LEFT = 120;
        private const int BOX_WIDTH = 300;
        private const int BOX_ROW_HEIGHT = 40;

        private readonly Film _film;
        private readonly Showing _showing;
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

            Button btnBack = new Button();
            btnBack.Text = "< Back to seats";
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.BackColor = UiTheme.CARD;
            btnBack.ForeColor = UiTheme.TEXT;
            btnBack.Cursor = Cursors.Hand;
            btnBack.SetBounds(PAGE_PADDING, PAGE_PADDING, 140, 34);
            btnBack.Click += (sender, e) => _go_back();

            Label lblTitle = new Label();
            lblTitle.Font = UiTheme.CARD_TITLE_FONT;
            lblTitle.AutoEllipsis = true;
            lblTitle.Text = _film.Title;
            lblTitle.SetBounds(PAGE_PADDING, 52, 700, 26);

            Label lblShowing = new Label();
            lblShowing.ForeColor = UiTheme.TEXT_MUTED;
            lblShowing.AutoEllipsis = true;
            lblShowing.Text = ShowingLabel.Describe(_showing);
            lblShowing.SetBounds(PAGE_PADDING, 80, 700, 22);

            Controls.Add(btnBack);
            Controls.Add(lblTitle);
            Controls.Add(lblShowing);

            int next_top = BuildLines();

            if (_member == null)
            {
                next_top = BuildGuestBoxes(next_top);
            }

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.AutoEllipsis = true;
            lblMessage.SetBounds(PAGE_PADDING, next_top, PAGE_WIDTH - 220, 24);

            Button btnConfirm = new Button();
            btnConfirm.Text = "Confirm booking";
            btnConfirm.FlatStyle = FlatStyle.Flat;
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.BackColor = UiTheme.ACCENT_RED;
            btnConfirm.ForeColor = UiTheme.TEXT;
            btnConfirm.Cursor = Cursors.Hand;
            btnConfirm.SetBounds(PAGE_PADDING + PAGE_WIDTH - 200, next_top, 200, 40);
            btnConfirm.Click += (sender, e) => ClickConfirm();

            Controls.Add(lblMessage);
            Controls.Add(btnConfirm);
        }

        // one label for every line, the lines are made by BookingSummary so this page has no price rules
        // returns the top of the space under the lines
        private int BuildLines()
        {
            string[] lines = _summary.DescribeLines();

            RoundedPanel pnlLines = new RoundedPanel();
            pnlLines.BackColor = UiTheme.CARD;
            pnlLines.SetBounds(PAGE_PADDING, LINES_TOP - 10, PAGE_WIDTH, (lines.Length * LINE_HEIGHT) + 20);
            Controls.Add(pnlLines);

            for (int line_index = 0; line_index < lines.Length; line_index++)
            {
                Label lblLine = new Label();
                lblLine.Text = lines[line_index];
                lblLine.AutoEllipsis = true;
                lblLine.BackColor = UiTheme.CARD;
                lblLine.SetBounds(PAGE_PADDING + 16, LINES_TOP + (line_index * LINE_HEIGHT), PAGE_WIDTH - 32, LINE_HEIGHT - 4);

                // the amount to pay stands out
                if (lines[line_index].StartsWith("To pay"))
                {
                    lblLine.Font = UiTheme.CARD_TITLE_FONT;
                }

                Controls.Add(lblLine);
                lblLine.BringToFront();
            }

            return LINES_TOP + (lines.Length * LINE_HEIGHT) + 24;
        }

        // the three boxes a guest fills in and the sign in button, returns the top of the space under them
        private int BuildGuestBoxes(int top)
        {
            Label lblHeading = new Label();
            lblHeading.Text = "Your details";
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.SetBounds(PAGE_PADDING, top, 200, 26);
            Controls.Add(lblHeading);

            Button btnSignIn = new Button();
            btnSignIn.Text = "Sign in to earn points";
            btnSignIn.FlatStyle = FlatStyle.Flat;
            btnSignIn.FlatAppearance.BorderSize = 0;
            btnSignIn.BackColor = UiTheme.CARD;
            btnSignIn.ForeColor = UiTheme.TEXT;
            btnSignIn.Cursor = Cursors.Hand;
            btnSignIn.SetBounds(PAGE_PADDING + PAGE_WIDTH - 200, top - 4, 200, 34);
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
