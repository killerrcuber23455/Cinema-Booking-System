// makes a drop-down menu dark like the rest of the program (owner: divyesh)
// the user box at the bottom of the side menu opens one, the default white menu would not fit
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class DarkMenuRenderer : ToolStripProfessionalRenderer
    {
        public DarkMenuRenderer() : base(new DarkColors())
        {
            RoundedEdges = false;
        }

        // the colours the standard renderer asks for
        private class DarkColors : ProfessionalColorTable
        {
            public override Color ToolStripDropDownBackground { get { return UiTheme.CARD; } }
            public override Color ImageMarginGradientBegin { get { return UiTheme.CARD; } }
            public override Color ImageMarginGradientMiddle { get { return UiTheme.CARD; } }
            public override Color ImageMarginGradientEnd { get { return UiTheme.CARD; } }
            public override Color MenuBorder { get { return UiTheme.LINE; } }
            public override Color MenuItemBorder { get { return UiTheme.ACCENT_RED_DARK; } }
            public override Color MenuItemSelected { get { return UiTheme.MENU_HOVER; } }
            public override Color MenuItemSelectedGradientBegin { get { return UiTheme.MENU_HOVER; } }
            public override Color MenuItemSelectedGradientEnd { get { return UiTheme.MENU_HOVER; } }
            public override Color SeparatorDark { get { return UiTheme.LINE; } }
            public override Color SeparatorLight { get { return UiTheme.LINE; } }
        }
    }
}
