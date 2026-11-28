// one row of the users table, read back from the database (owner: kushal)
using CinemaBooking.Models;
using System;

namespace CinemaBooking.Data
{
    public class UserRecord
    {
        public int Id { get; set; }
        public string Email { get; set; }
        public string PasswordHash { get; set; }
        public string Salt { get; set; }
        public Role Role { get; set; }
        public int FailedAttempts { get; set; }
        public DateTime? LockedUntil { get; set; }
        public int? MemberId { get; set; }

        public UserRecord(int id, string email, string password_hash, string salt, Role role,
            int failed_attempts, DateTime? locked_until, int? member_id)
        {
            Id = id;
            Email = email;
            PasswordHash = password_hash;
            Salt = salt;
            Role = role;
            FailedAttempts = failed_attempts;
            LockedUntil = locked_until;
            MemberId = member_id;
        }
    }
}