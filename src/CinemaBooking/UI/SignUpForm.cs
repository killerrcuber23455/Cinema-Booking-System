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
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "Create your account";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 26);
            Controls.Add(lblHeading);

            _box_messages[SignUpInput.BOX_NAME] = FormParts.AddRow(this, "Name", txtName, SignUpInput.BOX_NAME);
            _box_messages[SignUpInput.BOX_EMAIL] = FormParts.AddRow(this, "Email", txtEmail, SignUpInput.BOX_EMAIL);
            _box_messages[SignUpInput.BOX_PHONE] = FormParts.AddRow(this, "Phone", txtPhone, SignUpInput.BOX_PHONE);

            txtPassword.UseSystemPasswordChar = true;
            _box_messages[SignUpInput.BOX_PASSWORD] = FormParts.AddRow(this, "Password (at least 8 characters)", txtPassword, SignUpInput.BOX_PASSWORD);

            txtConfirm.UseSystemPasswordChar = true;
            _box_messages[SignUpInput.BOX_CONFIRM_PASSWORD] = FormParts.AddRow(this, "Password again", txtConfirm, SignUpInput.BOX_CONFIRM_PASSWORD);

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

            Button btnCreate = FormParts.MakeButton("Create account", UiTheme.ACCENT_RED);
            btnCreate.SetBounds(FormParts.PAGE_PADDING, buttons_top, 180, 40);
            btnCreate.Click += (sender, e) => ClickCreate();

            Button btnBack = FormParts.MakeButton("Back to sign in", UiTheme.CARD);
            btnBack.SetBounds(FormParts.PAGE_PADDING + 192, buttons_top, 188, 40);
            btnBack.Click += (sender, e) => _go_login();

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, buttons_top + 52, FormParts.FORM_WIDTH, 60);

            Controls.Add(btnCreate);
            Controls.Add(btnBack);
            Controls.Add(lblMessage);
        }

        // the boxes are checked first, then AuthService makes the account
        private void ClickCreate()
        {
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

            if (!_auth_service.SignUp(input!.Name, input.Email, input.Phone, input.Password, out error_message))
            {
                // the passwords are cleared, the other boxes stay so the user can fix one and try again
                txtPassword.Text = "";
                txtConfirm.Text = "";
                lblMessage.Text = error_message;
                return;
            }

            _account_created(input.Email);
        }
    }
}
