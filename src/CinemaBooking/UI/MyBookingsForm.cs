// the My bookings page for a signed in customer: the list of their bookings and a button to cancel the chosen one (owner: divyesh)
// the page holds no rules and no database, it gets small functions from the main window.
// the list comes from MyBookingList, the cancel rules (including "only your own booking") are in BookingService
using System;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class MyBookingsForm : UserControl
    {
        private const int LIST_WIDTH = 760;
        private const int LIST_TOP = 44;
        private const int LIST_HEIGHT = 330;

        private readonly Func<(Booking[] Bookings, string Message)> _load;
        private readonly Func<string, (bool Ok, string Error)> _cancel;
        private readonly Func<int, string> _describe_showing;
        private readonly Action<string> _show_toast;

        private readonly ListBox lstBookings = new ListBox();
        private readonly Label lblMessage = new Label();
        private readonly Button btnCancel = FormParts.MakeButton("Cancel chosen booking", UiTheme.ACCENT_RED);

        // the bookings shown in the list, in the same order as the list lines
        private Booking[] _listed_bookings = new Booking[0];

        // true while a call runs on a background thread, a second click is ignored
        private bool _is_busy;

        public MyBookingsForm(Func<(Booking[] Bookings, string Message)> load,
            Func<string, (bool Ok, string Error)> cancel, Func<int, string> describe_showing,
            Action<string> show_toast)
        {
            _load = load;
            _cancel = cancel;
            _describe_showing = describe_showing;
            _show_toast = show_toast;

            BuildPage();

            // the list is read when the page opens
            Load += (sender, e) => Reload();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "My bookings";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 30);
            Controls.Add(lblHeading);

            lstBookings.BackColor = UiTheme.CARD;
            lstBookings.ForeColor = UiTheme.TEXT;
            lstBookings.BorderStyle = BorderStyle.None;
            lstBookings.SetBounds(FormParts.PAGE_PADDING, LIST_TOP, LIST_WIDTH, LIST_HEIGHT);
            Controls.Add(lstBookings);

            btnCancel.SetBounds(FormParts.PAGE_PADDING, LIST_TOP + LIST_HEIGHT + 10, 240, 40);
            btnCancel.Click += (sender, e) => ClickCancel();
            Controls.Add(btnCancel);

            // problems and "you have no bookings yet" show here
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, LIST_TOP + LIST_HEIGHT + 60, LIST_WIDTH, 44);
            Controls.Add(lblMessage);
        }

        // reads the bookings again and fills the list
        private async void Reload()
        {
            if (_is_busy)
            {
                return;
            }

            SetBusy(true);
            lblMessage.Text = "Loading your bookings...";
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;

            CallResult<(Booking[] Bookings, string Message)> result = await BackgroundCall.RunAsync(() => _load());
            SetBusy(false);

            if (!result.IsOk)
            {
                ShowProblem(result.ErrorText);
                return;
            }

            _listed_bookings = result.Value.Bookings;
            lstBookings.Items.Clear();

            for (int booking_index = 0; booking_index < _listed_bookings.Length; booking_index++)
            {
                Booking booking = _listed_bookings[booking_index];
                lstBookings.Items.Add(MyBookingLine.Text(booking, _describe_showing(booking.ShowingId)));
            }

            // a message here means there is nothing to show, for example "You have no bookings yet"
            lblMessage.ForeColor = UiTheme.TEXT_MUTED;
            lblMessage.Text = result.Value.Message;
        }

        private async void ClickCancel()
        {
            if (_is_busy)
            {
                return;
            }

            int chosen_index = lstBookings.SelectedIndex;

            if (chosen_index < 0)
            {
                ShowProblem("Choose a booking in the list first");
                return;
            }

            Booking booking = _listed_bookings[chosen_index];

            // only a booking that is still confirmed can be cancelled
            if (booking.Status != BookingStatus.Confirmed)
            {
                ShowProblem("This booking is already cancelled");
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Cancel booking " + booking.Reference + "? The seats will be freed and this cannot be undone.",
                "Cancel booking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            string reference = booking.Reference;

            SetBusy(true);
            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() => _cancel(reference));
            SetBusy(false);

            if (!result.IsOk)
            {
                ShowProblem(result.ErrorText);
                return;
            }

            if (!result.Value.Ok)
            {
                ShowProblem(result.Value.Error);
                return;
            }

            _show_toast("Booking cancelled: " + reference);
            Reload();
        }

        private void ShowProblem(string message)
        {
            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.Text = message.TrimEnd('\n');
        }

        // while a call runs the cancel button cannot be clicked again
        private void SetBusy(bool is_busy)
        {
            _is_busy = is_busy;
            btnCancel.Enabled = !is_busy;
        }
    }
}
