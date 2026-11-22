// a small message that slides in at the bottom right of the window and goes away by itself (owner: divyesh)
// the pages use it for short good news like "Signed in", so a message does not need its own label on the page
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class ToastBanner : Control
    {
        private const int TOAST_WIDTH = 320;
        private const int TOAST_HEIGHT = 52;
        private const int WINDOW_MARGIN = 24;
        private const int BAR_WIDTH = 5;
        private const int SLIDE_STEP = 30;
        private const int SLIDE_INTERVAL_MS = 15;
        private const int SHOW_TIME_MS = 3000;

        private readonly Control _host;
        private readonly System.Windows.Forms.Timer _slide_timer = new System.Windows.Forms.Timer();
        private readonly System.Windows.Forms.Timer _hide_timer = new System.Windows.Forms.Timer();

        private string _text = "";
        private bool _is_error;
        private int _target_left;

        public ToastBanner(Control host)
        {
            _host = host;

            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer, true);

            Size = new Size(TOAST_WIDTH, TOAST_HEIGHT);
            Anchor = AnchorStyles.Bottom | AnchorStyles.Right;
            Visible = false;

            _slide_timer.Interval = SLIDE_INTERVAL_MS;
            _slide_timer.Tick += (sender, e) => SlideIn();

            _hide_timer.Interval = SHOW_TIME_MS;
            _hide_timer.Tick += (sender, e) => HideToast();

            host.Controls.Add(this);
        }

        // shows the message, an error one has a red bar and a good one a green bar
        public void ShowMessage(string text, bool is_error)
        {
            _text = text;
            _is_error = is_error;

            _target_left = _host.ClientSize.Width - TOAST_WIDTH - WINDOW_MARGIN;
            Top = _host.ClientSize.Height - TOAST_HEIGHT - WINDOW_MARGIN;
            Left = _host.ClientSize.Width;

            Visible = true;
            BringToFront();
            Invalidate();

            _hide_timer.Stop();
            _hide_timer.Start();
            _slide_timer.Start();
        }

        // moves the toast a few pixels to the left each tick until it is in place
        private void SlideIn()
        {
            Left = Left - SLIDE_STEP;

            if (Left <= _target_left)
            {
                Left = _target_left;
                _slide_timer.Stop();
            }
        }

        private void HideToast()
        {
            _hide_timer.Stop();
            _slide_timer.Stop();
            Visible = false;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            e.Graphics.Clear(UiTheme.CARD);

            Color bar_colour = _is_error ? UiTheme.ACCENT_RED : UiTheme.GOOD;
            using (SolidBrush bar_brush = new SolidBrush(bar_colour))
            {
                e.Graphics.FillRectangle(bar_brush, 0, 0, BAR_WIDTH, Height);
            }

            TextRenderer.DrawText(e.Graphics, _text, UiTheme.BODY_FONT,
                new Rectangle(BAR_WIDTH + 14, 0, Width - BAR_WIDTH - 24, Height), UiTheme.TEXT,
                TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.EndEllipsis | TextFormatFlags.NoPadding);
        }
    }
}
