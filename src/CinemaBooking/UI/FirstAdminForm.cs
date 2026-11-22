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
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "Create the first admin";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 26);
            Controls.Add(lblHeading);

            Label lblHelp = new Label();
            lblHelp.ForeColor = UiTheme.TEXT_MUTED;
            lblHelp.Text = "There is no admin yet. This account can manage films and staff.";
            lblHelp.SetBounds(FormParts.PAGE_PADDING, 34, FormParts.FORM_WIDTH + 100, 20);
            Controls.Add(lblHelp);

            _box_messages[FirstAdminInput.BOX_EMAIL] = FormParts.AddRow(this, "Admin email", txtEmail, FirstAdminInput.BOX_EMAIL);

            txtPassword.UseSystemPasswordChar = true;
            _box_messages[FirstAdminInput.BOX_PASSWORD] = FormParts.AddRow(this, "Password (at least 8 characters)", txtPassword, FirstAdminInput.BOX_PASSWORD);

            txtConfirm.UseSystemPasswordChar = true;
            _box_messages[FirstAdminInput.BOX_CONFIRM_PASSWORD] = FormParts.AddRow(this, "Password again", txtConfirm, FirstAdminInput.BOX_CONFIRM_PASSWORD);

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

            Button btnCreate = FormParts.MakeButton("Create admin", UiTheme.ACCENT_RED);
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

        // the boxes are checked first, then AuthService makes the admin
        private void ClickCreate()
        {
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

            if (!_auth_service.CreateFirstAdmin(input!.Email, input.Password, out error_message))
            {
                txtPassword.Text = "";
                txtConfirm.Text = "";
                lblMessage.Text = error_message;
                return;
            }

            _admin_created(input.Email);
        }
    }
}
