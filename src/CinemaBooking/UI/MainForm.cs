// the main window with the side menu, every function is reached from here (owner: divyesh)
// UI upgrade: the pages sit straight on the black background (no box around them), and the title grows when the window is maximised
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
        private readonly Panel pnlContent = new Panel();
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
        private readonly MenuButton btnMyBookings = new MenuButton();
        private readonly MenuButton btnCancel = new MenuButton();
        private readonly MenuButton btnMembers = new MenuButton();
        private readonly MenuButton btnTiers = new MenuButton();
        private readonly MenuButton btnStaffAccounts = new MenuButton();
        private readonly MenuButton btnAdmin = new MenuButton();
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

        // how big the text and spaces are now, it changes when the window gets wider or narrower
        private float _ui_step = UiScale.SMALL_STEP;

        // the title font made for a bigger step, kept so it can be freed when the step changes again
        private Font? _scaled_title_font;

        // true while the page shown draws its own header from edge to edge (the films page), then there is no title and no space around it
        private bool _is_full_bleed;

        public MainForm(ShowingService showing_service, AuthService auth_service, ShowingTable showing_table,
            BookingService booking_service, IDatabase database, LandingOutcome outcome)
        {
            _showing_service = showing_service;
            _auth_service = auth_service;
            _booking_service = booking_service;
            _database = database;

            // the landing window may have signed the user in already
            _current_role = outcome.Role;
            _current_member = outcome.Member;
            _current_email = outcome.Email;
            _showing_table = showing_table;
            _film_service = new FilmService(_films);
            _catalog_store = new CatalogStore(database, _films, showing_table, _film_service);
            _showing_admin_service = new ShowingAdminService(showing_table, FilmExists);
            _showing_store = new ShowingStore(database, _showing_admin_service, showing_table);

            BuildWindow();
            BuildSideMenu();
            BuildContentArea();
            _toast = new ToastBanner(this);

            // maximising or making the window wider makes the title and the spaces bigger
            Resize += (sender, e) => ApplyUiStep();
            ApplyUiStep();
            if (!_catalog_store.LoadAll(out string load_message))
            {
                _toast.ShowMessage(load_message, true);
            }
            ShowPage(btnFilms, "Films", MakeFilmsPage());

            if (outcome.Kind == LandingKind.SignedIn)
            {
                _toast.ShowMessage("Signed in as " + outcome.Email, false);
            }
        }

        // true when the user signed out or wants to sign in: the program then shows the landing window again
        public bool WantsLanding { get; private set; }

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
            AddMenuButton(btnMyBookings, "My bookings", UiTheme.ICON_MY_BOOKINGS, MakeMyBookingsPage);
            AddMenuButton(btnCancel, "Cancel booking", UiTheme.ICON_CANCEL, MakeFindBookingPage);
            AddMenuButton(btnMembers, "Members", UiTheme.ICON_MEMBERS, () => MakePlaceholderPage("Members and their loyalty points will be shown here."));
            AddMenuButton(btnTiers, "Tier rules", UiTheme.ICON_TIER_RULES, MakeTierRulesPage);
            AddMenuButton(btnStaffAccounts, "Staff accounts", UiTheme.ICON_MEMBERS, MakeStaffAccountsPage);
            AddMenuButton(btnAdmin, "Manage films", UiTheme.ICON_ADMIN, MakeAdminPage);

            // the user box is at the very bottom, exit sits just above it. both are docked, so they stay there at any window height
            _user_chip.Dock = DockStyle.Bottom;
            _user_chip.Cursor = Cursors.Hand;
            _user_chip.Click += (sender, e) => ShowUserMenu();
            _tips.SetToolTip(_user_chip, "Account");

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

        // the window got wider or narrower: when it crosses a step, the title and the spaces around the page change size
        private void ApplyUiStep()
        {
            float step = UiScale.StepFor(ClientSize.Width);

            if (step == _ui_step)
            {
                return;
            }

            _ui_step = step;

            Font? old_font = _scaled_title_font;

            if (step == UiScale.SMALL_STEP)
            {
                _scaled_title_font = null;
                lblPageTitle.Font = UiTheme.PAGE_TITLE_FONT;
            }
            else
            {
                _scaled_title_font = AppFonts.Display(UiScale.Size(UiTheme.SIZE_PAGE_TITLE, step));
                lblPageTitle.Font = _scaled_title_font;
            }

            // the shared theme font is never freed, only the one made here
            old_font?.Dispose();

            lblPageTitle.Height = UiScale.Pixels(UiTheme.PAGE_TITLE_HEIGHT, step);
            ApplyPageSpace();
        }

        // a normal page gets the title and the space around it, a full bleed page gets the whole area
        private void ApplyPageSpace()
        {
            if (_is_full_bleed)
            {
                lblPageTitle.Visible = false;
                pnlHost.Padding = new Padding(0);
                return;
            }

            lblPageTitle.Visible = true;
            int side = UiScale.Pixels(UiTheme.PAGE_SIDE_PADDING, _ui_step);
            int top = UiScale.Pixels(UiTheme.PAGE_TOP_PADDING, _ui_step);
            pnlHost.Padding = new Padding(side, top, side, top);
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
            pnlHost.Padding = new Padding(UiTheme.PAGE_SIDE_PADDING, UiTheme.PAGE_TOP_PADDING,
                UiTheme.PAGE_SIDE_PADDING, UiTheme.PAGE_TOP_PADDING);
            pnlHost.BackColor = UiTheme.BACKGROUND;

            // no box around the page any more, the page sits on the black background
            pnlContent.Dock = DockStyle.Fill;
            pnlContent.Padding = new Padding(0);
            pnlContent.BackColor = UiTheme.BACKGROUND;

            lblPageTitle.Dock = DockStyle.Top;
            lblPageTitle.Height = UiTheme.PAGE_TITLE_HEIGHT;
            lblPageTitle.TextAlign = ContentAlignment.TopLeft;
            lblPageTitle.Font = UiTheme.PAGE_TITLE_FONT;
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
                case MenuItem.MyBookings:
                    return btnMyBookings;
                case MenuItem.CancelBooking:
                    return btnCancel;
                case MenuItem.Members:
                    return btnMembers;
                case MenuItem.TierRules:
                    return btnTiers;
                case MenuItem.StaffAccounts:
                    return btnStaffAccounts;
                case MenuItem.Admin:
                    return btnAdmin;
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

        // the tier rules window (card 408) opens only for an admin, it only shows the rules, nothing can be changed
        private Control MakeTierRulesPage()
        {
            if (_current_role != Role.Admin)
            {
                return MakePlaceholderPage("Sign in as an admin to see the tier rules.");
            }

            return new AdminTiersForm();
        }

        // the page where an admin makes a staff or admin account (card 436), only an admin gets it
        private Control MakeStaffAccountsPage()
        {
            if (_current_role != Role.Admin)
            {
                return MakePlaceholderPage("Sign in as an admin to create staff accounts.");
            }

            return new StaffAccountsForm(_auth_service, _current_role, _current_email, ShowGoodToast);
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
                GoToLanding,
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

        // the user box at the bottom of the menu opens a small menu, what it holds depends on who is signed in (UserMenuRules)
        private void ShowUserMenu()
        {
            ContextMenuStrip menu = new ContextMenuStrip();
            menu.Renderer = new DarkMenuRenderer();
            menu.BackColor = UiTheme.CARD;
            menu.ForeColor = UiTheme.TEXT;
            menu.Font = UiTheme.BODY_FONT;
            menu.ShowImageMargin = false;

            foreach (UserMenuItem item in UserMenuRules.ItemsFor(_current_role))
            {
                UserMenuItem chosen_item = item;
                ToolStripMenuItem entry = new ToolStripMenuItem(UserMenuRules.LabelFor(item));
                entry.ForeColor = UiTheme.TEXT;
                entry.Padding = new Padding(6, 6, 6, 6);
                entry.Click += (sender, e) => RunUserMenuItem(chosen_item);
                menu.Items.Add(entry);
            }

            // the menu is freed after it closed, not while a click on it is still being handled
            menu.Closed += (sender, e) => BeginInvoke(new Action(menu.Dispose));
            menu.Show(_user_chip, new Point(8, 0), ToolStripDropDownDirection.AboveRight);
        }

        // does what the chosen entry of the user menu says
        private void RunUserMenuItem(UserMenuItem item)
        {
            switch (item)
            {
                case UserMenuItem.AccountDetails:
                    ShowPage(null, "Account details", new AccountForm(_current_role, _current_email, _current_member, GoToLanding));
                    break;
                case UserMenuItem.MyBookings:
                    ShowPage(btnMyBookings, "My bookings", MakeMyBookingsPage());
                    break;
                default:
                    GoToLanding();
                    break;
            }
        }

        // signing out, and signing in as a guest, both close this window: the program shows the landing window again
        private void GoToLanding()
        {
            WantsLanding = true;
            Close();
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
        // a page that belongs to no menu button (the account details) is shown with no button, then no button is marked
        private void ShowPage(MenuButton? clicked_button, string page_title, Control page)
        {
            if (_selected_button != null)
            {
                _selected_button.IsActive = false;
            }

            _selected_button = clicked_button;

            if (_selected_button != null)
            {
                _selected_button.IsActive = true;
            }

            lblPageTitle.Text = page_title;
            _is_full_bleed = page is IFullBleedPage;
            ApplyPageSpace();

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
