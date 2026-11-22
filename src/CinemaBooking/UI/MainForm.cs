// the main window with the side menu, every function is reached from here (owner: divyesh)
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

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

        // the menu buttons, in the order they appear
        private readonly Button btnFilms = new Button();
        private readonly Button btnShowings = new Button();
        private readonly Button btnBookSeats = new Button();
        private readonly Button btnMembers = new Button();
        private readonly Button btnCancel = new Button();
        private readonly Button btnAdmin = new Button();
        private readonly Button btnAccount = new Button();
        private readonly Button btnExit = new Button();

        // the button that is open now, it is shown in red
        private Button? _selected_button;

        // the page under the title now, it is taken out when another page opens
        private Control? _current_page;

        // every page shares one film table, so films loaded once are seen on every page
        private readonly HashTable<int, Film> _films = new HashTable<int, Film>();
        private readonly FilmService _film_service;
        private readonly ShowingService _showing_service;

        private readonly AuthService _auth_service;

        // who is signed in now, a guest until the login works
        private Role _current_role = Role.Guest;
        private Member? _current_member;
        private string _current_email = "";

        public MainForm(ShowingService showing_service, AuthService auth_service)
        {
            _showing_service = showing_service;
            _auth_service = auth_service;
            _film_service = new FilmService(_films);

            BuildWindow();
            BuildSideMenu();
            BuildContentArea();
            ShowPage(btnFilms, "Films", MakeFilmsPage());
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
            DoubleBuffered = true;
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
            AddMenuButton(btnFilms, "Films", 90, MakeFilmsPage);
            AddMenuButton(btnShowings, "Showings", 138, () => MakePlaceholderPage("The showings for each screen will be shown here."));
            AddMenuButton(btnBookSeats, "Book seats", 186, () => MakePlaceholderPage("Choosing a showing and seats will be done here."));
            AddMenuButton(btnCancel, "Cancel booking", 234, () => MakePlaceholderPage("A booking is cancelled here with its reference."));
            AddMenuButton(btnMembers, "Members", 282, () => MakePlaceholderPage("Members and their loyalty points will be shown here."));
            AddMenuButton(btnAdmin, "Admin", 330, MakeAdminPage);
            AddMenuButton(btnAccount, "My account", 378, MakeAccountPage);

            // exit sits at the bottom of the menu
            StyleMenuButton(btnExit, "Exit");
            // docked to the bottom of the menu, so it stays there at any window height
            btnExit.Dock = DockStyle.Bottom;
            btnExit.Height = UiTheme.MENU_BUTTON_HEIGHT;
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

            // the title sits on top, each page is added under it by ShowPage
            pnlContent.Controls.Add(lblPageTitle);
            pnlHost.Controls.Add(pnlContent);
            Controls.Add(pnlHost);

            // the fill panel must be laid out last so it takes the space left of the menu
            pnlHost.BringToFront();
        }

        // places one menu button. make_page builds a new page each time, so it always shows fresh data
        private void AddMenuButton(Button button, string text, int top, Func<Control> make_page)
        {
            StyleMenuButton(button, text);
            button.SetBounds(0, top, UiTheme.SIDE_MENU_WIDTH, UiTheme.MENU_BUTTON_HEIGHT);
            button.Click += (sender, e) => ShowPage(button, text, make_page());
            pnlSideMenu.Controls.Add(button);
        }

        // the films page, it only gets the film service, never the hash table (card 347)
        private Control MakeFilmsPage()
        {
            return new FilmBrowseForm(_film_service, OpenFilmDetail);
        }

        // the detail page of the film that was clicked (card 402), with the showings of that film
        private void OpenFilmDetail(Film film)
        {
            FilmDetailForm detail_page = new FilmDetailForm(
                film,
                () => ShowPage(btnFilms, "Films", MakeFilmsPage()),
                showing => OpenSeatMap(film, showing));

            detail_page.ShowShowings(_showing_service.GetShowingsForFilm(film.Id));

            ShowPage(btnFilms, "Film details", detail_page);
        }

        private Control MakeAdminPage()
        {
            if (_current_role != Role.Admin)
            {
                return MakePlaceholderPage("Sign in as an admin to manage films.");
            }

            return new AdminFilmsForm(_film_service, _current_role);
        }

        // the seat map of the chosen showing (card 348), back goes to the film's detail page
        private void OpenSeatMap(Film film, Showing showing)
        {
            SeatMapForm seat_page = new SeatMapForm(
                film,
                showing,
                () => OpenFilmDetail(film),
                seat_codes => OpenBookingSummary(film, showing, seat_codes));

            ShowPage(btnFilms, "Choose seats", seat_page);
        } 

        // the booking summary is the next card (403), for now the chosen seats are only named
        private void OpenBookingSummary(Film film, Showing showing, string[] seat_codes)
        {
            // a signed in customer is a member, everyone else books as a guest
            BookingSummaryForm summary_page = new BookingSummaryForm(
                film,
                showing,
                seat_codes,
                _current_member,
                () => OpenSeatMap(film, showing),
                () => ShowPage(btnAccount, "My account", MakeAccountPage()),
                guest_details => ShowPage(btnFilms, "Booking", MakePlaceholderPage(
                    "The booking is made when the booking service is connected.")));

            ShowPage(btnFilms, "Booking summary", summary_page);
        }

        // the account button shows the login page for a guest and the account page for someone signed in
        private Control MakeAccountPage()
        {
            if (_current_role == Role.Guest)
            {
                return new LoginForm(
                    _auth_service,
                    SignedIn,
                    () => ShowPage(btnAccount, "My account", MakePlaceholderPage("Sign up is built in its own card.")));
            }

            return new AccountForm(_current_role, _current_email, _current_member, SignOut);
        }

        // the login worked, so the window remembers who it is and shows the account page
        private void SignedIn(Role role, Member? member, string email)
        {
            _current_role = role;
            _current_member = member;
            _current_email = email;
            ShowPage(btnAccount, "My account", MakeAccountPage());
        }

        // back to a guest, and the login page opens again
        private void SignOut()
        {
            _current_role = Role.Guest;
            _current_member = null;
            _current_email = "";
            ShowPage(btnAccount, "My account", MakeAccountPage());
        }

        // a text page for the parts that are not built yet
        private static Control MakePlaceholderPage(string page_text)
        {
            Label lblPageText = new Label();
            lblPageText.Text = page_text;
            lblPageText.Font = UiTheme.BODY_FONT;
            lblPageText.ForeColor = UiTheme.TEXT_MUTED;
            lblPageText.BackColor = Color.Transparent;
            return lblPageText;
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
        private void ShowPage(Button clicked_button, string page_title, Control page)
        {
            if (_selected_button != null)
            {
                _selected_button.BackColor = UiTheme.SIDE_MENU;
            }

            _selected_button = clicked_button;
            _selected_button.BackColor = UiTheme.ACCENT_RED_DARK;

            lblPageTitle.Text = page_title;

            // the new page goes in before the old one comes out, so the area is never blank
            pnlContent.SuspendLayout();
            Control? old_page = _current_page;

            _current_page = page;
            page.Dock = DockStyle.Fill;
            pnlContent.Controls.Add(page);

            // front of the list is laid out last, so the page fills the space under the title
            page.BringToFront();

            if (old_page != null)
            {
                // free the old page so its posters do not stay in memory
                pnlContent.Controls.Remove(old_page);
                old_page.Dispose();
            }

            pnlContent.ResumeLayout();
        }
    }
}
