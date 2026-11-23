// the user and audit methods the login service uses, so a fake can replace sql server in tests (owner: kushal)
using CinemaBooking.Models;
using System;

namespace CinemaBooking.Data
{
    public interface IUserRepository
    {
        // returns the new id, or -1 if the email is used or the database refuses the data
        int SaveUser(string email, string password_hash, string salt, Role role, int? member_id);

        // finds a user by email (any case), null if there is none
        UserRecord? FindByEmail(string email);

        // saves the failed login count and the lock time together, null clears the lock
        bool SetFailedAttempts(int user_id, int failed_attempts, DateTime? locked_until);

        // writes one audit row, user_id is null when nobody is logged in
        bool WriteAudit(int? user_id, string action, string details);

        // true if at least one user has the role Admin
        bool HasAnyAdmin();

        // changes the role of a user and writes an audit row, false if the user is not found
        bool SetRole(int user_id, Role role);
    }
}