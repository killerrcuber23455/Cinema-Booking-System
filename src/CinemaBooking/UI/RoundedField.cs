// a rounded box that holds a text box or a drop-down list, with a thin edge that turns red while the user types in it (owner: divyesh)
// a normal text box has a square edge and cannot be rounded, so the box inside has no edge of its own and this draws it
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class RoundedField : Control
    {
        private const int RADIUS = 10;
        private const int SIDE_SPACE = 14;

        private Control? _inner;
        private bool _has_focus;

        public RoundedField()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            TabStop = false;
            BackColor = UiTheme.INPUT;
            Click += (sender, e) => _inner?.Focus();
        }

        // puts the text box (or the list) inside, in the middle, without its own edge
        public void Host(Control inner)
        {
            _inner = inner;
            inner.BackColor = UiTheme.INPUT;
            inner.ForeColor = UiTheme.TEXT;
            inner.Font = UiTheme.BODY_FONT;

            if (inner is TextBox box)
            {
                box.BorderStyle = BorderStyle.None;
            }

            inner.Enter += (sender, e) => SetFocus(true);
            inner.Leave += (sender, e) => SetFocus(false);
            Controls.Add(inner);
            LayoutInner();
        }

        private void SetFocus(bool has_focus)
        {
            _has_focus = has_focus;
            Invalidate();
        }

        protected override void OnResize(EventArgs e)
        {
            base.OnResize(e);
            LayoutInner();
        }

        // the inner control is as wide as the field minus the sides and sits in the middle from top to bottom
        private void LayoutInner()
        {
            if (_inner == null)
            {
                return;
            }

            _inner.Width = Math.Max(10, Width - (2 * SIDE_SPACE));
            _inner.Left = SIDE_SPACE;
            _inner.Top = Math.Max(0, (Height - _inner.Height) / 2);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.PANEL;
            e.Graphics.Clear(behind);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF body = new RectangleF(0.5f, 0.5f, Width - 2, Height - 2);
            Color edge = _has_focus ? UiTheme.ACCENT_RED : ColorMath.Blend(UiTheme.LINE, Color.White, 0.12);

            using (GraphicsPath path = GlowPainter.RoundedPath(body, RADIUS))
            using (SolidBrush fill = new SolidBrush(UiTheme.INPUT))
            using (Pen edge_pen = new Pen(edge, _has_focus ? 1.5f : 1f))
            {
                e.Graphics.FillPath(fill, path);
                e.Graphics.DrawPath(edge_pen, path);
            }
        }
    }
}
