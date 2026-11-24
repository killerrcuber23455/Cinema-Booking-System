// the film detail page: the film banner at the top, then the showings of one film as cards (owner: divyesh)
// UI upgrade step 3: the page fills the window from edge to edge and grows with it, in the same style as the films page.
// it is a page shown inside the main window. it only shows what it is given, the showings come from the ShowingService
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public class FilmDetailForm : UserControl, IFullBleedPage
    {
        // sizes at the small step, they grow with the window step
        private const int BASE_BACK_BAR_HEIGHT = 60;
        private const int BASE_BOTTOM_ROOM = 48;

        private readonly Film _film;

        // called when the user clicks the back button, the main window decides what to show next
        private readonly Action _go_back;

        // called with the showing the user chose, the main window opens the seat map
        private readonly Action<Showing> _open_seat_map;

        private readonly FilmBrowseForm.SmoothScrollPanel pnlScroll = new FilmBrowseForm.SmoothScrollPanel();
        private readonly Panel pnlBackBar = new Panel();
        private readonly ChipButton btnBack = new ChipButton("Back to films");
        private readonly FilmHero _hero;
        private readonly Panel pnlShowings = new Panel();
        private readonly Label lblHeading = new Label();
        private readonly Label lblCount = new Label();
        private readonly Label lblNone = new Label();
        private readonly List<ShowingCard> _cards = new List<ShowingCard>();
        private Image? _poster;
        private float _step = -1f;

        public FilmDetailForm(Film film, Action go_back, Action<Showing> open_seat_map)
        {
            _film = film;
            _go_back = go_back;
            _open_seat_map = open_seat_map;

            // the banner reads the poster once, the same picture is freed when the page closes
            _poster = FilmBrowseForm.LoadPoster(film.PosterFile);
            _hero = new FilmHero(PosterFor, ScrollToShowings);

            DoubleBuffered = true;
            BuildPage();
            ShowShowings(new Showing[0]);
            UpdateStep();
        }

        private Image? PosterFor(Film film)
        {
            return _poster;
        }

        // the back button on top, the banner under it and the showings under the banner
        private void BuildPage()
        {
            BackColor = UiTheme.BACKGROUND;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;

            pnlScroll.Dock = DockStyle.Fill;
            pnlScroll.AutoScroll = true;
            pnlScroll.BackColor = UiTheme.BACKGROUND;
            DarkScroll.Apply(pnlScroll);

            btnBack.Click += (sender, e) => _go_back();
            pnlBackBar.Dock = DockStyle.Top;
            pnlBackBar.BackColor = UiTheme.BACKGROUND;
            pnlBackBar.Controls.Add(btnBack);

            _hero.Dock = DockStyle.Top;
            _hero.SetFilms(new Film[] { _film });

            pnlShowings.Dock = DockStyle.Top;
            pnlShowings.BackColor = UiTheme.BACKGROUND;
            pnlShowings.Resize += (sender, e) => LayoutShowings();

            lblHeading.Text = "Showings";
            lblHeading.AutoSize = true;
            lblHeading.ForeColor = UiTheme.TEXT;
            lblHeading.BackColor = UiTheme.BACKGROUND;

            lblCount.AutoSize = true;
            lblCount.ForeColor = UiTheme.TEXT_FAINT;
            lblCount.BackColor = UiTheme.BACKGROUND;

            lblNone.ForeColor = UiTheme.TEXT_MUTED;
            lblNone.BackColor = UiTheme.BACKGROUND;
            lblNone.Text = "No showings for this film yet. Staff add them on the Showings page.";
            lblNone.Visible = false;

            pnlShowings.Controls.Add(lblHeading);
            pnlShowings.Controls.Add(lblCount);
            pnlShowings.Controls.Add(lblNone);

            // docked to the top in this order: the first added is placed last
            pnlScroll.Controls.Add(pnlShowings);
            pnlScroll.Controls.Add(_hero);
            pnlScroll.Controls.Add(pnlBackBar);
            Controls.Add(pnlScroll);
        }

        // the button of the banner scrolls down to the showings
        private void ScrollToShowings(Film film)
        {
            pnlScroll.ScrollControlIntoView(pnlShowings);
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
            btnBack.SetStep(step);

            lblHeading.Font = FontCache.Scaled(FontCache.DISPLAY, UiTheme.SIZE_TITLE, step);
            lblCount.Font = FontCache.Scaled(FontCache.MEDIUM, UiTheme.SIZE_BODY, step);
            lblNone.Font = FontCache.Scaled(FontCache.REGULAR, UiTheme.SIZE_BODY, step);
            pnlBackBar.Height = UiScale.Pixels(BASE_BACK_BAR_HEIGHT, step);

            foreach (ShowingCard card in _cards)
            {
                card.ApplyStep(step);
            }

            LayoutBackBar();
            LayoutShowings();
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

        private void LayoutBackBar()
        {
            float step = Math.Max(UiScale.SMALL_STEP, _step);
            btnBack.Location = new Point(UiScale.Pixels(UiTheme.PAGE_SIDE_PADDING, step),
                (pnlBackBar.Height - btnBack.Height) / 2);
        }

        // puts the heading and one card under the other, as wide as the page without the side space
        private void LayoutShowings()
        {
            float step = Math.Max(UiScale.SMALL_STEP, _step);
            int side = UiScale.Pixels(UiTheme.PAGE_SIDE_PADDING, step);
            int width = pnlShowings.Width;

            if (width <= 0)
            {
                return;
            }

            int top = UiScale.Pixels(UiTheme.SPACE_XL, step);
            lblHeading.Location = new Point(side - 2, top);
            lblCount.Location = new Point(lblHeading.Right + UiScale.Pixels(UiTheme.SPACE_M, step),
                lblHeading.Bottom - lblCount.Height - UiScale.Pixels(3, step));

            int y = lblHeading.Bottom + UiScale.Pixels(UiTheme.SPACE_L, step);
            int card_width = Math.Max(200, width - (2 * side));

            if (_cards.Count == 0)
            {
                lblNone.SetBounds(side, y, card_width, UiScale.Pixels(30, step));
                y = lblNone.Bottom;
            }

            foreach (ShowingCard card in _cards)
            {
                card.SetBounds(side, y, card_width, card.Height);
                y = card.Bottom + UiScale.Pixels(UiTheme.SPACE_M, step);
            }

            int height = y + UiScale.Pixels(BASE_BOTTOM_ROOM, step);
            if (pnlShowings.Height != height)
            {
                pnlShowings.Height = height;
            }
        }

        // draws one card for each showing, the main window gives them in time order
        public void ShowShowings(Showing[] showings)
        {
            pnlShowings.SuspendLayout();

            foreach (ShowingCard old_card in _cards)
            {
                pnlShowings.Controls.Remove(old_card);
                old_card.Dispose();
            }

            _cards.Clear();

            for (int showing_index = 0; showing_index < showings.Length; showing_index++)
            {
                ShowingCard card = new ShowingCard(showings[showing_index], _open_seat_map);

                if (_step > 0)
                {
                    card.ApplyStep(_step);
                }

                _cards.Add(card);
                pnlShowings.Controls.Add(card);
            }

            lblNone.Visible = showings.Length == 0;
            lblCount.Text = showings.Length + (showings.Length == 1 ? " showing" : " showings");
            pnlShowings.ResumeLayout();
            LayoutShowings();
        }

        // the page frees its poster when the main window opens another page
        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _poster?.Dispose();
                _poster = null;
            }

            base.Dispose(disposing);
        }
    }
}
