// the main window of the cinema program, built by hand with no designer file (owner: divyesh)
namespace CinemaBooking.UI
{
    public class MainForm : Form
    {
        private Label _lblTitle;

        public MainForm()
        {
            Text = "Cinema Booking System";
            ClientSize = new Size(1000, 640);
            StartPosition = FormStartPosition.CenterScreen;

            _lblTitle = new Label();
            _lblTitle.Text = "Cinema Booking System";
            _lblTitle.Font = new Font("Segoe UI", 20, FontStyle.Bold);
            _lblTitle.AutoSize = true;
            _lblTitle.Location = new Point(40, 40);

            Controls.Add(_lblTitle);
        }
    }
}
