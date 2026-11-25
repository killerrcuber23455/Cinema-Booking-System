// two tabs side by side, "Sign in" and "Create account", the chosen one has a red line under it (owner: divyesh)
// the words and the order come from GuestSignInRules, this only draws them and tells which one was clicked
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class AuthTabs : Control
    {
        private const int TAB_HEIGHT = 44;
        private const int LINE_HEIGHT = 3;

        private readonly AuthTab[] _tabs = GuestSignInRules.Tabs();
        private AuthTab _selected = AuthTab.SignIn;

        // raised when the user clicks a tab that is not the chosen one
        public event Action<AuthTab>? TabChosen;

        public AuthTabs()
        {
            SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
                | ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw, true);

            Height = TAB_HEIGHT;
            Cursor = Cursors.Hand;
            TabStop = false;
        }

        // the tab that is shown as chosen
        public void Choose(AuthTab tab)
        {
            _selected = tab;
            Invalidate();
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Color behind = Parent != null ? Parent.BackColor : UiTheme.CARD;
            e.Graphics.Clear(behind);

            if (_tabs.Length == 0)
            {
                return;
            }

            int tab_width = Width / _tabs.Length;

            using (SolidBrush rule = new SolidBrush(UiTheme.LINE))
            {
                e.Graphics.FillRectangle(rule, 0, Height - 1, Width, 1);
            }

            for (int tab_index = 0; tab_index < _tabs.Length; tab_index++)
            {
                bool is_chosen = _tabs[tab_index] == _selected;
                Rectangle area = new Rectangle(tab_index * tab_width, 0, tab_width, Height - LINE_HEIGHT);

                TextRenderer.DrawText(e.Graphics, GuestSignInRules.TabLabel(_tabs[tab_index]),
                    is_chosen ? UiTheme.BUTTON_FONT : UiTheme.MENU_FONT, area,
                    is_chosen ? UiTheme.TEXT : UiTheme.TEXT_MUTED,
                    TextFormatFlags.HorizontalCenter | TextFormatFlags.VerticalCenter);

                if (is_chosen)
                {
                    using (SolidBrush line = new SolidBrush(UiTheme.ACCENT_RED))
                    {
                        e.Graphics.FillRectangle(line, area.Left, Height - LINE_HEIGHT, area.Width, LINE_HEIGHT);
                    }
                }
            }
        }

        protected override void OnMouseClick(MouseEventArgs e)
        {
            base.OnMouseClick(e);

            if (_tabs.Length == 0 || Width == 0)
            {
                return;
            }

            int tab_index = Math.Min(_tabs.Length - 1, e.X * _tabs.Length / Width);

            if (_tabs[tab_index] != _selected)
            {
                _selected = _tabs[tab_index];
                Invalidate();
                TabChosen?.Invoke(_selected);
            }
        }
    }
}
