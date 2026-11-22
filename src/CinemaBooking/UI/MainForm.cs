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
        // where the first menu button starts, under the app name
        private const int FIRST_MENU_TOP = 90;

        // the parts of the window
        private readonly Panel pnlSideMenu = new Panel();
        private readonly Label lblAppName = new Label();
        private readonly Panel pnlHost = new Panel();
        private readonly RoundedPanel pnlContent = new RoundedPanel();
        private readonly Label lblPageTitle = new Label();

        // the button that opens and closes the menu, the user box at the bottom and the small messages
        private readonly Button btnToggle = new Button();
        private readonly UserChip _user_chip = new UserChip();
        private readonly ToastBanner _toast;
        private readonly ToolTip _tips = new ToolTip();

        // the menu is wide with names or narrow with only icons, it slides between the two
        private const int MENU_SLIDE_STEP = 26;
        private const int MENU_SLIDE_INTERVAL_MS = 15;
        private const int MENU_NAME_FROM_WIDTH = 150;
        private readonly System.Windows.Forms.Timer _menu_timer = new System.Windows.Forms.Timer();
        private bool _menu_is_collapsed;
        private int _menu_target_width = UiTheme.SIDE_MENU_WIDTH;

        // the menu buttons, RefreshMenu shows the ones the signed in role may see
        private readonly MenuButton btnFilms = new MenuButton();
        private readonly MenuButton btnShowings = new MenuButton();
        private readonly MenuButton btnBookSeats = new MenuButton();
        private readonly MenuButton btnMyBookings = new MenuButton();
        private readonly MenuButton btnCancel = new MenuButton();
        private readonly MenuButton btnMembers = new MenuButton();
        private readonly MenuButton btnAdmin = new MenuButton();
        private readonly MenuButton btnAccount = new MenuButton();
        private readonly MenuButton btnExit = new MenuButton();

        // the button that is open now, it is shown in red
        private MenuButton? _selected_button;

        // the page under the title now, it is taken out when another page opens
        private Control? _current_page;

        // every page shares one film table, so films loaded once are seen on every page
        private readonly HashTable<int, Film> _films = new HashTable<int, Film>();
        private readonly FilmService _film_service;
        private readonly ShowingService _showing_service;
        private readonly ShowingAdminService _showing_admin_service;
        private readonly AuthService _auth_service;

        // who is signed in now, a guest until the login works
        private Role _current_role = Role.Guest;
        private Member? _current_member;
        private string _current_email = "";

        public MainForm(ShowingService showing_service, AuthService auth_service, ShowingTable showing_table)
        {
            _showing_service = showing_service;
            _auth_service = auth_service;
            _film_service = new FilmService(_films);
            _showing_admin_service = new ShowingAdminService(showing_table, FilmExists);

            BuildWindow();
            BuildSideMenu();
            BuildContentArea();
            _toast = new ToastBanner(this);
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

        // builds the menu on the left with the toggle button, the app name, the buttons and the user box
        private void BuildSideMenu()
        {
            pnlSideMenu.Dock = DockStyle.Left;
            pnlSideMenu.Width = UiTheme.SIDE_MENU_WIDTH;
            pnlSideMenu.BackColor = UiTheme.SIDE_MENU;

            // the button at the top opens and closes the menu
            btnToggle.Text = UiTheme.ICON_MENU;
            btnToggle.Font = UiTheme.ICON_FONT;
            btnToggle.ForeColor = UiTheme.TEXT;
            btnToggle.BackColor = UiTheme.SIDE_MENU;
            btnToggle.FlatStyle = FlatStyle.Flat;
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.FlatAppearance.MouseOverBackColor = UiTheme.MENU_HOVER;
            btnToggle.Cursor = Cursors.Hand;
            btnToggle.SetBounds(12, 16, 40, 40);
            btnToggle.Click += (sender, e) => ToggleMenu();
            _tips.SetToolTip(btnToggle, "Open or close the menu");
            pnlSideMenu.Controls.Add(btnToggle);

            lblAppName.Text = "CINEMA";
            lblAppName.ForeColor = UiTheme.ACCENT_RED;
            lblAppName.Font = UiTheme.TITLE_FONT;
            lblAppName.SetBounds(60, 16, UiTheme.SIDE_MENU_WIDTH - 70, 44);
            lblAppName.TextAlign = ContentAlignment.MiddleLeft;
            pnlSideMenu.Controls.Add(lblAppName);

            // each menu item shows its page when it is clicked, RefreshMenu shows the ones this role may see
            AddMenuButton(btnFilms, "Films", UiTheme.ICON_FILMS, MakeFilmsPage);
            AddMenuButton(btnShowings, "Showings", UiTheme.ICON_SHOWINGS, MakeShowingsPage);
            AddMenuButton(btnBookSeats, "Book seats", UiTheme.ICON_BOOK_SEATS, () => MakePlaceholderPage("Choosing a showing and seats will be done here."));
            AddMenuButton(btnMyBookings, "My bookings", UiTheme.ICON_MY_BOOKINGS, () => MakePlaceholderPage("Your bookings and cancelling will be shown here."));
            AddMenuButton(btnCancel, "Cancel booking", UiTheme.ICON_CANCEL, () => MakePlaceholderPage("A booking is cancelled here with its reference."));
            AddMenuButton(btnMembers, "Members", UiTheme.ICON_MEMBERS, () => MakePlaceholderPage("Members and their loyalty points will be shown here."));
            AddMenuButton(btnAdmin, "Admin", UiTheme.ICON_ADMIN, MakeAdminPage);
            AddMenuButton(btnAccount, "My account", UiTheme.ICON_ACCOUNT, MakeAccountPage);

            // the user box is at the very bottom, exit sits just above it. both are docked, so they stay there at any window height
            _user_chip.Dock = DockStyle.Bottom;

            btnExit.Text = "Exit";
            btnExit.Glyph = UiTheme.ICON_EXIT;
            btnExit.Dock = DockStyle.Bottom;
            btnExit.Height = UiTheme.MENU_BUTTON_HEIGHT;
            btnExit.Click += (sender, e) => Close();
            _tips.SetToolTip(btnExit, "Exit");

            pnlSideMenu.Controls.Add(btnExit);
            pnlSideMenu.Controls.Add(_user_chip);

            _menu_timer.Interval = MENU_SLIDE_INTERVAL_MS;
            _menu_timer.Tick += (sender, e) => SlideMenu();

            RefreshMenu();

            Controls.Add(pnlSideMenu);
        }

        // opens a closed menu and closes an open one, the menu slides a few pixels each tick
        private void ToggleMenu()
        {
            _menu_is_collapsed = !_menu_is_collapsed;
            _menu_target_width = _menu_is_collapsed ? UiTheme.SIDE_MENU_COLLAPSED_WIDTH : UiTheme.SIDE_MENU_WIDTH;
            _menu_timer.Start();
        }

        private void SlideMenu()
        {
            int width = pnlSideMenu.Width;

            if (width < _menu_target_width)
            {
                width = Math.Min(width + MENU_SLIDE_STEP, _menu_target_width);
            }
            else
            {
                width = Math.Max(width - MENU_SLIDE_STEP, _menu_target_width);
            }

            pnlSideMenu.Width = width;

            // the app name has no room in a narrow menu
            lblAppName.Visible = width >= MENU_NAME_FROM_WIDTH;

            if (width == _menu_target_width)
            {
                _menu_timer.Stop();
            }
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

        // sets up one menu button. RefreshMenu decides if it is shown and where.
        // make_page builds a new page each time, so it always shows fresh data
        private void AddMenuButton(MenuButton button, string text, string glyph, Func<Control> make_page)
        {
            button.Text = text;
            button.Glyph = glyph;
            button.Visible = false;

            // the button gets as wide as the menu while the menu slides
            button.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
            button.Click += (sender, e) => ShowPage(button, button.Text, make_page());
            _tips.SetToolTip(button, text);
            pnlSideMenu.Controls.Add(button);
        }

        // shows the buttons this role may see, one under the other, and hides the others
        private void RefreshMenu()
        {
            foreach (MenuItem item in Enum.GetValues<MenuItem>())
            {
                MenuButtonFor(item).Visible = false;
            }

            MenuItem[] visible_items = MenuRules.ItemsFor(_current_role);

            for (int item_index = 0; item_index < visible_items.Length; item_index++)
            {
                MenuButton button = MenuButtonFor(visible_items[item_index]);
                button.SetBounds(0, FIRST_MENU_TOP + (item_index * UiTheme.MENU_BUTTON_HEIGHT),
                    pnlSideMenu.Width, UiTheme.MENU_BUTTON_HEIGHT);
                button.Visible = true;
            }

            btnAccount.Text = MenuRules.AccountLabel(_current_role);
            _tips.SetToolTip(btnAccount, btnAccount.Text);

            // the user box at the bottom shows who is signed in
            if (_current_role == Role.Guest)
            {
                _user_chip.SetUser("Guest", "Not signed in");
            }
            else
            {
                string chip_title = _current_member != null ? _current_member.Name : _current_email;
                _user_chip.SetUser(chip_title, _current_role.ToString());
            }
        }

        // the button that belongs to a menu item
        private MenuButton MenuButtonFor(MenuItem item)
        {
            switch (item)
            {
                case MenuItem.Showings:
                    return btnShowings;
                case MenuItem.BookSeats:
                    return btnBookSeats;
                case MenuItem.MyBookings:
                    return btnMyBookings;
                case MenuItem.CancelBooking:
                    return btnCancel;
                case MenuItem.Members:
                    return btnMembers;
                case MenuItem.Admin:
                    return btnAdmin;
                case MenuItem.Account:
                    return btnAccount;
                default:
                    return btnFilms;
            }
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

        // the showings page opens for staff and admins, the service also refuses anyone else
        private Control MakeShowingsPage()
        {
            if (_current_role != Role.Staff && _current_role != Role.Admin)
            {
                return MakePlaceholderPage("Sign in as staff or an admin to manage showings.");
            }

            return new AdminShowingsForm(_showing_admin_service, _current_role);
        }

        // tells the showing rules if a film id exists, by looking at the films the film service knows
        private bool FilmExists(int film_id)
        {
            Film[] all_films = _film_service.Search("", "");

            for (int film_index = 0; film_index < all_films.Length; film_index++)
            {
                if (all_films[film_index].Id == film_id)
                {
                    return true;
                }
            }

            return false;
        }

        // the admin page opens only when an admin is signed in
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
                () => ShowPage(btnAccount, btnAccount.Text, MakeAccountPage()),
                guest_details => ShowPage(btnFilms, "Booking", MakePlaceholderPage(
                    "The booking is made when the booking service is connected.")));

            ShowPage(btnFilms, "Booking summary", summary_page);
        }

        // the account button shows the login page for a guest and the account page for someone signed in
        private Control MakeAccountPage()
        {
            if (_current_role == Role.Guest)
            {
                return MakeLoginPage("");
            }

            return new AccountForm(_current_role, _current_email, _current_member, SignOut);
        }

        // the login page, the notice is a good message above the boxes, like "account created"
        // the first admin button only shows while there is no admin yet
        private Control MakeLoginPage(string notice)
        {
            Action? go_first_admin = null;

            if (!_auth_service.HasAnyAdmin())
            {
                go_first_admin = () => ShowPage(btnAccount, "Admin setup", new FirstAdminForm(
                    _auth_service,
                    email => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage("Admin account created for " + email + ", sign in now")),
                    () => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage(""))));
            }

            return new LoginForm(
                _auth_service,
                SignedIn,
                () => ShowPage(btnAccount, "Sign up", new SignUpForm(
                    _auth_service,
                    email => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage("Account created for " + email + ", sign in now")),
                    () => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage("")))),
                go_first_admin,
                notice);
        }

        // the login worked, so the window remembers who it is, updates the menu and shows the account page
        private void SignedIn(Role role, Member? member, string email)
        {
            _current_role = role;
            _current_member = member;
            _current_email = email;
            RefreshMenu();
            ShowPage(btnAccount, btnAccount.Text, MakeAccountPage());
            _toast.ShowMessage("Signed in as " + email, false);
        }

        // back to a guest, the menu shrinks and the login page opens again
        private void SignOut()
        {
            _current_role = Role.Guest;
            _current_member = null;
            _current_email = "";
            RefreshMenu();
            ShowPage(btnAccount, btnAccount.Text, MakeAccountPage());
            _toast.ShowMessage("You are signed out", false);
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

        // shows a page in the content area and marks the clicked button with the red bar
        private void ShowPage(MenuButton clicked_button, string page_title, Control page)
        {
            if (_selected_button != null)
            {
                _selected_button.IsActive = false;
            }

            _selected_button = clicked_button;
            _selected_button.IsActive = true;

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
