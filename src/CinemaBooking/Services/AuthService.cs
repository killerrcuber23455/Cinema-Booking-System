//login sign up and first admin checks (owner: shankar)
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class AuthService
    {
        public bool Login(string email, string password, out Role role, out Member? member, out string error_message)
        {
            role = Role.Guest;
            member = null;
            error_message = "login is not built yet";
            return false;
        }

        public bool SignUp(string name, string mail, string phone, string password, out string error_message)
        {
            error_message = "sign up is not built yet";
            return false;
        }

        public bool HasAnyAdmin()
        {
            return false;
        }

        public bool CreateFirstAdmin(string email, string password, out string error_message)
        {
            error_message = "first admin not built yet";
            return false;
        }
    }

}