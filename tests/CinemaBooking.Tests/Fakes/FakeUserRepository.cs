// a fake user repository that keeps users and audit rows in memory
// so login can be tested without sql server (owner: shankar)
using CinemaBooking.Data;
using CinemaBooking.Models;

namespace CinemaBooking.Tests.Fakes;

public class AuditRow
{
    public int? UserId { get; }
    public string Action { get; }
    public string Details { get; }

    public AuditRow(int? user_id, string action, string details)
    {
        UserId = user_id;
        Action = action;
        Details = details;
    }
}

public class FakeUserRepository : IUserRepository
{
    private readonly List<UserRecord> _users = new List<UserRecord>();
    private int _next_id = 1;

    // every audit row written, so a test can check what Login recorded
    public List<AuditRow> AuditRows { get; } = new List<AuditRow>();

    // same rule as the real table: the email is trimmed and lowercase, and it must be unique
    private static string MakeEmailKey(string email)
    {
        return email.Trim().ToLowerInvariant();
    }

    public int SaveUser(string email, string password_hash, string salt, Role role, int? member_id)
    {
        if (string.IsNullOrWhiteSpace(email) || FindByEmail(email) != null)
        {
            return -1;
        }

        int new_id = _next_id;
        _next_id++;
        _users.Add(new UserRecord(new_id, MakeEmailKey(email), password_hash, salt, role, 0, null, member_id));
        return new_id;
    }

    // returns a copy, like a row read fresh from the database
    public UserRecord? FindByEmail(string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return null;
        }

        string email_key = MakeEmailKey(email);

        for (int index = 0; index < _users.Count; index++)
        {
            UserRecord user = _users[index];
            if (user.Email == email_key)
            {
                return new UserRecord(user.Id, user.Email, user.PasswordHash, user.Salt, user.Role,
                    user.FailedAttempts, user.LockedUntil, user.MemberId);
            }
        }

        return null;
    }

    public bool SetFailedAttempts(int user_id, int failed_attempts, DateTime? locked_until)
    {
        for (int index = 0; index < _users.Count; index++)
        {
            if (_users[index].Id == user_id)
            {
                _users[index].FailedAttempts = failed_attempts;
                _users[index].LockedUntil = locked_until;
                return true;
            }
        }

        return false;
    }

    public bool WriteAudit(int? user_id, string action, string details)
    {
        AuditRows.Add(new AuditRow(user_id, action, details));
        return true;
    }

    public bool HasAnyAdmin()
    {
        for (int index = 0; index < _users.Count; index++)
        {
            if (_users[index].Role == Role.Admin)
            {
                return true;
            }
        }

        return false;
    }
}
