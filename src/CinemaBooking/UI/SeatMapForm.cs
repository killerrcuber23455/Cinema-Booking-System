// the seat map page: the 8 by 12 seat grid of one showing, the user clicks free seats to pick them (owner: divyesh)
// it is a page shown inside the main window. it only shows the grid and reads clicks, the choice of seats
// is kept by SeatSelection and the booking itself is done later by the booking service
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class SeatMapForm : UserControl
    {
        // sizes in pixels, kept here so every seat matches
        private const int SEAT_WIDTH = 38;
        private const int SEAT_HEIGHT = 32;
        private const int SEAT_GAP = 6;
        private const int PAGE_PADDING = 8;
        private const int ROW_LABEL_WIDTH = 30;
        private const int GRID_TOP = 142;

        private readonly Film _film;
        private readonly Showing _showing;
        private readonly SeatSelection _selection;

        // called when the user clicks back, the main window decides what to show next
        private readonly Action _go_back;

        // called with the chosen seat codes when the user clicks continue
        private readonly Action<string[]> _continue_booking;

        // one button for every seat, the same size as the grid
        private readonly Button[,] _seat_buttons;

        private readonly Label lblSelection = new Label();
        private readonly Label lblMessage = new Label();
        private readonly Button btnContinue = new Button();

        public SeatMapForm(Film film, Showing showing, Action go_back, Action<string[]> continue_booking)
        {
            _film = film;
            _showing = showing;
            _go_back = go_back;
            _continue_booking = continue_booking;
            _selection = new SeatSelection(showing.SeatGrid);
            _seat_buttons = new Button[showing.SeatGrid.Rows, showing.SeatGrid.SeatsPerRow];

            BuildPage();
            RefreshSeats();
        }

        // the header, the screen bar, the seats, the legend and the bottom row
        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Button btnBack = new Button();
            btnBack.Text = "< Back to film";
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

            int grid_left = PAGE_PADDING + ROW_LABEL_WIDTH;
            int seats_per_row = _showing.SeatGrid.SeatsPerRow;
            int rows = _showing.SeatGrid.Rows;
            int grid_width = (seats_per_row * (SEAT_WIDTH + SEAT_GAP)) - SEAT_GAP;
            int grid_bottom = GRID_TOP + (rows * (SEAT_HEIGHT + SEAT_GAP)) - SEAT_GAP;

            Label lblScreen = new Label();
            lblScreen.Text = "SCREEN";
            lblScreen.TextAlign = ContentAlignment.MiddleCenter;
            lblScreen.Font = UiTheme.SMALL_FONT;
            lblScreen.ForeColor = UiTheme.TEXT_MUTED;
            lblScreen.BackColor = UiTheme.INPUT;
            lblScreen.SetBounds(grid_left, 112, grid_width, 22);

            Controls.Add(btnBack);
            Controls.Add(lblTitle);
            Controls.Add(lblShowing);
            Controls.Add(lblScreen);

            BuildSeats(grid_left);

            AddLegendItem("Free", UiTheme.CARD, UiTheme.TEXT, grid_left, grid_bottom + 14);
            AddLegendItem("Chosen", UiTheme.ACCENT_RED, UiTheme.TEXT, grid_left + 84, grid_bottom + 14);
            AddLegendItem("Taken", UiTheme.BACKGROUND, UiTheme.TEXT_MUTED, grid_left + 168, grid_bottom + 14);

            int bottom_row_top = grid_bottom + 48;

            lblSelection.AutoEllipsis = true;
            lblSelection.SetBounds(PAGE_PADDING, bottom_row_top, grid_width - 170, 24);

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.AutoEllipsis = true;
            lblMessage.SetBounds(PAGE_PADDING, bottom_row_top + 28, grid_width - 170, 24);

            btnContinue.Text = "Continue";
            btnContinue.FlatStyle = FlatStyle.Flat;
            btnContinue.FlatAppearance.BorderSize = 0;
            btnContinue.BackColor = UiTheme.ACCENT_RED;
            btnContinue.ForeColor = UiTheme.TEXT;
            btnContinue.Cursor = Cursors.Hand;
            btnContinue.SetBounds(grid_left + grid_width - 160, bottom_row_top, 160, 38);
            btnContinue.Click += (sender, e) => _continue_booking(_selection.GetSelectedSeats());

            Controls.Add(lblSelection);
            Controls.Add(lblMessage);
            Controls.Add(btnContinue);
        }

        // draws the row letters and one button for every seat, A1 is top left
        private void BuildSeats(int grid_left)
        {
            for (int row_index = 0; row_index < _showing.SeatGrid.Rows; row_index++)
            {
                int row_top = GRID_TOP + (row_index * (SEAT_HEIGHT + SEAT_GAP));

                Label lblRow = new Label();
                lblRow.Text = ((char)('A' + row_index)).ToString();
                lblRow.TextAlign = ContentAlignment.MiddleCenter;
                lblRow.ForeColor = UiTheme.TEXT_MUTED;
                lblRow.SetBounds(PAGE_PADDING, row_top, ROW_LABEL_WIDTH - 6, SEAT_HEIGHT);
                Controls.Add(lblRow);

                for (int seat_index = 0; seat_index < _showing.SeatGrid.SeatsPerRow; seat_index++)
                {
                    string seat_code = SeatSelection.MakeSeatCode(row_index, seat_index);

                    Button btnSeat = new Button();
                    btnSeat.Text = (seat_index + 1).ToString();
                    btnSeat.Font = UiTheme.SMALL_FONT;
                    btnSeat.FlatStyle = FlatStyle.Flat;
                    btnSeat.FlatAppearance.BorderSize = 0;
                    btnSeat.SetBounds(grid_left + (seat_index * (SEAT_WIDTH + SEAT_GAP)), row_top, SEAT_WIDTH, SEAT_HEIGHT);
                    btnSeat.Click += (sender, e) => ClickSeat(seat_code);

                    _seat_buttons[row_index, seat_index] = btnSeat;
                    Controls.Add(btnSeat);
                }
            }
        }

        // one small coloured label that tells what a seat colour means
        private void AddLegendItem(string text, Color back_color, Color text_color, int left, int top)
        {
            Label lblItem = new Label();
            lblItem.Text = text;
            lblItem.TextAlign = ContentAlignment.MiddleCenter;
            lblItem.Font = UiTheme.SMALL_FONT;
            lblItem.BackColor = back_color;
            lblItem.ForeColor = text_color;
            lblItem.SetBounds(left, top, 76, 22);
            Controls.Add(lblItem);
        }

        // SeatSelection decides what a click does, here we only show the answer
        private void ClickSeat(string seat_code)
        {
            bool is_done = _selection.TryToggle(seat_code, out string error_message);

            lblMessage.Text = is_done ? "" : error_message;
            RefreshSeats();
        }

        // colours every seat again: chosen is red, taken is dark and cannot be clicked, free is a plain card
        private void RefreshSeats()
        {
            for (int row_index = 0; row_index < _showing.SeatGrid.Rows; row_index++)
            {
                for (int seat_index = 0; seat_index < _showing.SeatGrid.SeatsPerRow; seat_index++)
                {
                    string seat_code = SeatSelection.MakeSeatCode(row_index, seat_index);
                    Button btnSeat = _seat_buttons[row_index, seat_index];

                    if (_selection.IsSelected(seat_code))
                    {
                        btnSeat.BackColor = UiTheme.ACCENT_RED;
                        btnSeat.ForeColor = UiTheme.TEXT;
                        btnSeat.Enabled = true;
                        btnSeat.Cursor = Cursors.Hand;
                    }
                    else if (!_showing.SeatGrid.IsSeatFree(seat_code))
                    {
                        btnSeat.BackColor = UiTheme.BACKGROUND;
                        btnSeat.ForeColor = UiTheme.TEXT_MUTED;
                        btnSeat.Enabled = false;
                        btnSeat.Cursor = Cursors.Default;
                    }
                    else
                    {
                        btnSeat.BackColor = UiTheme.CARD;
                        btnSeat.ForeColor = UiTheme.TEXT;
                        btnSeat.Enabled = true;
                        btnSeat.Cursor = Cursors.Hand;
                    }
                }
            }

            lblSelection.Text = _selection.Describe();
            btnContinue.Enabled = _selection.Count > 0;
        }
    }
}
