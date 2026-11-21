// hashes and checks passwords with a random salt (owner: shankar)
using System.Security.Cryptography;

namespace CinemaBooking.Services
{
    public static class PasswordHasher
    {
        private const int SALT_SIZE = 16;
        private const int HASH_SIZE = 32;
        private const int ITERATIONS = 100000;
        public static string GenerateSalt()
        {
            byte[] salt_bytes = RandomNumberGenerator.GetBytes(SALT_SIZE);
            return Convert.ToBase64String(salt_bytes);
        }

        public static string HashPassword(string password, string salt)
        {
            byte[] salt_bytes = Convert.FromBase64String(salt);
            byte[] hash_bytes = Rfc2898DeriveBytes.Pbkdf2(password, salt_bytes, ITERATIONS, HashAlgorithmName.SHA256,
                HASH_SIZE);
            return Convert.ToBase64String(hash_bytes);
        }

        public static bool Verify(string typed_password, string stored_hash, string stored_salt)
        {
            if (string.IsNullOrEmpty(typed_password) || string.IsNullOrEmpty(stored_hash) || string.IsNullOrEmpty(stored_salt))
            {
                return false;
            }

            byte[] salt_bytes;
            byte[] stored_hash_bytes;
            try
            {
                salt_bytes = Convert.FromBase64String(stored_salt);
                stored_hash_bytes = Convert.FromBase64String(stored_hash);
            }
            catch (FormatException)
            {
                return false;
            }

            byte[] typed_hash_bytes = Rfc2898DeriveBytes.Pbkdf2(typed_password, salt_bytes, ITERATIONS,
                HashAlgorithmName.SHA256, HASH_SIZE);
            return CryptographicOperations.FixedTimeEquals(typed_hash_bytes, stored_hash_bytes);
        }


    }
}
