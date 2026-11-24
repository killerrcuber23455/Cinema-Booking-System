// the films page: a big film banner, a search box, genre and sort choices and the poster grid (owner: divyesh)
// UI upgrade step 2: the page fills the window from edge to edge and grows with it. it only reads input and shows
// results, the search lives in FilmService, the sorting in SortHelper, and the file loading in CatalogStore
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CinemaBooking.Algorithms;
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class FilmBrowseForm : UserControl, IFullBleedPage
    {
        // posters are looked for in this folder next to the program
        private const string POSTER_FOLDER = "posters";

        // how many films the banner at the top goes through
        private const int FEATURED_COUNT = 5;

        // the short names on the sort choices, in the same order as SortChoice.LABELS
        private static readonly string[] SORT_CHIP_TEXTS = { "Title", "Year", "Age rating" };

        // checks that a poster file is really in the posters folder, and gives the placeholder when it is not (card 320)
        private static readonly PosterStore POSTERS = new PosterStore(Path.Combine(AppContext.BaseDirectory, POSTER_FOLDER));

        private readonly FilmService _film_service;

        // loads the film file into the program and the database
        private readonly CatalogStore _catalog_store;

        // who is looking at the page, only staff and admin see the button that loads the film file (card 437)
        private readonly Role _role;

        // called with the film the user clicked, the main window opens its detail page (card 402)
        private readonly Action<Film> _open_film;

        // each poster is read from its file once while the page is open, the banner and the grid share it
        private readonly Dictionary<int, Image?> _posters = new Dictionary<int, Image?>();

        // the genre chosen now, empty means all genres, and the sort chosen now
        private string _genre = "";
        private string _sort_label = SortChoice.LABELS[0];
        private float _step = -1f;

        // the parts of the page, they scroll together
        private readonly SmoothScrollPanel pnlScroll = new SmoothScrollPanel();
        private readonly FilmHero _hero;
        private readonly Panel pnlTools = new Panel();
        private readonly PosterGrid _grid;
        private readonly SearchBox _search = new SearchBox("Search films by title");
        private readonly FlowLayoutPanel flpGenres = new FlowLayoutPanel();
        private readonly FlowLayoutPanel flpSort = new FlowLayoutPanel();
        private readonly Label lblHeading = new Label();
        private readonly Label lblCount = new Label();
        private readonly Label lblStatus = new Label();
        private readonly Button btnLoadFile = FormParts.MakeButton("Load film file", UiTheme.ACCENT_RED);
        private readonly List<ChipButton> _genre_chips = new List<ChipButton>();
        private readonly List<ChipButton> _sort_chips = new List<ChipButton>();

        public FilmBrowseForm(FilmService film_service, CatalogStore catalog_store, Role role, Action<Film> open_film)
        {
            _film_service = film_service;
            _catalog_store = catalog_store;
            _role = role;
            _open_film = open_film;
            _hero = new FilmHero(PosterFor, open_film);
            _grid = new PosterGrid(PosterFor, open_film);

            DoubleBuffered = true;
            BuildPage();
            FillGenres();
            _hero.SetFilms(FilmPageRules.FeaturedFilms(_film_service.Search("", ""), FEATURED_COUNT));
            ShowFilms();
            UpdateStep();
        }

        // the banner on top, the choices under it, then the posters, all in one scrolling area
        private void BuildPage()
        {
            BackColor = UiTheme.BACKGROUND;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;

            pnlScroll.Dock = DockStyle.Fill;
            pnlScroll.AutoScroll = true;
            pnlScroll.BackColor = UiTheme.BACKGROUND;
            DarkScroll.Apply(pnlScroll);

            _hero.Dock = DockStyle.Top;
            pnlTools.Dock = DockStyle.Top;
            pnlTools.BackColor = UiTheme.BACKGROUND;
            _grid.Dock = DockStyle.Top;

            _search.SearchChanged += (sender, e) => ShowFilms();

            flpGenres.AutoSize = true;
            flpGenres.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpGenres.WrapContents = true;
            flpGenres.BackColor = UiTheme.BACKGROUND;
            flpGenres.Margin = new Padding(0);

            flpSort.AutoSize = true;
            flpSort.AutoSizeMode = AutoSizeMode.GrowAndShrink;
            flpSort.WrapContents = false;
            flpSort.BackColor = UiTheme.BACKGROUND;

            for (int sort_index = 0; sort_index < SortChoice.LABELS.Length; sort_index++)
            {
                string sort_label = SortChoice.LABELS[sort_index];
                ChipButton chip = new ChipButton(SORT_CHIP_TEXTS[sort_index]);
                chip.IsSelected = sort_label == _sort_label;
                chip.Click += (sender, e) => ChooseSort(sort_label);
                _sort_chips.Add(chip);
                flpSort.Controls.Add(chip);
            }

            lblHeading.AutoSize = true;
            lblHeading.ForeColor = UiTheme.TEXT;
            lblHeading.BackColor = UiTheme.BACKGROUND;

            lblCount.AutoSize = true;
            lblCount.ForeColor = UiTheme.TEXT_FAINT;
            lblCount.BackColor = UiTheme.BACKGROUND;

            lblStatus.AutoEllipsis = true;
            lblStatus.ForeColor = UiTheme.TEXT_MUTED;
            lblStatus.BackColor = UiTheme.BACKGROUND;
            lblStatus.TextAlign = ContentAlignment.MiddleLeft;

            btnLoadFile.Click += (sender, e) => LoadFilmFile();
            btnLoadFile.Visible = FilmPageRules.CanLoadFilms(_role);

            pnlTools.Controls.Add(_search);
            pnlTools.Controls.Add(lblStatus);
            pnlTools.Controls.Add(btnLoadFile);
            pnlTools.Controls.Add(flpGenres);
            pnlTools.Controls.Add(lblHeading);
            pnlTools.Controls.Add(lblCount);
            pnlTools.Controls.Add(flpSort);
            pnlTools.Resize += (sender, e) => LayoutTools();

            // docked to the top in this order: the first added is placed last, so the grid ends up at the bottom
            pnlScroll.Controls.Add(_grid);
            pnlScroll.Controls.Add(pnlTools);
            pnlScroll.Controls.Add(_hero);
            Controls.Add(pnlScroll);
        }

        // the window crossed a size step: every part of the page grows or shrinks together
        private void UpdateStep()
        {
            Form? window = FindForm();
            float step = UiScale.StepFor(window != null ? window.ClientSize.Width : Width);

            if (step == _step)
            {
                return;
            }

            _step = step;
            _hero.SetStep(step);
            _grid.SetStep(step);
            _search.SetStep(step);

            foreach (ChipButton chip in _genre_chips)
            {
                chip.SetStep(step);
            }

            foreach (ChipButton chip in _sort_chips)
            {
                chip.SetStep(step);
            }

            lblHeading.Font = FontCache.Scaled(FontCache.DISPLAY, UiTheme.SIZE_TITLE, step);
            lblCount.Font = FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_BODY, step);
            lblStatus.Font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_SMALL, step);
            btnLoadFile.Font = FontCache.Scaled(FontCache.SEMIBOLD, UiTheme.SIZE_BODY, step);
            LayoutTools();
        }

        protected override void OnSizeChanged(EventArgs e)
        {
            base.OnSizeChanged(e);
            UpdateStep();
        }

        protected override void OnParentChanged(EventArgs e)
        {
            base.OnParentChanged(e);
            UpdateStep();
        }

        // places the search, the genres, the heading and the sort choices, and sets the height they need
        private void LayoutTools()
        {
            float step = Math.Max(UiScale.SMALL_STEP, _step);
            int side = UiScale.Pixels(UiTheme.PAGE_SIDE_PADDING, step);
            int width = pnlTools.Width;

            if (width <= 0)
            {
                return;
            }

            pnlTools.SuspendLayout();

            int top = UiScale.Pixels(UiTheme.SPACE_S, step);
            int search_width = Math.Min(UiScale.Pixels(420, step), Math.Max(160, width - (2 * side)));
            _search.SetBounds(side, top, search_width, _search.Height);

            int right_edge = width - side;
            if (btnLoadFile.Visible)
            {
                int button_width = UiScale.Pixels(160, step) + (2 * UiTheme.GLOW_SPREAD);
                int button_height = _search.Height + (2 * UiTheme.GLOW_SPREAD);
                btnLoadFile.SetBounds(right_edge - button_width + UiTheme.GLOW_SPREAD, top - UiTheme.GLOW_SPREAD, button_width, button_height);
                right_edge = btnLoadFile.Left - UiScale.Pixels(UiTheme.SPACE_L, step);
            }

            int status_left = _search.Right + UiScale.Pixels(UiTheme.SPACE_L, step);
            lblStatus.SetBounds(status_left, top, Math.Max(0, right_edge - status_left), _search.Height);

            int genres_top = _search.Bottom + UiScale.Pixels(UiTheme.SPACE_L, step);
            flpGenres.MaximumSize = new Size(Math.Max(100, width - (2 * side)), 0);
            flpGenres.Location = new Point(side, genres_top);

            int heading_top = flpGenres.Bottom + UiScale.Pixels(UiTheme.SPACE_XXL, step);
            lblHeading.Location = new Point(side - 2, heading_top);
            lblCount.Location = new Point(lblHeading.Right + UiScale.Pixels(UiTheme.SPACE_M, step),
                lblHeading.Bottom - lblCount.Height - UiScale.Pixels(3, step));

            flpSort.Location = new Point(Math.Max(lblCount.Right + UiScale.Pixels(UiTheme.SPACE_L, step), width - side - flpSort.Width),
                heading_top + ((lblHeading.Height - flpSort.Height) / 2) + UiScale.Pixels(4, step));

            int bottom = Math.Max(lblHeading.Bottom, flpSort.Bottom) + UiScale.Pixels(UiTheme.SPACE_S, step);
            pnlTools.ResumeLayout();

            if (pnlTools.Height != bottom)
            {
                pnlTools.Height = bottom;
            }
        }

        // the poster of a film, read from its file the first time it is needed
        private Image? PosterFor(Film film)
        {
            if (_posters.TryGetValue(film.Id, out Image? found))
            {
                return found;
            }

            Image? poster = LoadPoster(film.PosterFile);
            _posters[film.Id] = poster;
            return poster;
        }

        // asks the user which film file to load, so the file name is never typed in the code
        private async void LoadFilmFile()
        {
            using (OpenFileDialog dlgOpenFile = new OpenFileDialog())
            {
                dlgOpenFile.Title = "Choose the film file";
                dlgOpenFile.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";

                if (dlgOpenFile.ShowDialog(this) != DialogResult.OK)
                {
                    return;
                }

                string file_name = dlgOpenFile.FileName;

                // the database save can be slow, so it runs in the background and the page waits with a loading text
                SetLoading(true);
                CallResult<(bool Ok, string Message)> result = await BackgroundCall.RunAsync(() =>
                {
                    bool is_loaded = _catalog_store.LoadFilmFile(file_name, out string load_message);
                    return (is_loaded, load_message);
                });
                SetLoading(false);

                FillGenres();
                _hero.SetFilms(FilmPageRules.FeaturedFilms(_film_service.Search("", ""), FEATURED_COUNT));
                ShowFilms();

                bool is_good = result.IsOk && result.Value.Ok;
                lblStatus.ForeColor = is_good ? UiTheme.TEXT_MUTED : UiTheme.ACCENT_RED;
                lblStatus.Text = result.IsOk ? result.Value.Message : result.ErrorText;
            }
        }

        // while the file loads the choices and the button are switched off, so nothing reads the films half way
        private void SetLoading(bool is_loading)
        {
            btnLoadFile.Enabled = !is_loading;
            _search.SetEnabled(!is_loading);
            flpGenres.Enabled = !is_loading;
            flpSort.Enabled = !is_loading;

            if (is_loading)
            {
                lblStatus.ForeColor = UiTheme.TEXT_MUTED;
                lblStatus.Text = "Loading films...";
            }
        }

        // one chip for all genres and one for each genre we have now, the chosen genre stays chosen if it still exists
        private void FillGenres()
        {
            string[] genres = _film_service.ListGenres();
            bool is_still_there = false;

            for (int genre_index = 0; genre_index < genres.Length; genre_index++)
            {
                if (genres[genre_index] == _genre)
                {
                    is_still_there = true;
                }
            }

            if (!is_still_there)
            {
                _genre = "";
            }

            flpGenres.SuspendLayout();

            foreach (ChipButton old_chip in _genre_chips)
            {
                flpGenres.Controls.Remove(old_chip);
                old_chip.Dispose();
            }

            _genre_chips.Clear();
            AddGenreChip("All", "");

            for (int genre_index = 0; genre_index < genres.Length; genre_index++)
            {
                AddGenreChip(genres[genre_index], genres[genre_index]);
            }

            flpGenres.ResumeLayout();
            LayoutTools();
        }

        private void AddGenreChip(string text, string genre)
        {
            ChipButton chip = new ChipButton(text);
            chip.IsSelected = genre == _genre;
            chip.Tag = genre;

            if (_step > 0)
            {
                chip.SetStep(_step);
            }

            chip.Click += (sender, e) => ChooseGenre(genre);
            _genre_chips.Add(chip);
            flpGenres.Controls.Add(chip);
        }

        private void ChooseGenre(string genre)
        {
            _genre = genre;

            foreach (ChipButton chip in _genre_chips)
            {
                chip.IsSelected = (chip.Tag as string) == genre;
            }

            ShowFilms();
        }

        private void ChooseSort(string sort_label)
        {
            _sort_label = sort_label;

            for (int sort_index = 0; sort_index < _sort_chips.Count; sort_index++)
            {
                _sort_chips[sort_index].IsSelected = SortChoice.LABELS[sort_index] == sort_label;
            }

            ShowFilms();
        }

        // asks FilmService for the films that match and gives them to the grid
        private void ShowFilms()
        {
            Film[] found_films = _film_service.Search(_search.SearchText, _genre);

            // sort what was found, with the merge sort from SortHelper
            Film[] films = SortHelper.MergeSort(found_films, SortChoice.KeyFor(_sort_label));

            bool is_searching = _search.SearchText.Trim() != "" || _genre != "";
            string empty_text = is_searching ? "No film matches your search. Try another title or genre." : FilmPageRules.EmptyFilmsText(_role);
            _grid.SetFilms(films, empty_text);

            if (_search.SearchText.Trim() != "")
            {
                lblHeading.Text = "Results";
            }
            else
            {
                lblHeading.Text = _genre == "" ? "All films" : _genre;
            }

            lblCount.Text = films.Length + (films.Length == 1 ? " film" : " films");
            lblStatus.ForeColor = UiTheme.TEXT_MUTED;
            lblStatus.Text = "";
            LayoutTools();
        }

        // reads the poster image, the poster store gives the placeholder file when the film has no poster
        // null is only returned when even the placeholder is missing, then the card shows an empty frame
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

        // the page frees its posters when the main window opens another page
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                foreach (Image? poster in _posters.Values)
                {
                    poster?.Dispose();
                }

                _posters.Clear();
            }

            base.Dispose(disposing);
        }

        // a scrolling area that draws off screen first, so the page does not flicker while it scrolls
        internal class SmoothScrollPanel : Panel
        {
            public SmoothScrollPanel()
            {
                DoubleBuffered = true;
            }
        }
    }
}
