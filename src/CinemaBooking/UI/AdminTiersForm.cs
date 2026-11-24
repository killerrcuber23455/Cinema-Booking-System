// the tier rules window: shows the loyalty tiers, the points rate and how a tier is reached (owner: divyesh)
// it is a page shown inside the main window. it is read-only on purpose (decision of 25 Nov: the tiers are fixed),
// it holds no numbers of its own: TierRuleList reads them from LoyaltyService, the same code the bookings use
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class AdminTiersForm : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int CARD_WIDTH = 640;
        private const int ROW_HEIGHT = 40;
        private const int CARD_INSET = 20;

        // the same colour as the tier name on a card or ticket: grey, silver, amber
        private static readonly Color SILVER_COLOUR = Color.FromArgb(200, 205, 215);

        public AdminTiersForm()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            TierRuleRow[] rows = TierRuleList.Rows();

            Label lblNote = new Label();
            lblNote.Text = "These rules are fixed and cannot be changed here. Every booking uses them.";
            lblNote.ForeColor = UiTheme.TEXT_MUTED;
            lblNote.BackColor = UiTheme.PANEL;
            lblNote.AutoEllipsis = true;
            lblNote.SetBounds(PAGE_PADDING, PAGE_PADDING, CARD_WIDTH, 24);
            Controls.Add(lblNote);

            int card_top = PAGE_PADDING + 36;
            int card_height = (rows.Length * ROW_HEIGHT) + 24;

            RoundedPanel pnlTiers = new RoundedPanel();
            pnlTiers.BackColor = UiTheme.CARD;
            pnlTiers.SetBounds(PAGE_PADDING, card_top, CARD_WIDTH, card_height);
            Controls.Add(pnlTiers);

            for (int row_index = 0; row_index < rows.Length; row_index++)
            {
                Label lblRow = new Label();
                lblRow.Text = TierRuleLine.Text(rows[row_index]);
                lblRow.Font = UiTheme.CARD_TITLE_FONT;
                lblRow.ForeColor = ColourFor(rows[row_index].Tier);
                lblRow.BackColor = UiTheme.CARD;
                lblRow.AutoEllipsis = true;
                lblRow.TextAlign = ContentAlignment.MiddleLeft;
                lblRow.SetBounds(PAGE_PADDING + CARD_INSET, card_top + 12 + (row_index * ROW_HEIGHT), CARD_WIDTH - (2 * CARD_INSET), ROW_HEIGHT);
                Controls.Add(lblRow);
                lblRow.BringToFront();
            }

            int info_top = card_top + card_height + 16;

            AddInfoLine(TierRuleList.PointsRateText(), info_top);
            AddInfoLine("A tier is reached from lifetime points: all the points a member has ever earned.", info_top + 28);
            AddInfoLine("Cancelling a booking takes back the points it earned and gives back the points used.", info_top + 56);
        }

        // one grey line of explanation under the tiers
        private void AddInfoLine(string text, int top)
        {
            Label lblInfo = new Label();
            lblInfo.Text = text;
            lblInfo.ForeColor = UiTheme.TEXT_MUTED;
            lblInfo.BackColor = UiTheme.PANEL;
            lblInfo.AutoEllipsis = true;
            lblInfo.SetBounds(PAGE_PADDING, top, CARD_WIDTH, 24);
            Controls.Add(lblInfo);
        }

        // the colour of a tier name
        private static Color ColourFor(Tier tier)
        {
            if (tier == Tier.Gold)
            {
                return UiTheme.AMBER;
            }

            if (tier == Tier.Silver)
            {
                return SILVER_COLOUR;
            }

            return UiTheme.TEXT;
        }
    }
}
