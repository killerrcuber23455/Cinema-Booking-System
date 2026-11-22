// starts the program and opens the main window (owner: divyesh)
using CinemaBooking.DataStructures;
using CinemaBooking.Services;

namespace CinemaBooking
{
    internal static class Program
    {
        [STAThread]
        private static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);

            // one showing table for the whole program, the booking service must be given this same one
            ShowingTable showing_table = new ShowingTable();

#if DEBUG
            DemoShowings.Load(showing_table);
#endif
            // the login service. its constructor will take the user repository and the member service when
            // Shankar pushes the real login, then only this line changes
            AuthService auth_service = new AuthService();

            ShowingService showing_service = new ShowingService(showing_table);
            Application.Run(new UI.MainForm(showing_service, auth_service));
        }
    }
}
