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
        private const string NOT_ADMIN_MESSAGE = "only an admin can create staff accounts";
        private const string BAD_NEW_ROLE_MESSAGE = "a new account must be staff or admin";
        private const string AUDIT_STAFF_CREATED = "staff_created";

        // card 447: the words the change password page shows
        private const string SIGN_IN_TO_CHANGE_MESSAGE = "sign in to change your password";
        private const string WRONG_OLD_PASSWORD_MESSAGE = "your current password is wrong";
        private const string SAME_PASSWORD_MESSAGE = "the new password must be different";
        private const string SAVE_FAILED_MESSAGE = "the password could not be saved";
        private const string AUDIT_PASSWORD_CHANGE_FAILED = "password_change_failed";

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

            // a lock that is still running refuses everything, even the right password, and changes nothing
            if (IsStillLocked(user, now, out int failed_attempts, out string locked_message))
            {
                _user_repository.WriteAudit(user.Id, AUDIT_LOGIN_REFUSED_LOCKED,
                    "locked until " + user.LockedUntil!.Value.ToString("yyyy-MM-dd HH:mm"));
                error_message = locked_message;
                return false;
            }

            if (!PasswordHasher.Verify(password, user.PasswordHash, user.Salt))
            {
                if (CountWrongPassword(user, failed_attempts, now, AUDIT_LOGIN_FAILED, "wrong password", out string lock_message))
                {
                    error_message = lock_message;
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

        // true while a lock is still running, with the lock message
        // a lock that has ended is cleared so the account starts again with a fresh set of tries
        // used by Login and ChangePassword so the two can never drift apart
        private bool IsStillLocked(UserRecord user, DateTime now, out int failed_attempts, out string locked_message)
        {
            failed_attempts = user.FailedAttempts;
            locked_message = "";

            if (user.LockedUntil.HasValue && now < user.LockedUntil.Value)
            {
                locked_message = MakeLockedMessage(user.LockedUntil.Value, now);
                return true;
            }

            if (user.LockedUntil.HasValue)
            {
                failed_attempts = 0;
                _user_repository.SetFailedAttempts(user.Id, 0, null);
            }

            return false;
        }

        // counts one wrong password, the try that reaches MAX_LOGIN_TRIES locks the account
        // true when this try locked it with the lock message
        // the audit text never holds the password, only which try it was
        private bool CountWrongPassword(UserRecord user, int failed_attempts, DateTime now,
            string failed_action, string failed_text, out string locked_message)
        {
            failed_attempts++;
            locked_message = "";

            if (failed_attempts >= MAX_LOGIN_TRIES)
            {
                DateTime locked_until = now.AddMinutes(LOCK_MINUTES);
                _user_repository.SetFailedAttempts(user.Id, failed_attempts, locked_until);
                _user_repository.WriteAudit(user.Id, AUDIT_ACCOUNT_LOCKED,
                    "locked until " + locked_until.ToString("yyyy-MM-dd HH:mm"));
                locked_message = MakeLockedMessage(locked_until, now);
                return true;
            }

            _user_repository.SetFailedAttempts(user.Id, failed_attempts, null);
            _user_repository.WriteAudit(user.Id, failed_action,
                failed_text + ", try " + failed_attempts + " of " + MAX_LOGIN_TRIES);
            return false;
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
            // a null email counts as empty, we trim the email but never the password
            string clean_email = (email ?? "").Trim();

            // the member rules come first (name, email, phone), nothing is saved yet
            if (!_member_service.IsValidNewMember(name, clean_email, phone, out error_message))
            {
                return false;
            }

            if (!IsValidPassword(password, out error_message))
            {
                return false;
            }

            // both emails are checked before anything is saved, so we never save a member that has no account
            if (_user_repository.FindByEmail(clean_email) != null || _member_service.IsEmailUsed(clean_email))
            {
                error_message = EMAIL_USED_MESSAGE;
                return false;
            }

            int member_id = _member_service.RegisterMember(name, clean_email, phone, out error_message);

            if (member_id == -1)
            {
                return false;
            }

            return SaveNewUser(clean_email, password, Role.Customer, member_id, out error_message);
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
        
        // card 436: an admin creates a staff or admin account
        public bool CreateStaff(Role acting_role, string acting_email, string email, Role new_role, string password, out string error_message)
        {
            error_message = NOT_ADMIN_MESSAGE;

            if (acting_role != Role.Admin)
            {
                return false;
            }

            //FindByEmail trims then lowercases and returns null for a null or empty email
            UserRecord? acting_user = _user_repository.FindByEmail(acting_email);
            if (acting_user == null || acting_user.Role != Role.Admin)
            {
                return false;
            }

            if (new_role != Role.Staff && new_role != Role.Admin)
            {
                error_message = BAD_NEW_ROLE_MESSAGE;
                return false;
            }

            //we trim the email but never the password, a null email counts as empty
            string clean_email = (email ?? "").Trim();

            if (!Member.IsValidEmail(clean_email, out error_message))
            {
                return false;
            }
            if (!IsValidPassword(password, out error_message))
            {
                return false;
            }
            if (_user_repository.FindByEmail(clean_email) != null)
            {
                error_message=EMAIL_USED_MESSAGE;
                return false;
            }

            //staff and admin accounts have no member record
            if (!SaveNewUser(clean_email, password, new_role, null, out error_message))
            {
                return false;
            }

            // SaveUser already wrote user_created for the new user, this row says who made the account
            // the details hold the new email and role only, never a password or hash
            _user_repository.WriteAudit(acting_user.Id, AUDIT_STAFF_CREATED, clean_email + " as " + new_role);

            error_message = "";
            return true;
        }

        // card 447: lets a signed-in user change their own password
        // a wrong current password counts as a failed login (same counter and lock as Login)
        public bool ChangePassword(Role acting_role, string acting_email, string old_password,
                                   string new_password, out string error_message)
        {
            error_message = SIGN_IN_TO_CHANGE_MESSAGE;

            // a guest, or a number that is not one of the four roles, is refused first
            if (acting_role == Role.Guest || !Enum.IsDefined(acting_role))
            {
                return false;
            }

            // the role passed in is only a claim, so the saved role is checked too
            // FindByEmail trims then lowercases and returns null for a null or empty email
            UserRecord? user = _user_repository.FindByEmail(acting_email);
            if (user == null || user.Role == Role.Guest)
            {
                return false;
            }

            DateTime now = _now();

            // the same lock check as Login, a lock that is still running refuses even the right password
            if (IsStillLocked(user, now, out int failed_attempts, out string locked_message))
            {
                error_message = locked_message;
                return false;
            }

            // a null or empty current password is not a guess, so it is refused without counting a try
            if (string.IsNullOrWhiteSpace(old_password))
            {
                error_message = WRONG_OLD_PASSWORD_MESSAGE;
                return false;
            }

            if (!PasswordHasher.Verify(old_password, user.PasswordHash, user.Salt))
            {
                error_message = WRONG_OLD_PASSWORD_MESSAGE;

                if (CountWrongPassword(user, failed_attempts, now, AUDIT_PASSWORD_CHANGE_FAILED, "wrong current password", out string lock_message))
                {
                    error_message = lock_message;
                }

                return false;
            }

            // the new password follows the same rule as sign up
            if (!IsValidPassword(new_password, out error_message))
            {
                return false;
            }

            if (new_password == old_password)
            {
                error_message = SAME_PASSWORD_MESSAGE;
                return false;
            }

            // a new salt every time, so the same password never gives the same hash twice
            string new_salt = PasswordHasher.GenerateSalt();
            string new_hash = PasswordHasher.HashPassword(new_password, new_salt);

            // UpdatePassword (card 446) saves the hash and salt, sets the tries back to 0, clears the lock
            // and writes the password_changed audit row, all in one transaction, so we do not write it again
            if (!_user_repository.UpdatePassword(user.Id, new_hash, new_salt))
            {
                error_message = SAVE_FAILED_MESSAGE;
                return false;
            }

            error_message = "";
            return true;
        }
    }

}