// the settings page: what the user box at the bottom of the side menu opens (owner: divyesh)
// it draws the parts that SettingsRules gives for the signed in role, one card for each part, from top to bottom.
// it holds no rules: the words and the parts come from SettingsRules, the new account is made by StaffAccountsForm
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class SettingsPage : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int CARD_WIDTH = 480;
        private const int CARD_INNER = 16;
        private const int TITLE_HEIGHT = 30;
        private const int LINE_HEIGHT = 28;
        private const int CARD_GAP = 16;
        private const int BUTTON_WIDTH = 180;
        private const int BUTTON_HEIGHT = 40;

        // the add account form holds four boxes, a button and a message
        private static readonly int ADD_FORM_HEIGHT = FormParts.HeightForRows(StaffInput.BOX_COUNT);

        private readonly Role _role;
        private readonly string _email;
        private readonly Member? _member;
        private readonly AuthService _auth_service;
        private readonly Action<string> _show_good_message;
        private readonly Action _open_my_bookings;
        private readonly Action _sign_out;

        // the top of the next card
        private int _next_top = PAGE_PADDING;

        public SettingsPage(Role role, string email, Member? member, AuthService auth_service,
            Action<string> show_good_message, Action open_my_bookings, Action sign_out)
        {
            _role = role;
            _email = email;
            _member = member;
            _auth_service = auth_service;
            _show_good_message = show_good_message;
            _open_my_bookings = open_my_bookings;
            _sign_out = sign_out;

            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            foreach (SettingsSection section in SettingsRules.SectionsFor(_role))
            {
                switch (section)
                {
                    case SettingsSection.Account:
                        AddAccountCard();
                        break;
                    case SettingsSection.Password:
                        AddPasswordCard();
                        break;
                    case SettingsSection.Bookings:
                        AddBookingsCard();
                        break;
                    case SettingsSection.AddAccount:
                        AddNewAccountCard();
                        break;
                    default:
                        AddSignOutCard();
                        break;
                }
            }
        }

        // who is signed in, a member also has a name, a tier and points
        private void AddAccountCard()
        {
            string[] lines = DescribeLines();
            RoundedPanel card = MakeCard(SettingsSection.Account, TITLE_HEIGHT + (lines.Length * LINE_HEIGHT));

            for (int line_index = 0; line_index < lines.Length; line_index++)
            {
                AddText(card, lines[line_index], UiTheme.TEXT, TITLE_HEIGHT + (line_index * LINE_HEIGHT));
            }
        }

        // the password change waits for the database code of the team (cards 446 and 447)
        private void AddPasswordCard()
        {
            RoundedPanel card = MakeCard(SettingsSection.Password, TITLE_HEIGHT + LINE_HEIGHT);
            AddText(card, "Changing your password is coming soon.", UiTheme.TEXT_MUTED, TITLE_HEIGHT);
        }

        private void AddBookingsCard()
        {
            RoundedPanel card = MakeCard(SettingsSection.Bookings, TITLE_HEIGHT + LINE_HEIGHT + BUTTON_HEIGHT + 8);
            AddText(card, "See your tickets or cancel a booking.", UiTheme.TEXT_MUTED, TITLE_HEIGHT);
            AddButton(card, "Open my bookings", UiTheme.INPUT, TITLE_HEIGHT + LINE_HEIGHT + 8, _open_my_bookings);
        }

        // only an admin gets this card, the form is the one that makes the staff or admin account
        private void AddNewAccountCard()
        {
            RoundedPanel card = MakeCard(SettingsSection.AddAccount, ADD_FORM_HEIGHT, false);

            StaffAccountsForm form = new StaffAccountsForm(_auth_service, _role, _email, _show_good_message);
            form.BackColor = UiTheme.CARD;
            form.SetBounds(CARD_INNER, CARD_INNER, CARD_WIDTH - (2 * CARD_INNER), ADD_FORM_HEIGHT);
            card.Controls.Add(form);
        }

        private void AddSignOutCard()
        {
            RoundedPanel card = MakeCard(SettingsSection.SignOut, TITLE_HEIGHT + BUTTON_HEIGHT);
            AddButton(card, "Sign out", UiTheme.ACCENT_RED, TITLE_HEIGHT, _sign_out);
        }

        // a rounded card with the title of the part, content_height is the room under the title for the content.
        // the card of the add account form has its own heading, so it has no title (has_title false)
        private RoundedPanel MakeCard(SettingsSection section, int content_height, bool has_title = true)
        {
            RoundedPanel card = new RoundedPanel();
            card.BackColor = UiTheme.CARD;
            int height = content_height + (2 * CARD_INNER);
            card.SetBounds(PAGE_PADDING, _next_top, CARD_WIDTH, height);
            Controls.Add(card);
            _next_top += height + CARD_GAP;

            if (has_title)
            {
                Label lblTitle = new Label();
                lblTitle.Text = SettingsRules.TitleFor(section);
                lblTitle.Font = UiTheme.CARD_TITLE_FONT;
                lblTitle.ForeColor = UiTheme.TEXT;
                lblTitle.BackColor = UiTheme.CARD;
                lblTitle.SetBounds(CARD_INNER, CARD_INNER, CARD_WIDTH - (2 * CARD_INNER), TITLE_HEIGHT - 4);
                card.Controls.Add(lblTitle);
            }

            return card;
        }

        // one line of text inside a card, top is measured from the top of the card content (under the card padding)
        private static void AddText(RoundedPanel card, string text, Color colour, int top)
        {
            Label lblLine = new Label();
            lblLine.Text = text;
            lblLine.ForeColor = colour;
            lblLine.BackColor = UiTheme.CARD;
            lblLine.AutoEllipsis = true;
            lblLine.SetBounds(CARD_INNER, CARD_INNER + top, CARD_WIDTH - (2 * CARD_INNER), LINE_HEIGHT - 4);
            card.Controls.Add(lblLine);
        }

        // a rounded button inside a card, the button keeps a few pixels around it for its glow
        private static void AddButton(RoundedPanel card, string text, Color colour, int top, Action click)
        {
            Button button = FormParts.MakeButton(text, colour);
            int space = UiTheme.GLOW_SPREAD;
            button.SetBounds(CARD_INNER - space, CARD_INNER + top - space, BUTTON_WIDTH + (2 * space), BUTTON_HEIGHT + (2 * space));
            button.Click += (sender, e) => click();
            card.Controls.Add(button);
        }

        // the lines of the account card
        private string[] DescribeLines()
        {
            if (_member == null)
            {
                return new string[] { "Signed in as " + _email, "Role: " + _role };
            }

            return new string[]
            {
                "Signed in as " + _email,
                "Role: " + _role,
                "Name: " + _member.Name,
                "Tier: " + _member.Tier,
                "Points: " + _member.Points
            };
        }
    }
}
