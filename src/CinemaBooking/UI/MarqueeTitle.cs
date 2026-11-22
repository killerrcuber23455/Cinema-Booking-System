// the film title on the film page, in a frame of small marquee bulbs like the sign outside a cinema (owner: divyesh)
// when the page opens the bulbs light up one after another, once, then stay lit. this is the only movement that
// starts by itself. if windows is set to show fewer animations, the bulbs are lit at once
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class MarqueeTitle : Control
    {
        private const int FRAME_INSET = 7;
        private const int FRAME_RADIUS = 10;
        private const int BULB_SPACING = 16;
        private const float BULB_RADIUS = 2.6f;
        private const float BULB_GLOW_RADIUS = 6f;
        private const int TEXT_PADDING = 24;
        private const int LIGHT_INTERVAL_MS = 16;
        private const int BULBS_PER_TICK = 2;

        private readonly System.Windows.Forms.Timer _light_timer = new System.Windows.Forms.Timer();
        private PointF[] _bulbs = new PointF[0];
        private int _lit_count;
        private bool _has_played;

        public MarqueeTitle()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;

            _light_timer.Interval = LIGHT_INTERVAL_MS;
            _light_timer.Tick += (sender, e) => LightNextBulbs();
        }

        // the bulbs sit every few pixels along the frame: top, right side, bottom, left side
        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            PlaceBulbs();
        }

        private void PlaceBulbs()
        {
            float left = FRAME_INSET;
            float top = FRAME_INSET;
            float right = Width - FRAME_INSET;
            float bottom = Height - FRAME_INSET;

            int across = Math.Max(2, (int)((right - left) / BULB_SPACING));
            int down = Math.Max(1, (int)((bottom - top) / BULB_SPACING));
            PointF[] bulbs = new PointF[(across * 2) + (down * 2)];
            int next = 0;

            for (int step = 0; step < across; step++)
            {
                bulbs[next++] = new PointF(left + ((right - left) * step / across), top);
            }
            for (int step = 0; step < down; step++)
            {
                bulbs[next++] = new PointF(right, top + ((bottom - top) * step / down));
            }
            for (int step = 0; step < across; step++)
            {
                bulbs[next++] = new PointF(right - ((right - left) * step / across), bottom);
            }
            for (int step = 0; step < down; step++)
            {
                bulbs[next++] = new PointF(left, bottom - ((bottom - top) * step / down));
            }

            _bulbs = bulbs;
            if (_has_played && !_light_timer.Enabled)
            {
                _lit_count = _bulbs.Length;
            }
        }

        // starts the light run once, when the title is first put on screen
        protected override void OnHandleCreated(EventArgs e)
        {
            base.OnHandleCreated(e);

            if (_has_played)
            {
                return;
            }

            _has_played = true;

            if (!SystemInformation.UIEffectsEnabled)
            {
                _lit_count = _bulbs.Length;
                Invalidate();
                return;
            }

            _lit_count = 0;
            _light_timer.Start();
        }

        private void LightNextBulbs()
        {
            _lit_count = Math.Min(_bulbs.Length, _lit_count + BULBS_PER_TICK);
            Invalidate();

            if (_lit_count >= _bulbs.Length)
            {
                _light_timer.Stop();
            }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics graphics = e.Graphics;
            Color behind = Parent != null ? Parent.BackColor : UiTheme.PANEL;
            graphics.Clear(behind);
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF frame = new RectangleF(FRAME_INSET, FRAME_INSET, Width - (2 * FRAME_INSET), Height - (2 * FRAME_INSET));
            using (GraphicsPath frame_path = GlowPainter.RoundedPath(frame, FRAME_RADIUS))
            using (Pen frame_pen = new Pen(ColorMath.Blend(behind, UiTheme.AMBER, 0.25), 1.2f))
            {
                graphics.DrawPath(frame_pen, frame_path);
            }

            Color dark_bulb = ColorMath.Blend(behind, UiTheme.AMBER, 0.18);

            for (int bulb_index = 0; bulb_index < _bulbs.Length; bulb_index++)
            {
                PointF bulb = _bulbs[bulb_index];

                if (bulb_index < _lit_count)
                {
                    using (SolidBrush glow = new SolidBrush(Color.FromArgb(55, UiTheme.AMBER)))
                    {
                        graphics.FillEllipse(glow, bulb.X - BULB_GLOW_RADIUS, bulb.Y - BULB_GLOW_RADIUS,
                            BULB_GLOW_RADIUS * 2, BULB_GLOW_RADIUS * 2);
                    }

                    using (SolidBrush lit = new SolidBrush(UiTheme.AMBER))
                    {
                        graphics.FillEllipse(lit, bulb.X - BULB_RADIUS, bulb.Y - BULB_RADIUS, BULB_RADIUS * 2, BULB_RADIUS * 2);
                    }
                }
                else
                {
                    using (SolidBrush unlit = new SolidBrush(dark_bulb))
                    {
                        graphics.FillEllipse(unlit, bulb.X - BULB_RADIUS, bulb.Y - BULB_RADIUS, BULB_RADIUS * 2, BULB_RADIUS * 2);
                    }
                }
            }

            Rectangle text_area = new Rectangle(TEXT_PADDING, FRAME_INSET, Width - (2 * TEXT_PADDING), Height - (2 * FRAME_INSET));
            TextRenderer.DrawText(graphics, Text, UiTheme.TITLE_FONT, text_area, UiTheme.TEXT,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _light_timer.Dispose();
            }

            base.Dispose(disposing);
        }
    }
}
