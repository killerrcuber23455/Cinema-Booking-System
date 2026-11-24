// a small rounded choice button, used for the genre and sort choices on the films page (owner: divyesh)
// the chosen one is filled red, the others are dark and light up under the mouse. it can be used with the keyboard too
using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Text;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class ChipButton : Control
    {
        private const int BASE_HEIGHT = 34;
        private const int BASE_SIDE_PADDING = 16;

        private bool _is_selected;
        private bool _is_hovered;
        private float _step = UiScale.SMALL_STEP;

        // true for the choice that is on now
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsSelected
        {
            get { return _is_selected; }
            set
            {
                _is_selected = value;
                Invalidate();
            }
        }

        public ChipButton(string text)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.Selectable, true);

            Text = text;
            Cursor = Cursors.Hand;
            TabStop = true;
            Margin = new Padding(0, 0, UiTheme.SPACE_S, UiTheme.SPACE_S);
            FitToText();
        }

        // the window got bigger or smaller, the chip follows
        public void SetStep(float step)
        {
            _step = step;
            FitToText();
            Invalidate();
        }

        private Font ChipFont
        {
            get { return FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_BODY, _step); }
        }

        private void FitToText()
        {
            Size text_size = TextRenderer.MeasureText(Text, ChipFont);
            int side = UiScale.Pixels(BASE_SIDE_PADDING, _step);
            Size = new Size(text_size.Width + (2 * side), UiScale.Pixels(BASE_HEIGHT, _step));
        }

        protected override void OnTextChanged(EventArgs e)
        {
            base.OnTextChanged(e);
            FitToText();
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.BACKGROUND;
            e.Graphics.Clear(behind);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            e.Graphics.TextRenderingHint = TextRenderingHint.AntiAliasGridFit;

            RectangleF body = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            Color fill;
            Color text_colour;

            if (_is_selected)
            {
                fill = _is_hovered ? ColorMath.Blend(UiTheme.ACCENT_RED, Color.White, 0.10) : UiTheme.ACCENT_RED;
                text_colour = Color.White;
            }
            else if (_is_hovered)
            {
                fill = UiTheme.MENU_HOVER;
                text_colour = UiTheme.TEXT;
            }
            else
            {
                fill = UiTheme.PANEL;
                text_colour = UiTheme.TEXT_MUTED;
            }

            using (GraphicsPath path = GlowPainter.RoundedPath(body, body.Height / 2f))
            {
                using (SolidBrush brush = new SolidBrush(fill))
                {
                    e.Graphics.FillPath(brush, path);
                }

                // a thin line around the dark chips, so they still show on the black page
                if (!_is_selected)
                {
                    using (Pen line = new Pen(UiTheme.LINE))
                    {
                        e.Graphics.DrawPath(line, path);
                    }
                }

                // the keyboard focus is shown with a light ring
                if (Focused && ShowFocusCues)
                {
                    using (Pen ring = new Pen(UiTheme.TEXT, 1.5f))
                    {
                        e.Graphics.DrawPath(ring, path);
                    }
                }
            }

            TextRenderer.DrawText(e.Graphics, Text, ChipFont, Rectangle.Round(body), text_colour,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
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
            Invalidate();
        }

        protected override void OnMouseDown(MouseEventArgs e)
        {
            base.OnMouseDown(e);
            Focus();
        }

        // space and enter press the chip, like a normal button
        protected override void OnKeyDown(KeyEventArgs e)
        {
            base.OnKeyDown(e);

            if (e.KeyCode == Keys.Space || e.KeyCode == Keys.Enter)
            {
                OnClick(EventArgs.Empty);
                e.Handled = true;
            }
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
