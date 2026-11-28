// the Members page for staff and admins: the list of members, find one by email, and register a new member (card 342, owner: divyesh)
// the page holds no rules and no database, it gets small functions from the main window.
// the boxes are checked by MemberInput, the list lines come from MemberLine, the rules are in MemberService
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class MembersForm : UserControl
    {
        private const int LIST_WIDTH = 760;
        private const int FIND_TOP = 44;
        private const int LIST_TOP = 100;
        private const int LIST_HEIGHT = 230;
        private const int REGISTER_TOP = LIST_TOP + LIST_HEIGHT + 20;
        private const int REGISTER_BOXES_TOP = REGISTER_TOP + 30;
        private const int BUTTON_ROW_HEIGHT = 46;

        private readonly Func<(Member[] Members, string Error)> _load;
        private readonly Func<string, (Member? Found, string Error)> _find;
        private readonly Func<string, string, string, (int NewId, string Error)> _register;
        private readonly Action<string> _show_toast;

        private readonly TextBox txtFind = new TextBox();
        private readonly Button btnFind = FormParts.MakeButton("Find", UiTheme.ACCENT_RED);
        private readonly Button btnShowAll = FormParts.MakeButton("Show all", UiTheme.CARD);
        private readonly ListBox lstMembers = new ListBox();

        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtPhone = new TextBox();
        private readonly Button btnRegister = FormParts.MakeButton("Register member", UiTheme.ACCENT_RED);

        // one red message under each register box, in the order of MemberInput.BOX_NAME and the others
        private readonly Label[] _box_messages = new Label[MemberInput.BOX_COUNT];

        // problems and counts of the whole page show here
        private readonly Label lblMessage = new Label();

        // true while a call runs on a background thread, a second click is ignored
        private bool _is_busy;

        public MembersForm(Func<(Member[] Members, string Error)> load,
            Func<string, (Member? Found, string Error)> find,
            Func<string, string, string, (int NewId, string Error)> register,
            Action<string> show_toast)
        {
            _load = load;
            _find = find;
            _register = register;
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
            lblHeading.Text = "Members";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 30);
            Controls.Add(lblHeading);

            // find a member by email
            FormParts.PlaceBox(this, txtFind, FormParts.PAGE_PADDING, FIND_TOP, 420, "Find a member by email");
            PlaceButton(btnFind, FormParts.PAGE_PADDING + 436, FIND_TOP, 120);
            PlaceButton(btnShowAll, FormParts.PAGE_PADDING + 572, FIND_TOP, 140);
            btnFind.Click += (sender, e) => ClickFind();
            btnShowAll.Click += (sender, e) => Reload();
            txtFind.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ClickFind();
                }
            };

            lstMembers.BackColor = UiTheme.CARD;
            lstMembers.ForeColor = UiTheme.TEXT;
            lstMembers.BorderStyle = BorderStyle.None;
            lstMembers.SetBounds(FormParts.PAGE_PADDING, LIST_TOP, LIST_WIDTH, LIST_HEIGHT);
            Controls.Add(lstMembers);

            // register a new member
            Label lblRegister = new Label();
            lblRegister.Text = "Register a new member";
            lblRegister.Font = UiTheme.CARD_TITLE_FONT;
            lblRegister.SetBounds(FormParts.PAGE_PADDING, REGISTER_TOP, FormParts.FORM_WIDTH, 26);
            Controls.Add(lblRegister);

            FormParts.PlaceBox(this, txtName, FormParts.PAGE_PADDING, REGISTER_BOXES_TOP, 240, "Name");
            FormParts.PlaceBox(this, txtEmail, FormParts.PAGE_PADDING + 256, REGISTER_BOXES_TOP, 260, "Email");
            FormParts.PlaceBox(this, txtPhone, FormParts.PAGE_PADDING + 532, REGISTER_BOXES_TOP, 228, "Phone");

            int messages_top = REGISTER_BOXES_TOP + FormParts.BOX_HEIGHT + 2;
            _box_messages[MemberInput.BOX_NAME] = MakeBoxMessage(FormParts.PAGE_PADDING, messages_top, 240);
            _box_messages[MemberInput.BOX_EMAIL] = MakeBoxMessage(FormParts.PAGE_PADDING + 256, messages_top, 260);
            _box_messages[MemberInput.BOX_PHONE] = MakeBoxMessage(FormParts.PAGE_PADDING + 532, messages_top, 228);

            int button_top = messages_top + FormParts.BOX_MESSAGE_HEIGHT + 8;
            PlaceButton(btnRegister, FormParts.PAGE_PADDING, button_top, 220);
            btnRegister.Click += (sender, e) => ClickRegister();

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
            message.SetBounds(left, top, width, FormParts.BOX_MESSAGE_HEIGHT);
            Controls.Add(message);
            return message;
        }

        // reads all members again and fills the list
        private async void Reload()
        {
            if (_is_busy)
            {
                return;
            }

            SetBusy(true);
            ShowInfo("Loading members...");

            CallResult<(Member[] Members, string Error)> result = await BackgroundCall.RunAsync(() => _load());
            SetBusy(false);

            if (!result.IsOk)
            {
                ShowProblem(result.ErrorText);
                return;
            }

            if (result.Value.Error != "")
            {
                lstMembers.Items.Clear();
                ShowProblem(result.Value.Error);
                return;
            }

            FillList(result.Value.Members);
            ShowInfo(result.Value.Members.Length == 0 ? "No members yet" : result.Value.Members.Length + " members");
        }

        // finds one member by email and shows only that member in the list
        private async void ClickFind()
        {
            if (_is_busy)
            {
                return;
            }

            string hint = MemberLine.FindHint(txtFind.Text);

            if (hint != "")
            {
                ShowProblem(hint);
                return;
            }

            string email = txtFind.Text.Trim();

            SetBusy(true);
            CallResult<(Member? Found, string Error)> result = await BackgroundCall.RunAsync(() => _find(email));
            SetBusy(false);

            if (!result.IsOk)
            {
                ShowProblem(result.ErrorText);
                return;
            }

            Member? found = result.Value.Found;

            if (found == null)
            {
                ShowProblem(result.Value.Error);
                return;
            }

            FillList(new Member[] { found });
            ShowInfo("Found 1 member. Click Show all to see everyone.");
        }

        // the boxes are checked first, then the member is registered on a background thread
        private async void ClickRegister()
        {
            if (_is_busy)
            {
                return;
            }

            ShowInfo("");

            string[] box_errors = MemberInput.CheckBoxes(txtName.Text, txtEmail.Text, txtPhone.Text);

            if (FormParts.ShowBoxMessages(_box_messages, box_errors))
            {
                return;
            }

            string name = txtName.Text.Trim();
            string email = txtEmail.Text.Trim();
            string phone = txtPhone.Text.Trim();

            SetBusy(true);
            CallResult<(int NewId, string Error)> result = await BackgroundCall.RunAsync(() => _register(name, email, phone));
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
            txtEmail.Text = "";
            txtPhone.Text = "";
            _show_toast("Member registered: " + email);
            Reload();
        }

        private void FillList(Member[] members)
        {
            lstMembers.Items.Clear();

            for (int member_index = 0; member_index < members.Length; member_index++)
            {
                lstMembers.Items.Add(MemberLine.Text(members[member_index]));
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

        // while a call runs the buttons cannot be clicked again
        private void SetBusy(bool is_busy)
        {
            _is_busy = is_busy;
            btnFind.Enabled = !is_busy;
            btnShowAll.Enabled = !is_busy;
            btnRegister.Enabled = !is_busy;
        }
    }
}
