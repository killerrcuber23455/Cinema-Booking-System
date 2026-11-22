// the user and audit methods the login service uses, so a fake can replace sql server in tests (owner: kushal)
using CinemaBooking.Models;
using System;

namespace CinemaBooking.Data
{
    public interface IUserRepository
    {
        int SaveUser(string email, string password_hash, string salt, Role role, int? member_id);
        UserRecord? FindByEmail(string email);
        bool SetFailedAttempts(int user_id, int failed_attempts, DateTime? locked_until);
        bool WriteAudit(int? user_id, string action, string details);
        bool HasAnyAdmin();
    }
}