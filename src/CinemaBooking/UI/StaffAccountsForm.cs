// the page where an admin makes a staff or admin account (card 436, owner: divyesh)
// it is a page shown inside the main window. it only reads the boxes and shows messages: the boxes are
// checked by StaffInput, the words come from StaffAccountText and the account is made by AuthService.CreateStaff
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class StaffAccountsForm : UserControl
    {
        private readonly AuthService _auth_service;

        // who is making the account, AuthService checks again that this user is really an admin
        private readonly Role _acting_role;
        private readonly string _acting_email;

        // called with the success text, the main window shows it as a message at the top
        private readonly Action<string> _show_good_message;

        private readonly TextBox txtEmail = new TextBox();
        private readonly ComboBox cmbRole = new ComboBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtConfirm = new TextBox();

        // one red message under each box, in the order of StaffInput.BOX_EMAIL and the others
        private readonly Label[] _box_messages = new Label[StaffInput.BOX_COUNT];

        // a message about the whole page, like "this email is already used"
        private readonly Label lblMessage = new Label();

        private readonly Button btnCreate = FormParts.MakeButton("Create account", UiTheme.ACCENT_RED);

        // true while the account is being made on a background thread, a second click is ignored
        private bool _is_busy;

        public StaffAccountsForm(AuthService auth_service, Role acting_role, string acting_email, Action<string> show_good_message)
        {
            _auth_service = auth_service;
            _acting_role = acting_role;
            _acting_email = acting_email;
            _show_good_message = show_good_message;

            BuildPage();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "Create a staff account";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 26);
            Controls.Add(lblHeading);

            Label lblHelp = new Label();
            lblHelp.ForeColor = UiTheme.TEXT_MUTED;
            lblHelp.Text = "The person signs in with this email and password. Staff manage showings and bookings, admins manage everything.";
            lblHelp.SetBounds(FormParts.PAGE_PADDING, 34, FormParts.FORM_WIDTH + 200, 20);
            Controls.Add(lblHelp);

            _box_messages[StaffInput.BOX_EMAIL] = FormParts.AddRow(this, "Email", txtEmail, StaffInput.BOX_EMAIL);

            cmbRole.Items.AddRange(StaffAccountText.RoleOptions());
            cmbRole.SelectedIndex = 0;
            _box_messages[StaffInput.BOX_ROLE] = FormParts.AddListRow(this, "Role", cmbRole, StaffInput.BOX_ROLE);

            txtPassword.UseSystemPasswordChar = true;
            _box_messages[StaffInput.BOX_PASSWORD] = FormParts.AddRow(this, "Password (at least 8 characters)", txtPassword, StaffInput.BOX_PASSWORD);

            txtConfirm.UseSystemPasswordChar = true;
            _box_messages[StaffInput.BOX_CONFIRM_PASSWORD] = FormParts.AddRow(this, "Password again", txtConfirm, StaffInput.BOX_CONFIRM_PASSWORD);

            // pressing Enter in the last box is the same as clicking the create button
            txtConfirm.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ClickCreate();
                }
            };

            int buttons_top = FormParts.TopUnderRows(StaffInput.BOX_COUNT);

            btnCreate.SetBounds(FormParts.PAGE_PADDING, buttons_top, 180, 40);
            btnCreate.Click += (sender, e) => ClickCreate();

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, buttons_top + 52, FormParts.FORM_WIDTH, 60);

            Controls.Add(btnCreate);
            Controls.Add(lblMessage);
        }

        // the boxes are checked first, then AuthService makes the account
        private async void ClickCreate()
        {
            if (_is_busy)
            {
                return;
            }

            lblMessage.Text = "";

            Role? chosen_role = StaffAccountText.ParseRole(cmbRole.Text);
            string[] box_errors = StaffInput.CheckBoxes(txtEmail.Text, chosen_role, txtPassword.Text, txtConfirm.Text);

            if (FormParts.ShowBoxMessages(_box_messages, box_errors))
            {
                return;
            }

            StaffInput? input;
            string error_message;

            if (!StaffInput.TryCreate(txtEmail.Text, chosen_role, txtPassword.Text, txtConfirm.Text, out input, out error_message))
            {
                lblMessage.Text = error_message;
                return;
            }

            // the database call runs on a background thread so the window does not freeze
            string email = input!.Email;
            Role new_role = input.NewRole;
            string password = input.Password;

            SetBusy(true);
            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                string create_error;
                bool ok = _auth_service.CreateStaff(_acting_role, _acting_email, email, new_role, password, out create_error);
                return (ok, create_error);
            });
            SetBusy(false);

            if (!result.IsOk)
            {
                lblMessage.Text = result.ErrorText;
                return;
            }

            if (!result.Value.Ok)
            {
                txtPassword.Text = "";
                txtConfirm.Text = "";
                lblMessage.Text = result.Value.Error;
                return;
            }

            ClearBoxes();
            _show_good_message(StaffAccountText.SuccessMessage(email, new_role));
        }

        // after an account is made the page is ready for the next one, the role goes back to Staff
        private void ClearBoxes()
        {
            txtEmail.Text = "";
            txtPassword.Text = "";
            txtConfirm.Text = "";
            cmbRole.SelectedIndex = 0;
            lblMessage.Text = "";

            for (int box_index = 0; box_index < _box_messages.Length; box_index++)
            {
                _box_messages[box_index].Text = "";
            }
        }

        // while the call runs the button says "Creating..." and cannot be clicked again
        private void SetBusy(bool is_busy)
        {
            _is_busy = is_busy;
            btnCreate.Enabled = !is_busy;
            btnCreate.Text = is_busy ? "Creating..." : "Create account";
        }
    }
}
