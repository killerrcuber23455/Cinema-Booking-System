// one button of the side menu: an icon, the name, a red bar on the left when it is the open page, and a hover colour (owner: divyesh)
// the name is only drawn when the menu is wide enough, so a collapsed menu shows only the icons
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class MenuButton : Button
    {
        private const int BAR_WIDTH = 4;
        private const int BAR_MARGIN = 8;
        private const int ICON_BOX_LEFT = 14;
        private const int ICON_BOX_WIDTH = 36;
        private const int TEXT_LEFT = 58;

        // below this width the name does not fit, so only the icon is drawn
        private const int SHOW_TEXT_FROM_WIDTH = 120;

        private bool _is_active;
        private bool _is_hovered;

        // the icon, one character of the icon font
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public string Glyph { get; set; } = "";

        // true for the button of the page that is open now
        [DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
        public bool IsActive
        {
            get { return _is_active; }
            set
            {
                _is_active = value;
                Invalidate();
            }
        }

        public MenuButton()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            FlatStyle = FlatStyle.Flat;
            FlatAppearance.BorderSize = 0;
            Cursor = Cursors.Hand;
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

        protected override void OnPaint(PaintEventArgs e)
        {
            Color back_colour = UiTheme.SIDE_MENU;
            if (_is_active)
            {
                back_colour = UiTheme.MENU_ACTIVE;
            }
            else if (_is_hovered)
            {
                back_colour = UiTheme.MENU_HOVER;
            }

            e.Graphics.Clear(back_colour);

            // the red bar on the left shows which page is open
            if (_is_active)
            {
                using (SolidBrush bar_brush = new SolidBrush(UiTheme.ACCENT_RED))
                {
                    e.Graphics.FillRectangle(bar_brush, 0, BAR_MARGIN, BAR_WIDTH, Height - (2 * BAR_MARGIN));
                }
            }

            Color icon_colour = _is_active ? UiTheme.ACCENT_RED : (_is_hovered ? UiTheme.TEXT : UiTheme.TEXT_MUTED);

            TextRenderer.DrawText(e.Graphics, Glyph, UiTheme.ICON_FONT,
                new Rectangle(ICON_BOX_LEFT, 0, ICON_BOX_WIDTH, Height), icon_colour,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            if (Width >= SHOW_TEXT_FROM_WIDTH)
            {
                TextRenderer.DrawText(e.Graphics, Text, UiTheme.MENU_FONT,
                    new Rectangle(TEXT_LEFT, 0, Width - TEXT_LEFT, Height), UiTheme.TEXT,
                    TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            }
        }
    }
}