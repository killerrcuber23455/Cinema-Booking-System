// the small user box at the bottom of the side menu: a round badge with the first letter, the name and the role (owner: divyesh)
// it only draws what it is told, the main window gives it the name and the role
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class UserChip : Control
    {
        private const int CHIP_HEIGHT = 64;
        private const int BADGE_LEFT = 14;
        private const int BADGE_SIZE = 36;
        private const int TEXT_LEFT = 58;

        // below this width the text does not fit, so only the badge is drawn
        private const int SHOW_TEXT_FROM_WIDTH = 120;

        private string _title = "Guest";
        private string _subtitle = "Not signed in";

        public UserChip()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Height = CHIP_HEIGHT;
            BackColor = UiTheme.SIDE_MENU_BOTTOM;
        }

        // the title is the name or email, the subtitle is the role
        public void SetUser(string title, string subtitle)
        {
            _title = title;
            _subtitle = subtitle;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            // the chip shows the menu fade behind it
            GlowPainter.FillMenuBackground(e.Graphics, this);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

            int badge_top = (Height - BADGE_SIZE) / 2;
            Rectangle badge = new Rectangle(BADGE_LEFT, badge_top, BADGE_SIZE, BADGE_SIZE);

            using (SolidBrush badge_brush = new SolidBrush(UiTheme.ACCENT_RED_DARK))
            {
                e.Graphics.FillEllipse(badge_brush, badge);
            }

            string initial = _title.Length > 0 ? _title.Substring(0, 1).ToUpper() : "?";
            TextRenderer.DrawText(e.Graphics, initial, UiTheme.CARD_TITLE_FONT, badge, UiTheme.TEXT,
                TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter | TextFormatFlags.NoPadding);

            if (Width >= SHOW_TEXT_FROM_WIDTH)
            {
                int text_width = Width - TEXT_LEFT - 8;
                TextRenderer.DrawText(e.Graphics, _title, UiTheme.BODY_FONT,
                    new Rectangle(TEXT_LEFT, badge_top - 2, text_width, 20), UiTheme.TEXT,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
                TextRenderer.DrawText(e.Graphics, _subtitle, UiTheme.SMALL_FONT,
                    new Rectangle(TEXT_LEFT, badge_top + 18, text_width, 18), UiTheme.TEXT_MUTED,
                    TextFormatFlags.Left | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
            }
        }
    }
}
