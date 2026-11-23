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

            // one showing table for the whole program, the booking service must be given this same one
            ShowingTable showing_table = new ShowingTable();

            // the login needs the database, so read appsettings.json first
            AuthService auth_service;
            BookingService booking_service;
            IDatabase booking_database;
            try
            {
                AppConfig config = new AppConfig();
                Database database = new Database(config);
                UserRepository users = new UserRepository(config);
                MemberService member_service = new MemberService(database);
                auth_service = new AuthService(users, member_service);
                booking_database = database;
                booking_service = new BookingService(database, showing_table);
            }
            catch (Exception ex)
            {
                MessageBox.Show("The program could not start the database settings.\n\n" + ex.Message,
                    "Cinema", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            ShowingService showing_service = new ShowingService(showing_table);
            // the landing window comes first and gives one choice back, closing it without a choice ends the program
            UI.LandingChoice choice;
            using (UI.LandingWindow landing = new UI.LandingWindow())
            {
                landing.ShowDialog();
                choice = landing.Choice;
            }

            if (!UI.LandingRules.ShouldOpenMain(choice))
            {
                return;
            }

            Application.Run(new UI.MainForm(showing_service, auth_service, showing_table, booking_service, booking_database,
                UI.LandingRules.FirstPageFor(choice)));
        }
    }
}