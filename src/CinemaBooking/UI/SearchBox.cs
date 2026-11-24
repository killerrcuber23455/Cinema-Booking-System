// the search box of the films page: a rounded dark box with a search icon, the box glows red while the user types (owner: divyesh)
// the typing itself is done by a normal text box without a border inside it
using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class SearchBox : UserControl
    {
        private const int BASE_HEIGHT = 46;
        private const int BASE_ICON_LEFT = 16;
        private const int BASE_TEXT_LEFT = 46;
        private const string ICON_SEARCH = "";

        private readonly TextBox txtSearch = new TextBox();
        private float _step = UiScale.SMALL_STEP;

        // the page listens to this to update the films while the user types
        public event EventHandler? SearchChanged;

        public string SearchText
        {
            get { return txtSearch.Text; }
        }

        public SearchBox(string placeholder)
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            BackColor = UiTheme.BACKGROUND;
            Cursor = Cursors.IBeam;
            Height = BASE_HEIGHT;

            txtSearch.BorderStyle = BorderStyle.None;
            txtSearch.BackColor = UiTheme.PANEL;
            txtSearch.ForeColor = UiTheme.TEXT;
            txtSearch.PlaceholderText = placeholder;
            txtSearch.Font = UiTheme.BODY_FONT;
            txtSearch.TextChanged += (sender, e) => SearchChanged?.Invoke(this, EventArgs.Empty);
            txtSearch.GotFocus += (sender, e) => Invalidate();
            txtSearch.LostFocus += (sender, e) => Invalidate();
            Controls.Add(txtSearch);

            // a click anywhere on the box starts the typing
            Click += (sender, e) => txtSearch.Focus();
        }

        public void SetStep(float step)
        {
            _step = step;
            txtSearch.Font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, step);
            Height = UiScale.Pixels(BASE_HEIGHT, step);
            PerformLayout();
            Invalidate();
        }

        public void SetEnabled(bool is_enabled)
        {
            txtSearch.Enabled = is_enabled;
            Invalidate();
        }

        protected override void OnLayout(LayoutEventArgs e)
        {
            base.OnLayout(e);

            int text_left = UiScale.Pixels(BASE_TEXT_LEFT, _step);
            int right_space = UiScale.Pixels(UiTheme.SPACE_L, _step);
            txtSearch.SetBounds(text_left, (Height - txtSearch.Height) / 2,
                Math.Max(10, Width - text_left - right_space), txtSearch.Height);
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.BACKGROUND;
            e.Graphics.Clear(behind);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            RectangleF body = new RectangleF(0.5f, 0.5f, Width - 1.5f, Height - 1.5f);
            bool is_typing = txtSearch.Focused;

            using (GraphicsPath path = GlowPainter.RoundedPath(body, body.Height / 2f))
            {
                using (SolidBrush brush = new SolidBrush(UiTheme.PANEL))
                {
                    e.Graphics.FillPath(brush, path);
                }

                Color edge = is_typing ? ColorMath.Blend(UiTheme.LINE, UiTheme.ACCENT_RED, 0.75) : UiTheme.LINE;
                using (Pen edge_pen = new Pen(edge, is_typing ? 1.5f : 1f))
                {
                    e.Graphics.DrawPath(edge_pen, path);
                }
            }

            int icon_left = UiScale.Pixels(BASE_ICON_LEFT, _step);
            Rectangle icon_area = new Rectangle(icon_left, 0, UiScale.Pixels(20, _step), Height);
            TextRenderer.DrawText(e.Graphics, ICON_SEARCH, UiTheme.ICON_FONT, icon_area,
                is_typing ? UiTheme.TEXT : UiTheme.TEXT_MUTED,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);
        }
    }
}
