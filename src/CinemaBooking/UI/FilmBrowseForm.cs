// the films page: poster cards, a title search, a genre filter and loading the film file (owner: divyesh)
// it is a page shown inside the main window, not a separate window. it only reads input and shows
// results, the search and the file loading live in FilmService
using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class FilmBrowseForm : UserControl
    {
        // the first choice in the genre filter, it means no filter
        private const string ALL_GENRES = "All genres";

        // posters are looked for in this folder next to the program
        private const string POSTER_FOLDER = "posters";

        // card sizes in pixels, kept here so the cards all match
        private const int CARD_WIDTH = 190;
        private const int CARD_HEIGHT = 320;
        private const int POSTER_HEIGHT = 230;
        private const int CARD_PADDING = 10;
        private const int TOP_BAR_HEIGHT = 56;

        // checks that a poster file is really in the posters folder, and gives the placeholder when it is not (card 320)
        private static readonly PosterStore POSTERS = new PosterStore(Path.Combine(AppContext.BaseDirectory, POSTER_FOLDER));

        private readonly FilmService _film_service;

        // who is looking at the page, only staff and admin see the button that loads the film file (card 437)
        private readonly Role _role;

        // called with the film the user clicked, the main window opens its detail page (card 402)
        private readonly Action<Film> _open_film;

        // stops the genre filter from searching while we refill it
        private bool _is_filling_genres;

        // the parts of the page
        private readonly Panel pnlTopBar = new Panel();
        private readonly TextBox txtSearch = new TextBox();
        private readonly ComboBox cmbGenre = new ComboBox();
        private readonly Button btnLoadFile = FormParts.MakeButton("Load film file", UiTheme.ACCENT_RED);
        private readonly Label lblStatus = new Label();
        private readonly FlowLayoutPanel flpCards = new SmoothFlowPanel();
        public FilmBrowseForm(FilmService film_service, Role role, Action<Film> open_film)
        {
            _film_service = film_service;
            _role = role;
            _open_film = open_film;
            DoubleBuffered = true;
            BuildPage();
            FillGenres();
            ShowFilms();
        }

        // places the search bar on top and the card area under it
        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;

            pnlTopBar.Dock = DockStyle.Top;
            pnlTopBar.Height = TOP_BAR_HEIGHT;
            pnlTopBar.BackColor = UiTheme.PANEL;

            txtSearch.PlaceholderText = "Search by title";
            txtSearch.BorderStyle = BorderStyle.FixedSingle;
            txtSearch.BackColor = UiTheme.INPUT;
            txtSearch.ForeColor = UiTheme.TEXT;
            txtSearch.SetBounds(0, 12, 260, 30);
            // the list updates while the user types, there is no search button to forget
            txtSearch.TextChanged += (sender, e) => ShowFilms();

            cmbGenre.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbGenre.FlatStyle = FlatStyle.Flat;
            cmbGenre.BackColor = UiTheme.INPUT;
            cmbGenre.ForeColor = UiTheme.TEXT;
            cmbGenre.SetBounds(272, 12, 180, 30);
            cmbGenre.SelectedIndexChanged += (sender, e) =>
            {
                if (!_is_filling_genres)
                {
                    ShowFilms();
                }
            };

            // the button keeps a few pixels around it for its glow
            btnLoadFile.SetBounds(464 - UiTheme.GLOW_SPREAD, 10 - UiTheme.GLOW_SPREAD, 160 + (2 * UiTheme.GLOW_SPREAD),
                34 + (2 * UiTheme.GLOW_SPREAD));
            btnLoadFile.Click += (sender, e) => LoadFilmFile();
            btnLoadFile.Visible = FilmPageRules.CanLoadFilms(_role);

            lblStatus.ForeColor = UiTheme.TEXT_MUTED;
            lblStatus.Font = UiTheme.SMALL_FONT;
            lblStatus.AutoEllipsis = true;
            // without the button the status text moves left, so there is no gap
            int status_left = FilmPageRules.CanLoadFilms(_role) ? 636 : 464;
            lblStatus.SetBounds(status_left, 17, 300, 24);
            lblStatus.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;

            pnlTopBar.Controls.Add(txtSearch);
            pnlTopBar.Controls.Add(cmbGenre);
            pnlTopBar.Controls.Add(btnLoadFile);
            pnlTopBar.Controls.Add(lblStatus);

            // the search box glows while the user types in it
            FocusGlow.AddTo(pnlTopBar, txtSearch);

            flpCards.Dock = DockStyle.Fill;
            flpCards.AutoScroll = true;
            flpCards.BackColor = UiTheme.PANEL;
            flpCards.Padding = new Padding(0, 8, 0, 0);

            // fill goes in first, then the top bar, same order as the main window
            Controls.Add(flpCards);
            Controls.Add(pnlTopBar);
        }

        // asks the user which film file to load, so the file name is never typed in the code
        private void LoadFilmFile()
        {
            using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
            {
                dlgOpenFile.Title = "Choose the film file";
                dlgOpenFile.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

                if (dlgOpenFile.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                bool is_loaded = _film_service.LoadFromFile(dlgOpenFile.FileName, out string message);

                FillGenres();
                ShowFilms();

                // the load message is shown last so the film count does not hide it
                lblStatus.ForeColor = is_loaded ? UiTheme.TEXT_MUTED : UiTheme.ACCENT_RED;
                lblStatus.Text = message;
            }
        }

        // refills the genre filter from the films we have now, and keeps the choice if it still exists
        private void FillGenres()
        {
            string? chosen_genre = cmbGenre.SelectedItem as string;
            string[] genres = _film_service.ListGenres();

            _is_filling_genres = true;
            cmbGenre.Items.Clear();
            cmbGenre.Items.Add(ALL_GENRES);

            for (int genre_index = 0; genre_index < genres.Length; genre_index++)
            {
                cmbGenre.Items.Add(genres[genre_index]);
            }

            int chosen_index = chosen_genre == null ? -1 : cmbGenre.Items.IndexOf(chosen_genre);
            cmbGenre.SelectedIndex = chosen_index >= 0 ? chosen_index : 0;
            _is_filling_genres = false;
        }

        // asks FilmService for the films that match and draws one card for each
        private void ShowFilms()
        {
            string? chosen_genre = cmbGenre.SelectedItem as string;
            string genre_filter = chosen_genre == null || chosen_genre == ALL_GENRES ? "" : chosen_genre;
            Film[] films = _film_service.Search(txtSearch.Text, genre_filter);

            // suspend layout so the page is drawn once, not once per card
            flpCards.SuspendLayout();
            ClearCards();

            for (int film_index = 0; film_index < films.Length; film_index++)
            {
                flpCards.Controls.Add(BuildCard(films[film_index]));
            }

            if (films.Length == 0)
            {
                flpCards.Controls.Add(BuildEmptyMessage(genre_filter));
            }

            flpCards.ResumeLayout();

            lblStatus.ForeColor = UiTheme.TEXT_MUTED;
            lblStatus.Text = "showing " + films.Length + (films.Length == 1 ? " film" : " films");
        }

        // tells the user why the page is empty and what to do next
        private Label BuildEmptyMessage(string genre_filter)
        {
            Label lblEmpty = new Label();
            lblEmpty.AutoSize = true;
            lblEmpty.ForeColor = UiTheme.TEXT_MUTED;
            lblEmpty.Margin = new Padding(0, 20, 0, 0);

            bool is_searching = txtSearch.Text.Trim() != "" || genre_filter != "";
            lblEmpty.Text = is_searching
                ? "no film matches your search"
                : FilmPageRules.EmptyFilmsText(_role);
            return lblEmpty;
        }

        // one card: poster on top, then title, genre and year, length and age
        private RoundedPanel BuildCard(Film film)
        {
            RoundedPanel pnlCard = new RoundedPanel();
            pnlCard.BackColor = UiTheme.CARD;
            pnlCard.Size = new Size(CARD_WIDTH, CARD_HEIGHT);
            pnlCard.Margin = new Padding(0, 0, 14, 14);
            // the film is kept on the card so the detail window (card 402) knows which film was clicked
            pnlCard.Tag = film;

            int inner_width = CARD_WIDTH - (CARD_PADDING * 2);

            PictureBox picPoster = new PictureBox();
            picPoster.SetBounds(CARD_PADDING, CARD_PADDING, inner_width, POSTER_HEIGHT);
            picPoster.SizeMode = PictureBoxSizeMode.Zoom;
            picPoster.BackColor = UiTheme.BACKGROUND;

            Image? poster = LoadPoster(film.PosterFile);
            if (poster != null)
            {
                picPoster.Image = poster;
            }
            else
            {
                // no poster file, so the title stands in for it
                Label lblNoPoster = new Label();
                lblNoPoster.Dock = DockStyle.Fill;
                lblNoPoster.TextAlign = ContentAlignment.MiddleCenter;
                lblNoPoster.ForeColor = UiTheme.TEXT_MUTED;
                lblNoPoster.Text = film.Title + "\n\n(no poster)";
                picPoster.Controls.Add(lblNoPoster);
            }

            int text_top = CARD_PADDING + POSTER_HEIGHT + 8;

            Label lblTitle = new Label();
            lblTitle.Font = UiTheme.CARD_TITLE_FONT;
            lblTitle.ForeColor = UiTheme.TEXT;
            lblTitle.BackColor = Color.Transparent;
            lblTitle.AutoEllipsis = true;
            lblTitle.Text = film.Title;
            lblTitle.SetBounds(CARD_PADDING, text_top, inner_width, 24);

            // the genre and the age rating as small tags, the year and the length in a quiet line under them
            Pill pilGenre = new Pill(film.Genre, ColorMath.Blend(UiTheme.CARD, UiTheme.TEXT, 0.12), UiTheme.TEXT);
            pilGenre.Location = new Point(CARD_PADDING, text_top + 28);

            // age 0 means anyone can watch it, 16 and over is shown in red
            string age_text = film.AgeRating == 0 ? "all ages" : film.AgeRating + "+";
            Color age_colour = film.AgeRating >= 16 ? UiTheme.ACCENT_RED : UiTheme.AMBER;
            Pill pilAge = new Pill(age_text, ColorMath.Blend(UiTheme.CARD, age_colour, 0.22), age_colour);
            pilAge.Location = new Point(pilGenre.Right + 6, text_top + 28);

            Label lblYearLength = new Label();
            lblYearLength.Font = UiTheme.SMALL_FONT;
            lblYearLength.ForeColor = UiTheme.TEXT_MUTED;
            lblYearLength.BackColor = Color.Transparent;
            lblYearLength.Text = film.Year + " · " + film.DurationMinutes + " min";
            lblYearLength.SetBounds(CARD_PADDING, text_top + 52, inner_width, 18);

            pnlCard.Controls.Add(picPoster);
            pnlCard.Controls.Add(lblTitle);
            pnlCard.Controls.Add(pilGenre);
            pnlCard.Controls.Add(pilAge);
            pnlCard.Controls.Add(lblYearLength);

            // a click on the card or on any part of it opens the film, the parts swallow clicks otherwise
            pnlCard.Cursor = Cursors.Hand;
            pnlCard.Click += (sender, e) => _open_film(film);
            pnlCard.MouseEnter += (sender, e) => pnlCard.IsHighlighted = true;
            pnlCard.MouseLeave += (sender, e) => UpdateHighlight(pnlCard);

            for (int part_index = 0; part_index < pnlCard.Controls.Count; part_index++)
            {
                Control part = pnlCard.Controls[part_index];
                part.Cursor = Cursors.Hand;
                part.Click += (sender, e) => _open_film(film);

                // the parts cover the card, so the mouse on a part also lights the card
                part.MouseEnter += (sender, e) => pnlCard.IsHighlighted = true;
                part.MouseLeave += (sender, e) => UpdateHighlight(pnlCard);
            }

            return pnlCard;
        }

        // the mouse left the card or one of its parts: the glow goes out only if the mouse is really off the card
        private static void UpdateHighlight(RoundedPanel card)
        {
            Point mouse = card.PointToClient(Cursor.Position);
            card.IsHighlighted = card.ClientRectangle.Contains(mouse);
        }

        // reads the poster image, the poster store gives the placeholder file when the film has no poster
        // null is only returned when even the placeholder is missing, then the card shows the title
        internal static Image? LoadPoster(string poster_file)
        {
            string poster_name = POSTERS.GetPosterFile(poster_file);
            string poster_path = Path.Combine(AppContext.BaseDirectory, POSTER_FOLDER, poster_name);
            if (!File.Exists(poster_path))
            {
                return null;
            }

            try
            {
                // we copy the picture so the file is not kept locked while the program runs
                using (Image file_image = Image.FromFile(poster_path))
                {
                    return new Bitmap(file_image);
                }
            }
            catch (OutOfMemoryException)
            {
                // windows throws this when the file is not a real picture
                return null;
            }
            catch (IOException)
            {
                return null;
            }
        }

        // removes the old cards and frees their posters, so memory does not grow on every search
        private void ClearCards()
        {
            for (int card_index = flpCards.Controls.Count - 1; card_index >= 0; card_index--)
            {
                Control old_card = flpCards.Controls[card_index];
                flpCards.Controls.RemoveAt(card_index);

                for (int part_index = 0; part_index < old_card.Controls.Count; part_index++)
                {
                    PictureBox? picOld = old_card.Controls[part_index] as PictureBox;
                    if (picOld != null && picOld.Image != null)
                    {
                        picOld.Image.Dispose();
                    }
                }

                old_card.Dispose();
            }
        }

        // the page frees its posters when the main window opens another page
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                ClearCards();
            }
            base.Dispose(disposing);
        }

        // a card area that draws off screen first, so the cards do not flicker
        internal class SmoothFlowPanel : FlowLayoutPanel
        {
            public SmoothFlowPanel()
            {
                DoubleBuffered = true;
            }
        }
    }
}
