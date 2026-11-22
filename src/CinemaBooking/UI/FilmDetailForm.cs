// the film detail page: poster, the title in a marquee frame, film details and the showings of one film (owner: divyesh)
// the page takes a faint tint from the poster's main colour, so each film feels a little different.
// it is a page shown inside the main window. it only shows what it is given, the showings come from the ShowingService
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
        private const int TEXT_WIDTH = 620;
        private const int SHOWING_ROW_WIDTH = 600;
        private const int SHOWING_ROW_HEIGHT = 72;

        // how much of the poster colour goes into the page, small so the text stays easy to read
        private const double POSTER_TINT = 0.12;

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

        // poster on the left, the marquee title, details and showings on the right
        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            Padding = new Padding(PAGE_PADDING);
            AutoScroll = true;

            Button btnBack = FormParts.MakeButton("< Back to films", UiTheme.CARD);
            btnBack.SetBounds(PAGE_PADDING - UiTheme.GLOW_SPREAD, PAGE_PADDING - UiTheme.GLOW_SPREAD, 150, 42);
            btnBack.Click += (sender, e) => _go_back();

            PictureBox picPoster = new PictureBox();
            picPoster.SetBounds(PAGE_PADDING, 56, POSTER_WIDTH, POSTER_HEIGHT);
            picPoster.SizeMode = PictureBoxSizeMode.Zoom;
            picPoster.BackColor = UiTheme.BACKGROUND;

            Image? poster = FilmBrowseForm.LoadPoster(_film.PosterFile);
            if (poster != null)
            {
                picPoster.Image = poster;

                // the page takes a little of the poster's colour
                Bitmap? poster_bitmap = poster as Bitmap;
                if (poster_bitmap != null)
                {
                    BackColor = ColorMath.Blend(UiTheme.PANEL, ColorMath.Average(poster_bitmap), POSTER_TINT);
                }
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

            MarqueeTitle marqueeTitle = new MarqueeTitle();
            marqueeTitle.Text = _film.Title;
            marqueeTitle.SetBounds(text_left - 8, 48, TEXT_WIDTH, 70);

            // age 0 means anyone can watch it
            string age_text = _film.AgeRating == 0 ? "all ages" : "age " + _film.AgeRating + "+";

            Label lblFacts = new Label();
            lblFacts.ForeColor = UiTheme.TEXT_MUTED;
            lblFacts.Text = _film.Genre + " · " + _film.Year + " · " + _film.DurationMinutes + " min · " + age_text;
            lblFacts.SetBounds(text_left, 126, TEXT_WIDTH, 24);

            Label lblDescription = new Label();
            lblDescription.Text = _film.Description;
            lblDescription.SetBounds(text_left, 154, TEXT_WIDTH, 80);

            Label lblShowingsTitle = new Label();
            lblShowingsTitle.Font = UiTheme.CARD_TITLE_FONT;
            lblShowingsTitle.Text = "Showings";
            lblShowingsTitle.SetBounds(text_left, 240, 300, 26);

            flpShowings.FlowDirection = FlowDirection.TopDown;
            flpShowings.WrapContents = false;
            flpShowings.AutoScroll = true;
            flpShowings.BackColor = BackColor;
            flpShowings.SetBounds(text_left, 270, TEXT_WIDTH, 240);

            Controls.Add(btnBack);
            Controls.Add(picPoster);
            Controls.Add(marqueeTitle);
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
                lblNone.Text = "No showings for this film yet. Staff add them on the Showings page.";
                flpShowings.Controls.Add(lblNone);
            }

            flpShowings.ResumeLayout();
        }

        // one row: the showing text, how soon it starts, how full it is, and a button that opens the seat map
        private RoundedPanel BuildShowingRow(Showing showing)
        {
            RoundedPanel pnlRow = new RoundedPanel();
            pnlRow.BackColor = UiTheme.CARD;
            pnlRow.Size = new Size(SHOWING_ROW_WIDTH, SHOWING_ROW_HEIGHT);
            pnlRow.Margin = new Padding(0, 0, 0, 8);

            Label lblText = new Label();
            lblText.Text = ShowingLabel.Describe(showing);
            lblText.AutoEllipsis = true;
            lblText.BackColor = Color.Transparent;
            lblText.SetBounds(14, 8, 400, 24);

            // only showings later today get a countdown
            Label lblStartsIn = new Label();
            lblStartsIn.Text = ShowingTimes.StartsIn(showing.StartsAt, DateTime.Now);
            lblStartsIn.Font = UiTheme.SMALL_FONT;
            lblStartsIn.ForeColor = UiTheme.AMBER;
            lblStartsIn.BackColor = Color.Transparent;
            lblStartsIn.SetBounds(14, 33, 300, 18);

            FillBar fillBar = new FillBar(ShowingTimes.FillFraction(showing));
            fillBar.SetBounds(14, 56, 400, 5);

            Button btnChoose = FormParts.MakeButton("Choose seats", UiTheme.ACCENT_RED);
            btnChoose.SetBounds(SHOWING_ROW_WIDTH - 172, 12, 160, 48);
            btnChoose.Click += (sender, e) => _open_seat_map(showing);

            pnlRow.Controls.Add(lblText);
            pnlRow.Controls.Add(lblStartsIn);
            pnlRow.Controls.Add(fillBar);
            pnlRow.Controls.Add(btnChoose);
            return pnlRow;
        }
    }
}
