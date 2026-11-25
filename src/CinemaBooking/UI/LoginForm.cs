// the sign in form of the landing window: who you are signing in as, an email box, a password box (owner: divyesh)
// it only reads the boxes and shows messages: the boxes are checked by LoginInput, the login itself, the lock-out and
// the audit rows are done by AuthService, and SignInRules says if the chosen "sign in as" fits the account
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class LoginForm : UserControl
    {
        // the "sign in as" list is the first row, so the email and password rows start one row lower
        private const int ROW_AS = 0;
        private const int FIRST_BOX_ROW = 1;

        private readonly AuthService _auth_service;

        // called when the login worked and the choice fits, with the role, the member (null for staff and admin) and the email
        private readonly Action<Role, Member?, string> _signed_in;

        // called when the user clicks the sign up button
        private readonly Action _go_sign_up;

        // called when the user clicks back, the landing window shows its first buttons again
        private readonly Action _go_back;

        // a good message above the boxes, like "account created", it can be empty
        private readonly string _notice;

        private readonly ComboBox cmbSignInAs = new ComboBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly TextBox txtPassword = new TextBox();

        // one red message under each box, in the same order as the boxes (LoginInput.BOX_EMAIL, BOX_PASSWORD)
        private readonly Label[] _box_messages = new Label[LoginInput.BOX_COUNT];

        // a message about the whole login, like a wrong password, a locked account or a wrong "sign in as"
        private readonly Label lblMessage = new Label();

        private readonly Button btnSignIn = FormParts.MakeButton("Sign in", UiTheme.ACCENT_RED);

        // true while the login is running on a background thread, a second click is ignored
        private bool _is_busy;

        public LoginForm(AuthService auth_service, SignInAs start_choice, Action<Role, Member?, string> signed_in,
            Action go_sign_up, Action go_back, string notice = "")
        {
            _auth_service = auth_service;
            _signed_in = signed_in;
            _go_sign_up = go_sign_up;
            _go_back = go_back;
            _notice = notice;

            BuildPage(start_choice);
        }

        private void BuildPage(SignInAs start_choice)
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "Sign in to your account";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 26);
            Controls.Add(lblHeading);

            Label lblNotice = new Label();
            lblNotice.ForeColor = UiTheme.GOOD;
            lblNotice.Text = _notice;
            lblNotice.SetBounds(FormParts.PAGE_PADDING, 34, FormParts.FORM_WIDTH, 20);
            Controls.Add(lblNotice);

            foreach (SignInAs choice in SignInRules.Choices())
            {
                cmbSignInAs.Items.Add(choice.ToString());
            }

            cmbSignInAs.SelectedItem = start_choice.ToString();
            FormParts.AddListRow(this, "Sign in as", cmbSignInAs, ROW_AS);

            _box_messages[LoginInput.BOX_EMAIL] = FormParts.AddRow(this, "Email", txtEmail, FIRST_BOX_ROW + LoginInput.BOX_EMAIL);

            txtPassword.UseSystemPasswordChar = true;
            _box_messages[LoginInput.BOX_PASSWORD] = FormParts.AddRow(this, "Password", txtPassword, FIRST_BOX_ROW + LoginInput.BOX_PASSWORD);

            // pressing Enter in the password box is the same as clicking sign in
            txtPassword.KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Enter)
                {
                    e.SuppressKeyPress = true;
                    ClickSignIn();
                }
            };

            int buttons_top = FormParts.TopUnderRows(FIRST_BOX_ROW + LoginInput.BOX_COUNT);

            btnSignIn.SetBounds(FormParts.PAGE_PADDING, buttons_top, 180, 40);
            btnSignIn.Click += (sender, e) => ClickSignIn();

            Button btnSignUp = FormParts.MakeButton("Create an account", UiTheme.CARD);
            btnSignUp.SetBounds(FormParts.PAGE_PADDING + 192, buttons_top, 188, 40);
            btnSignUp.Click += (sender, e) => _go_sign_up();

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, buttons_top + 52, FormParts.FORM_WIDTH, 44);

            Button btnBack = FormParts.MakeButton("Back", UiTheme.PANEL);
            btnBack.ForeColor = UiTheme.TEXT_MUTED;
            btnBack.SetBounds(FormParts.PAGE_PADDING, buttons_top + 100, 120, 36);
            btnBack.Click += (sender, e) => _go_back();

            Controls.Add(btnSignIn);
            Controls.Add(btnSignUp);
            Controls.Add(lblMessage);
            Controls.Add(btnBack);
        }

        // the "sign in as" choice, the first one if nothing is chosen
        private SignInAs ChosenSignInAs()
        {
            SignInAs[] choices = SignInRules.Choices();
            int chosen_index = cmbSignInAs.SelectedIndex;

            return chosen_index >= 0 && chosen_index < choices.Length ? choices[chosen_index] : choices[0];
        }

        // the boxes are checked first, then AuthService tries the login, then the choice is checked against the real role
        private async void ClickSignIn()
        {
            if (_is_busy)
            {
                return;
            }

            lblMessage.Text = "";

            // every wrong box shows its own message, all at once
            if (FormParts.ShowBoxMessages(_box_messages, LoginInput.CheckBoxes(txtEmail.Text, txtPassword.Text)))
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

            // the database call runs on a background thread so the window does not freeze
            string email = input!.Email;
            string password = input.Password;
            SignInAs chosen = ChosenSignInAs();

            SetBusy(true);
            CallResult<(bool Ok, Role Role, Member? Member, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                Role found_role;
                Member? found_member;
                string found_error;
                bool ok = _auth_service.Login(email, password, out found_role, out found_member, out found_error);
                return (ok, found_role, found_member, found_error);
            });
            SetBusy(false);

            if (!result.IsOk)
            {
                lblMessage.Text = result.ErrorText;
                return;
            }

            Role role = result.Value.Role;
            Member? member = result.Value.Member;

            if (!result.Value.Ok)
            {
                // the password box is cleared, the email stays so the user can try again
                txtPassword.Text = "";
                lblMessage.Text = result.Value.Error;
                return;
            }

            txtPassword.Text = "";

            // the password was right, but the account may be another kind than the one chosen
            if (!SignInRules.IsAllowed(chosen, role))
            {
                lblMessage.Text = SignInRules.RefusalMessage(chosen, role);
                return;
            }

            _signed_in(role, member, email);
        }

        // while the call runs the button says "Signing in..." and cannot be clicked again
        private void SetBusy(bool is_busy)
        {
            _is_busy = is_busy;
            btnSignIn.Enabled = !is_busy;
            btnSignIn.Text = is_busy ? "Signing in..." : "Sign in";
        }
    }
}
