// a panel with rounded corners, used for the cards on every window (owner: divyesh)
// it has a very faint light at the top, and a soft red glow along its edge when it is highlighted (a film card under the mouse)
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class RoundedPanel : Panel
    {
        // how much lighter the top of the panel is than the bottom, kept small so labels on it do not show edges
        private const double TOP_LIGHT = 0.025;

        // the edge glow is a few thin outlines, each a little further in and fainter
        private const int EDGE_GLOW_LINES = 3;

        private bool _is_highlighted;

        // how round the corners are, in pixels
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int CornerRadius { get; set; } = UiTheme.PANEL_RADIUS;

        // true while the mouse is on a film card, the edge then glows
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public bool IsHighlighted
        {
            get { return _is_highlighted; }
            set
            {
                if (_is_highlighted == value)
                {
                    return;
                }

                _is_highlighted = value;
                Invalidate();
            }
        }

        public RoundedPanel()
        {
            // draw the panel ourselves and avoid flicker when it is resized
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = UiTheme.PANEL;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // colour behind the corners, so they look cut out
            Color corner_colour = Parent != null ? Parent.BackColor : UiTheme.BACKGROUND;
            e.Graphics.Clear(corner_colour);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            if (Width <= 1 || Height <= 1)
            {
                return;
            }

            RectangleF area = new RectangleF(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = GlowPainter.RoundedPath(area, CornerRadius))
            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle,
                ColorMath.Blend(BackColor, Color.White, TOP_LIGHT), BackColor, LinearGradientMode.Vertical))
            {
                e.Graphics.FillPath(brush, path);
            }

            if (_is_highlighted)
            {
                for (int line_index = 0; line_index < EDGE_GLOW_LINES; line_index++)
                {
                    float inset = 0.5f + line_index;
                    RectangleF line_area = RectangleF.Inflate(area, -inset, -inset);
                    int alpha = 200 - (line_index * 70);

                    using (GraphicsPath line_path = GlowPainter.RoundedPath(line_area, CornerRadius - inset))
                    using (Pen line_pen = new Pen(Color.FromArgb(alpha, UiTheme.ACCENT_RED), 1f))
                    {
                        e.Graphics.DrawPath(line_pen, line_path);
                    }
                }
            }
        }
    }
}
