// a rounded flat button. the red main buttons glow softly when the mouse is on them or they have the focus (owner: divyesh)
// the fill is the BackColor, so the pages set colours the same way as on a normal button.
// a little space is kept around the button for the glow, because a control cannot draw outside its edges
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class GlowButton : Button
    {
        // the hover makes the fill a little lighter
        private const double HOVER_LIGHTER = 0.10;
        private const double PRESSED_DARKER = 0.15;

        private bool _is_hovered;
        private bool _is_pressed;

        public GlowButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            ForeColor = UiTheme.TEXT;
            Cursor = Cursors.Hand;
        }

        // only the red main buttons glow, the grey ones stay calm
        private bool GlowsWhenActive
        {
            get { return BackColor.ToArgb() == UiTheme.ACCENT_RED.ToArgb(); }
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.PANEL;
            e.Graphics.Clear(behind);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int space = UiTheme.GLOW_SPREAD;
            RectangleF body = new RectangleF(space, space, Width - (2 * space) - 1, Height - (2 * space) - 1);

            Color fill = Enabled ? BackColor : UiTheme.CARD;
            if (Enabled && _is_pressed)
            {
                fill = ColorMath.Blend(fill, Color.Black, PRESSED_DARKER);
            }
            else if (Enabled && _is_hovered)
            {
                fill = ColorMath.Blend(fill, Color.White, HOVER_LIGHTER);
            }

            if (Enabled && GlowsWhenActive && (_is_hovered || Focused))
            {
                GlowPainter.DrawGlow(e.Graphics, body, UiTheme.BUTTON_RADIUS, UiTheme.ACCENT_RED, 4, space);
            }

            using (GraphicsPath path = GlowPainter.RoundedPath(body, UiTheme.BUTTON_RADIUS))
            using (SolidBrush brush = new SolidBrush(fill))
            {
                e.Graphics.FillPath(brush, path);
            }

            Color text_colour = Enabled ? ForeColor : UiTheme.TEXT_MUTED;
            TextRenderer.DrawText(e.Graphics, Text, Font, Rectangle.Round(body), text_colour,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis);
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
            _is_hovered = false;
            _is_pressed = false;
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs mevent)
        {
            base.OnMouseDown(mevent);
            _is_pressed = true;
            Invalidate();
        }

        protected override void OnMouseUp(MouseEventArgs mevent)
        {
            base.OnMouseUp(mevent);
            _is_pressed = false;
            Invalidate();
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

        protected override void OnEnabledChanged(EventArgs e)
        {
            base.OnEnabledChanged(e);
            Invalidate();
        }

        protected override void OnBackColorChanged(EventArgs e)
        {
            base.OnBackColorChanged(e);
            Invalidate();
        }
    }
}
