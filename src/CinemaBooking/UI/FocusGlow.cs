// a thin red glow around the text box the user is typing in (owner: divyesh)
// a text box cannot draw its own glow, so this sits just behind the box, a few pixels bigger, and is only shown
// while the box has the focus. hidden the rest of the time, so it never covers the label above the box
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class FocusGlow : Control
    {
        private const int SPREAD = 3;

        private readonly TextBox _box;

        private FocusGlow(TextBox box)
        {
            _box = box;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);
            TabStop = false;
            Visible = false;

            _box.Enter += (sender, e) => Visible = true;
            _box.Leave += (sender, e) => Visible = false;
        }

        // puts a glow around the box. call it after the box has its place on the page
        public static void AddTo(Control page, TextBox box)
        {
            FocusGlow glow = new FocusGlow(box);
            glow.SetBounds(box.Left - SPREAD, box.Top - SPREAD, box.Width + (2 * SPREAD), box.Height + (2 * SPREAD));

            page.Controls.Add(glow);

            // the glow above the labels, the box above the glow
            glow.BringToFront();
            box.BringToFront();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.PANEL;
            e.Graphics.Clear(behind);

            RectangleF box_area = new RectangleF(SPREAD, SPREAD, _box.Width, _box.Height);
            GlowPainter.DrawGlow(e.Graphics, box_area, 2, UiTheme.ACCENT_RED, 3, SPREAD);
        }
    }
}
