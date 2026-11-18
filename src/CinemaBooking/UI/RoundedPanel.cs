// a panel with rounded corners, used for the cards on every window (owner: divyesh)
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class RoundedPanel : Panel
    {
        // how round the corners are, in pixels
        [System.ComponentModel.DesignerSerializationVisibility(System.ComponentModel.DesignerSerializationVisibility.Hidden)]
        public int CornerRadius { get; set; } = UiTheme.PANEL_RADIUS;

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

            Rectangle area = new Rectangle(0, 0, Width - 1, Height - 1);
            using (GraphicsPath path = BuildRoundedPath(area, CornerRadius))
            using (SolidBrush brush = new SolidBrush(BackColor))
            {
                e.Graphics.FillPath(brush, path);
            }
        }

        // builds a rectangle outline whose four corners are quarter circles
        private static GraphicsPath BuildRoundedPath(Rectangle area, int radius)
        {
            int diameter = radius * 2;
            GraphicsPath path = new GraphicsPath();
            path.AddArc(area.X, area.Y, diameter, diameter, 180, 90);
            path.AddArc(area.Right - diameter, area.Y, diameter, diameter, 270, 90);
            path.AddArc(area.Right - diameter, area.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(area.X, area.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }
    }
}

