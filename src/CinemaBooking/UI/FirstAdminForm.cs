// the page that makes the very first admin account, used only while no admin exists (owner: divyesh)
// it is a page shown inside the main window. it only reads the boxes and shows messages: the boxes are
// checked by FirstAdminInput and the admin is made by AuthService
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class FirstAdminForm : UserControl
    {
        private readonly AuthService _auth_service;

        // called when the admin is made, with the email, the main window shows the login page
        private readonly Action<string> _admin_created;

        // called when the user clicks back to the login page
        private readonly Action _go_login;

        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtConfirm = new TextBox();

        // one red message under each box, in the order of FirstAdminInput.BOX_EMAIL and the others
        private readonly Label[] _box_messages = new Label[FirstAdminInput.BOX_COUNT];

        // a message about the whole page, like "an admin already exists"
        private readonly Label lblMessage = new Label();

        private readonly Button btnCreate = FormParts.MakeButton("Create admin", UiTheme.ACCENT_RED);

        // true while the admin is being made on a background thread, a second click is ignored
        private bool _is_busy;

        public FirstAdminForm(AuthService auth_service, Action<string> admin_created, Action go_login)
        {
            _auth_service = auth_service;
            _admin_created = admin_created;
            _go_login = go_login;

            BuildPage();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = false;

            FormParts.AddHeading(this, "Set up the first admin", "There is no admin yet. This account manages everything.");

            _box_messages[FirstAdminInput.BOX_EMAIL] = FormParts.AddRow(this, "Admin email", txtEmail, FirstAdminInput.BOX_EMAIL, "admin@example.com");

            txtPassword.UseSystemPasswordChar = true;
            _box_messages[FirstAdminInput.BOX_PASSWORD] = FormParts.AddRow(this, "Password", txtPassword, FirstAdminInput.BOX_PASSWORD, "At least 8 characters");

            txtConfirm.UseSystemPasswordChar = true;
            _box_messages[FirstAdminInput.BOX_CONFIRM_PASSWORD] = FormParts.AddRow(this, "Password again", txtConfirm, FirstAdminInput.BOX_CONFIRM_PASSWORD, "Repeat the password");

            // pressing Enter in the last box is the same as clicking the create button
            txtConfirm.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ClickCreate();
                }
            };

            int buttons_top = FormParts.TopUnderRows(FirstAdminInput.BOX_COUNT);

            FormParts.PlaceWide(btnCreate, buttons_top);
            btnCreate.Click += (sender, e) => ClickCreate();
            Controls.Add(btnCreate);

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.Font = UiTheme.SMALL_FONT;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, buttons_top + FormParts.BUTTON_HEIGHT + 6, FormParts.FORM_WIDTH, 34);
            Controls.Add(lblMessage);

            FormParts.AddLinkLine(this, "", "Back", buttons_top + FormParts.BUTTON_HEIGHT + 44, _go_login, true);
        }

        // the boxes are checked first, then AuthService makes the admin
        private async void ClickCreate()
        {
            if (_is_busy)
            {
                return;
            }

            lblMessage.Text = "";

            string[] box_errors = FirstAdminInput.CheckBoxes(txtEmail.Text, txtPassword.Text, txtConfirm.Text);

            if (FormParts.ShowBoxMessages(_box_messages, box_errors))
            {
                return;
            }

            FirstAdminInput? input;
            string error_message;

            if (!FirstAdminInput.TryCreate(txtEmail.Text, txtPassword.Text, txtConfirm.Text, out input, out error_message))
            {
                lblMessage.Text = error_message;
                return;
            }

            // the database call runs on a background thread so the window does not freeze
            string email = input!.Email;
            string password = input.Password;

            SetBusy(true);
            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                string create_error;
                bool ok = _auth_service.CreateFirstAdmin(email, password, out create_error);
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

            _admin_created(email);
        }

        // while the call runs the button says "Creating..." and cannot be clicked again
        private void SetBusy(bool is_busy)
        {
            _is_busy = is_busy;
            btnCreate.Enabled = !is_busy;
            btnCreate.Text = is_busy ? "Creating..." : "Create admin";
        }
    }
}
