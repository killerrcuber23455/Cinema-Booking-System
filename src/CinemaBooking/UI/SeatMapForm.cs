// the seat map page: the drawn seat map of one showing and a summary panel, the user picks free seats with the mouse or the keys (owner: divyesh)
// UI upgrade step 3: the seat map fills the window and the seats grow with it, the summary panel is on the right
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
        // sizes at the small step, they grow with the window step
        private const int BASE_TOP_BAR_HEIGHT = 84;
        private const int BASE_PANEL_WIDTH = 340;
        private const int BASE_PANEL_PADDING = 28;
        private const int BASE_BUTTON_HEIGHT = 50;

        private readonly Film _film;
        private readonly Showing _showing;
        private readonly SeatSelection _selection;

        // called when the user clicks back, the main window decides what to show next
        private readonly Action _go_back;

        // called with the chosen seat codes when the user clicks continue
        private readonly Action<string[]> _continue_booking;

        private readonly SeatMapCanvas _seat_map;
        private readonly Panel pnlTopBar = new Panel();
        private readonly Panel pnlSummary = new Panel();
        private readonly ChipButton btnBack = new ChipButton("Back to film");
        private readonly Label lblTitle = new Label();
        private readonly Label lblShowing = new Label();
        private readonly Label lblPanelHeading = new Label();
        private readonly Label lblSeats = new Label();
        private readonly Label lblTickets = new Label();
        private readonly Label lblPriceEach = new Label();
        private readonly Label lblTotalHeading = new Label();
        private readonly Label lblTotal = new Label();
        private readonly Label lblView = new Label();
        private readonly Label lblMessage = new Label();
        private readonly Button btnContinue;
        private float _step = -1f;

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
            UpdateStep();
        }

        // the top bar, the summary panel on the right and the seat map in the rest
        private void BuildPage()
        {
            BackColor = UiTheme.BACKGROUND;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;

            btnBack.Click += (sender, e) => _go_back();

            lblTitle.Text = _film.Title;
            lblTitle.AutoEllipsis = true;
            lblTitle.ForeColor = UiTheme.TEXT;
            lblTitle.BackColor = UiTheme.BACKGROUND;

            lblShowing.Text = ShowingLabel.Describe(_showing);
            lblShowing.AutoEllipsis = true;
            lblShowing.ForeColor = UiTheme.TEXT_MUTED;
            lblShowing.BackColor = UiTheme.BACKGROUND;

            pnlTopBar.Dock = DockStyle.Top;
            pnlTopBar.BackColor = UiTheme.BACKGROUND;
            pnlTopBar.Controls.Add(btnBack);
            pnlTopBar.Controls.Add(lblTitle);
            pnlTopBar.Controls.Add(lblShowing);
            pnlTopBar.Resize += (sender, e) => LayoutTopBar();

            pnlSummary.Dock = DockStyle.Right;
            pnlSummary.BackColor = UiTheme.PANEL;
            pnlSummary.Resize += (sender, e) => LayoutSummary();

            lblPanelHeading.Text = "Your seats";
            lblPanelHeading.ForeColor = UiTheme.TEXT;
            lblSeats.ForeColor = UiTheme.TEXT;
            lblTickets.ForeColor = UiTheme.TEXT_MUTED;
            lblPriceEach.ForeColor = UiTheme.TEXT_MUTED;
            lblTotalHeading.Text = "Total";
            lblTotalHeading.ForeColor = UiTheme.TEXT_MUTED;
            lblTotal.ForeColor = UiTheme.TEXT;

            // what the seat under the mouse or the keys is like, in the warm marquee colour
            lblView.ForeColor = UiTheme.AMBER;
            lblView.Text = "Point at a seat to see its view. You can also use the arrow keys and space.";
            lblMessage.ForeColor = UiTheme.ACCENT_RED;

            foreach (Label label in new Label[] { lblPanelHeading, lblSeats, lblTickets, lblPriceEach, lblTotalHeading, lblTotal, lblView, lblMessage })
            {
                label.BackColor = UiTheme.PANEL;
                label.AutoEllipsis = false;
                pnlSummary.Controls.Add(label);
            }

            btnContinue.Click += (sender, e) => _continue_booking(_selection.GetSelectedSeats());
            pnlSummary.Controls.Add(btnContinue);

            _seat_map.Dock = DockStyle.Fill;

            // fill goes in first, then the side and the top, same order as the main window
            Controls.Add(_seat_map);
            Controls.Add(pnlSummary);
            Controls.Add(pnlTopBar);
            _seat_map.BringToFront();
        }

        // the window crossed a size step: the text and the spaces of the page grow or shrink together
        private void UpdateStep()
        {
            Form? window = FindForm();
            float step = UiScale.StepFor(window != null ? window.ClientSize.Width : Width);

            if (step == _step)
            {
                return;
            }

            _step = step;
            btnBack.SetStep(step);
            lblTitle.Font = FontCache.Scaled(FontCache.DISPLAY, UiTheme.SIZE_TITLE, step);
            lblShowing.Font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, step);
            lblPanelHeading.Font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_SECTION, step);
            lblSeats.Font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_CARD_TITLE, step);
            lblTickets.Font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, step);
            lblPriceEach.Font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, step);
            lblTotalHeading.Font = FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_SMALL, step);
            lblTotal.Font = FontCache.Scaled(FontCache.DISPLAY, UiTheme.SIZE_PAGE_TITLE + 6, step);
            lblView.Font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_SMALL, step);
            lblMessage.Font = FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_SMALL, step);
            btnContinue.Font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_BODY, step);

            pnlTopBar.Height = UiScale.Pixels(BASE_TOP_BAR_HEIGHT, step);
            pnlSummary.Width = UiScale.Pixels(BASE_PANEL_WIDTH, step);
            LayoutTopBar();
            LayoutSummary();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateStep();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateStep();
        }

        private void LayoutTopBar()
        {
            float step = Math.Max(UiScale.SMALL_STEP, _step);
            int side = UiScale.Pixels(UiTheme.SPACE_XL, step);
            int top = UiScale.Pixels(UiTheme.SPACE_M, step);

            btnBack.Location = new Point(side, top);

            int text_left = btnBack.Right + UiScale.Pixels(UiTheme.SPACE_XL, step);
            int text_width = Math.Max(50, pnlTopBar.Width - text_left - side);

            lblTitle.SetBounds(text_left, top - UiScale.Pixels(4, step), text_width, UiScale.Pixels(36, step));
            lblShowing.SetBounds(text_left, lblTitle.Bottom, text_width, UiScale.Pixels(24, step));
        }

        // the summary panel from the top: heading, chosen seats, tickets and price, then the total and the button at the bottom
        private void LayoutSummary()
        {
            float step = Math.Max(UiScale.SMALL_STEP, _step);
            int pad = UiScale.Pixels(BASE_PANEL_PADDING, step);
            int width = Math.Max(50, pnlSummary.Width - (2 * pad));
            int gap = UiScale.Pixels(UiTheme.SPACE_M, step);

            int y = pad;
            lblPanelHeading.SetBounds(pad, y, width, UiScale.Pixels(30, step));
            y = lblPanelHeading.Bottom + gap;

            lblSeats.SetBounds(pad, y, width, UiScale.Pixels(72, step));
            y = lblSeats.Bottom + gap;

            lblTickets.SetBounds(pad, y, width, UiScale.Pixels(26, step));
            y = lblTickets.Bottom;
            lblPriceEach.SetBounds(pad, y, width, UiScale.Pixels(26, step));
            y = lblPriceEach.Bottom + UiScale.Pixels(UiTheme.SPACE_XL, step);

            lblTotalHeading.SetBounds(pad, y, width, UiScale.Pixels(20, step));
            lblTotal.SetBounds(pad, lblTotalHeading.Bottom, width, UiScale.Pixels(50, step));
            y = lblTotal.Bottom + UiScale.Pixels(UiTheme.SPACE_XL, step);

            int button_height = UiScale.Pixels(BASE_BUTTON_HEIGHT, step) + (2 * UiTheme.GLOW_SPREAD);
            btnContinue.SetBounds(pad - UiTheme.GLOW_SPREAD, y, width + (2 * UiTheme.GLOW_SPREAD), button_height);
            y = btnContinue.Bottom + gap;

            lblMessage.SetBounds(pad, y, width, UiScale.Pixels(44, step));
            lblView.SetBounds(pad, pnlSummary.Height - pad - UiScale.Pixels(64, step), width, UiScale.Pixels(64, step));
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

            string[] seats = _selection.GetSelectedSeats();
            lblSeats.Text = seats.Length == 0 ? "Click a free seat to choose it." : string.Join(", ", seats);
            lblTickets.Text = SeatPanelText.TicketsText(seats.Length);
            lblPriceEach.Text = SeatPanelText.PriceEachText(_showing.Price);
            lblTotal.Text = SeatPanelText.TotalText(seats.Length, _showing.Price);
            btnContinue.Enabled = seats.Length > 0;
        }
    }
}
