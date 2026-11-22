// the drawing helpers every glowing part shares: rounded shapes, soft glows and the side menu fade (owner: divyesh)
// windows forms has no blur, so a glow is a few see-through rounded shapes, each a little bigger, drawn under the shape
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public static class GlowPainter
    {
        // how strong each glow layer is, the layers add up near the shape
        private const int LAYER_ALPHA = 26;

        // a rectangle whose four corners are quarter circles, a plain rectangle when the radius is 0
        public static GraphicsPath RoundedPath(RectangleF area, float radius)
        {
            GraphicsPath path = new GraphicsPath();
            float diameter = Math.Min(radius * 2, Math.Min(area.Width, area.Height));

            if (diameter < 1)
            {
                path.AddRectangle(area);
                return path;
            }

            path.AddArc(area.X, area.Y, diameter, diameter, 180, 90);
            path.AddArc(area.Right - diameter, area.Y, diameter, diameter, 270, 90);
            path.AddArc(area.Right - diameter, area.Bottom - diameter, diameter, diameter, 0, 90);
            path.AddArc(area.X, area.Bottom - diameter, diameter, diameter, 90, 90);
            path.CloseFigure();
            return path;
        }

        // a soft light around a shape. draw it first, then draw the shape on top
        public static void DrawGlow(Graphics graphics, RectangleF area, float radius, Color colour, int layers, float spread)
        {
            SmoothingMode old_mode = graphics.SmoothingMode;
            graphics.SmoothingMode = SmoothingMode.AntiAlias;

            for (int layer = layers; layer >= 1; layer--)
            {
                float grow = spread * layer / layers;
                RectangleF layer_area = RectangleF.Inflate(area, grow, grow);

                using (GraphicsPath path = RoundedPath(layer_area, radius + grow))
                using (SolidBrush brush = new SolidBrush(Color.FromArgb(LAYER_ALPHA, colour)))
                {
                    graphics.FillPath(brush, path);
                }
            }

            graphics.SmoothingMode = old_mode;
        }

        // fills a control with the part of the side menu fade that is behind it, so it matches the menu
        public static void FillMenuBackground(Graphics graphics, Control control)
        {
            Control? menu = control.Parent;

            if (menu == null || menu.Height <= 0)
            {
                graphics.Clear(UiTheme.SIDE_MENU);
                return;
            }

            // the fade is as tall as the menu and starts where the menu starts
            Rectangle fade_area = new Rectangle(0, -control.Top, Math.Max(1, control.Width), menu.Height);

            using (LinearGradientBrush brush = new LinearGradientBrush(fade_area, UiTheme.SIDE_MENU_TOP,
                UiTheme.SIDE_MENU_BOTTOM, LinearGradientMode.Vertical))
            {
                graphics.FillRectangle(brush, control.ClientRectangle);
            }
        }
    }
}
