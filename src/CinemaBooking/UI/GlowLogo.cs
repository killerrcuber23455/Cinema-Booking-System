// the CINEMA name at the top of the side menu, drawn in red with a soft neon glow around the letters (owner: divyesh)
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class GlowLogo : Control
    {
        private const float LETTER_SIZE = 26f;

        // the glow is a few wide see-through outlines of the letters, the widest first
        private static readonly int[] GLOW_WIDTHS = { 12, 8, 5 };
        private const int GLOW_ALPHA = 34;

        public GlowLogo()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // the menu behind is a fade, so the logo paints the same part of it
            GlowPainter.FillMenuBackground(e.Graphics, this);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            using (GraphicsPath letters = new GraphicsPath())
            {
                float top = (Height - LETTER_SIZE) / 2f - 4f;
                letters.AddString(Text, UiTheme.TITLE_FONT.FontFamily, (int)FontStyle.Bold, LETTER_SIZE,
                    new PointF(2f, top), StringFormat.GenericTypographic);

                for (int glow_index = 0; glow_index < GLOW_WIDTHS.Length; glow_index++)
                {
                    using (Pen glow_pen = new Pen(Color.FromArgb(GLOW_ALPHA, UiTheme.ACCENT_RED), GLOW_WIDTHS[glow_index]))
                    {
                        glow_pen.LineJoin = LineJoin.Round;
                        e.Graphics.DrawPath(glow_pen, letters);
                    }
                }

                using (SolidBrush letter_brush = new SolidBrush(UiTheme.ACCENT_RED))
                {
                    e.Graphics.FillPath(letter_brush, letters);
                }
            }
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            Invalidate();
        }
    }
}
