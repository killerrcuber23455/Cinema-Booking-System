// the landing window: the first window when the program runs, before the main window (owner: divyesh)
// it shows the choices. sign in, sign up and the first admin are forms inside the card of this window, so the user
// is already signed in when the main window opens. what the window gives back is a LandingOutcome, the rules for
// it are in LandingRules, SignInRules and AuthService, so this window holds no rules.
// two film strips of the real posters drift past on the sides (one up, one down) and a red glow sits behind the card.
// the strips stop and the window shows at once when Windows has animations switched off.
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class LandingWindow : Form
    {
        private const int WINDOW_WIDTH = 1000;
        private const int WINDOW_HEIGHT = 720;

        // the film strips on the left and the right
        private const int STRIP_WIDTH = 240;
        private const int BAND_MARGIN = 20;
        private const int BAND_WIDTH = 200;
        private const int POSTER_WIDTH = 150;
        private const int POSTER_HEIGHT = 225;
        private const int POSTER_GAP = 18;
        private const int CELL_HEIGHT = POSTER_HEIGHT + POSTER_GAP;
        private const int POSTER_CORNER = 10;
        private const int MAX_POSTERS = 12;
        private const int FADE_HEIGHT = 130;
        private const int HOLE_GAP = 28;

        private const int CARD_WIDTH = 460;
        private const int CARD_HEIGHT = 500;

        // the card is taller while a form is open, the form starts under the logo
        private const int CARD_FORM_HEIGHT = 640;
        private const int FORM_TOP = 96;
        private const int FORM_WIDTH = 400;
        private const int BUTTON_WIDTH = 340;

        private const float SCROLL_PER_FRAME = 0.9f;
        private const int FRAME_MS = 33;
        private const double FADE_IN_STEP = 0.07;
        private const string POSTER_FOLDER = "posters";
        private const string PLACEHOLDER_NAME = "placeholder";
        private const int WM_NCHITTEST = 0x84;
        private const int HTCLIENT = 1;
        private const int HTCAPTION = 2;

        private readonly List<Bitmap> _posters;
        private readonly System.Windows.Forms.Timer _timer = new System.Windows.Forms.Timer();
        private readonly bool _is_animated = SystemInformation.UIEffectsEnabled;
        private float _scroll;

        private readonly LandingCard _card = new LandingCard();
        private readonly Label lblTagline = new Label();
        private readonly Label lblStaff = new Label();
        private readonly Label lblClose = new Label();
        private readonly Button btnSignIn = FormParts.MakeButton("Sign in", UiTheme.ACCENT_RED);
        private readonly Button btnSignUp = FormParts.MakeButton("Sign up", UiTheme.CARD);
        private readonly Button btnGuest = FormParts.MakeButton("Continue as guest", UiTheme.PANEL);
        private readonly Button btnStaff = FormParts.MakeButton("Staff and admin sign in", UiTheme.CARD);
        private readonly Button btnFirstAdmin = FormParts.MakeButton("No admin yet? Set up the first admin", UiTheme.PANEL);

        private readonly AuthService _auth_service;

        // the parts of the first view of the card, they are hidden while a form is open
        private readonly List<Control> _home_parts = new List<Control>();

        // the form open inside the card (sign in, sign up or first admin), null while the first view shows
        private Control? _form_view;

        // how the window ended: closed, a guest, or someone who signed in here
        public LandingOutcome Outcome { get; private set; } = LandingOutcome.Closed();

        public LandingWindow(AuthService auth_service)
        {
            _auth_service = auth_service;
            FormBorderStyle = FormBorderStyle.None;
            StartPosition = FormStartPosition.CenterScreen;
            ClientSize = new Size(WINDOW_WIDTH, WINDOW_HEIGHT);
            BackColor = UiTheme.BACKGROUND;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            Text = "Cinema Booking System";
            DoubleBuffered = true;
            KeyPreview = true;
            Opacity = _is_animated ? 0 : 1;

            _posters = LoadPosters();
            BuildCard();
            BuildCloseLabel();

            AcceptButton = btnSignIn;
            ActiveControl = btnSignIn;

            KeyDown += (sender, e) =>
            {
                if (e.KeyCode == Keys.Escape)
                {
                    // Esc goes back from a form to the first view, and closes the window from the first view
                    if (_form_view != null)
                    {
                        ShowHome();
                    }
                    else
                    {
                        Close();
                    }
                }
            };

            if (_is_animated)
            {
                _timer.Interval = FRAME_MS;
                _timer.Tick += (sender, e) => NextFrame();
                _timer.Start();
            }
        }

        // the card in the middle: logo, one line about the app and the four ways in
        private void BuildCard()
        {
            PlaceCard(CARD_HEIGHT);

            lblTagline.Text = "Pick a film, choose your seats and keep every ticket in one place.";
            lblTagline.Font = UiTheme.BODY_FONT;
            lblTagline.ForeColor = UiTheme.TEXT_MUTED;
            lblTagline.BackColor = Color.Transparent;
            lblTagline.TextAlign = ContentAlignment.TopCenter;
            lblTagline.SetBounds(60, 100, CARD_WIDTH - 120, 48);
            _card.Controls.Add(lblTagline);

            PlaceButton(btnSignIn, 172, 50, UiTheme.CARD_TITLE_FONT, UiTheme.TEXT);
            PlaceButton(btnSignUp, 236, 50, UiTheme.MENU_FONT, UiTheme.TEXT);
            PlaceButton(btnGuest, 300, 50, UiTheme.MENU_FONT, UiTheme.TEXT_MUTED);

            lblStaff.Text = "For staff and admin";
            lblStaff.Font = UiTheme.SMALL_FONT;
            lblStaff.ForeColor = UiTheme.TEXT_MUTED;
            lblStaff.BackColor = Color.Transparent;
            lblStaff.TextAlign = ContentAlignment.MiddleCenter;
            lblStaff.SetBounds(60, 398, CARD_WIDTH - 120, 22);
            _card.Controls.Add(lblStaff);

            PlaceButton(btnStaff, 426, 42, UiTheme.MENU_FONT, UiTheme.TEXT);

            // the first admin button only shows while there is no admin yet
            PlaceButton(btnFirstAdmin, 358, 30, UiTheme.SMALL_FONT, UiTheme.TEXT_MUTED);
            btnFirstAdmin.Visible = !HasAdmin();
            btnFirstAdmin.Click += (sender, e) => ShowFirstAdmin();

            _home_parts.AddRange(new Control[] { lblTagline, btnSignIn, btnSignUp, btnGuest, lblStaff, btnStaff, btnFirstAdmin });

            btnSignIn.Click += (sender, e) => Choose(LandingChoice.LogIn);
            btnSignUp.Click += (sender, e) => Choose(LandingChoice.SignUp);
            btnGuest.Click += (sender, e) => Choose(LandingChoice.Guest);
            btnStaff.Click += (sender, e) => Choose(LandingChoice.Staff);

            Controls.Add(_card);
        }

        // puts a button on the card, centred, so the visible body (not the glow space around it) starts at body_top
        private void PlaceButton(Button button, int body_top, int body_height, Font font, Color text_colour)
        {
            int space = UiTheme.GLOW_SPREAD;
            button.Font = font;
            button.ForeColor = text_colour;
            button.SetBounds((CARD_WIDTH - BUTTON_WIDTH) / 2 - space, body_top - space,
                BUTTON_WIDTH + (2 * space), body_height + (2 * space));
            _card.Controls.Add(button);
        }

        // a small close mark in the top right corner, Esc does the same
        private void BuildCloseLabel()
        {
            lblClose.Text = "";
            lblClose.Font = UiTheme.ICON_FONT;
            lblClose.ForeColor = UiTheme.TEXT_MUTED;
            lblClose.BackColor = Color.Transparent;
            lblClose.TextAlign = ContentAlignment.MiddleCenter;
            lblClose.Cursor = Cursors.Hand;
            lblClose.AccessibleName = "Close";
            lblClose.SetBounds(WINDOW_WIDTH - 52, 10, 40, 34);
            lblClose.MouseEnter += (sender, e) => lblClose.ForeColor = UiTheme.TEXT;
            lblClose.MouseLeave += (sender, e) => lblClose.ForeColor = UiTheme.TEXT_MUTED;
            lblClose.Click += (sender, e) => Close();
            Controls.Add(lblClose);
        }

        // true when an admin exists. if the database cannot be reached the button stays hidden: the sign in shows the clear message
        private bool HasAdmin()
        {
            try
            {
                return _auth_service.HasAnyAdmin();
            }
            catch (InvalidOperationException)
            {
                return true;
            }
        }

        // puts the card in the middle of the window with the given height
        private void PlaceCard(int height)
        {
            _card.SetBounds((WINDOW_WIDTH - CARD_WIDTH) / 2, (WINDOW_HEIGHT - height) / 2, CARD_WIDTH, height);
        }

        // a guest closes the window, the other buttons open their form inside the card
        private void Choose(LandingChoice choice)
        {
            switch (choice)
            {
                case LandingChoice.Guest:
                    Outcome = LandingOutcome.Guest();
                    Close();
                    break;
                case LandingChoice.SignUp:
                    ShowSignUp();
                    break;
                case LandingChoice.LogIn:
                case LandingChoice.Staff:
                    ShowSignIn(SignInRules.StartingChoice(choice), "");
                    break;
            }
        }

        // the sign in form, the notice is a good message above the boxes like "account created"
        private void ShowSignIn(SignInAs start_choice, string notice)
        {
            ShowForm(new LoginForm(
                _auth_service,
                start_choice,
                SignedIn,
                ShowSignUp,
                ShowHome,
                notice));
        }

        // the sign up form, after it works the sign in form opens with a notice
        private void ShowSignUp()
        {
            ShowForm(new SignUpForm(
                _auth_service,
                email => ShowSignIn(SignInAs.Customer, "Account created for " + email + ", sign in now"),
                () => ShowSignIn(SignInAs.Customer, "")));
        }

        // the form for the very first admin, after it works the staff sign in opens with a notice
        private void ShowFirstAdmin()
        {
            ShowForm(new FirstAdminForm(
                _auth_service,
                email =>
                {
                    btnFirstAdmin.Visible = false;
                    ShowSignIn(SignInAs.Admin, "Admin account created for " + email + ", sign in now");
                },
                ShowHome));
        }

        // the login worked and the choice fits the account: the window ends with the outcome and the main window opens
        private void SignedIn(Role role, Member? member, string email)
        {
            Outcome = LandingOutcome.SignedIn(role, member, email);
            Close();
        }

        // shows a form inside the card instead of the first view, the old form is freed
        private void ShowForm(Control form)
        {
            CloseForm();

            foreach (Control part in _home_parts)
            {
                part.Visible = false;
            }

            PlaceCard(CARD_FORM_HEIGHT);
            form.BackColor = _card.BackColor;
            form.SetBounds((CARD_WIDTH - FORM_WIDTH) / 2, FORM_TOP, FORM_WIDTH, CARD_FORM_HEIGHT - FORM_TOP - 16);
            _card.Controls.Add(form);
            _form_view = form;
            form.Focus();
        }

        // back to the first view of the card: logo, tagline and the buttons
        private void ShowHome()
        {
            CloseForm();
            PlaceCard(CARD_HEIGHT);

            foreach (Control part in _home_parts)
            {
                part.Visible = true;
            }

            btnFirstAdmin.Visible = !HasAdmin();
            ActiveControl = btnSignIn;
        }

        // frees the open form, if there is one
        private void CloseForm()
        {
            if (_form_view != null)
            {
                _card.Controls.Remove(_form_view);
                _form_view.Dispose();
                _form_view = null;
            }
        }

        // one animation step: fade the window in once, then move the strips
        private void NextFrame()
        {
            if (Opacity < 1)
            {
                Opacity = Math.Min(1.0, Opacity + FADE_IN_STEP);
            }

            _scroll += SCROLL_PER_FRAME;
            float loop_length = CELL_HEIGHT * Math.Max(1, _posters.Count);
            if (_scroll >= loop_length)
            {
                _scroll -= loop_length;
            }

            // only the two strips are painted again, the card is left alone
            Invalidate(new Rectangle(0, 0, STRIP_WIDTH, ClientSize.Height));
            Invalidate(new Rectangle(ClientSize.Width - STRIP_WIDTH, 0, STRIP_WIDTH, ClientSize.Height));
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            Graphics g = e.Graphics;
            g.Clear(BackColor);
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.InterpolationMode = InterpolationMode.HighQualityBilinear;

            DrawGlow(g);
            DrawStrip(g, 0, true);
            DrawStrip(g, ClientSize.Width - STRIP_WIDTH, false);

            using (Pen edge = new Pen(UiTheme.ACCENT_RED_DARK, 1))
            {
                g.DrawRectangle(edge, 0, 0, ClientSize.Width - 1, ClientSize.Height - 1);
            }
        }

        // a soft red light behind the card, like a lit screen in a dark room
        private void DrawGlow(Graphics g)
        {
            int centre_x = ClientSize.Width / 2;
            int centre_y = ClientSize.Height / 2;

            using (GraphicsPath light = new GraphicsPath())
            {
                light.AddEllipse(centre_x - 400, centre_y - 310, 800, 620);
                using (PathGradientBrush brush = new PathGradientBrush(light))
                {
                    brush.CenterColor = Color.FromArgb(70, UiTheme.ACCENT_RED);
                    brush.SurroundColors = new Color[] { Color.FromArgb(0, UiTheme.ACCENT_RED) };
                    g.FillPath(brush, light);
                }
            }
        }

        // one film strip: a dark band with holes down both sides and the posters moving along it
        private void DrawStrip(Graphics g, int strip_x, bool goes_up)
        {
            int band_x = strip_x + BAND_MARGIN;
            int count = _posters.Count;
            int step = (int)(_scroll / CELL_HEIGHT);
            float frac = _scroll - (step * CELL_HEIGHT);
            int height = ClientSize.Height;

            using (SolidBrush band = new SolidBrush(Color.FromArgb(10, 10, 13)))
            {
                g.FillRectangle(band, band_x, 0, BAND_WIDTH, height);
            }

            // the holes move the same way as the posters
            float hole_shift = _scroll % HOLE_GAP;
            using (SolidBrush hole = new SolidBrush(Color.FromArgb(34, 34, 40)))
            {
                for (int hole_index = -1; hole_index * HOLE_GAP < height + HOLE_GAP; hole_index++)
                {
                    float hole_y = goes_up ? (hole_index * HOLE_GAP) - hole_shift : (hole_index * HOLE_GAP) + hole_shift;
                    g.FillRectangle(hole, band_x + 7, hole_y, 9, 14);
                    g.FillRectangle(hole, band_x + BAND_WIDTH - 16, hole_y, 9, 14);
                }
            }

            int rows = (height / CELL_HEIGHT) + 3;
            for (int row = 0; row < rows; row++)
            {
                int poster_index;
                float poster_y;

                if (goes_up)
                {
                    poster_index = WrapIndex(step + row, count);
                    poster_y = (row * CELL_HEIGHT) - frac;
                }
                else
                {
                    poster_index = WrapIndex(-step + row + (count / 2), count);
                    poster_y = (row * CELL_HEIGHT) - CELL_HEIGHT + frac;
                }

                g.DrawImage(_posters[poster_index], band_x + ((BAND_WIDTH - POSTER_WIDTH) / 2f), poster_y,
                    POSTER_WIDTH, POSTER_HEIGHT);
            }

            // the posters are dimmed so the card stays the brightest thing in the window
            using (SolidBrush dim = new SolidBrush(Color.FromArgb(135, UiTheme.BACKGROUND)))
            {
                g.FillRectangle(dim, band_x, 0, BAND_WIDTH, height);
            }

            DrawFade(g, strip_x, 0, Color.FromArgb(255, UiTheme.BACKGROUND), Color.FromArgb(0, UiTheme.BACKGROUND));
            DrawFade(g, strip_x, height - FADE_HEIGHT, Color.FromArgb(0, UiTheme.BACKGROUND), Color.FromArgb(255, UiTheme.BACKGROUND));
        }

        // a band that fades from one colour to the other, so the posters appear and vanish softly
        private static void DrawFade(Graphics g, int strip_x, int top, Color from, Color to)
        {
            Rectangle area = new Rectangle(strip_x, top, STRIP_WIDTH, FADE_HEIGHT);
            using (LinearGradientBrush brush = new LinearGradientBrush(area, from, to, LinearGradientMode.Vertical))
            {
                g.FillRectangle(brush, area);
            }
        }

        // keeps an index inside 0..count-1, also for negative numbers
        private static int WrapIndex(int index, int count)
        {
            return ((index % count) + count) % count;
        }

        // reads the poster pictures next to the program into small rounded pictures, the placeholder is left out.
        // when there are none (or none can be read) simple red tiles are used so the strips still show
        private static List<Bitmap> LoadPosters()
        {
            List<Bitmap> posters = new List<Bitmap>();

            try
            {
                string folder = Path.Combine(AppContext.BaseDirectory, POSTER_FOLDER);
                if (Directory.Exists(folder))
                {
                    string[] files = Directory.GetFiles(folder);
                    Array.Sort(files, StringComparer.OrdinalIgnoreCase);

                    foreach (string file in files)
                    {
                        string extension = Path.GetExtension(file).ToLowerInvariant();
                        bool is_picture = extension == ".jpg" || extension == ".jpeg" || extension == ".png";
                        bool is_placeholder = Path.GetFileNameWithoutExtension(file).Equals(PLACEHOLDER_NAME, StringComparison.OrdinalIgnoreCase);

                        if (!is_picture || is_placeholder || posters.Count >= MAX_POSTERS)
                        {
                            continue;
                        }

                        try
                        {
                            posters.Add(MakePosterTile(file));
                        }
                        catch (Exception ex) when (ex is ArgumentException || ex is OutOfMemoryException || ex is IOException)
                        {
                            // a picture that cannot be read is skipped
                        }
                    }
                }
            }
            catch (Exception ex) when (ex is IOException || ex is UnauthorizedAccessException)
            {
                // no posters folder to read, the red tiles below are used
            }

            if (posters.Count == 0)
            {
                for (int tile = 0; tile < 6; tile++)
                {
                    posters.Add(MakeColourTile(ColorMath.Blend(UiTheme.ACCENT_RED_DARK, UiTheme.BACKGROUND, tile / 7.0)));
                }
            }

            return posters;
        }

        // one picture cut to the poster shape (the middle part is kept) with rounded corners
        private static Bitmap MakePosterTile(string file)
        {
            using (Image source = Image.FromFile(file))
            {
                Bitmap tile = new Bitmap(POSTER_WIDTH, POSTER_HEIGHT);
                using (Graphics g = Graphics.FromImage(tile))
                using (GraphicsPath shape = GlowPainter.RoundedPath(new RectangleF(0, 0, POSTER_WIDTH, POSTER_HEIGHT), POSTER_CORNER))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    g.InterpolationMode = InterpolationMode.HighQualityBicubic;
                    g.SetClip(shape);

                    double scale = Math.Max((double)POSTER_WIDTH / source.Width, (double)POSTER_HEIGHT / source.Height);
                    int source_width = (int)(POSTER_WIDTH / scale);
                    int source_height = (int)(POSTER_HEIGHT / scale);
                    int source_x = (source.Width - source_width) / 2;
                    int source_y = (source.Height - source_height) / 2;

                    g.DrawImage(source, new Rectangle(0, 0, POSTER_WIDTH, POSTER_HEIGHT),
                        source_x, source_y, source_width, source_height, GraphicsUnit.Pixel);
                }

                return tile;
            }
        }

        // a plain rounded tile, used when no poster pictures are available
        private static Bitmap MakeColourTile(Color colour)
        {
            Bitmap tile = new Bitmap(POSTER_WIDTH, POSTER_HEIGHT);
            using (Graphics g = Graphics.FromImage(tile))
            using (GraphicsPath shape = GlowPainter.RoundedPath(new RectangleF(0, 0, POSTER_WIDTH, POSTER_HEIGHT), POSTER_CORNER))
            using (SolidBrush brush = new SolidBrush(colour))
            {
                g.SmoothingMode = SmoothingMode.AntiAlias;
                g.FillPath(brush, shape);
            }

            return tile;
        }

        // the window has no title bar, so a drag on any empty part of it moves it
        protected override void WndProc(ref Message m)
        {
            base.WndProc(ref m);

            if (m.Msg == WM_NCHITTEST && (int)m.Result == HTCLIENT)
            {
                m.Result = (IntPtr)HTCAPTION;
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _timer.Stop();
                _timer.Dispose();

                foreach (Bitmap poster in _posters)
                {
                    poster.Dispose();
                }
            }

            base.Dispose(disposing);
        }

        // the rounded card with the glowing CINEMA name at the top and a line above the staff entry
        private class LandingCard : RoundedPanel
        {
            private const float LOGO_SIZE = 48f;
            private static readonly int[] GLOW_WIDTHS = { 14, 9, 5 };
            private const int GLOW_ALPHA = 34;

            protected override void OnPaint(PaintEventArgs e)
            {
                base.OnPaint(e);
                e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;

                using (GraphicsPath letters = new GraphicsPath())
                using (StringFormat centred = new StringFormat(StringFormat.GenericTypographic))
                {
                    centred.Alignment = StringAlignment.Center;
                    letters.AddString("CINEMA", UiTheme.TITLE_FONT.FontFamily, (int)FontStyle.Bold, LOGO_SIZE,
                        new RectangleF(0, 32, Width, 64), centred);

                    foreach (int glow_width in GLOW_WIDTHS)
                    {
                        using (Pen glow_pen = new Pen(Color.FromArgb(GLOW_ALPHA, UiTheme.ACCENT_RED), glow_width))
                        {
                            glow_pen.LineJoin = LineJoin.Round;
                            e.Graphics.DrawPath(glow_pen, letters);
                        }
                    }

                    using (SolidBrush letter_brush = new SolidBrush(UiTheme.ACCENT_RED))
                    {
                        e.Graphics.FillPath(letter_brush, letters);
                    }
                }

                using (Pen line = new Pen(Color.FromArgb(50, 255, 255, 255), 1))
                {
                    e.Graphics.DrawLine(line, 60, 386, Width - 60, 386);
                }
            }
        }
    }
}
