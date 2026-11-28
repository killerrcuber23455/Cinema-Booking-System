// the admin screens window: the list of screens and a form to add a new screen (card 406, owner: divyesh)
// the page holds no rules and no database, it gets small functions from the main window.
// the boxes are checked by ScreenInput, the list lines come from ScreenLine, the rules are in ScreenService
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class AdminScreensForm : UserControl
    {
        private const int LIST_WIDTH = 760;
        private const int LIST_TOP = 44;
        private const int LIST_HEIGHT = 200;
        private const int ADD_TOP = LIST_TOP + LIST_HEIGHT + 20;
        private const int ADD_BOXES_TOP = ADD_TOP + 30;
        private const int BUTTON_ROW_HEIGHT = 46;

        private readonly Func<(CinemaScreen[] Screens, string Error)> _load;
        private readonly Func<string, int, int, (int NewId, string Error)> _add;
        private readonly Action<string> _show_toast;

        private readonly ListBox lstScreens = new ListBox();

        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtRows = new TextBox();
        private readonly TextBox txtSeats = new TextBox();
        private readonly Button btnAdd = FormParts.MakeButton("Add screen", UiTheme.ACCENT_RED);

        // one red message under each box, in the order of ScreenInput.BOX_NAME and the others
        private readonly Label[] _box_messages = new Label[ScreenInput.BOX_COUNT];

        // problems and counts of the whole page show here
        private readonly Label lblMessage = new Label();

        // true while a call runs on a background thread, a second click is ignored
        private bool _is_busy;

        public AdminScreensForm(Func<(CinemaScreen[] Screens, string Error)> load,
            Func<string, int, int, (int NewId, string Error)> add,
            Action<string> show_toast)
        {
            _load = load;
            _add = add;
            _show_toast = show_toast;

            BuildPage();

            // the list is read when the page opens
            Load += (sender, e) => Reload();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "Screens";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 30);
            Controls.Add(lblHeading);

            lstScreens.BackColor = UiTheme.CARD;
            lstScreens.ForeColor = UiTheme.TEXT;
            lstScreens.BorderStyle = BorderStyle.None;
            lstScreens.SetBounds(FormParts.PAGE_PADDING, LIST_TOP, LIST_WIDTH, LIST_HEIGHT);
            Controls.Add(lstScreens);

            // add a new screen
            Label lblAdd = new Label();
            lblAdd.Text = "Add a new screen";
            lblAdd.Font = UiTheme.CARD_TITLE_FONT;
            lblAdd.SetBounds(FormParts.PAGE_PADDING, ADD_TOP, FormParts.FORM_WIDTH, 26);
            Controls.Add(lblAdd);

            FormParts.PlaceBox(this, txtName, FormParts.PAGE_PADDING, ADD_BOXES_TOP, 300, "Name");
            FormParts.PlaceBox(this, txtRows, FormParts.PAGE_PADDING + 316, ADD_BOXES_TOP, 200, "Rows (1 to 26)");
            FormParts.PlaceBox(this, txtSeats, FormParts.PAGE_PADDING + 532, ADD_BOXES_TOP, 228, "Seats per row (1 to 30)");

            int messages_top = ADD_BOXES_TOP + FormParts.BOX_HEIGHT + 2;
            _box_messages[ScreenInput.BOX_NAME] = MakeBoxMessage(FormParts.PAGE_PADDING, messages_top, 300);
            _box_messages[ScreenInput.BOX_ROWS] = MakeBoxMessage(FormParts.PAGE_PADDING + 316, messages_top, 200);
            _box_messages[ScreenInput.BOX_SEATS] = MakeBoxMessage(FormParts.PAGE_PADDING + 532, messages_top, 228);

            int button_top = messages_top + FormParts.MESSAGE_HEIGHT + 8;
            PlaceButton(btnAdd, FormParts.PAGE_PADDING, button_top, 220);
            btnAdd.Click += (sender, e) => ClickAdd();

            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.Font = UiTheme.SMALL_FONT;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, button_top + BUTTON_ROW_HEIGHT + 10, LIST_WIDTH, 44);
            Controls.Add(lblMessage);
        }

        // a button whose body starts at left and top, the glow space is around it
        private void PlaceButton(Button button, int left, int top, int width)
        {
            int space = UiTheme.GLOW_SPREAD;
            button.SetBounds(left - space, top - space, width + (2 * space), FormParts.BOX_HEIGHT + (2 * space));
            Controls.Add(button);
        }

        private Label MakeBoxMessage(int left, int top, int width)
        {
            Label message = new Label();
            message.ForeColor = UiTheme.ACCENT_RED;
            message.Font = UiTheme.SMALL_FONT;
            message.SetBounds(left, top, width, FormParts.MESSAGE_HEIGHT);
            Controls.Add(message);
            return message;
        }

        // reads all screens again and fills the list
        private async void Reload()
        {
            if (_is_busy)
            {
                return;
            }

            SetBusy(true);
            ShowInfo("Loading screens...");

            CallResult<(CinemaScreen[] Screens, string Error)> result = await BackgroundCall.RunAsync(() => _load());
            SetBusy(false);

            if (!result.IsOk)
            {
                ShowProblem(result.ErrorText);
                return;
            }

            if (result.Value.Error != "")
            {
                lstScreens.Items.Clear();
                ShowProblem(result.Value.Error);
                return;
            }

            FillList(result.Value.Screens);
            ShowInfo(result.Value.Screens.Length == 1 ? "1 screen" : result.Value.Screens.Length + " screens");
        }

        // the boxes are checked first, then the screen is added on a background thread
        private async void ClickAdd()
        {
            if (_is_busy)
            {
                return;
            }

            ShowInfo("");

            string[] box_errors = ScreenInput.CheckBoxes(txtName.Text, txtRows.Text, txtSeats.Text);

            if (FormParts.ShowBoxMessages(_box_messages, box_errors))
            {
                return;
            }

            string name = txtName.Text.Trim();
            int row_count = ScreenInput.ReadNumber(txtRows.Text);
            int seats_per_row = ScreenInput.ReadNumber(txtSeats.Text);

            SetBusy(true);
            CallResult<(int NewId, string Error)> result = await BackgroundCall.RunAsync(() => _add(name, row_count, seats_per_row));
            SetBusy(false);

            if (!result.IsOk)
            {
                ShowProblem(result.ErrorText);
                return;
            }

            if (result.Value.NewId < 1)
            {
                ShowProblem(result.Value.Error);
                return;
            }

            txtName.Text = "";
            txtRows.Text = "";
            txtSeats.Text = "";
            _show_toast("Screen added: " + name);
            Reload();
        }

        private void FillList(CinemaScreen[] screens)
        {
            lstScreens.Items.Clear();

            for (int screen_index = 0; screen_index < screens.Length; screen_index++)
            {
                lstScreens.Items.Add(ScreenLine.Text(screens[screen_index]));
            }
        }

        private void ShowInfo(string text)
        {
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.Text = text;
        }

        private void ShowProblem(string text)
        {
            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.Text = text.TrimEnd('\n');
        }

        // while a call runs the button cannot be clicked again
        private void SetBusy(bool is_busy)
        {
            _is_busy = is_busy;
            btnAdd.Enabled = !is_busy;
        }
    }
}
