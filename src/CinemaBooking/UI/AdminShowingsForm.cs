// the showings page for staff and admins: the boxes to add a showing, each with its own message, and a list
// with a button to remove the chosen showing (owner: divyesh)
// it is a page shown inside the main window. it only reads the boxes and shows messages. the boxes are checked
// by ShowingInput, and the add and remove rules (including the role check) are in ShowingAdminService
using System;
using System.Globalization;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class AdminShowingsForm : UserControl
    {
        private const int LIST_LEFT = 430;
        private const int LIST_WIDTH = 400;
        private const int LIST_TOP = 56;
        private const int LIST_HEIGHT = 330;

        private readonly ShowingAdminService _showing_admin_service;
        private readonly ShowingStore _showing_store;

        // the role of the person using the page, the service refuses add and remove unless it is Staff or Admin
        private readonly Role _role;

        private readonly TextBox txtId = new TextBox();
        private readonly TextBox txtFilm = new TextBox();
        private readonly TextBox txtScreen = new TextBox();
        private readonly TextBox txtStarts = new TextBox();
        private readonly TextBox txtPrice = new TextBox();
        private readonly ListBox lstShowings = new ListBox();
        private readonly Label lblMessage = new Label();
        private Button btnAdd = null!;

        // true while the database call of an add runs, so the button cannot be clicked twice
        private bool _is_busy;

        // the message under each box, in the same order as ShowingInput gives them
        private Label[] _box_labels = new Label[ShowingInput.BOX_COUNT];

        // the showings shown in the list, in the same order as the list lines
        private Showing[] _listed_showings = new Showing[0];

        // shows a good news message as a toast in the main window
        private readonly Action<string> _show_toast;

        public AdminShowingsForm(ShowingAdminService showing_admin_service, ShowingStore showing_store, Role role, Action<string> show_toast)
        {
            _showing_admin_service = showing_admin_service;
            _showing_store = showing_store;
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

            // the boxes to add a showing on the left
            Label lblFormHeading = new Label();
            lblFormHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblFormHeading.Text = "Add a showing";
            lblFormHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 30);
            Controls.Add(lblFormHeading);

            _box_labels[ShowingInput.BOX_ID] = FormParts.AddRow(this, "Showing id", txtId, ShowingInput.BOX_ID);
            _box_labels[ShowingInput.BOX_FILM] = FormParts.AddRow(this, "Film id", txtFilm, ShowingInput.BOX_FILM);
            _box_labels[ShowingInput.BOX_SCREEN] = FormParts.AddRow(this, "Screen id", txtScreen, ShowingInput.BOX_SCREEN);
            _box_labels[ShowingInput.BOX_STARTS] = FormParts.AddRow(this, "Start (like 2026-12-01 18:30)", txtStarts, ShowingInput.BOX_STARTS);
            _box_labels[ShowingInput.BOX_PRICE] = FormParts.AddRow(this, "Price", txtPrice, ShowingInput.BOX_PRICE);

            int button_top = FormParts.TopUnderRows(ShowingInput.BOX_COUNT);
            btnAdd = FormParts.MakeButton("Add showing", UiTheme.ACCENT_RED);
            btnAdd.SetBounds(FormParts.PAGE_PADDING, button_top, 180, 40);
            btnAdd.Click += (sender, e) => ClickAdd();
            Controls.Add(btnAdd);

            // the message under the add button is for what is not about one box: a clash, a missing film, a good notice
            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, button_top + 50, FormParts.FORM_WIDTH, 60);
            Controls.Add(lblMessage);

            // the list of showings on the right
            Label lblListHeading = new Label();
            lblListHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblListHeading.Text = "Showings";
            lblListHeading.SetBounds(LIST_LEFT, FormParts.PAGE_PADDING, LIST_WIDTH, 30);

            lstShowings.BackColor = UiTheme.CARD;
            lstShowings.ForeColor = UiTheme.TEXT;
            lstShowings.BorderStyle = BorderStyle.None;
            lstShowings.SetBounds(LIST_LEFT, LIST_TOP, LIST_WIDTH, LIST_HEIGHT);

            Button btnRemove = FormParts.MakeButton("Remove chosen showing", UiTheme.CARD);
            btnRemove.SetBounds(LIST_LEFT, LIST_TOP + LIST_HEIGHT + 10, LIST_WIDTH, 40);
            btnRemove.Click += (sender, e) => ClickRemove();

            Controls.Add(lblListHeading);
            Controls.Add(lstShowings);
            Controls.Add(btnRemove);
        }

        // shows every showing, earliest first, one line each
        private void FillList()
        {
            _listed_showings = _showing_admin_service.ListShowings();
            lstShowings.Items.Clear();

            for (int showing_index = 0; showing_index < _listed_showings.Length; showing_index++)
            {
                Showing showing = _listed_showings[showing_index];
                lstShowings.Items.Add(showing.Id
                    + "  -  film " + showing.FilmId
                    + ", screen " + showing.ScreenId
                    + "  -  " + showing.StartsAt.ToString(ShowingInput.START_FORMAT, CultureInfo.InvariantCulture)
                    + "  -  " + showing.Price.ToString("0.00", CultureInfo.InvariantCulture));
            }
        }

        // the boxes are checked by ShowingInput, the showing is added by ShowingStore, which also checks the role and saves it to the database
        private async void ClickAdd()
        {
            if (_is_busy)
            {
                return;
            }

            lblMessage.Text = "";

            string[] messages = ShowingInput.CheckBoxes(txtId.Text, txtFilm.Text, txtScreen.Text,
                txtStarts.Text, txtPrice.Text, DateTime.Now);

            if (FormParts.ShowBoxMessages(_box_labels, messages))
            {
                return;
            }

            ShowingInput? input;
            string error_message;

            if (!ShowingInput.TryCreate(txtId.Text, txtFilm.Text, txtScreen.Text, txtStarts.Text, txtPrice.Text,
                    DateTime.Now, out input, out error_message))
            {
                ShowError(error_message);
                return;
            }

            // the database call runs on a background thread so the window does not freeze, it must not touch any control
            ShowingInput new_showing = input!;

            _is_busy = true;
            btnAdd.Enabled = false;
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.Text = "Saving...";

            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() =>
            {
                bool is_added = _showing_store.AddShowing(new_showing.Id, new_showing.FilmId, new_showing.ScreenId,
                    new_showing.StartsAt, new_showing.Price, _role, out string add_error);
                return (is_added, add_error);
            });

            _is_busy = false;
            btnAdd.Enabled = true;

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

            ClearBoxes();
            FillList();
            ShowGood("Showing added: " + new_showing.Id);
        }

        private void ClickRemove()
        {
            lblMessage.Text = "";
            int chosen_index = lstShowings.SelectedIndex;

            if (chosen_index < 0)
            {
                ShowError("Choose a showing in the list first");
                return;
            }

            Showing showing = _listed_showings[chosen_index];
            DialogResult answer = MessageBox.Show("Remove showing " + showing.Id + "?", "Remove showing", MessageBoxButtons.YesNo);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            string error_message;

            if (!_showing_admin_service.RemoveShowing(showing.Id, _role, out error_message))
            {
                ShowError(error_message);
                return;
            }

            FillList();
            ShowGood("Showing removed: " + showing.Id);
        }

        private void ClearBoxes()
        {
            txtId.Text = "";
            txtFilm.Text = "";
            txtScreen.Text = "";
            txtStarts.Text = "";
            txtPrice.Text = "";

            for (int box_index = 0; box_index < _box_labels.Length; box_index++)
            {
                _box_labels[box_index].Text = "";
            }
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
