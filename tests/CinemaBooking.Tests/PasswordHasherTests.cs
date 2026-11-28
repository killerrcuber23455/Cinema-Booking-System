// tests for the password hasher (owner: shankar)
using CinemaBooking.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class PasswordHasherTests
    {
        [TestMethod]
        public void GenerateSalt_CalledTwice_GiveDifferentSalts()
        {
            string first_salt = PasswordHasher.GenerateSalt();
            string second_salt = PasswordHasher.GenerateSalt();

            Assert.AreNotEqual(first_salt, second_salt);
        }

        [TestMethod]
        public void HashPassword_SamePasswordTwoSalts_GivesDifferentHashes()
        {
            string salt_one = PasswordHasher.GenerateSalt();
            string salt_two = PasswordHasher.GenerateSalt();

            string hash_one = PasswordHasher.HashPassword("Secret123", salt_one);
            string hash_two = PasswordHasher.HashPassword("Secret123", salt_two);

            Assert.AreNotEqual(hash_one, hash_two);
        }

        [TestMethod]
        public void HashPassword_SamePasswordSameSalt_GivesSameHash()
        {
            string salt = PasswordHasher.GenerateSalt();

            string hash_one = PasswordHasher.HashPassword("Secret123", salt);
            string hash_two = PasswordHasher.HashPassword("Secret123", salt);

            Assert.AreEqual(hash_one, hash_two);
        }

        [TestMethod]
        public void HashPassword_AnyPassword_IsNotThePlainPassword()
        {
            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.HashPassword("Secret123", salt);

            Assert.AreNotEqual("Secret123", hash);
        }

        [TestMethod]
        public void Verify_CorrectPassword_ReturnsTrue()
        {
            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.HashPassword("Secret123", salt);

            bool is_ok = PasswordHasher.Verify("Secret123", hash, salt);

            Assert.IsTrue(is_ok);
        }

        [TestMethod]
        public void Verify_WrongPassword_ReturnsFalse()
        {
            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.HashPassword("Secret123", salt);

            bool is_ok = PasswordHasher.Verify("secret123", hash, salt);

            Assert.IsFalse(is_ok);
        }

        [TestMethod]
        public void Verify_EmptyPassword_ReturnsFalse()
        {
            string salt = PasswordHasher.GenerateSalt();
            string hash = PasswordHasher.HashPassword("Secret123", salt);

            bool is_ok = PasswordHasher.Verify("", hash, salt);

            Assert.IsFalse(is_ok);
        }

        [TestMethod]
        public void Verify_StoredSaltIsNotValidText_ReturnsFalse()
        {
            bool is_ok = PasswordHasher.Verify("Secret123", "abc", "not base64!!");

            Assert.IsFalse(is_ok);
        }
    }
}