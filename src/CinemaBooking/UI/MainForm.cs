// the main window with the side menu, every function is reached from here (owner: divyesh)
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Data;
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
        private readonly Panel pnlSideMenu = new SideMenuPanel();
        private readonly GlowLogo lblAppName = new GlowLogo();
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
        private readonly CatalogStore _catalog_store;
        private readonly ShowingService _showing_service;
        private readonly ShowingAdminService _showing_admin_service;
        private readonly ShowingStore _showing_store;
        private readonly AuthService _auth_service;
        private readonly BookingService _booking_service;
        private readonly IDatabase _database;
        private readonly ShowingTable _showing_table;

        // who is signed in now, a guest until the login works
        private Role _current_role = MenuRules.STARTING_ROLE;
        private Member? _current_member;
        private string _current_email = "";

        // true while a booking is being made on a background task
        private bool _is_booking;

        public MainForm(ShowingService showing_service, AuthService auth_service, ShowingTable showing_table,
            BookingService booking_service, IDatabase database, FirstPage first_page = FirstPage.Films)
        {
            _showing_service = showing_service;
            _auth_service = auth_service;
            _booking_service = booking_service;
            _database = database;
            _showing_table = showing_table;
            _film_service = new FilmService(_films);
            _catalog_store = new CatalogStore(database, _films, showing_table, _film_service);
            _showing_admin_service = new ShowingAdminService(showing_table, FilmExists);
            _showing_store = new ShowingStore(database, _showing_admin_service, showing_table);

            BuildWindow();
            BuildSideMenu();
            BuildContentArea();
            _toast = new ToastBanner(this);
            if (!_catalog_store.LoadAll(out string load_message))
            {
                _toast.ShowMessage(load_message, true);
            }
            ShowFirstPage(first_page);
        }

        // the page the landing window asked for: the films for a guest, otherwise the sign in or sign up page
        private void ShowFirstPage(FirstPage first_page)
        {
            string account_title = MenuRules.AccountLabel(_current_role);

            switch (first_page)
            {
                case FirstPage.Login:
                    ShowPage(btnAccount, account_title, MakeLoginPage(""));
                    break;
                case FirstPage.SignUp:
                    ShowPage(btnAccount, "Sign up", MakeSignUpPage());
                    break;
                case FirstPage.StaffLogin:
                    ShowPage(btnAccount, account_title, MakeLoginPage(LandingRules.STAFF_NOTICE));
                    break;
                default:
                    ShowPage(btnFilms, "Films", MakeFilmsPage());
                    break;
            }
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
            btnToggle.BackColor = UiTheme.SIDE_MENU_TOP;
            btnToggle.FlatStyle = FlatStyle.Flat;
            btnToggle.FlatAppearance.BorderSize = 0;
            btnToggle.FlatAppearance.MouseOverBackColor = UiTheme.MENU_HOVER;
            btnToggle.Cursor = Cursors.Hand;
            btnToggle.TabStop = false;
            btnToggle.SetBounds(12, 16, 40, 40);
            btnToggle.Click += (sender, e) => ToggleMenu();
            _tips.SetToolTip(btnToggle, "Open or close the menu");
            pnlSideMenu.Controls.Add(btnToggle);

            // the name glows in red like a neon sign
            lblAppName.Text = "CINEMA";
            lblAppName.SetBounds(60, 14, UiTheme.SIDE_MENU_WIDTH - 66, 48);
            pnlSideMenu.Controls.Add(lblAppName);

            // each menu item shows its page when it is clicked, RefreshMenu shows the ones this role may see
            AddMenuButton(btnFilms, "Films", UiTheme.ICON_FILMS, MakeFilmsPage);
            AddMenuButton(btnShowings, "Showings", UiTheme.ICON_SHOWINGS, MakeShowingsPage);
            AddMenuButton(btnBookSeats, "Book seats", UiTheme.ICON_BOOK_SEATS, () => MakePlaceholderPage("Choosing a showing and seats will be done here."));
            AddMenuButton(btnMyBookings, "My bookings", UiTheme.ICON_MY_BOOKINGS, MakeMyBookingsPage);
            AddMenuButton(btnCancel, "Cancel booking", UiTheme.ICON_CANCEL, MakeFindBookingPage);
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
            return new FilmBrowseForm(_film_service, _catalog_store, _current_role, OpenFilmDetail);
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

            return new AdminShowingsForm(_showing_admin_service, _showing_store, _current_role, ShowGoodToast);
        }

        // the page where a guest finds a booking with the reference and email and can cancel it
        // the page gets small functions, so it holds no rules and no database of its own
        private Control MakeFindBookingPage()
        {
            return new FindBookingForm(
                (reference, email) =>
                {
                    Booking? found = BookingLookup.Find(_database, reference, email, out string find_error);
                    return (found, find_error);
                },
                (reference, email) =>
                {
                    bool is_cancelled = _booking_service.CancelBooking(Role.Guest, null, email, reference, out string cancel_error);
                    return (is_cancelled, cancel_error);
                },
                DescribeShowingById,
                ShowGoodToast);
        }

        // the page where a signed in customer sees their bookings and can cancel one
        // the page gets small functions, so it holds no rules and no database of its own
        private Control MakeMyBookingsPage()
        {
            int? member_id = _current_member?.Id;
            Role role = _current_role;

            if (member_id == null)
            {
                return MakePlaceholderPage(MyBookingList.SIGN_IN_TEXT);
            }

            return new MyBookingsForm(
                () =>
                {
                    Booking[] found = MyBookingList.ForMember(_database, member_id, out string list_message);
                    return (found, list_message);
                },
                reference =>
                {
                    bool is_cancelled = _booking_service.CancelBooking(role, member_id, "", reference, out string cancel_error);
                    return (is_cancelled, cancel_error);
                },
                DescribeShowingById,
                ShowGoodToast);
        }

        // the film name and the time of a showing, for the booking card
        private string DescribeShowingById(int showing_id)
        {
            Showing? showing = _showing_table.FindShowing(showing_id);

            if (showing == null)
            {
                return "Showing " + showing_id;
            }

            Film[] all_films = _film_service.Search("", "");

            for (int film_index = 0; film_index < all_films.Length; film_index++)
            {
                if (all_films[film_index].Id == showing.FilmId)
                {
                    return all_films[film_index].Title + " · " + showing.StartsAt.ToString("ddd d MMM yyyy · HH:mm", System.Globalization.CultureInfo.InvariantCulture);
                }
            }

            return "Showing " + showing_id + " · " + showing.StartsAt.ToString("ddd d MMM yyyy · HH:mm", System.Globalization.CultureInfo.InvariantCulture);
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

        // the admin pages call this when something worked, for example a film was added
        private void ShowGoodToast(string message)
        {
            _toast.ShowMessage(message, false);
        }

        // the admin page opens only when an admin is signed in
        private Control MakeAdminPage()
        {
            if (_current_role != Role.Admin)
            {
                return MakePlaceholderPage("Sign in as an admin to manage films.");
            }

            return new AdminFilmsForm(_film_service, _catalog_store, _current_role, ShowGoodToast);
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
                guest_details => ConfirmBooking(film, showing, seat_codes, guest_details));

            ShowPage(btnFilms, "Booking summary", summary_page);
        }

        // the guest or the signed in customer pressed Confirm: the booking is made on a background task
        // a customer has no guest details, the booking uses the member's account
        private async void ConfirmBooking(Film film, Showing showing, string[] seat_codes, GuestDetails? guest_details)
        {
            // a second click while the booking is being made is ignored, so the seats are never booked twice
            if (_is_booking)
            {
                return;
            }

            _is_booking = true;
            Role role = _current_role;
            int? member_id = _current_member?.Id;
            string name = guest_details?.Name ?? "";
            string email = guest_details?.Email ?? "";
            string phone = guest_details?.Phone ?? "";

            CallResult<(Booking? Booking, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                Booking? made;
                string error;
                _booking_service.BookSeats(role, member_id, name, email, phone, showing.Id, seat_codes, 0, out made, out error);
                return (made, error);
            });

            _is_booking = false;

            if (!result.IsOk)
            {
                _toast.ShowMessage(result.ErrorText, true);
                return;
            }

            if (result.Value.Booking == null)
            {
                // for example a seat was taken a moment ago: show the seat map again with the seats now taken
                _toast.ShowMessage(result.Value.Error, true);
                OpenSeatMap(film, showing);
                return;
            }

            _toast.ShowMessage("Booking confirmed: " + result.Value.Booking.Reference, false);
            ShowPage(btnFilms, "Booking confirmed",
                new BookingConfirmedForm(result.Value.Booking, film, showing, () => ShowPage(btnFilms, "Films", MakeFilmsPage())));
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

            // if the database cannot be reached, no button is shown here: the sign in will show the clear message
            bool has_admin = true;

            try
            {
                has_admin = _auth_service.HasAnyAdmin();
            }
            catch (InvalidOperationException)
            {
                has_admin = true;
            }

            if (!has_admin)
            {
                go_first_admin = () => ShowPage(btnAccount, "Admin setup", new FirstAdminForm(
                    _auth_service,
                    email => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage("Admin account created for " + email + ", sign in now")),
                    () => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage(""))));
            }

            return new LoginForm(
                _auth_service,
                SignedIn,
                () => ShowPage(btnAccount, "Sign up", MakeSignUpPage()),
                go_first_admin,
                notice);
        }

        // the sign up page: after it works the login page opens with a notice
        private Control MakeSignUpPage()
        {
            return new SignUpForm(
                _auth_service,
                email => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage("Account created for " + email + ", sign in now")),
                () => ShowPage(btnAccount, btnAccount.Text, MakeLoginPage("")));
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
