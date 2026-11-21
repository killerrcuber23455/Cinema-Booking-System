// hashes and checks passwords with a random salt (owner: shankar)

namespace CinemaBooking.Services
{
    public static class PasswordHasher
    {
        public static string GenerateSalt()
        {
            throw new NotImplementedException();
        }

        public static string HashPassword(string password, string salt)
        {
            throw new NotImplementedException();
        }

        public static bool Verify(string typed_password, string stored_hash, string stored_salt)
        {
            throw new NotImplementedException();
        }
    }
}
