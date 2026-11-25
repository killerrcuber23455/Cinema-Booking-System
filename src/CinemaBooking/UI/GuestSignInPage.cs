// the page a guest gets from "Sign in to earn points" on the booking summary: two tabs, Sign in and Create account (owner: divyesh)
// it is a page inside the main window, so the landing window never opens and the booking is kept by the main window.
// it only shows the forms and passes on what they say, the rules are in GuestSignInRules and AuthService
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class GuestSignInPage : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int CARD_WIDTH = 460;
        private const int CARD_TOP = 56;
        private const int CARD_SIDE = 32;
        private const int CARD_TOP_SPACE = 24;
        private const int CARD_BOTTOM_SPACE = 16;
        private const int TABS_GAP = 12;
        private const int FORM_WIDTH = 396;

        private readonly AuthService _auth_service;

        // called when the sign in worked: the role, the member (null for staff and admin) and the email
        private readonly Action<Role, Member?, string> _signed_in;

        // called when the user wants the booking summary back without signing in
        private readonly Action _go_back;

        private readonly RoundedPanel _card = new RoundedPanel();
        private readonly AuthTabs _tabs = new AuthTabs();
        private Control? _form_view;

        public GuestSignInPage(AuthService auth_service, Action<Role, Member?, string> signed_in, Action go_back)
        {
            _auth_service = auth_service;
            _signed_in = signed_in;
            _go_back = go_back;

            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Button btnBack = FormParts.MakeButton("< Back to booking", UiTheme.CARD);
            btnBack.SetBounds(PAGE_PADDING - UiTheme.GLOW_SPREAD, PAGE_PADDING - UiTheme.GLOW_SPREAD, 180, 42);
            btnBack.Click += (sender, e) => _go_back();
            Controls.Add(btnBack);

            _card.BackColor = UiTheme.CARD;
            Controls.Add(_card);

            _tabs.SetBounds(CARD_SIDE, CARD_TOP_SPACE, FORM_WIDTH, _tabs.Height);
            _tabs.TabChosen += tab => ShowTab(tab, "");
            _card.Controls.Add(_tabs);

            ShowTab(AuthTab.SignIn, "");
        }

        // shows the form of a tab inside the card, the card is as tall as that form needs. the notice is a good message above the boxes
        private void ShowTab(AuthTab tab, string notice)
        {
            if (_form_view != null)
            {
                _card.Controls.Remove(_form_view);
                _form_view.Dispose();
                _form_view = null;
            }

            _tabs.Choose(tab);

            int row_count = tab == AuthTab.SignIn ? LoginInput.BOX_COUNT : SignUpInput.BOX_COUNT;
            int form_height = FormParts.HeightForRows(row_count);

            Control form;

            if (tab == AuthTab.SignIn)
            {
                form = new LoginForm(_auth_service, _signed_in, () => ShowTab(AuthTab.CreateAccount, ""), _go_back, notice);
            }
            else
            {
                form = new SignUpForm(
                    _auth_service,
                    email => ShowTab(AuthTab.SignIn, AuthText.AccountCreatedNotice(email)),
                    () => ShowTab(AuthTab.SignIn, ""));
            }

            int form_top = CARD_TOP_SPACE + _tabs.Height + TABS_GAP;
            form.BackColor = UiTheme.CARD;
            form.SetBounds(CARD_SIDE, form_top, FORM_WIDTH, form_height);
            _card.Controls.Add(form);
            _form_view = form;

            _card.SetBounds(PAGE_PADDING, CARD_TOP, CARD_WIDTH, form_top + form_height + CARD_BOTTOM_SPACE);
            form.Focus();
        }
    }
}
