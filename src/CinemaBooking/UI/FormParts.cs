// the parts every box form shares: a label, a box and a red message under the box, and a flat button (owner: divyesh)
// kept in one place so the login, sign up and first admin pages look the same and the code is not repeated
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public static class FormParts
    {
        public const int PAGE_PADDING = 8;
        public const int FORM_WIDTH = 380;
        public const int LABEL_HEIGHT = 22;
        public const int BOX_HEIGHT = 28;
        public const int ROW_HEIGHT = 74;
        public const int FIRST_ROW_TOP = 56;
        public const int MESSAGE_HEIGHT = 18;
        public const int BUTTON_HEIGHT = 46;

        // puts a label, a box and an empty red message on the page for row number row_index (0 is the first row)
        // returns the message label, so the page can fill it later
        public static Label AddRow(Control page, string label_text, TextBox box, int row_index)
        {
            int row_top = FIRST_ROW_TOP + (row_index * ROW_HEIGHT);

            Label lblName = new Label();
            lblName.Text = label_text;
            lblName.ForeColor = UiTheme.TEXT_MUTED;
            lblName.SetBounds(PAGE_PADDING, row_top, FORM_WIDTH, LABEL_HEIGHT);

            box.BackColor = UiTheme.INPUT;
            box.ForeColor = UiTheme.TEXT;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.SetBounds(PAGE_PADDING, row_top + LABEL_HEIGHT, FORM_WIDTH, BOX_HEIGHT);

            Label lblBoxMessage = new Label();
            lblBoxMessage.ForeColor = UiTheme.ACCENT_RED;
            lblBoxMessage.Font = UiTheme.SMALL_FONT;
            lblBoxMessage.SetBounds(PAGE_PADDING, row_top + LABEL_HEIGHT + BOX_HEIGHT + 2, FORM_WIDTH, 20);

            page.Controls.Add(lblName);
            page.Controls.Add(box);
            page.Controls.Add(lblBoxMessage);

            // the box glows while the user types in it
            FocusGlow.AddTo(page, box);
            return lblBoxMessage;
        }

        // the same row but with a drop-down list (the role list), the list cannot be typed in
        public static Label AddListRow(Control page, string label_text, ComboBox list, int row_index)
        {
            int row_top = FIRST_ROW_TOP + (row_index * ROW_HEIGHT);

            Label lblName = new Label();
            lblName.Text = label_text;
            lblName.ForeColor = UiTheme.TEXT_MUTED;
            lblName.SetBounds(PAGE_PADDING, row_top, FORM_WIDTH, LABEL_HEIGHT);

            list.DropDownStyle = ComboBoxStyle.DropDownList;
            list.FlatStyle = FlatStyle.Flat;
            list.BackColor = UiTheme.INPUT;
            list.ForeColor = UiTheme.TEXT;
            list.SetBounds(PAGE_PADDING, row_top + LABEL_HEIGHT, FORM_WIDTH, BOX_HEIGHT);

            Label lblBoxMessage = new Label();
            lblBoxMessage.ForeColor = UiTheme.ACCENT_RED;
            lblBoxMessage.Font = UiTheme.SMALL_FONT;
            lblBoxMessage.SetBounds(PAGE_PADDING, row_top + LABEL_HEIGHT + BOX_HEIGHT + 2, FORM_WIDTH, 20);

            page.Controls.Add(lblName);
            page.Controls.Add(list);
            page.Controls.Add(lblBoxMessage);
            return lblBoxMessage;
        }

        // the top of the space under the last row, where the buttons go
        public static int TopUnderRows(int row_count)
        {
            return FIRST_ROW_TOP + (row_count * ROW_HEIGHT) + 4;
        }

        // the height a whole form needs: its rows, the full width button and the room under it for the message and the links
        // STUB for the red step: not worked out yet, so the tests fail first
        public static int HeightForRows(int row_count)
        {
            return 0;
        }

        // a rounded button, the red ones glow when the mouse is on them. it keeps a few pixels around it for the glow
        public static Button MakeButton(string text, Color colour)
        {
            GlowButton button = new GlowButton();
            button.Text = text;
            button.BackColor = colour;
            return button;
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
    }
}
