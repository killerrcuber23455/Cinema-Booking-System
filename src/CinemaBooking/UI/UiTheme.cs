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
        public static readonly Color MENU_ACTIVE = Color.FromArgb(48, 28, 32);
        public static readonly Color GOOD = Color.FromArgb(46, 160, 90);

        // the second accent: the warm light of marquee bulbs, only for small details
        public static readonly Color AMBER = Color.FromArgb(255, 184, 77);

        // the cool white light of the cinema screen on the seat map
        public static readonly Color SCREEN_LIGHT = Color.FromArgb(226, 234, 255);

        // the side menu fades from a little lighter at the top to darker at the bottom
        public static readonly Color SIDE_MENU_TOP = Color.FromArgb(32, 30, 38);
        public static readonly Color SIDE_MENU_BOTTOM = Color.FromArgb(17, 17, 21);
        public static readonly Color TEXT = Color.FromArgb(240, 240, 240);
        public static readonly Color TEXT_MUTED = Color.FromArgb(160, 160, 170);
        public static readonly Color CARD = Color.FromArgb(40, 40, 48);
        public static readonly Color INPUT = Color.FromArgb(44, 44, 52);

        // fonts
        public static readonly Font TITLE_FONT = new Font("Segoe UI", 20F, FontStyle.Bold);
        public static readonly Font MENU_FONT = new Font("Segoe UI", 11F, FontStyle.Regular);
        public static readonly Font BODY_FONT = new Font("Segoe UI", 10.5F, FontStyle.Regular);
        public static readonly Font CARD_TITLE_FONT = new Font("Segoe UI", 11F, FontStyle.Bold);
        public static readonly Font SMALL_FONT = new Font("Segoe UI", 9F, FontStyle.Regular);

        // the icon font that comes with Windows 10 and 11, each icon is one character
        public static readonly Font ICON_FONT = new Font("Segoe MDL2 Assets", 14F, FontStyle.Regular);

        // sizes
        public const int PANEL_RADIUS = 16;
        public const int SIDE_MENU_WIDTH = 220;
        public const int MENU_BUTTON_HEIGHT = 48;
        public const int SIDE_MENU_COLLAPSED_WIDTH = 64;

        // glows: how far they spread around a shape, and how round buttons are
        public const int GLOW_SPREAD = 4;
        public const int BUTTON_RADIUS = 8;

        // the icons of the side menu, change a character here to change an icon everywhere
        public const string ICON_MENU = "\uE700";
        public const string ICON_FILMS = "\uE714";
        public const string ICON_SHOWINGS = "\uE787";
        public const string ICON_BOOK_SEATS = "\uE719";
        public const string ICON_MY_BOOKINGS = "\uE8A5";
        public const string ICON_CANCEL = "\uE711";
        public const string ICON_MEMBERS = "\uE716";
        public const string ICON_ADMIN = "\uE713";
        public const string ICON_TIER_RULES = "\uE734";
        public const string ICON_ACCOUNT = "\uE77B";
        public const string ICON_EXIT = "\uE7E8";
    }
}
