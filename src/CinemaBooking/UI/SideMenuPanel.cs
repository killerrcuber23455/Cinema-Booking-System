// the side menu panel: a soft fade from a little lighter at the top to darker at the bottom (owner: divyesh)
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class SideMenuPanel : Panel
    {
        public SideMenuPanel()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            BackColor = UiTheme.SIDE_MENU;
        }

        protected override void OnPaintBackground(PaintEventArgs e)
        {
            if (Width <= 0 || Height <= 0)
            {
                return;
            }

            using (LinearGradientBrush brush = new LinearGradientBrush(ClientRectangle, UiTheme.SIDE_MENU_TOP,
                UiTheme.SIDE_MENU_BOTTOM, LinearGradientMode.Vertical))
            {
                e.Graphics.FillRectangle(brush, ClientRectangle);
            }
        }
    }
}
