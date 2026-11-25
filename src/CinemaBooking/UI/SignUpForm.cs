// the sign up page for a new customer: name, email, phone, password and the password again (owner: divyesh)
// it is a page shown inside the main window. it only reads the boxes and shows messages: the boxes are
// checked by SignUpInput and the account is made by AuthService
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class SignUpForm : UserControl
    {
        private readonly AuthService _auth_service;

        // called when the account is made, with the email, the main window shows the login page
        private readonly Action<string> _account_created;

        // called when the user clicks back to the login page
        private readonly Action _go_login;

        private readonly TextBox txtName = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtPhone = new TextBox();
        private readonly TextBox txtPassword = new TextBox();
        private readonly TextBox txtConfirm = new TextBox();

        // one red message under each box, in the order of SignUpInput.BOX_NAME and the others
        private readonly Label[] _box_messages = new Label[SignUpInput.BOX_COUNT];

        // a message about the whole sign up, like an email that is already used
        private readonly Label lblMessage = new Label();

        private readonly Button btnCreate = FormParts.MakeButton("Create account", UiTheme.ACCENT_RED);

        // true while the account is being made on a background thread, a second click is ignored
        private bool _is_busy;

        public SignUpForm(AuthService auth_service, Action<string> account_created, Action go_login)
        {
            _auth_service = auth_service;
            _account_created = account_created;
            _go_login = go_login;

            BuildPage();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = false;

            FormParts.AddHeading(this, "Create your account", "Join to earn points on every booking.");

            _box_messages[SignUpInput.BOX_NAME] = FormParts.AddRow(this, "Name", txtName, SignUpInput.BOX_NAME, "Your full name");
            _box_messages[SignUpInput.BOX_EMAIL] = FormParts.AddRow(this, "Email", txtEmail, SignUpInput.BOX_EMAIL, "you@example.com");
            _box_messages[SignUpInput.BOX_PHONE] = FormParts.AddRow(this, "Phone", txtPhone, SignUpInput.BOX_PHONE, "Phone number");

            txtPassword.UseSystemPasswordChar = true;
            _box_messages[SignUpInput.BOX_PASSWORD] = FormParts.AddRow(this, "Password", txtPassword, SignUpInput.BOX_PASSWORD, "At least 8 characters");

            txtConfirm.UseSystemPasswordChar = true;
            _box_messages[SignUpInput.BOX_CONFIRM_PASSWORD] = FormParts.AddRow(this, "Password again", txtConfirm, SignUpInput.BOX_CONFIRM_PASSWORD, "Repeat the password");

            // pressing Enter in the last box is the same as clicking the create button
            txtConfirm.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ClickCreate();
                }
            };

            int buttons_top = FormParts.TopUnderRows(SignUpInput.BOX_COUNT);

            FormParts.PlaceWide(btnCreate, buttons_top);
            btnCreate.Click += (sender, e) => ClickCreate();
            Controls.Add(btnCreate);

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.Font = UiTheme.SMALL_FONT;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, buttons_top + FormParts.BUTTON_HEIGHT + 6, FormParts.FORM_WIDTH, 34);
            Controls.Add(lblMessage);

            FormParts.AddLinkLine(this, "Already have an account?", "Sign in", buttons_top + FormParts.BUTTON_HEIGHT + 44, _go_login);
        }

        // the boxes are checked first, then AuthService makes the account
        private async void ClickCreate()
        {
            if (_is_busy)
            {
                return;
            }

            lblMessage.Text = "";

            string[] box_errors = SignUpInput.CheckBoxes(txtName.Text, txtEmail.Text, txtPhone.Text,
                txtPassword.Text, txtConfirm.Text);

            if (FormParts.ShowBoxMessages(_box_messages, box_errors))
            {
                return;
            }

            SignUpInput? input;
            string error_message;

            if (!SignUpInput.TryCreate(txtName.Text, txtEmail.Text, txtPhone.Text, txtPassword.Text,
                    txtConfirm.Text, out input, out error_message))
            {
                lblMessage.Text = error_message;
                return;
            }

            // the database call runs on a background thread so the window does not freeze
            string name = input!.Name;
            string email = input.Email;
            string phone = input.Phone;
            string password = input.Password;

            SetBusy(true);
            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                string sign_up_error;
                bool ok = _auth_service.SignUp(name, email, phone, password, out sign_up_error);
                return (ok, sign_up_error);
            });
            SetBusy(false);

            if (!result.IsOk)
            {
                lblMessage.Text = result.ErrorText;
                return;
            }

            if (!result.Value.Ok)
            {
                // the passwords are cleared, the other boxes stay so the user can fix one and try again
                txtPassword.Text = "";
                txtConfirm.Text = "";
                lblMessage.Text = result.Value.Error;
                return;
            }

            _account_created(email);
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
