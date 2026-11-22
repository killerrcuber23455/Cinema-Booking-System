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

        // three wrong passwords in a row lock the account (rule from card 400)
        private const int MAX_LOGIN_TRIES = 3;

        // how long a locked account stays closed, the tests and the message both use this one number
        private const int LOCK_MINUTES = 15;

        // the action names written to the audit log where the column holds up to 50 characters
        private const string AUDIT_LOGIN = "login";
        private const string AUDIT_LOGIN_FAILED = "login_failed";
        private const string AUDIT_ACCOUNT_LOCKED = "account_locked";
        private const string AUDIT_LOGIN_REFUSED_LOCKED = "login_refused_locked";

        // audit_log.details holds 500 characters, so a very long email is cut before it is written
        private const int MAX_EMAIL_IN_AUDIT = 200;

        // the password rule, decided in docs/design/validation-rules.md
        private const int MIN_PASSWORD_LENGTH = 8;

        // the same words as the windows use
        private const string SHORT_PASSWORD_MESSAGE = "password must be at least 8 characters";
        private const string EMAIL_USED_MESSAGE = "this email is already used";

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
                // there is no user id, so the email goes in details (cut to fit the column), never a password
                string short_email = email.Trim();
                if (short_email.Length > MAX_EMAIL_IN_AUDIT)
                {
                    short_email = short_email.Substring(0, MAX_EMAIL_IN_AUDIT);
                }

                _user_repository.WriteAudit(null, AUDIT_LOGIN_FAILED, short_email);
                return false;
            }

            DateTime now = _now();
            int failed_attempts = user.FailedAttempts;

            //a lock that is still running refuses everything
            //even the right password, and changes nothing
            if (user.LockedUntil.HasValue && now < user.LockedUntil.Value)
            {
                _user_repository.WriteAudit(user.Id, AUDIT_LOGIN_REFUSED_LOCKED,
                    "locked until " + user.LockedUntil.Value.ToString("yyyy-MM-dd HH:mm"));
                error_message = MakeLockedMessage(user.LockedUntil.Value, now);
                return false;
            }

            // the lock has ended, so the account starts again with a fresh set of tries
            if (user.LockedUntil.HasValue)
            {
                failed_attempts = 0;
                _user_repository.SetFailedAttempts(user.Id, 0, null);
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash, user.Salt))
            {
                failed_attempts++;

                if (failed_attempts >= MAX_LOGIN_TRIES)
                {
                    DateTime locked_until = now.AddMinutes(LOCK_MINUTES);
                    _user_repository.SetFailedAttempts(user.Id, failed_attempts, locked_until);
                    _user_repository.WriteAudit(user.Id, AUDIT_ACCOUNT_LOCKED,
                        "locked until " + locked_until.ToString("yyyy-MM-dd HH:mm"));
                    error_message = MakeLockedMessage(locked_until, now);
                }
                else
                {
                    _user_repository.SetFailedAttempts(user.Id, failed_attempts, null);
                    _user_repository.WriteAudit(user.Id, AUDIT_LOGIN_FAILED,
                        "wrong password, try " + failed_attempts + " of " + MAX_LOGIN_TRIES);
                }

                return false;
            }

            // a right password clears the wrong tries
            if (failed_attempts > 0)
            {
                _user_repository.SetFailedAttempts(user.Id, 0, null);
            }

            _user_repository.WriteAudit(user.Id, AUDIT_LOGIN, user.Email);

            role = user.Role;
            if (user.MemberId.HasValue)
            {
                member = _member_service.FindMemberById(user.MemberId.Value);
            }

            error_message = "";
            return true;
        }

        // says how long is left, rounded up so "30 seconds left" never shows as "0 minutes"
        private static string MakeLockedMessage(DateTime locked_until, DateTime now)
        {
            int minutes_left = (int)Math.Ceiling((locked_until - now).TotalMinutes);

            if (minutes_left == 1)
            {
                return "this account is locked, try again in 1 minute";
            }

            return "this account is locked, try again in " + minutes_left + " minutes";
        }

        // the password rule, used by the first admin and by sign up
        // a password of only spaces is refused because Login treats it as empty
        private static bool IsValidPassword(string password, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(password) || password.Length < MIN_PASSWORD_LENGTH)
            {
                error_message = SHORT_PASSWORD_MESSAGE;
                return false;
            }

            error_message = "";
            return true;
        }

        // makes a new salt, hashes the password and saves the user
        // false if the email is already used
        private bool SaveNewUser(string email, string password, Role role, int? member_id, out string error_message)
        {
            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.HashPassword(password, salt);

            if (_user_repository.SaveUser(email, hash, salt, role, member_id) == -1)
            {
                error_message = EMAIL_USED_MESSAGE;
                return false;
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
            return _user_repository.HasAnyAdmin();
        }

        public bool CreateFirstAdmin(string email, string password, out string error_message)
        {
            // only the very first admin can be made this way, so nobody can use it later to get admin rights
            if (_user_repository.HasAnyAdmin())
            {                
                error_message = "an admin already exists";
                return false;
            }

            // we trim the email but never the password, spaces at the start or end are part of a password
            // a null email counts as empty so it is refused with the normal email message
            string clean_email = (email ?? "").Trim();

            if (!Member.IsValidEmail(clean_email, out error_message))
            {
                return false;
            }

            if (!IsValidPassword(password, out error_message))
            {
                return false;
            }

            return SaveNewUser(clean_email, password, Role.Admin, null, out error_message);
        }
    }

}