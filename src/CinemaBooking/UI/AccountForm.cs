// the account page for someone who is signed in: who they are, their role and a sign out button (owner: divyesh)
// it is a page shown inside the main window. a customer also sees the member details
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class AccountForm : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int CARD_WIDTH = 480;
        private const int LINE_HEIGHT = 30;

        public AccountForm(Role role, string email, Member? member, Action sign_out)
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;

            string[] lines = DescribeLines(role, email, member);

            RoundedPanel pnlCard = new RoundedPanel();
            pnlCard.BackColor = UiTheme.CARD;
            pnlCard.SetBounds(PAGE_PADDING, PAGE_PADDING, CARD_WIDTH, (lines.Length * LINE_HEIGHT) + 24);
            Controls.Add(pnlCard);

            for (int line_index = 0; line_index < lines.Length; line_index++)
            {
                Label lblLine = new Label();
                lblLine.Text = lines[line_index];
                lblLine.AutoEllipsis = true;
                lblLine.BackColor = UiTheme.CARD;
                lblLine.SetBounds(PAGE_PADDING + 16, PAGE_PADDING + 12 + (line_index * LINE_HEIGHT), CARD_WIDTH - 32, LINE_HEIGHT - 4);
                Controls.Add(lblLine);
                lblLine.BringToFront();
            }

            Button btnSignOut = new Button();
            btnSignOut.Text = "Sign out";
            btnSignOut.FlatStyle = FlatStyle.Flat;
            btnSignOut.FlatAppearance.BorderSize = 0;
            btnSignOut.BackColor = UiTheme.ACCENT_RED;
            btnSignOut.ForeColor = UiTheme.TEXT;
            btnSignOut.Cursor = Cursors.Hand;
            btnSignOut.SetBounds(PAGE_PADDING, PAGE_PADDING + (lines.Length * LINE_HEIGHT) + 44, 160, 40);
            btnSignOut.Click += (sender, e) => sign_out();
            Controls.Add(btnSignOut);
        }

        // the lines of the card, a member also has a name, a tier and points
        private static string[] DescribeLines(Role role, string email, Member? member)
        {
            if (member == null)
            {
                return new string[] { "Signed in as " + email, "Role: " + role };
            }

            return new string[]
            {
                "Signed in as " + email,
                "Role: " + role,
                "Name: " + member.Name,
                "Tier: " + member.Tier,
                "Points: " + member.Points
            };
        }
    }
}
