// the colours, fonts and sizes every window uses, so nobody types colours in a form (owner: divyesh)
// UI upgrade: one design system for the whole app, Inter for all text, deeper blacks, one red accent
using System.Drawing;

namespace CinemaBooking.UI
{
    public static class UiTheme
    {
        // colours: deep black for the screen, three raised greys for depth, cinema red as the one accent
        public static readonly Color BACKGROUND = Color.FromArgb(12, 12, 15);
        public static readonly Color PANEL = Color.FromArgb(22, 22, 27);
        public static readonly Color CARD = Color.FromArgb(30, 30, 37);
        public static readonly Color INPUT = Color.FromArgb(34, 34, 42);

        // thin lines that split parts of a page, instead of a box around every part
        public static readonly Color LINE = Color.FromArgb(44, 44, 54);

        public static readonly Color SIDE_MENU = Color.FromArgb(16, 16, 20);
        public static readonly Color ACCENT_RED = Color.FromArgb(229, 9, 20);
        public static readonly Color ACCENT_RED_DARK = Color.FromArgb(150, 6, 13);
        public static readonly Color MENU_HOVER = Color.FromArgb(32, 32, 40);
        public static readonly Color MENU_ACTIVE = Color.FromArgb(40, 20, 24);
        public static readonly Color GOOD = Color.FromArgb(46, 160, 90);

        // the second accent: the warm light of marquee bulbs, only for small details
        public static readonly Color AMBER = Color.FromArgb(255, 184, 77);

        // the cool white light of the cinema screen on the seat map
        public static readonly Color SCREEN_LIGHT = Color.FromArgb(226, 234, 255);

        // the side menu fades very little now, from near black to black
        public static readonly Color SIDE_MENU_TOP = Color.FromArgb(20, 20, 25);
        public static readonly Color SIDE_MENU_BOTTOM = Color.FromArgb(12, 12, 15);

        // three levels of text: main, muted for details, faint for hints
        public static readonly Color TEXT = Color.FromArgb(244, 244, 246);
        public static readonly Color TEXT_MUTED = Color.FromArgb(154, 154, 166);
        public static readonly Color TEXT_FAINT = Color.FromArgb(102, 102, 114);

        // the type scale (points), every font of the app is one of these sizes
        public const float SIZE_SMALL = 9F;
        public const float SIZE_BODY = 10.5F;
        public const float SIZE_CARD_TITLE = 11F;
        public const float SIZE_SECTION = 14F;
        public const float SIZE_TITLE = 20F;
        public const float SIZE_PAGE_TITLE = 24F;
        public const float SIZE_HERO = 34F;

        // fonts: Inter for everything, Inter Display for big titles
        public static readonly Font TITLE_FONT = AppFonts.Display(SIZE_TITLE);
        public static readonly Font PAGE_TITLE_FONT = AppFonts.Display(SIZE_PAGE_TITLE);
        public static readonly Font HERO_FONT = AppFonts.Display(SIZE_HERO);
        public static readonly Font SECTION_FONT = AppFonts.SemiBold(SIZE_SECTION);
        public static readonly Font MENU_FONT = AppFonts.Medium(SIZE_BODY);
        public static readonly Font BODY_FONT = AppFonts.Regular(SIZE_BODY);
        public static readonly Font CARD_TITLE_FONT = AppFonts.SemiBold(SIZE_CARD_TITLE);
        public static readonly Font BUTTON_FONT = AppFonts.SemiBold(SIZE_BODY);
        public static readonly Font LABEL_FONT = AppFonts.Medium(SIZE_SMALL);
        public static readonly Font SMALL_FONT = AppFonts.Regular(SIZE_SMALL);

        // the icon font that comes with Windows 10 and 11, each icon is one character
        public static readonly Font ICON_FONT = new Font("Segoe MDL2 Assets", 14F, FontStyle.Regular);

        // spaces: every gap and padding is one of these, so the pages line up
        public const int SPACE_XS = 4;
        public const int SPACE_S = 8;
        public const int SPACE_M = 12;
        public const int SPACE_L = 16;
        public const int SPACE_XL = 24;
        public const int SPACE_XXL = 32;
        public const int SPACE_XXXL = 48;

        // the space around a page and the height of its title, at the small step
        public const int PAGE_SIDE_PADDING = 40;
        public const int PAGE_TOP_PADDING = 28;
        public const int PAGE_TITLE_HEIGHT = 64;

        // sizes
        public const int PANEL_RADIUS = 16;
        public const int SIDE_MENU_WIDTH = 220;
        public const int MENU_BUTTON_HEIGHT = 48;
        public const int SIDE_MENU_COLLAPSED_WIDTH = 64;

        // glows: how far they spread around a shape, and how round buttons are
        public const int GLOW_SPREAD = 4;
        public const int BUTTON_RADIUS = 8;

        // the icons of the side menu, change a character here to change an icon everywhere
        public const string ICON_MENU = "";
        public const string ICON_FILMS = "";
        public const string ICON_SHOWINGS = "";
        public const string ICON_BOOK_SEATS = "";
        public const string ICON_MY_BOOKINGS = "";
        public const string ICON_CANCEL = "";
        public const string ICON_MEMBERS = "";
        public const string ICON_ADMIN = "";
        public const string ICON_TIER_RULES = "";
        public const string ICON_SCREENS = "";
        public const string ICON_ACCOUNT = "";
        public const string ICON_EXIT = "";
    }
}
