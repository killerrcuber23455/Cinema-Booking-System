// the admin films page: a list of every film, boxes to add a film and a button to remove the chosen film (owner: divyesh)
// it is a page shown inside the main window. it only reads the boxes and shows messages, reading the numbers is
// done by FilmInput and the add and remove rules (including the admin check) are in FilmService
using System;
using System.Drawing;
using System.IO;
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
        private const int BOX_HEIGHT = 28;
        private const int MESSAGE_HEIGHT = 20;
        // one row is the box, a gap of 2 pixels, the red message under it and a gap of 4 pixels
        private const int ROW_HEIGHT = BOX_HEIGHT + 2 + MESSAGE_HEIGHT + 4;
        private const int CHOOSE_WIDTH = 104;
        private const string POSTER_FOLDER = "posters";

        private readonly FilmService _film_service;
        private readonly CatalogStore _catalog_store;

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

        // the red message under each box, the place of each one is the BOX_ number in FilmInput
        private readonly Label[] _box_messages = new Label[FilmInput.BOX_COUNT];
        private readonly Label lblFormHeading = new Label();
        private Button btnAdd = null!;
        private Button btnCancelEdit = null!;
        private Button btnRemove = null!;
        private Button btnEdit = null!;

        // true while the database call of an add, edit or remove runs, so the buttons cannot be clicked twice
        private bool _is_busy;

        // the id of the film being edited, 0 means the boxes are for a new film
        private int _editing_film_id;

        // the films shown in the list, in the same order as the list lines
        private Film[] _listed_films = new Film[0];

        // shows a good news message as a toast in the main window
        private readonly Action<string> _show_toast;

        public AdminFilmsForm(FilmService film_service, CatalogStore catalog_store, Role role, Action<string> show_toast)
        {
            _film_service = film_service;
            _catalog_store = catalog_store;
            _role = role;
            _show_toast = show_toast;

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

            btnRemove = MakeButton("Remove chosen film", UiTheme.CARD);
            btnRemove.SetBounds(PAGE_PADDING, 470, LIST_WIDTH, 40);
            btnRemove.Click += (sender, e) => ClickRemove();

            btnEdit = MakeButton("Edit chosen film", UiTheme.CARD);
            btnEdit.SetBounds(PAGE_PADDING, 516, LIST_WIDTH, 40);
            btnEdit.Click += (sender, e) => ClickEdit();

            Controls.Add(lblListHeading);
            Controls.Add(lstFilms);
            Controls.Add(btnRemove);
            Controls.Add(btnEdit);

            // the boxes to add a film on the right
            lblFormHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblFormHeading.Text = "Add a film";
            lblFormHeading.SetBounds(FORM_LEFT, PAGE_PADDING, 300, 26);
            Controls.Add(lblFormHeading);

            int box_top = 40;
            AddRow("Id", txtId, box_top, BOX_HEIGHT, FilmInput.BOX_ID);
            AddRow("Title", txtTitle, box_top + ROW_HEIGHT, BOX_HEIGHT, FilmInput.BOX_TITLE);
            AddRow("Genre", txtGenre, box_top + (2 * ROW_HEIGHT), BOX_HEIGHT, FilmInput.BOX_GENRE);
            AddRow("Year", txtYear, box_top + (3 * ROW_HEIGHT), BOX_HEIGHT, FilmInput.BOX_YEAR);
            AddRow("Duration (min)", txtDuration, box_top + (4 * ROW_HEIGHT), BOX_HEIGHT, FilmInput.BOX_DURATION);
            AddRow("Age rating", txtAgeRating, box_top + (5 * ROW_HEIGHT), BOX_HEIGHT, FilmInput.BOX_AGE_RATING);
            AddRow("Poster file", txtPoster, box_top + (6 * ROW_HEIGHT), BOX_HEIGHT, FilmInput.BOX_POSTER);

            // the poster box is a bit shorter so the choose button fits on the same row
            txtPoster.Width = BOX_WIDTH - CHOOSE_WIDTH - 6;
            Button btnChoose = MakeButton("Choose...", UiTheme.CARD);
            btnChoose.SetBounds(BOX_LEFT + BOX_WIDTH - CHOOSE_WIDTH, box_top + (6 * ROW_HEIGHT), CHOOSE_WIDTH, BOX_HEIGHT);
            btnChoose.Click += (sender, e) => ClickChoosePoster();
            Controls.Add(btnChoose);

            // the description is taller than the other boxes
            txtDescription.Multiline = true;
            AddRow("Description", txtDescription, box_top + (7 * ROW_HEIGHT), 90, FilmInput.BOX_DESCRIPTION);

            int button_top = box_top + (7 * ROW_HEIGHT) + 90 + 2 + MESSAGE_HEIGHT + 10;
            btnAdd = MakeButton("Add film", UiTheme.ACCENT_RED);
            btnAdd.SetBounds(BOX_LEFT + BOX_WIDTH - 160, button_top, 160, 40);
            btnAdd.Click += (sender, e) => ClickAdd();
            Controls.Add(btnAdd);

            // only shown while a film is being edited
            btnCancelEdit = MakeButton("Cancel edit", UiTheme.CARD);
            btnCancelEdit.SetBounds(BOX_LEFT + BOX_WIDTH - 160 - 130, button_top, 120, 40);
            btnCancelEdit.Visible = false;
            btnCancelEdit.Click += (sender, e) => StopEditing();
            Controls.Add(btnCancelEdit);

            // this message sits under the add button, it is for problems that are not about one box
            // (for example a wrong role or an id that is already used). the box problems show under their box
            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(FORM_LEFT, button_top + 50, BOX_LEFT + BOX_WIDTH - FORM_LEFT, 60);
            Controls.Add(lblMessage);
        }

        // one label, one box and an empty red message under it on the same row
        private void AddRow(string label_text, TextBox box, int top, int box_height, int box_number)
        {
            Label lblName = new Label();
            lblName.Text = label_text;
            lblName.ForeColor = UiTheme.TEXT_MUTED;
            lblName.SetBounds(FORM_LEFT, top + 4, LABEL_WIDTH, 24);

            box.BackColor = UiTheme.INPUT;
            box.ForeColor = UiTheme.TEXT;
            box.BorderStyle = BorderStyle.FixedSingle;
            box.SetBounds(BOX_LEFT, top, BOX_WIDTH, box_height);

            Label lblBoxMessage = new Label();
            lblBoxMessage.ForeColor = UiTheme.ACCENT_RED;
            lblBoxMessage.Font = UiTheme.SMALL_FONT;
            lblBoxMessage.SetBounds(BOX_LEFT, top + box_height + 2, BOX_WIDTH, MESSAGE_HEIGHT);
            _box_messages[box_number] = lblBoxMessage;

            Controls.Add(lblName);
            Controls.Add(box);
            Controls.Add(lblBoxMessage);
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

        // the boxes are read by FilmInput, the film is added, edited or removed by CatalogStore, which also saves it to the database
        private async void ClickAdd()
        {
            if (_is_busy)
            {
                return;
            }

            Film? film;
            string error_message;
            lblMessage.Text = "";

            // every wrong box shows its own message under it, and nothing is saved while one is wrong
            string[] box_errors = FilmInput.CheckBoxes(txtId.Text, txtTitle.Text, txtGenre.Text, txtYear.Text,
                txtDuration.Text, txtAgeRating.Text, txtDescription.Text, txtPoster.Text);
            if (FormParts.ShowBoxMessages(_box_messages, box_errors))
            {
                return;
            }

            if (!FilmInput.TryRead(txtId.Text, txtTitle.Text, txtGenre.Text, txtYear.Text, txtDuration.Text,
                    txtAgeRating.Text, txtDescription.Text, txtPoster.Text, out film, out error_message))
            {
                ShowError(error_message);
                return;
            }

            // the database call runs on a background thread so the window does not freeze, it must not touch any control
            Film film_to_save = film!;
            bool is_edit = _editing_film_id != 0;

            SetBusy(true);
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.Text = "Saving...";

            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                string save_error;
                bool is_saved = is_edit
                    ? _catalog_store.EditFilm(film_to_save, _role, out save_error)
                    : _catalog_store.AddFilm(film_to_save, _role, out save_error);
                return (is_saved, save_error);
            });

            SetBusy(false);

            if (!result.IsOk)
            {
                ShowError(result.ErrorText);
                return;
            }

            if (!result.Value.Ok)
            {
                ShowError(result.Value.Error);
                return;
            }

            if (is_edit)
            {
                StopEditing();
                FillList();
                ShowGood("Film saved: " + film_to_save.Title);
                return;
            }

            ClearBoxes();
            FillList();
            ShowGood("Film added: " + film_to_save.Title);
        }

        // puts the chosen film into the boxes, the id cannot be changed while editing
        private void ClickEdit()
        {
            int chosen_index = lstFilms.SelectedIndex;

            if (chosen_index < 0)
            {
                ShowError("Choose a film in the list first");
                return;
            }

            Film film = _listed_films[chosen_index];

            _editing_film_id = film.Id;
            txtId.Text = film.Id.ToString();
            txtId.ReadOnly = true;
            txtTitle.Text = film.Title;
            txtGenre.Text = film.Genre;
            txtYear.Text = film.Year.ToString();
            txtDuration.Text = film.DurationMinutes.ToString();
            txtAgeRating.Text = film.AgeRating.ToString();
            txtDescription.Text = film.Description;
            txtPoster.Text = film.PosterFile;

            lblFormHeading.Text = "Edit film";
            btnAdd.Text = "Save changes";
            btnCancelEdit.Visible = true;
            lblMessage.Text = "";
            ClearBoxMessages();
        }

        // back to the add form with empty boxes
        private void StopEditing()
        {
            _editing_film_id = 0;
            txtId.ReadOnly = false;
            ClearBoxes();
            lblFormHeading.Text = "Add a film";
            btnAdd.Text = "Add film";
            btnCancelEdit.Visible = false;
        }

        // the admin picks a picture, PosterImport checks it and copies it into the posters folder
        private void ClickChoosePoster()
        {
            using OpenFileDialog dialog = new OpenFileDialog();
            dialog.Title = "Choose a poster picture";
            dialog.Filter = "Pictures (*.jpg;*.jpeg;*.png)|*.jpg;*.jpeg;*.png";

            if (dialog.ShowDialog() != DialogResult.OK)
            {
                return;
            }

            string poster_file;
            string error_message;

            if (!PosterImport.Import(dialog.FileName, Path.Combine(AppContext.BaseDirectory, POSTER_FOLDER),
                    out poster_file, out error_message))
            {
                ShowError(error_message);
                return;
            }

            lblMessage.Text = "";
            txtPoster.Text = poster_file;
        }

        private async void ClickRemove()
        {
            if (_is_busy)
            {
                return;
            }

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

            int film_id = film.Id;
            string film_title = film.Title;

            SetBusy(true);
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.Text = "Removing...";

            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                bool is_removed = _catalog_store.RemoveFilm(film_id, _role, out string remove_error);
                return (is_removed, remove_error);
            });

            SetBusy(false);

            if (!result.IsOk)
            {
                ShowError(result.ErrorText);
                return;
            }

            if (!result.Value.Ok)
            {
                ShowError(result.Value.Error);
                return;
            }

            FillList();
            ShowGood("Film removed: " + film_title);
        }

        // while a call runs the add, edit and remove buttons cannot be clicked again
        private void SetBusy(bool is_busy)
        {
            _is_busy = is_busy;
            btnAdd.Enabled = !is_busy;
            btnRemove.Enabled = !is_busy;
            btnEdit.Enabled = !is_busy;
        }

        // empties the red message under every box
        private void ClearBoxMessages()
        {
            for (int box_index = 0; box_index < _box_messages.Length; box_index++)
            {
                _box_messages[box_index].Text = "";
            }
        }

        private void ClearBoxes()
        {
            ClearBoxMessages();
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

        // a success is shown as a toast in the main window, so the red message area stays only for problems
        private void ShowGood(string message)
        {
            lblMessage.Text = "";
            _show_toast(message);
        }
    }
}
