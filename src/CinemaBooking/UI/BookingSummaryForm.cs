// the booking summary page: shows the film, the showing, the chosen seats, the price and the points (owner: divyesh)
// it is a page shown inside the main window. it only shows the lines made by BookingSummary and reads two clicks,
// the booking itself is done later by the booking service
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

        private readonly Film _film;
        private readonly Showing _showing;
        private readonly BookingSummary _summary;

        // called when the user clicks back to the seats, the main window decides what to show next
        private readonly Action _go_back;

        // called when the user clicks confirm
        private readonly Action _confirm_booking;

        // member is null for a guest
        public BookingSummaryForm(Film film, Showing showing, string[] seats, Member? member, Action go_back, Action confirm_booking)
        {
            _film = film;
            _showing = showing;
            _summary = new BookingSummary(showing, seats, member);
            _go_back = go_back;
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

            // one label for every line, the lines are made by BookingSummary so this page has no price rules
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

            int button_top = LINES_TOP + (lines.Length * LINE_HEIGHT) + 30;

            Button btnConfirm = new Button();
            btnConfirm.Text = "Confirm booking";
            btnConfirm.FlatStyle = FlatStyle.Flat;
            btnConfirm.FlatAppearance.BorderSize = 0;
            btnConfirm.BackColor = UiTheme.ACCENT_RED;
            btnConfirm.ForeColor = UiTheme.TEXT;
            btnConfirm.Cursor = Cursors.Hand;
            btnConfirm.SetBounds(PAGE_PADDING + PAGE_WIDTH - 200, button_top, 200, 40);
            btnConfirm.Click += (sender, e) => _confirm_booking();

            Controls.Add(btnConfirm);
        }
    }
}
