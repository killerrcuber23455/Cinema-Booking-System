// a thin bar that shows how full a showing is: warm amber, and red when it is nearly sold out (owner: divyesh)
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class FillBar : Control
    {
        // from this share on, the bar turns red to say "hurry"
        private const double NEARLY_FULL = 0.85;

        private readonly double _fraction;

        public FillBar(double fraction)
        {
            _fraction = Math.Max(0, Math.Min(1, fraction));

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.CARD;
            e.Graphics.Clear(behind);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            float radius = Height / 2f;
            RectangleF track = new RectangleF(0, 0, Width - 1, Height - 1);

            using (GraphicsPath track_path = GlowPainter.RoundedPath(track, radius))
            using (SolidBrush track_brush = new SolidBrush(ColorMath.Blend(behind, UiTheme.BACKGROUND, 0.6)))
            {
                e.Graphics.FillPath(track_brush, track_path);
            }

            float filled_width = (float)((Width - 1) * _fraction);
            if (filled_width < 1)
            {
                return;
            }

            Color fill_colour = _fraction >= NEARLY_FULL ? UiTheme.ACCENT_RED : UiTheme.AMBER;
            using (GraphicsPath fill_path = GlowPainter.RoundedPath(new RectangleF(0, 0, filled_width, Height - 1), radius))
            using (SolidBrush fill_brush = new SolidBrush(fill_colour))
            {
                e.Graphics.FillPath(fill_brush, fill_path);
            }
        }
    }
}
