// one showing on the film page: the date block, the time, the screen and price, how full it is, and a button to choose seats (owner: divyesh)
// it is drawn as one control so the page scrolls smoothly. the text comes from ShowingLabel and ShowingTimes, so it is tested
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Globalization;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class ShowingCard : Control
    {
        private const int BASE_HEIGHT = 96;
        private const int BASE_DATE_BLOCK = 72;
        private const int BASE_BUTTON_WIDTH = 170;
        private const int BASE_BUTTON_HEIGHT = 48;
        private const int BASE_BAR_WIDTH = 220;

        private readonly Showing _showing;
        private readonly GlowButton btnChoose = new GlowButton();
        private float _step = UiScale.SMALL_STEP;
        private bool _is_hovered;

        public ShowingCard(Showing showing, Action<Showing> choose_seats)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            _showing = showing;
            BackColor = UiTheme.BACKGROUND;
            Margin = new Padding(0, 0, 0, UiTheme.SPACE_M);
            Height = BASE_HEIGHT;

            btnChoose.Text = "Choose seats";
            btnChoose.BackColor = UiTheme.ACCENT_RED;
            btnChoose.Click += (sender, e) => choose_seats(_showing);
            Controls.Add(btnChoose);

            ApplyStep(UiScale.SMALL_STEP);
        }

        public void ApplyStep(float step)
        {
            _step = step;
            Height = UiScale.Pixels(BASE_HEIGHT, step);
            btnChoose.Font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_BODY, step);
            LayoutButton();
            Invalidate();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            LayoutButton();
        }

        private void LayoutButton()
        {
            int pad = UiScale.Pixels(UiTheme.SPACE_XL, _step);
            int button_width = UiScale.Pixels(BASE_BUTTON_WIDTH, _step);
            int button_height = UiScale.Pixels(BASE_BUTTON_HEIGHT, _step);

            btnChoose.SetBounds(Width - pad - button_width - UiTheme.GLOW_SPREAD,
                ((Height - button_height) / 2) - UiTheme.GLOW_SPREAD,
                button_width + (2 * UiTheme.GLOW_SPREAD), button_height + (2 * UiTheme.GLOW_SPREAD));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.Clear(UiTheme.BACKGROUND);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            RectangleF body = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            float radius = UiScale.Pixels(14, _step);

            using (GraphicsPath path = GlowPainter.RoundedPath(body, radius))
            {
                using (SolidBrush fill = new SolidBrush(_is_hovered ? UiTheme.CARD : UiTheme.PANEL))
                {
                    graphics.FillPath(fill, path);
                }

                using (Pen edge = new Pen(_is_hovered ? ColorMath.Blend(UiTheme.LINE, UiTheme.ACCENT_RED, 0.5) : UiTheme.LINE))
                {
                    graphics.DrawPath(edge, path);
                }
            }

            CultureInfo culture = CultureInfo.InvariantCulture;
            int pad = UiScale.Pixels(UiTheme.SPACE_XL, _step);
            int date_width = UiScale.Pixels(BASE_DATE_BLOCK, _step);

            // the date block: the weekday, the day in big letters, the month
            Font small = FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_SMALL, _step);
            Font day_font = FontCache.Scaled(FontCache.DISPLAY, UiTheme.SIZE_TITLE + 4, _step);
            Rectangle date_area = new Rectangle(pad, 0, date_width, Height);
            int small_height = TextRenderer.MeasureText("Ag", small).Height;
            int day_height = TextRenderer.MeasureText("28", day_font).Height;
            int block_top = (Height - (small_height + day_height + small_height)) / 2;

            TextFormatFlags centre = TextFormatFlags.HorizontalCenter | TextFormatFlags.Top | TextFormatFlags.NoPadding;
            TextRenderer.DrawText(graphics, _showing.StartsAt.ToString("ddd", culture).ToUpperInvariant(), small,
                new Rectangle(date_area.X, block_top, date_area.Width, small_height), UiTheme.TEXT_MUTED, centre);
            TextRenderer.DrawText(graphics, _showing.StartsAt.Day.ToString(culture), day_font,
                new Rectangle(date_area.X, block_top + small_height, date_area.Width, day_height), UiTheme.TEXT, centre);
            TextRenderer.DrawText(graphics, _showing.StartsAt.ToString("MMM", culture).ToUpperInvariant(), small,
                new Rectangle(date_area.X, block_top + small_height + day_height, date_area.Width, small_height), UiTheme.AMBER, centre);

            // a thin line between the date block and the details
            int line_x = date_area.Right + UiScale.Pixels(UiTheme.SPACE_M, _step);
            using (Pen divider = new Pen(UiTheme.LINE))
            {
                graphics.DrawLine(divider, line_x, UiScale.Pixels(UiTheme.SPACE_L, _step), line_x, Height - UiScale.Pixels(UiTheme.SPACE_L, _step));
            }

            int text_left = line_x + UiScale.Pixels(UiTheme.SPACE_XL, _step);
            int text_right = btnChoose.Left - UiScale.Pixels(UiTheme.SPACE_L, _step);

            Font time_font = FontCache.Scaled(FontCache.DISPLAY, UiTheme.SIZE_TITLE, _step);
            Font detail_font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, _step);
            int time_height = TextRenderer.MeasureText("00:00", time_font).Height;
            int detail_height = TextRenderer.MeasureText("Ag", detail_font).Height;
            int bar_height = UiScale.Pixels(5, _step);
            int gap = UiScale.Pixels(UiTheme.SPACE_S, _step);
            int text_top = (Height - (time_height + detail_height + gap + bar_height)) / 2;

            TextFormatFlags left = TextFormatFlags.Left | TextFormatFlags.Top | TextFormatFlags.NoPadding | TextFormatFlags.EndEllipsis;
            Rectangle time_area = new Rectangle(text_left, text_top, Math.Max(10, text_right - text_left), time_height);
            TextRenderer.DrawText(graphics, _showing.StartsAt.ToString("HH:mm", culture), time_font, time_area, UiTheme.TEXT, left);

            // "Screen 2 · Rs 300.00" and, when it is later today, how soon it starts
            string starts_in = ShowingTimes.StartsIn(_showing.StartsAt, DateTime.Now);
            string detail = "Screen " + _showing.ScreenId + "  ·  Rs " + _showing.Price.ToString("0.00", culture)
                + "  ·  " + ShowingLabel.DescribeSeatsLeft(_showing);
            Rectangle detail_area = new Rectangle(text_left, time_area.Bottom, Math.Max(10, text_right - text_left), detail_height);
            TextRenderer.DrawText(graphics, detail, detail_font, detail_area, UiTheme.TEXT_MUTED, left);

            if (starts_in != "")
            {
                int detail_width = TextRenderer.MeasureText(detail, detail_font).Width;
                Rectangle starts_area = new Rectangle(text_left + detail_width + UiScale.Pixels(UiTheme.SPACE_M, _step), detail_area.Y,
                    Math.Max(10, text_right - text_left - detail_width - UiScale.Pixels(UiTheme.SPACE_M, _step)), detail_height);
                TextRenderer.DrawText(graphics, starts_in, detail_font, starts_area, UiTheme.AMBER, left);
            }

            // how full the showing is: amber, and red when it is nearly sold out
            RectangleF track = new RectangleF(text_left, detail_area.Bottom + gap,
                Math.Min(UiScale.Pixels(BASE_BAR_WIDTH, _step), Math.Max(10, text_right - text_left)), bar_height);
            DrawFillBar(graphics, track, ShowingTimes.FillFraction(_showing));
        }

        private static void DrawFillBar(Graphics graphics, RectangleF track, double fraction)
        {
            float radius = track.Height / 2f;

            using (GraphicsPath track_path = GlowPainter.RoundedPath(track, radius))
            using (SolidBrush track_brush = new SolidBrush(UiTheme.BACKGROUND))
            {
                graphics.FillPath(track_brush, track_path);
            }

            float filled = (float)(track.Width * Math.Max(0, Math.Min(1, fraction)));
            if (filled < 1)
            {
                return;
            }

            Color bar_colour = fraction >= 0.85 ? UiTheme.ACCENT_RED : UiTheme.AMBER;
            RectangleF bar = new RectangleF(track.X, track.Y, Math.Max(filled, track.Height), track.Height);

            using (GraphicsPath bar_path = GlowPainter.RoundedPath(bar, radius))
            using (SolidBrush bar_brush = new SolidBrush(bar_colour))
            {
                graphics.FillPath(bar_brush, bar_path);
            }
        }

        protected override void OnMouseEnter(EventArgs e)
        {
            base.OnMouseEnter(e);
            _is_hovered = true;
            Invalidate();
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);

            // the button covers part of the card, so the mouse moving onto it is not leaving the card
            _is_hovered = ClientRectangle.Contains(PointToClient(Cursor.Position));
            Invalidate();
        }
    }
}
