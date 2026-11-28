// the film detail page: poster, film details and the list of showings for one film (owner: divyesh)
// it is a page shown inside the main window. it only shows what it is given, the showings come
// from the ShowingService (card 402 waits for Shankar's GetShowingsForFilm)
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class FilmDetailForm : UserControl
    {
        private const int POSTER_WIDTH = 260;
        private const int POSTER_HEIGHT = 380;
        private const int PAGE_PADDING = 8;
        private const int SHOWING_ROW_HEIGHT = 52;

        private readonly Film _film;

        // called when the user clicks the back button, the main window decides what to show next
        private readonly Action _go_back;

        // called with the showing the user chose, the main window opens the seat map
        private readonly Action<Showing> _open_seat_map;

        private readonly FlowLayoutPanel flpShowings = new FlowLayoutPanel();

        public FilmDetailForm(Film film, Action go_back, Action<Showing> open_seat_map)
        {
            _film = film;
            _go_back = go_back;
            _open_seat_map = open_seat_map;
            BuildPage();
            ShowShowings(new Showing[0]);
        }

        // poster on the left, details and showings on the right
        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            Padding = new Padding(PAGE_PADDING);

            Button btnBack = new Button();
            btnBack.Text = "< Back to films";
            btnBack.FlatStyle = FlatStyle.Flat;
            btnBack.FlatAppearance.BorderSize = 0;
            btnBack.BackColor = UiTheme.CARD;
            btnBack.ForeColor = UiTheme.TEXT;
            btnBack.Cursor = Cursors.Hand;
            btnBack.SetBounds(PAGE_PADDING, PAGE_PADDING, 140, 34);
            btnBack.Click += (sender, e) => _go_back();

            PictureBox picPoster = new PictureBox();
            picPoster.SetBounds(PAGE_PADDING, 56, POSTER_WIDTH, POSTER_HEIGHT);
            picPoster.SizeMode = PictureBoxSizeMode.Zoom;
            picPoster.BackColor = UiTheme.BACKGROUND;

            Image? poster = FilmBrowseForm.LoadPoster(_film.PosterFile);
            if (poster != null)
            {
                picPoster.Image = poster;
            }
            else
            {
                Label lblNoPoster = new Label();
                lblNoPoster.Dock = DockStyle.Fill;
                lblNoPoster.TextAlign = ContentAlignment.MiddleCenter;
                lblNoPoster.ForeColor = UiTheme.TEXT_MUTED;
                lblNoPoster.Text = _film.Title + "\n\n(no poster)";
                picPoster.Controls.Add(lblNoPoster);
            }

            int text_left = PAGE_PADDING + POSTER_WIDTH + 24;

            Label lblTitle = new Label();
            lblTitle.Font = UiTheme.TITLE_FONT;
            lblTitle.AutoEllipsis = true;
            lblTitle.Text = _film.Title;
            lblTitle.SetBounds(text_left, 52, 600, 40);

            // age 0 means anyone can watch it
            string age_text = _film.AgeRating == 0 ? "all ages" : "age " + _film.AgeRating + "+";

            Label lblFacts = new Label();
            lblFacts.ForeColor = UiTheme.TEXT_MUTED;
            lblFacts.Text = _film.Genre + " · " + _film.Year + " · " + _film.DurationMinutes + " min · " + age_text;
            lblFacts.SetBounds(text_left, 96, 600, 24);

            Label lblDescription = new Label();
            lblDescription.Text = _film.Description;
            lblDescription.SetBounds(text_left, 128, 600, 90);

            Label lblShowingsTitle = new Label();
            lblShowingsTitle.Font = UiTheme.CARD_TITLE_FONT;
            lblShowingsTitle.Text = "Showings";
            lblShowingsTitle.SetBounds(text_left, 226, 300, 26);

            flpShowings.FlowDirection = FlowDirection.TopDown;
            flpShowings.WrapContents = false;
            flpShowings.AutoScroll = true;
            flpShowings.BackColor = UiTheme.PANEL;
            flpShowings.SetBounds(text_left, 256, 600, 180);

            Controls.Add(btnBack);
            Controls.Add(picPoster);
            Controls.Add(lblTitle);
            Controls.Add(lblFacts);
            Controls.Add(lblDescription);
            Controls.Add(lblShowingsTitle);
            Controls.Add(flpShowings);
        }

        // draws one row for each showing, the main window gives them in time order
        public void ShowShowings(Showing[] showings)
        {
            flpShowings.SuspendLayout();
            flpShowings.Controls.Clear();

            for (int showing_index = 0; showing_index < showings.Length; showing_index++)
            {
                flpShowings.Controls.Add(BuildShowingRow(showings[showing_index]));
            }

            if (showings.Length == 0)
            {
                Label lblNone = new Label();
                lblNone.AutoSize = true;
                lblNone.ForeColor = UiTheme.TEXT_MUTED;
                lblNone.Text = "no showings for this film yet";
                flpShowings.Controls.Add(lblNone);
            }

            flpShowings.ResumeLayout();
        }

        // one row: the showing text and a button that opens the seat map
        private RoundedPanel BuildShowingRow(Showing showing)
        {
            RoundedPanel pnlRow = new RoundedPanel();
            pnlRow.BackColor = UiTheme.CARD;
            pnlRow.Size = new Size(580, SHOWING_ROW_HEIGHT);
            pnlRow.Margin = new Padding(0, 0, 0, 8);

            Label lblText = new Label();
            lblText.Text = ShowingLabel.Describe(showing);
            lblText.TextAlign = ContentAlignment.MiddleLeft;
            lblText.SetBounds(12, 0, 400, SHOWING_ROW_HEIGHT);

            Button btnChoose = new Button();
            btnChoose.Text = "Choose seats";
            btnChoose.FlatStyle = FlatStyle.Flat;
            btnChoose.FlatAppearance.BorderSize = 0;
            btnChoose.BackColor = UiTheme.ACCENT_RED;
            btnChoose.ForeColor = UiTheme.TEXT;
            btnChoose.Cursor = Cursors.Hand;
            btnChoose.SetBounds(424, 9, 144, 34);
            btnChoose.Click += (sender, e) => _open_seat_map(showing);

            pnlRow.Controls.Add(lblText);
            pnlRow.Controls.Add(btnChoose);
            return pnlRow;
        }
    }
}
