// the drawn seat map: a curved glowing screen at the top that lights the room, and every seat under it (owner: divyesh)
// UI upgrade step 3: the map fills the space it is given and the seats grow with it (SeatMapGeometry does the sums).
// seats nearer the screen and nearer the centre catch more of its light. free seats have a thin glowing outline,
// chosen seats glow red, taken seats are dim. it works with the mouse and with the arrow keys and space.
// it only draws and reads clicks and keys, SeatSelection decides what a click does and SeatGuide gives the words
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class SeatMapCanvas : Control
    {
        private const int SCREEN_TOP = 16;
        private const int SCREEN_CURVE = 14;
        private const int SCREEN_THICKNESS = 6;

        // the seats were first drawn 38 wide, every size that is not the seat itself grows with this
        private const double FIRST_SEAT_WIDTH = 38.0;

        // how much light a seat gets: the front row and the centre get the most
        private const double LIGHT_LOST_TO_THE_BACK = 0.55;
        private const double LIGHT_LOST_TO_THE_SIDE = 0.25;
        private const double LEAST_LIGHT = 0.2;

        private readonly Showing _showing;
        private readonly SeatSelection _selection;

        // called with the seat code when a seat is clicked or picked with space
        private readonly Action<string> _click_seat;

        // called with the view text of the seat under the mouse or the keyboard, an empty text when there is none
        private readonly Action<string> _show_view;

        private readonly int _rows;
        private readonly int _seats_per_row;
        private SeatMapGeometry _geometry;

        // the seat under the mouse, -1 when there is none
        private int _hover_row = -1;
        private int _hover_seat = -1;

        // the seat the arrow keys are on, the ring only shows once a key was used
        private int _key_row;
        private int _key_seat;
        private bool _is_using_keys;

        public SeatMapCanvas(Showing showing, SeatSelection selection, Action<string> click_seat, Action<string> show_view)
        {
            _showing = showing;
            _selection = selection;
            _click_seat = click_seat;
            _show_view = show_view;
            _rows = showing.SeatGrid.Rows;
            _seats_per_row = showing.SeatGrid.SeatsPerRow;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer
                | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);
            TabStop = true;
            Cursor = Cursors.Hand;
            BackColor = UiTheme.BACKGROUND;

            _geometry = SeatMapGeometry.Make(Width, Height, _rows, _seats_per_row);

            // the middle of the front row is a good place to start with the keys
            _key_seat = _seats_per_row / 2;
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            _geometry = SeatMapGeometry.Make(Width, Height, _rows, _seats_per_row);
            Invalidate();
        }

        // how much bigger than the first drawing the seats are now
        private float Grow
        {
            get { return (float)(_geometry.SeatWidth / FIRST_SEAT_WIDTH); }
        }

        private int Scaled(int first_size)
        {
            return (int)Math.Round(first_size * Grow);
        }

        private Font NumberFont
        {
            get { return FontCache.Get(FontCache.MEDIUM, UiScale.Size(Math.Max(8f, _geometry.SeatWidth * 0.2f), 1f)); }
        }

        private Font LabelFont
        {
            get { return FontCache.Get(FontCache.MEDIUM, UiScale.Size(Math.Max(9f, _geometry.SeatWidth * 0.22f), 1f)); }
        }

        // ---- drawing ----

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            graphics.Clear(BackColor);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;
            graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            DrawLightCone(graphics);
            DrawScreen(graphics);

            for (int row_index = 0; row_index < _rows; row_index++)
            {
                DrawRowLetter(graphics, row_index);

                for (int seat_index = 0; seat_index < _seats_per_row; seat_index++)
                {
                    DrawSeat(graphics, row_index, seat_index);
                }
            }

            DrawLegend(graphics);
        }

        // the light from the screen falls on the first rows and fades towards the back
        private void DrawLightCone(Graphics graphics)
        {
            int cone_top = SCREEN_TOP + SCREEN_CURVE;
            int cone_bottom = _geometry.GridTop + (_geometry.GridHeight * 2 / 3);
            int spill = SeatMapGeometry.SIDE_SPACE / 2;
            int left = _geometry.GridLeft;
            int width = _geometry.GridWidth;

            if (cone_bottom <= cone_top)
            {
                return;
            }

            PointF[] cone =
            {
                new PointF(left, cone_top),
                new PointF(left + width, cone_top),
                new PointF(left + width + spill, cone_bottom),
                new PointF(left - spill, cone_bottom)
            };

            Rectangle fade_area = new Rectangle(left, cone_top, width, cone_bottom - cone_top);
            using (LinearGradientBrush light = new LinearGradientBrush(fade_area,
                Color.FromArgb(34, UiTheme.SCREEN_LIGHT), Color.FromArgb(0, UiTheme.SCREEN_LIGHT), LinearGradientMode.Vertical))
            {
                // flip instead of repeat, so the light does not start again below the fade (that made a thin line between two rows)
                light.WrapMode = WrapMode.TileFlipXY;
                graphics.FillPolygon(light, cone);
            }
        }

        // a thin curved band of light, higher in the middle, with a soft glow
        private void DrawScreen(Graphics graphics)
        {
            float left = _geometry.GridLeft;
            float right = _geometry.GridLeft + _geometry.GridWidth;
            float third = _geometry.GridWidth / 3f;
            int curve = Math.Max(SCREEN_CURVE, Scaled(SCREEN_CURVE));
            int thickness = Math.Max(SCREEN_THICKNESS, Scaled(SCREEN_THICKNESS));

            using (GraphicsPath screen = new GraphicsPath())
            {
                screen.AddBezier(left, SCREEN_TOP + curve, left + third, SCREEN_TOP,
                    right - third, SCREEN_TOP, right, SCREEN_TOP + curve);
                screen.AddLine(right, SCREEN_TOP + curve, right, SCREEN_TOP + curve + thickness);
                screen.AddBezier(right, SCREEN_TOP + curve + thickness, right - third, SCREEN_TOP + thickness,
                    left + third, SCREEN_TOP + thickness, left, SCREEN_TOP + curve + thickness);
                screen.CloseFigure();

                int[] glow_widths = { 18, 12, 7 };
                for (int glow_index = 0; glow_index < glow_widths.Length; glow_index++)
                {
                    using (Pen glow = new Pen(Color.FromArgb(22, UiTheme.SCREEN_LIGHT), Scaled(glow_widths[glow_index])))
                    {
                        glow.LineJoin = LineJoin.Round;
                        graphics.DrawPath(glow, screen);
                    }
                }

                using (SolidBrush screen_brush = new SolidBrush(UiTheme.SCREEN_LIGHT))
                {
                    graphics.FillPath(screen_brush, screen);
                }
            }

            Rectangle label_area = new Rectangle(_geometry.GridLeft, SCREEN_TOP + curve + thickness + 8, _geometry.GridWidth, 22);
            TextRenderer.DrawText(graphics, "SCREEN", LabelFont, label_area, UiTheme.TEXT_FAINT,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        private void DrawRowLetter(Graphics graphics, int row_index)
        {
            Rectangle seat = _geometry.SeatArea(row_index, 0);
            Rectangle letter_area = new Rectangle(_geometry.GridLeft - SeatMapGeometry.ROW_LABEL_WIDTH, seat.Top,
                SeatMapGeometry.ROW_LABEL_WIDTH - 6, seat.Height);
            string row_letter = ((char)('A' + row_index)).ToString();

            TextRenderer.DrawText(graphics, row_letter, LabelFont, letter_area, UiTheme.TEXT_MUTED,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);
        }

        // 1 for the middle of the front row, less towards the back and the sides
        private double LightOnSeat(int row_index, int seat_index)
        {
            double back_share = _rows > 1 ? (double)row_index / (_rows - 1) : 0;
            double centre = (_seats_per_row - 1) / 2.0;
            double side_share = centre > 0 ? Math.Abs(seat_index - centre) / centre : 0;

            double light = 1 - (LIGHT_LOST_TO_THE_BACK * back_share) - (LIGHT_LOST_TO_THE_SIDE * side_share);
            return Math.Max(LEAST_LIGHT, light);
        }

        private void DrawSeat(Graphics graphics, int row_index, int seat_index)
        {
            string seat_code = SeatSelection.MakeSeatCode(row_index, seat_index);
            Rectangle seat = _geometry.SeatArea(row_index, seat_index);
            RectangleF seat_shape = new RectangleF(seat.X, seat.Y, seat.Width, seat.Height);
            float radius = Math.Max(5, _geometry.SeatWidth / 5f);
            int glow_spread = Math.Max(5, Scaled(6));

            bool is_chosen = _selection.IsSelected(seat_code);
            bool is_taken = !is_chosen && !_showing.SeatGrid.IsSeatFree(seat_code);
            bool is_hovered = row_index == _hover_row && seat_index == _hover_seat;
            double light = LightOnSeat(row_index, seat_index);

            Color fill;
            Color outline = Color.Empty;
            Color number_colour = UiTheme.TEXT;

            if (is_chosen)
            {
                GlowPainter.DrawGlow(graphics, seat_shape, radius, UiTheme.ACCENT_RED, 4, glow_spread);
                fill = UiTheme.ACCENT_RED;
            }
            else if (is_taken)
            {
                fill = ColorMath.Blend(UiTheme.BACKGROUND, UiTheme.PANEL, 0.3);
                number_colour = ColorMath.Blend(UiTheme.PANEL, UiTheme.TEXT_MUTED, 0.45);
            }
            else
            {
                // a free seat catches a little of the screen light, and its outline glows with it
                fill = ColorMath.Blend(UiTheme.CARD, UiTheme.SCREEN_LIGHT, 0.07 * light);
                outline = ColorMath.Blend(UiTheme.PANEL, UiTheme.SCREEN_LIGHT, 0.22 + (0.45 * light));

                if (is_hovered)
                {
                    GlowPainter.DrawGlow(graphics, seat_shape, radius, UiTheme.SCREEN_LIGHT, 3, Math.Max(4, Scaled(4)));
                    outline = UiTheme.TEXT;
                }
            }

            using (GraphicsPath shape = GlowPainter.RoundedPath(seat_shape, radius))
            {
                using (SolidBrush fill_brush = new SolidBrush(fill))
                {
                    graphics.FillPath(fill_brush, shape);
                }

                if (outline != Color.Empty)
                {
                    using (Pen outline_pen = new Pen(outline, is_hovered ? 1.6f : 1.2f))
                    {
                        graphics.DrawPath(outline_pen, shape);
                    }
                }
            }

            // the ring of the arrow keys, in the warm marquee colour
            if (_is_using_keys && Focused && row_index == _key_row && seat_index == _key_seat)
            {
                RectangleF ring = RectangleF.Inflate(seat_shape, 3, 3);
                using (GraphicsPath ring_path = GlowPainter.RoundedPath(ring, radius + 3))
                using (Pen ring_pen = new Pen(UiTheme.AMBER, 2f))
                {
                    graphics.DrawPath(ring_pen, ring_path);
                }
            }

            TextRenderer.DrawText(graphics, (seat_index + 1).ToString(), NumberFont, seat, number_colour,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }

        // three small seats under the map that say what the colours mean
        private void DrawLegend(Graphics graphics)
        {
            int legend_top = _geometry.GridTop + _geometry.GridHeight + 20;
            string[] names = { "Free", "Chosen", "Taken" };
            int swatch_width = Math.Max(22, Scaled(22));
            int swatch_height = Math.Max(18, Scaled(18));
            int step = Math.Max(110, Scaled(110));

            for (int item_index = 0; item_index < names.Length; item_index++)
            {
                int left = _geometry.GridLeft + (item_index * step);
                RectangleF swatch = new RectangleF(left, legend_top, swatch_width, swatch_height);

                using (GraphicsPath shape = GlowPainter.RoundedPath(swatch, 5))
                {
                    if (item_index == 0)
                    {
                        using (SolidBrush brush = new SolidBrush(UiTheme.CARD))
                        using (Pen pen = new Pen(ColorMath.Blend(UiTheme.PANEL, UiTheme.SCREEN_LIGHT, 0.55), 1.2f))
                        {
                            graphics.FillPath(brush, shape);
                            graphics.DrawPath(pen, shape);
                        }
                    }
                    else if (item_index == 1)
                    {
                        GlowPainter.DrawGlow(graphics, swatch, 5, UiTheme.ACCENT_RED, 3, 4);
                        using (SolidBrush brush = new SolidBrush(UiTheme.ACCENT_RED))
                        {
                            graphics.FillPath(brush, shape);
                        }
                    }
                    else
                    {
                        using (SolidBrush brush = new SolidBrush(ColorMath.Blend(UiTheme.BACKGROUND, UiTheme.PANEL, 0.3)))
                        {
                            graphics.FillPath(brush, shape);
                        }
                    }
                }

                TextRenderer.DrawText(graphics, names[item_index], LabelFont,
                    new Rectangle(left + swatch_width + 8, legend_top - 2, step - swatch_width - 12, swatch_height + 4), UiTheme.TEXT_MUTED,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter);
            }
        }

        // ---- mouse ----

        protected override void OnMouseMove(MouseEventArgs e)
        {
            base.OnMouseMove(e);
            _geometry.FindSeat(e.Location, _rows, _seats_per_row, out int row_index, out int seat_index);

            if (row_index == _hover_row && seat_index == _hover_seat)
            {
                return;
            }

            _hover_row = row_index;
            _hover_seat = seat_index;
            Invalidate();

            _show_view(row_index >= 0 ? SeatGuide.DescribeView(row_index, seat_index, _rows, _seats_per_row) : "");
        }

        protected override void OnMouseLeave(EventArgs e)
        {
            base.OnMouseLeave(e);
            _hover_row = -1;
            _hover_seat = -1;
            Invalidate();
            _show_view("");
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();

            if (e.Button != MouseButtons.Left || !_geometry.FindSeat(e.Location, _rows, _seats_per_row, out int row_index, out int seat_index))
            {
                return;
            }

            // a mouse click moves the key ring there too, but hides it until a key is used
            _key_row = row_index;
            _key_seat = seat_index;
            _is_using_keys = false;
            _click_seat(SeatSelection.MakeSeatCode(row_index, seat_index));
        }

        // ---- keys ----

        // the arrow keys must reach this control instead of moving the focus to the next button
        protected override bool IsInputKey(Keys keyData)
        {
            if (keyData == Keys.Up || keyData == Keys.Down || keyData == Keys.Left || keyData == Keys.Right)
            {
                return true;
            }

            return base.IsInputKey(keyData);
        }

        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                _is_using_keys = true;
                _click_seat(SeatSelection.MakeSeatCode(_key_row, _key_seat));
                e.Handled = true;
                return;
            }

            SeatMove move;
            switch (e.KeyCode)
            {
                case Keys.Up:
                    move = SeatMove.Up;
                    break;
                case Keys.Down:
                    move = SeatMove.Down;
                    break;
                case Keys.Left:
                    move = SeatMove.Left;
                    break;
                case Keys.Right:
                    move = SeatMove.Right;
                    break;
                default:
                    return;
            }

            // the first key press only shows the ring where it is, the next ones move it
            if (_is_using_keys)
            {
                SeatGuide.Move(_key_row, _key_seat, move, _rows, _seats_per_row, out _key_row, out _key_seat);
            }

            _is_using_keys = true;
            e.Handled = true;
            Invalidate();
            _show_view(SeatGuide.DescribeView(_key_row, _key_seat, _rows, _seats_per_row));
        }

        protected override void OnGotFocus(EventArgs e)
        {
            base.OnGotFocus(e);
            Invalidate();
        }

        protected override void OnLostFocus(EventArgs e)
        {
            base.OnLostFocus(e);
            Invalidate();
        }
    }
}
