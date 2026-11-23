// the page where a guest finds their booking with the reference and email, and can cancel it (owner: divyesh)
// the page only reads the boxes and shows the result, the rules are in BookingLookup and BookingService
using System;
using System.Drawing;
using System.Globalization;
using System.Windows.Forms;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.UI
{
    public class FindBookingForm : UserControl
    {
        private const int BOX_REFERENCE = 0;
        private const int BOX_EMAIL = 1;
        private const int BOX_COUNT = 2;
        private const int RESULT_WIDTH = 380;
        private const int RESULT_HEIGHT = 170;

        // these are given by the main window, so this page holds no rules and no database
        private readonly Func<string, string, (Booking? Booking, string Error)> _find;
        private readonly Func<string, string, (bool Ok, string Error)> _cancel;
        private readonly Func<int, string> _describe_showing;
        private readonly Action<string> _show_toast;

        private readonly TextBox txtReference = new TextBox();
        private readonly TextBox txtEmail = new TextBox();
        private readonly Label[] _box_messages = new Label[BOX_COUNT];
        private readonly Label lblMessage = new Label();
        private readonly Button btnFind = FormParts.MakeButton("Find booking", UiTheme.ACCENT_RED);
        private readonly Button btnCancel = FormParts.MakeButton("Cancel booking", UiTheme.CARD);
        private readonly Panel pnlResult = new Panel();
        private readonly Label lblResult = new Label();

        // the reference and email of the booking on show, used when the guest cancels it
        private string _shown_reference = "";
        private string _shown_email = "";

        // true while a call runs on a background thread, a second click is ignored
        private bool _is_busy;

        public FindBookingForm(Func<string, string, (Booking? Booking, string Error)> find,
            Func<string, string, (bool Ok, string Error)> cancel, Func<int, string> describe_showing,
            Action<string> show_toast)
        {
            _find = find;
            _cancel = cancel;
            _describe_showing = describe_showing;
            _show_toast = show_toast;

            BuildPage();
        }

        private void BuildPage()
        {
            BackColor = UiTheme.PANEL;
            ForeColor = UiTheme.TEXT;
            Font = UiTheme.BODY_FONT;
            AutoScroll = true;

            Label lblHeading = new Label();
            lblHeading.Font = UiTheme.CARD_TITLE_FONT;
            lblHeading.Text = "Find your booking";
            lblHeading.SetBounds(FormParts.PAGE_PADDING, FormParts.PAGE_PADDING, FormParts.FORM_WIDTH, 26);
            Controls.Add(lblHeading);

            Label lblHelp = new Label();
            lblHelp.ForeColor = UiTheme.TEXT_MUTED;
            lblHelp.Text = "Type the reference from your ticket and the email you booked with";
            lblHelp.SetBounds(FormParts.PAGE_PADDING, 34, FormParts.FORM_WIDTH + 200, 20);
            Controls.Add(lblHelp);

            _box_messages[BOX_REFERENCE] = FormParts.AddRow(this, "Booking reference", txtReference, BOX_REFERENCE);
            _box_messages[BOX_EMAIL] = FormParts.AddRow(this, "Email used for the booking", txtEmail, BOX_EMAIL);

            int button_top = FormParts.TopUnderRows(BOX_COUNT);

            btnFind.SetBounds(FormParts.PAGE_PADDING, button_top, 170, 40);
            btnFind.Click += (sender, e) => ClickFind();
            Controls.Add(btnFind);

            lblMessage.ForeColor = UiTheme.ACCENT_RED;
            lblMessage.SetBounds(FormParts.PAGE_PADDING, button_top + 48, FormParts.FORM_WIDTH + 200, 24);
            Controls.Add(lblMessage);

            // the result card is hidden until a booking is found
            pnlResult.BackColor = UiTheme.CARD;
            pnlResult.SetBounds(FormParts.PAGE_PADDING, button_top + 80, RESULT_WIDTH, RESULT_HEIGHT);
            pnlResult.Visible = false;

            lblResult.ForeColor = UiTheme.TEXT;
            lblResult.BackColor = Color.Transparent;
            lblResult.SetBounds(12, 10, RESULT_WIDTH - 24, 96);
            pnlResult.Controls.Add(lblResult);

            btnCancel.SetBounds(12, 114, 170, 40);
            btnCancel.Click += (sender, e) => ClickCancel();
            pnlResult.Controls.Add(btnCancel);

            Controls.Add(pnlResult);

            // Enter in either box finds the booking
            txtReference.KeyDown += EnterFinds;
            txtEmail.KeyDown += EnterFinds;
        }

        private void EnterFinds(object? sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                ClickFind();
            }
        }

        private async void ClickFind()
        {
            if (_is_busy)
            {
                return;
            }

            ClearMessages();
            string reference = txtReference.Text;
            string email = txtEmail.Text;

            SetBusy(true, "Finding...");
            CallResult<(Booking? Booking, string Error)> result = await BackgroundCall.RunAsync(() => _find(reference, email));
            SetBusy(false, "");

            if (!result.IsOk)
            {
                lblMessage.Text = result.ErrorText;
                return;
            }

            if (result.Value.Booking == null)
            {
                ShowFindError(result.Value.Error);
                return;
            }

            _shown_reference = reference.Trim();
            _shown_email = email.Trim();
            ShowBooking(result.Value.Booking);
        }

        // each message goes where it belongs: the email under the email box, the not found message under the button
        private void ShowFindError(string error_message)
        {
            pnlResult.Visible = false;

            if (error_message == BookingLookup.EMAIL_TEXT)
            {
                _box_messages[BOX_EMAIL].Text = error_message;
            }
            else if (error_message == BookingLookup.NOT_FOUND_TEXT)
            {
                lblMessage.Text = error_message;
            }
            else
            {
                _box_messages[BOX_REFERENCE].Text = error_message;
            }
        }

        private void ShowBooking(Booking booking)
        {
            CultureInfo culture = CultureInfo.InvariantCulture;

            lblResult.Text = "Reference: " + booking.Reference + "\n"
                + _describe_showing(booking.ShowingId) + "\n"
                + "Seats: " + string.Join(", ", booking.Seats) + "\n"
                + "Total: Rs " + booking.Total.ToString("0.00", culture) + "\n"
                + "Status: " + booking.Status;

            // only a booking that is still confirmed can be cancelled
            btnCancel.Visible = booking.Status == BookingStatus.Confirmed;
            pnlResult.Visible = true;
        }

        private async void ClickCancel()
        {
            if (_is_busy)
            {
                return;
            }

            DialogResult answer = MessageBox.Show(
                "Cancel booking " + _shown_reference + "? The seats will be freed and this cannot be undone.",
                "Cancel booking", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (answer != DialogResult.Yes)
            {
                return;
            }

            ClearMessages();
            string reference = _shown_reference;
            string email = _shown_email;

            SetBusy(true, "Cancelling...");
            CallResult<(bool Ok, string Error)> result = await BackgroundCall.RunAsync(() => _cancel(reference, email));
            SetBusy(false, "");

            if (!result.IsOk)
            {
                lblMessage.Text = result.ErrorText;
                return;
            }

            if (!result.Value.Ok)
            {
                lblMessage.Text = result.Value.Error;
                return;
            }

            _show_toast("Booking cancelled: " + reference);

            // show the booking again so the status reads Cancelled
            SetBusy(true, "Finding...");
            CallResult<(Booking? Booking, string Error)> again = await BackgroundCall.RunAsync(() => _find(reference, email));
            SetBusy(false, "");

            if (again.IsOk && again.Value.Booking != null)
            {
                ShowBooking(again.Value.Booking);
            }
        }

        private void ClearMessages()
        {
            _box_messages[BOX_REFERENCE].Text = "";
            _box_messages[BOX_EMAIL].Text = "";
            lblMessage.Text = "";
        }

        // while a call runs the buttons cannot be clicked again and the find button shows what is happening
        private void SetBusy(bool is_busy, string busy_text)
        {
            _is_busy = is_busy;
            btnFind.Enabled = !is_busy;
            btnCancel.Enabled = !is_busy;
            btnFind.Text = is_busy && busy_text == "Finding..." ? busy_text : "Find booking";
            btnCancel.Text = is_busy && busy_text == "Cancelling..." ? busy_text : "Cancel booking";
        }
    }
}
