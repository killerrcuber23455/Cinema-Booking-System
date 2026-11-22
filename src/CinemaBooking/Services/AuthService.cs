// login sign up and first admin checks (owner: shankar)
using CinemaBooking.Data;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class AuthService
    {
        private IUserRepository _user_repository;
        private MemberService _member_service;
        private Func<DateTime> _now;

        // the same message for a wrong email and a wrong password
        // so nobody can find out which emails exist
        private const string WRONG_LOGIN_MESSAGE = "email or password is wrong";

        // the normal constructor uses the real time
        public AuthService(IUserRepository user_repository, MemberService member_service)
            : this(user_repository, member_service, () => DateTime.Now)
        {
        }

        // only the tests use this one, so they can move the clock instead of waiting 15 real minutes
        public AuthService(IUserRepository user_repository, MemberService member_service, Func<DateTime> now)
        {
            _user_repository = user_repository;
            _member_service = member_service;
            _now = now;
        }


        public bool Login(string email, string password, out Role role, out Member? member, out string error_message)
        {
            role = Role.Guest;
            member = null;
            error_message = WRONG_LOGIN_MESSAGE;
            
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return false;
            }

            UserRecord? user = _user_repository.FindByEmail(email);
            if (user == null)
            {
                return false;
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash, user.Salt))
            {
                return false;
            }

            role = user.Role;
            if (user.MemberId.HasValue)
            {
                member = _member_service.FindMemberById(user.MemberId.Value);
            }

            error_message = "";
            return true;
        }

        public bool SignUp(string name, string email, string phone, string password, out string error_message)
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