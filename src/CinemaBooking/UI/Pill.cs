// a small rounded tag for the film cards, like the genre or the age rating (owner: divyesh)
// it sizes itself to its text
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class Pill : Control
    {
        private const int SIDE_PADDING = 9;
        private const int PILL_HEIGHT = 20;

        private readonly Color _fill;
        private readonly Color _text_colour;

        public Pill(string text, Color fill, Color text_colour)
        {
            _fill = fill;
            _text_colour = text_colour;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;

            Text = text;
            Size text_size = TextRenderer.MeasureText(text, UiTheme.SMALL_FONT, Size.Empty, TextFormatFlags.NoPadding);
            Size = new Size(text_size.Width + (2 * SIDE_PADDING), PILL_HEIGHT);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.CARD;
            e.Graphics.Clear(behind);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath shape = GlowPainter.RoundedPath(new RectangleF(0, 0, Width - 1, Height - 1), Height / 2f))
            using (SolidBrush brush = new SolidBrush(_fill))
            {
                e.Graphics.FillPath(brush, shape);
            }

            TextRenderer.DrawText(e.Graphics, Text, UiTheme.SMALL_FONT, ClientRectangle, _text_colour,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
