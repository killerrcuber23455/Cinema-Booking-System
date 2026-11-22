// the admin films page: a list of every film, boxes to add a film and a button to remove the chosen film (owner: divyesh)
// it is a page shown inside the main window. it only reads the boxes and shows messages, reading the numbers is
// done by FilmInput and the add and remove rules (including the admin check) are in FilmService
using System;
using System.Drawing;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class AdminFilmsForm : UserControl
    {
        private const int PAGE_PADDING = 8;
        private const int LIST_WIDTH = 300;
        private const int FORM_LEFT = 330;
        private const int LABEL_WIDTH = 110;
        private const int BOX_LEFT = FORM_LEFT + LABEL_WIDTH;
        private const int BOX_WIDTH = 300;
        private const int ROW_HEIGHT = 36;

        private readonly FilmService _film_service;

        // the role of the person using the page, FilmService refuses add and remove unless it is Admin
        private readonly Role _role;

        private readonly ListBox lstFilms = new ListBox();
        private readonly TextBox txtId = new TextBox();
        private readonly TextBox txtTitle = new TextBox();
        private readonly TextBox txtGenre = new TextBox();
        private readonly TextBox txtYear = new TextBox();
        private readonly TextBox txtDuration = new TextBox();
        private readonly TextBox txtAgeRating = new TextBox();
        private readonly TextBox txtDescription = new TextBox();
        private readonly TextBox txtPoster = new TextBox();
        private readonly Label lblMessage = new Label();

        // the films shown in the list, in the same order as the list lines
        private Film[] _listed_films = new Film[0];

        public AdminFilmsForm(FilmService film_service, Role role)
        {
            _film_service = film_service;
            _role = role;

            BuildPage();
            FillList();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            // the list of films on the left
            Label lblListHeading = new Label();
            lblListHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblListHeading.Text = "Films";
            lblListHeading.SetBounds(PAGE_PADDING, PAGE_PADDING, LIST_WIDTH, 26);

            lstFilms.BackColor = UiTheme.CARD;
            lstFilms.ForeColor = UiTheme.TEXT;
            lstFilms.BorderStyle = BorderStyle.None;
            lstFilms.SetBounds(PAGE_PADDING, 40, LIST_WIDTH, 420);

            Button btnRemove = MakeButton("Remove chosen film", UiTheme.CARD);
            btnRemove.SetBounds(PAGE_PADDING, 470, LIST_WIDTH, 40);
            btnRemove.Click += (sender, e) => ClickRemove();

            Controls.Add(lblListHeading);
            Controls.Add(lstFilms);
            Controls.Add(btnRemove);

            // the boxes to add a film on the right
            Label lblFormHeading = new Label();
            lblFormHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblFormHeading.Text = "Add a film";
            lblFormHeading.SetBounds(FORM_LEFT, PAGE_PADDING, 300, 26);
            Controls.Add(lblFormHeading);

            int box_top = 40;
            AddRow("Id", txtId, box_top, 28);
            AddRow("Title", txtTitle, box_top + ROW_HEIGHT, 28);
            AddRow("Genre", txtGenre, box_top + (2 * ROW_HEIGHT), 28);
            AddRow("Year", txtYear, box_top + (3 * ROW_HEIGHT), 28);
            AddRow("Duration (min)", txtDuration, box_top + (4 * ROW_HEIGHT), 28);
            AddRow("Age rating", txtAgeRating, box_top + (5 * ROW_HEIGHT), 28);
            AddRow("Poster file", txtPoster, box_top + (6 * ROW_HEIGHT), 28);

            // the description is taller than the other boxes
            txtDescription.Multiline = true;
            AddRow("Description", txtDescription, box_top + (7 * ROW_HEIGHT), 90);

            int button_top = box_top + (7 * ROW_HEIGHT) + 104;
            Button btnAdd = MakeButton("Add film", UiTheme.ACCENT_RED);
            btnAdd.SetBounds(BOX_LEFT + BOX_WIDTH - 160, button_top, 160, 40);
            btnAdd.Click += (sender, e) => ClickAdd();
            Controls.Add(btnAdd);

            // the message sits under the add button and can have several lines, one for each wrong box
            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(FORM_LEFT, button_top + 50, BOX_LEFT + BOX_WIDTH - FORM_LEFT, 130);
            Controls.Add(lblMessage);
        }

        // one label and one box on the same row
        private void AddRow(string label_text, TextBox box, int top, int box_height)
        {
            Label lblName = new Label();
            lblName.Text = label_text;
            lblName.ForeColor = UiTheme.TEXT_MUTED;
            lblName.SetBounds(FORM_LEFT, top + 4, LABEL_WIDTH, 24);

            box.BackColor = UiTheme.INPUT;
            box.ForeColor = UiTheme.TEXT;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.SetBounds(BOX_LEFT, top, BOX_WIDTH, box_height);

            Controls.Add(lblName);
            Controls.Add(box);
        }

        private Button MakeButton(string text, Color colour)
        {
            Button button = new Button();
            button.Text = text;
            button.FlatStyle = FlatStyle.Flat;
            button.FlatAppearance.BorderSize = 0;
            button.BackColor = colour;
            button.ForeColor = UiTheme.TEXT;
            button.Cursor = Cursors.Hand;
            return button;
        }

        // shows every film in the list, one line each
        private void FillList()
        {
            _listed_films = _film_service.Search("", "");
            lstFilms.Items.Clear();

            for (int film_index = 0; film_index < _listed_films.Length; film_index++)
            {
                Film film = _listed_films[film_index];
                lstFilms.Items.Add(film.Id + "  -  " + film.Title + " (" + film.Year + ")");
            }
        }

        // the boxes are read by FilmInput, the film is added by FilmService, which also checks the role
        private void ClickAdd()
        {
            Film? film;
            string error_message;

            if (!FilmInput.TryRead(txtId.Text, txtTitle.Text, txtGenre.Text, txtYear.Text, txtDuration.Text,
                    txtAgeRating.Text, txtDescription.Text, txtPoster.Text, out film, out error_message))
            {
                ShowError(error_message);
                return;
            }

            if (!_film_service.AddFilm(film!, _role, out error_message))
            {
                ShowError(error_message);
                return;
            }

            ClearBoxes();
            FillList();
            ShowGood("Film added: " + film!.Title);
        }

        private void ClickRemove()
        {
            int chosen_index = lstFilms.SelectedIndex;

            if (chosen_index < 0)
            {
                ShowError("Choose a film in the list first");
                return;
            }

            Film film = _listed_films[chosen_index];
            DialogResult answer = MessageBox.Show("Remove " + film.Title + "?", "Remove film", MessageBoxButtons.YesNo);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            string error_message;

            if (!_film_service.RemoveFilm(film.Id, _role, out error_message))
            {
                ShowError(error_message);
                return;
            }

            FillList();
            ShowGood("Film removed: " + film.Title);
        }

        private void ClearBoxes()
        {
            txtId.Text = "";
            txtTitle.Text = "";
            txtGenre.Text = "";
            txtYear.Text = "";
            txtDuration.Text = "";
            txtAgeRating.Text = "";
            txtDescription.Text = "";
            txtPoster.Text = "";
        }

        private void ShowError(string message)
        {
            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.Text = message.TrimEnd('\n');
        }

        private void ShowGood(string message)
        {
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.Text = message;
        }
    }
}
