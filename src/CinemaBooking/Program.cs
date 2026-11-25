// starts the program: the landing window first, then the main window (owner: divyesh)
using CinemaBooking.Data;
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

            // the login needs the database, so read appsettings.json first
            AuthService auth_service;
            Database database;
            try
            {
                AppConfig config = new AppConfig();
                database = new Database(config);
                UserRepository users = new UserRepository(config);
                MemberService member_service = new MemberService(database);
                auth_service = new AuthService(users, member_service);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The program could not start the database settings.\n\n" + ex.Message,
                    "Cinema", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            // the landing window comes first and gives one outcome back, closing it without a choice ends the program
            // signing out in the main window comes back here, so the landing window shows again for the next person
            while (true)
            {
                UI.LandingOutcome outcome;
                using (UI.LandingWindow landing = new UI.LandingWindow(auth_service))
                {
                    landing.ShowDialog();
                    outcome = landing.Outcome;
                }

                if (!UI.LandingRules.ShouldOpenMain(outcome))
                {
                    return;
                }

                // one showing table for each time the main window opens, the booking service must be given this same one
                ShowingTable showing_table = new ShowingTable();
                BookingService booking_service = new BookingService(database, showing_table);
                ShowingService showing_service = new ShowingService(showing_table);

                UI.MainForm main_window = new UI.MainForm(showing_service, auth_service, showing_table, booking_service, database, outcome);
                Application.Run(main_window);

                if (!main_window.WantsLanding)
                {
                    return;
                }
            }
        }
    }
}
