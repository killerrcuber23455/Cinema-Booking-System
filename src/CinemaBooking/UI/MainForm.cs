// the main window with the side menu, every function is reached from here (owner: divyesh)
using System;
using System.Drawing;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public class MainForm : Form
    {
        // the parts of the window
        private readonly Panel pnlSideMenu = new Panel();
        private readonly Label lblAppName = new Label();
        private readonly Panel pnlHost = new Panel();
        private readonly RoundedPanel pnlContent = new RoundedPanel();
        private readonly Label lblPageTitle = new Label();
        private readonly Label lblPageText = new Label();

        // the menu buttons, in the order they appear
        private readonly Button btnFilms = new Button();
        private readonly Button btnShowings = new Button();
        private readonly Button btnBookSeats = new Button();
        private readonly Button btnMembers = new Button();
        private readonly Button btnAccount = new Button();
        private readonly Button btnExit = new Button();

        // the button that is open now, it is shown in red
        private Button? _selected_button;

        public MainForm()
        {
            BuildWindow();
            BuildSideMenu();
            BuildContentArea();
            ShowPage(btnFilms, "Films", "The film list will be shown here.");
        }

        // sets up the window itself
        private void BuildWindow()
        {
            Text = "Cinema Booking System";
            Size = new Size(1100, 700);
            MinimumSize = new Size(900, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor = UiTheme.BACKGROUND;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
        }

        // builds the menu on the left with the app name and the buttons
        private void BuildSideMenu()
        {
            pnlSideMenu.Dock = DockStyle.Left;
            pnlSideMenu.Width = UiTheme.SIDE_MENU_WIDTH;
            pnlSideMenu.BackColor = UiTheme.SIDE_MENU;

            lblAppName.Text = "CINEMA";
            lblAppName.ForeColor = UiTheme.ACCENT_RED;
            lblAppName.Font = UiTheme.TITLE_FONT;
            lblAppName.SetBounds(20, 20, UiTheme.SIDE_MENU_WIDTH - 40, 50);
            pnlSideMenu.Controls.Add(lblAppName);

            // each menu item shows its page when it is clicked
            AddMenuButton(btnFilms, "Films", 90, "Films", "The film list will be shown here.");
            AddMenuButton(btnShowings, "Showings", 138, "Showings", "The showings for each screen will be shown here.");
            AddMenuButton(btnBookSeats, "Book seats", 186, "Book seats", "Choosing a showing and seats will be done here.");
            AddMenuButton(btnMembers, "Members", 234, "Members", "Members and their loyalty points will be shown here.");
            AddMenuButton(btnAccount, "My account", 282, "My account", "Login and account details will be shown here.");

            // exit sits at the bottom of the menu
            StyleMenuButton(btnExit, "Exit");
            btnExit.SetBounds(0, 560, UiTheme.SIDE_MENU_WIDTH, UiTheme.MENU_BUTTON_HEIGHT);
            btnExit.Anchor = AnchorStyles.Bottom | AnchorStyles.Left | AnchorStyles.Right;
            btnExit.Click += (sender, e) => Close();
            pnlSideMenu.Controls.Add(btnExit);

            Controls.Add(pnlSideMenu);
        }

        // builds the area on the right where the pages appear
        private void BuildContentArea()
        {
            pnlHost.Dock = DockStyle.Fill;
            pnlHost.Padding = new Padding(24);
            pnlHost.BackColor = UiTheme.BACKGROUND;

            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(28);

            lblPageTitle.Dock = DockStyle.Top;
            lblPageTitle.Height = 60;
            lblPageTitle.Font = UiTheme.TITLE_FONT;
            lblPageTitle.ForeColor = UiTheme.TEXT;
            lblPageTitle.BackColor = Color.Transparent;

            lblPageText.Dock = DockStyle.Fill;
            lblPageText.Font = UiTheme.BODY_FONT;
            lblPageText.ForeColor = UiTheme.TEXT_MUTED;
            lblPageText.BackColor = Color.Transparent;

            // fill goes in first, then the title on top of it
            pnlContent.Controls.Add(lblPageText);
            pnlContent.Controls.Add(lblPageTitle);
            pnlHost.Controls.Add(pnlContent);
            Controls.Add(pnlHost);

            // the fill panel must be laid out last so it takes the space left of the menu
            pnlHost.BringToFront();
        }

        // places one menu button and says which page it opens
        private void AddMenuButton(Button button, string text, int top, string page_title, string page_text)
        {
            StyleMenuButton(button, text);
            button.SetBounds(0, top, UiTheme.SIDE_MENU_WIDTH, UiTheme.MENU_BUTTON_HEIGHT);
            button.Click += (sender, e) => ShowPage(button, page_title, page_text);
            pnlSideMenu.Controls.Add(button);
        }

        // gives a menu button the theme look
        private static void StyleMenuButton(Button button, string text)
        {
            button.Text = text;
            button.Font = UiTheme.MENU_FONT;
            button.ForeColor = UiTheme.TEXT;
            button.BackColor = UiTheme.SIDE_MENU;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.FlatAppearance.MouseOverBackColor = UiTheme.MENU_HOVER;
            button.TextAlign = ContentAlignment.MiddleLeft;
            button.Padding = new Padding(24, 0, 0, 0);
            button.Cursor = Cursors.Hand;
        }

        // shows a page in the content area and turns the clicked button red
        private void ShowPage(Button clicked_button, string page_title, string page_text)
        {
            if (_selected_button != null)
            {
                _selected_button.BackColor = UiTheme.SIDE_MENU;
            }

            _selected_button = clicked_button;
            _selected_button.BackColor = UiTheme.ACCENT_RED_DARK;

            lblPageTitle.Text = page_title;
            lblPageText.Text = page_text;
        }
    }
}
