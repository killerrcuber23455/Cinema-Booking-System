// the colours, fonts and sizes every window uses, so nobody types colours in a form (owner: divyesh)
using System.Drawing;

namespace CinemaBooking.UI
{
    public static class UiTheme
    {
        // colours (dark theme with a cinema red accent)
        public static readonly Color BACKGROUND = Color.FromArgb(18, 18, 22);
        public static readonly Color PANEL = Color.FromArgb(30, 30, 36);
        public static readonly Color SIDE_MENU = Color.FromArgb(24, 24, 29);
        public static readonly Color ACCENT_RED = Color.FromArgb(229, 9, 20);
        public static readonly Color ACCENT_RED_DARK = Color.FromArgb(150, 6, 13);
        public static readonly Color MENU_HOVER = Color.FromArgb(45, 45, 54);
        public static readonly Color TEXT = Color.FromArgb(240, 240, 240);
        public static readonly Color TEXT_MUTED = Color.FromArgb(160, 160, 170);

        // fonts
        public static readonly Font TITLE_FONT = new Font("Segoe UI", 20F, FontStyle.Bold);
        public static readonly Font MENU_FONT = new Font("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font BODY_FONT = new Font("Segoe UI", 10.5F, FontStyle.Regular);

        // sizes
        public const int PANEL_RADIUS = 16;
        public const int SIDE_MENU_WIDTH = 220;
        public const int MENU_BUTTON_HEIGHT = 48;
    }
}
