// the login page: an email box, a password box and a sign in button (owner: divyesh)
// it is a page shown inside the main window. it only reads the boxes and shows messages: the boxes are
// checked by LoginInput and the login itself, the lock-out and the audit rows are done by AuthService
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class LoginForm : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int FORM_WIDTH = 380;
        private const int LABEL_HEIGHT = 22;
        private const int BOX_HEIGHT = 28;
        private const int ROW_HEIGHT = 74;
        private const int FIRST_ROW_TOP = 56;

        private readonly AuthService _auth_service;

        // called when the login worked, with the role, the member (null for staff and admin) and the email
        private readonly Action<Role, Member?, string> _signed_in;

        // called when the user clicks the sign up button
        private readonly Action _go_sign_up;

        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtPassword = new TextBox();

        // one red message under each box, in the same order as the boxes (LoginInput.BOX_EMAIL, BOX_PASSWORD)
        private readonly Label[] _box_messages = new Label[LoginInput.BOX_COUNT];

        // a message about the whole login, like a wrong password or a locked account
        private readonly Label lblMessage = new Label();

        public LoginForm(AuthService auth_service, Action<Role, Member?, string> signed_in, Action go_sign_up)
        {
            _auth_service = auth_service;
            _signed_in = signed_in;
            _go_sign_up = go_sign_up;

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
            lblHeading.Text = "Sign in to your account";
            lblHeading.SetBounds(PAGE_PADDING, PAGE_PADDING, FORM_WIDTH, 26);
            Controls.Add(lblHeading);

            AddRow("Email", txtEmail, LoginInput.BOX_EMAIL);

            txtPassword.UseSystemPasswordChar = true;
            AddRow("Password", txtPassword, LoginInput.BOX_PASSWORD);

            // pressing Enter in the password box is the same as clicking sign in
            txtPassword.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ClickSignIn();
                }
            };

            int buttons_top = FIRST_ROW_TOP + (LoginInput.BOX_COUNT * ROW_HEIGHT) + 4;

            Button btnSignIn = MakeButton("Sign in", UiTheme.ACCENT_RED);
            btnSignIn.SetBounds(PAGE_PADDING, buttons_top, 180, 40);
            btnSignIn.Click += (sender, e) => ClickSignIn();

            Button btnSignUp = MakeButton("Create an account", UiTheme.CARD);
            btnSignUp.SetBounds(PAGE_PADDING + 192, buttons_top, 188, 40);
            btnSignUp.Click += (sender, e) => _go_sign_up();

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(PAGE_PADDING, buttons_top + 52, FORM_WIDTH, 60);

            Controls.Add(btnSignIn);
            Controls.Add(btnSignUp);
            Controls.Add(lblMessage);
        }

        // a label, a box and the red message under the box
        private void AddRow(string label_text, TextBox box, int box_place)
        {
            int row_top = FIRST_ROW_TOP + (box_place * ROW_HEIGHT);

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
            _box_messages[box_place] = lblBoxMessage;

            Controls.Add(lblName);
            Controls.Add(box);
            Controls.Add(lblBoxMessage);
        }

        private Button MakeButton(string text, Color colour)
        {
            Button button = new Button();
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = colour;
            button.ForeColor = UiTheme.TEXT;
            button.Cursor = Cursors.Hand;
            return button;
        }

        // the boxes are checked first, then AuthService tries the login
        private void ClickSignIn()
        {
            lblMessage.Text = "";

            // every wrong box shows its own message, all at once
            string[] box_errors = LoginInput.CheckBoxes(txtEmail.Text, txtPassword.Text);
            bool is_any_wrong = false;

            for (int box_index = 0; box_index < box_errors.Length; box_index++)
            {
                _box_messages[box_index].Text = box_errors[box_index];

                if (box_errors[box_index] != "")
                {
                    is_any_wrong = true;
                }
            }

            if (is_any_wrong)
            {
                return;
            }

            LoginInput? input;
            string error_message;

            if (!LoginInput.TryCreate(txtEmail.Text, txtPassword.Text, out input, out error_message))
            {
                lblMessage.Text = error_message;
                return;
            }

            Role role;
            Member? member;

            if (!_auth_service.Login(input!.Email, input.Password, out role, out member, out error_message))
            {
                // the password box is cleared, the email stays so the user can try again
                txtPassword.Text = "";
                lblMessage.Text = error_message;
                return;
            }

            txtPassword.Text = "";
            _signed_in(role, member, input.Email);
        }
    }
}
