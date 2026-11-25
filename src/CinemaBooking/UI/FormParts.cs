// the parts every box form shares: a title, labels, rounded boxes with a red message under them, a full width button
// and small text links (owner: divyesh)
// kept in one place so the sign in, sign up, first admin, add account, guest details and cancel booking forms look the same
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public static class FormParts
    {
        public const int PAGE_PADDING = 8;
        public const int FORM_WIDTH = 380;
        public const int LABEL_HEIGHT = 20;
        public const int BOX_HEIGHT = 42;
        public const int MESSAGE_HEIGHT = 16;
        public const int ROW_HEIGHT = 80;

        // the title and the line under it take the first 80 pixels, the first row starts under them
        public const int FIRST_ROW_TOP = 80;
        public const int BUTTON_HEIGHT = 46;

        private const int HEADING_HEIGHT = 36;
        private const int SUBTITLE_TOP = 38;
        private const int SUBTITLE_HEIGHT = 22;
        private const int LINK_HEIGHT = 22;
        private const int LINK_GAP = 5;

        // the room under the full width button: the red message, a link line and a second link line
        private const int BELOW_BUTTON = 96;

        // the big title and the one line under it. returns the line, a form can use it for a good message
        public static Label AddHeading(Control page, string title, string subtitle)
        {
            Label lblTitle = new Label();
            lblTitle.Text = title;
            lblTitle.Font = UiTheme.TITLE_FONT;
            lblTitle.ForeColor = UiTheme.TEXT;
            lblTitle.SetBounds(PAGE_PADDING, 0, FORM_WIDTH, HEADING_HEIGHT);

            Label lblSubtitle = new Label();
            lblSubtitle.Text = subtitle;
            lblSubtitle.Font = UiTheme.BODY_FONT;
            lblSubtitle.ForeColor = UiTheme.TEXT_MUTED;
            lblSubtitle.AutoEllipsis = true;
            lblSubtitle.SetBounds(PAGE_PADDING, SUBTITLE_TOP, FORM_WIDTH, SUBTITLE_HEIGHT);

            page.Controls.Add(lblTitle);
            page.Controls.Add(lblSubtitle);
            return lblSubtitle;
        }

        // puts a label, a rounded box and an empty red message on the page for row number row_index (0 is the first row)
        // returns the message label, so the page can fill it later. the placeholder is the grey hint inside the empty box
        public static Label AddRow(Control page, string label_text, TextBox box, int row_index, string placeholder = "")
        {
            int row_top = FIRST_ROW_TOP + (row_index * ROW_HEIGHT);

            RoundedField frame = new RoundedField();
            frame.SetBounds(PAGE_PADDING, row_top + LABEL_HEIGHT, FORM_WIDTH, BOX_HEIGHT);
            frame.Host(box);
            box.PlaceholderText = placeholder;

            page.Controls.Add(MakeLabel(label_text, row_top));
            page.Controls.Add(frame);
            return AddMessage(page, row_top);
        }

        // the same row but with a drop-down list (the role list), the list cannot be typed in
        public static Label AddListRow(Control page, string label_text, ComboBox list, int row_index)
        {
            int row_top = FIRST_ROW_TOP + (row_index * ROW_HEIGHT);

            list.DropDownStyle = ComboBoxStyle.DropDownList;
            list.FlatStyle = FlatStyle.Flat;

            RoundedField frame = new RoundedField();
            frame.SetBounds(PAGE_PADDING, row_top + LABEL_HEIGHT, FORM_WIDTH, BOX_HEIGHT);
            frame.Host(list);

            page.Controls.Add(MakeLabel(label_text, row_top));
            page.Controls.Add(frame);
            return AddMessage(page, row_top);
        }

        // a rounded box at any place on a page, for pages that lay their own rows out (the guest details)
        public static void PlaceBox(Control page, TextBox box, int left, int top, int width, string placeholder = "")
        {
            RoundedField frame = new RoundedField();
            frame.SetBounds(left, top, width, BOX_HEIGHT);
            frame.Host(box);
            box.PlaceholderText = placeholder;
            page.Controls.Add(frame);
        }

        // the top of the space under the last row, where the button goes
        public static int TopUnderRows(int row_count)
        {
            return FIRST_ROW_TOP + (row_count * ROW_HEIGHT) + 4;
        }

        // the height a whole form needs: its rows, the full width button and the room under it for the message and the links
        public static int HeightForRows(int row_count)
        {
            return TopUnderRows(row_count) + BUTTON_HEIGHT + BELOW_BUTTON;
        }

        // a rounded button, the red ones glow when the mouse is on them. it keeps a few pixels around it for the glow
        public static Button MakeButton(string text, Color colour)
        {
            GlowButton button = new GlowButton();
            button.Text = text;
            button.Font = UiTheme.BUTTON_FONT;
            button.BackColor = colour;
            return button;
        }

        // puts a button as wide as the boxes, its body starts at top (the glow space is around it)
        public static void PlaceWide(Button button, int top)
        {
            int space = UiTheme.GLOW_SPREAD;
            button.SetBounds(PAGE_PADDING - space, top - space, FORM_WIDTH + (2 * space), BUTTON_HEIGHT + (2 * space));
        }

        // a centred line like "Don't have an account? Sign up": plain words and a link. an empty prefix gives only the link.
        // a quiet link is grey (a way back), the other is red (the main way on)
        public static void AddLinkLine(Control page, string prefix, string link_text, int top, Action click, bool is_quiet = false)
        {
            Font prefix_font = UiTheme.BODY_FONT;
            Font link_font = UiTheme.MENU_FONT;
            Size room = new Size(FORM_WIDTH, LINK_HEIGHT);
            TextFormatFlags flags = TextFormatFlags.NoPadding;

            int prefix_width = prefix == "" ? 0 : TextRenderer.MeasureText(prefix, prefix_font, room, flags).Width + 2;
            int link_width = TextRenderer.MeasureText(link_text, link_font, room, flags).Width + 2;
            int gap = prefix == "" ? 0 : LINK_GAP;
            int left = PAGE_PADDING + ((FORM_WIDTH - (prefix_width + gap + link_width)) / 2);

            if (prefix != "")
            {
                Label lblPrefix = new Label();
                lblPrefix.Text = prefix;
                lblPrefix.Font = prefix_font;
                lblPrefix.ForeColor = UiTheme.TEXT_MUTED;
                lblPrefix.BackColor = Color.Transparent;
                lblPrefix.TextAlign = ContentAlignment.MiddleLeft;
                lblPrefix.SetBounds(left, top, prefix_width, LINK_HEIGHT);
                page.Controls.Add(lblPrefix);
            }

            Color normal = is_quiet ? UiTheme.TEXT_MUTED : UiTheme.ACCENT_RED;
            Color hover = is_quiet ? UiTheme.TEXT : ColorMath.Blend(UiTheme.ACCENT_RED, Color.White, 0.25);

            Label lblLink = new Label();
            lblLink.Text = link_text;
            lblLink.Font = link_font;
            lblLink.ForeColor = normal;
            lblLink.BackColor = Color.Transparent;
            lblLink.TextAlign = ContentAlignment.MiddleLeft;
            lblLink.Cursor = Cursors.Hand;
            lblLink.SetBounds(left + prefix_width + gap, top, link_width, LINK_HEIGHT);
            lblLink.MouseEnter += (sender, e) => lblLink.ForeColor = hover;
            lblLink.MouseLeave += (sender, e) => lblLink.ForeColor = normal;
            lblLink.Click += (sender, e) => click();
            page.Controls.Add(lblLink);
        }

        // shows each message under its box, returns true when at least one box is wrong
        public static bool ShowBoxMessages(Label[] box_labels, string[] messages)
        {
            bool is_any_wrong = false;

            for (int box_index = 0; box_index < messages.Length; box_index++)
            {
                box_labels[box_index].Text = messages[box_index];

                if (messages[box_index] != "")
                {
                    is_any_wrong = true;
                }
            }

            return is_any_wrong;
        }

        private static Label MakeLabel(string label_text, int row_top)
        {
            Label lblName = new Label();
            lblName.Text = label_text;
            lblName.Font = UiTheme.LABEL_FONT;
            lblName.ForeColor = UiTheme.TEXT_MUTED;
            lblName.SetBounds(PAGE_PADDING, row_top, FORM_WIDTH, LABEL_HEIGHT);
            return lblName;
        }

        private static Label AddMessage(Control page, int row_top)
        {
            Label lblBoxMessage = new Label();
            lblBoxMessage.ForeColor = UiTheme.ACCENT_RED;
            lblBoxMessage.Font = UiTheme.SMALL_FONT;
            lblBoxMessage.SetBounds(PAGE_PADDING, row_top + LABEL_HEIGHT + BOX_HEIGHT + 2, FORM_WIDTH, MESSAGE_HEIGHT);
            page.Controls.Add(lblBoxMessage);
            return lblBoxMessage;
        }
    }
}
