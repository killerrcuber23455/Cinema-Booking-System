// the seat map page: the drawn seat map of one showing, the user picks free seats with the mouse or the keys (owner: divyesh)
// it is a page shown inside the main window. the drawing is in SeatMapCanvas, the choice of seats is kept by
// SeatSelection and the booking itself is done later by the booking service
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class SeatMapForm : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int MAP_TOP = 108;

        private readonly Film _film;
        private readonly Showing _showing;
        private readonly SeatSelection _selection;

        // called when the user clicks back, the main window decides what to show next
        private readonly Action _go_back;

        // called with the chosen seat codes when the user clicks continue
        private readonly Action<string[]> _continue_booking;

        private readonly SeatMapCanvas _seat_map;
        private readonly Label lblView = new Label();
        private readonly Label lblSelection = new Label();
        private readonly Label lblMessage = new Label();
        private readonly Button btnContinue;

        public SeatMapForm(Film film, Showing showing, Action go_back, Action<string[]> continue_booking)
        {
            _film = film;
            _showing = showing;
            _go_back = go_back;
            _continue_booking = continue_booking;
            _selection = new SeatSelection(showing.SeatGrid);

            _seat_map = new SeatMapCanvas(showing, _selection, ClickSeat, ShowView);
            btnContinue = FormParts.MakeButton("Continue", UiTheme.ACCENT_RED);

            BuildPage();
            RefreshChoice();
        }

        // the header, the seat map, the view line under it and the bottom row
        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Button btnBack = FormParts.MakeButton("< Back to film", UiTheme.CARD);
            btnBack.SetBounds(PAGE_PADDING - UiTheme.GLOW_SPREAD, PAGE_PADDING - UiTheme.GLOW_SPREAD, 150, 42);
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
            lblShowing.SetBounds(PAGE_PADDING, 78, 700, 22);

            // the map keeps some space on its left for the light of the screen, so it starts at the page edge
            _seat_map.Location = new Point(0, MAP_TOP);

            int map_bottom = _seat_map.Bottom;
            // the text lines stop before the Continue button, so they never draw over its glowing edge
            int line_width = _seat_map.Width - 24 - 168 - PAGE_PADDING;

            // what the seat under the mouse or the keys is like, in the warm marquee colour
            lblView.ForeColor = UiTheme.AMBER;
            lblView.AutoEllipsis = true;
            lblView.Text = "Point at a seat to see its view. You can also use the arrow keys and space.";
            lblView.SetBounds(PAGE_PADDING, map_bottom + 2, _seat_map.Width - 16, 22);

            int bottom_row_top = map_bottom + 32;

            lblSelection.AutoEllipsis = true;
            lblSelection.SetBounds(PAGE_PADDING, bottom_row_top, line_width, 24);

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.AutoEllipsis = true;
            lblMessage.SetBounds(PAGE_PADDING, bottom_row_top + 26, line_width, 24);

            btnContinue.SetBounds(_seat_map.Right - 24 - 168, bottom_row_top - UiTheme.GLOW_SPREAD, 168, 46);
            btnContinue.Click += (sender, e) => _continue_booking(_selection.GetSelectedSeats());

            Controls.Add(btnBack);
            Controls.Add(lblTitle);
            Controls.Add(lblShowing);
            Controls.Add(_seat_map);
            Controls.Add(lblView);
            Controls.Add(lblSelection);
            Controls.Add(lblMessage);
            Controls.Add(btnContinue);
        }

        // SeatSelection decides what a click does, here we only show the answer
        private void ClickSeat(string seat_code)
        {
            bool is_done = _selection.TryToggle(seat_code, out string error_message);

            lblMessage.Text = is_done ? "" : error_message;
            RefreshChoice();
        }

        // the seat map tells us the view of the seat under the mouse or the keys
        private void ShowView(string view_text)
        {
            if (view_text != "")
            {
                lblView.Text = view_text;
            }
        }

        private void RefreshChoice()
        {
            _seat_map.Invalidate();
            lblSelection.Text = _selection.Describe();
            btnContinue.Enabled = _selection.Count > 0;
        }
    }
}
