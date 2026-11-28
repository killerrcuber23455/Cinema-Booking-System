// tests the users and audit_log database code on the separate test database (owner: kushal)
using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CinemaBooking.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class UserRepositoryTests
    {
        private string _connection_string = "";
        private UserRepository _repository = null!;

        [TestInitialize]
        public void Setup()
        {
            string? value = Environment.GetEnvironmentVariable("CINEMA_TEST_DB");

            if (string.IsNullOrWhiteSpace(value) || !value.Contains("Test", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Inconclusive("Set CINEMA_TEST_DB to a TEST database (name must contain Test).");
            }

            _connection_string = value;
            _repository = new UserRepository(_connection_string);

            Run("DELETE FROM audit_log; DELETE FROM users;");
        }

        private void Run(string sql)
        {
            using (SqlConnection connection = new SqlConnection(_connection_string))
            using (SqlCommand command = new SqlCommand(sql, connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        [TestMethod]
        public void UserRepository_Implements_IUserRepository()
        {
            Assert.IsNotNull(typeof(UserRepository).GetInterface("IUserRepository"));
        }

        [TestMethod]
        public void SaveUser_New_ReturnsId()
        {
            int id = _repository.SaveUser("ann@x.com", "hash1", "salt1", Role.Customer, null);
            Assert.IsGreaterThan(0, id);
        }

        [TestMethod]
        public void SaveUser_DuplicateEmailAnyCase_ReturnsMinusOne()
        {
            _repository.SaveUser("ann@x.com", "hash1", "salt1", Role.Customer, null);
            Assert.AreEqual(-1, _repository.SaveUser(" ANN@x.com ", "hash2", "salt2", Role.Staff, null));
        }

        [TestMethod]
        public void SaveUser_WritesAuditRow()
        {
            _repository.SaveUser("ann@x.com", "hash1", "salt1", Role.Customer, null);
            Assert.AreEqual(1, _repository.CountAuditRows("user_created"));
        }

        [TestMethod]
        public void FindByEmail_Found_ReturnsSavedValues()
        {
            _repository.SaveUser("bob@x.com", "hash9", "salt9", Role.Staff, null);

            UserRecord? user = _repository.FindByEmail("BOB@x.com");

            Assert.IsNotNull(user);
            Assert.AreEqual("hash9", user.PasswordHash);
            Assert.AreEqual("salt9", user.Salt);
            Assert.AreEqual(Role.Staff, user.Role);
            Assert.AreEqual(0, user.FailedAttempts);
            Assert.IsNull(user.LockedUntil);
            Assert.IsNull(user.MemberId);
        }

        [TestMethod]
        public void FindByEmail_Missing_ReturnsNull()
        {
            Assert.IsNull(_repository.FindByEmail("nobody@x.com"));
        }

        [TestMethod]
        public void SetFailedAttempts_SavesCountAndLock()
        {
            int id = _repository.SaveUser("cy@x.com", "h", "s", Role.Customer, null);
            DateTime until = new DateTime(2030, 1, 1, 10, 0, 0);

            Assert.IsTrue(_repository.SetFailedAttempts(id, 5, until));

            UserRecord? user = _repository.FindByEmail("cy@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual(5, user.FailedAttempts);
            Assert.AreEqual(until, user.LockedUntil);
        }

        [TestMethod]
        public void SetFailedAttempts_NullClearsLock()
        {
            int id = _repository.SaveUser("dee@x.com", "h", "s", Role.Customer, null);
            _repository.SetFailedAttempts(id, 5, new DateTime(2030, 1, 1, 10, 0, 0));

            _repository.SetFailedAttempts(id, 0, null);

            UserRecord? user = _repository.FindByEmail("dee@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual(0, user.FailedAttempts);
            Assert.IsNull(user.LockedUntil);
        }

        [TestMethod]
        public void SetFailedAttempts_UnknownUser_ReturnsFalse()
        {
            Assert.IsFalse(_repository.SetFailedAttempts(99999, 1, null));
        }

        [TestMethod]
        public void WriteAudit_WithoutUser_IsSaved()
        {
            // a failed login for an unknown email has no user id
            Assert.IsTrue(_repository.WriteAudit(null, "login_failed", "nobody@x.com"));
            Assert.AreEqual(1, _repository.CountAuditRows("login_failed"));
        }

        [TestMethod]
        public void HasAnyAdmin_NoUsers_ReturnsFalse()
        {
            Assert.IsFalse(_repository.HasAnyAdmin());
        }

        [TestMethod]
        public void HasAnyAdmin_WithAdmin_ReturnsTrue()
        {
            _repository.SaveUser("staff@x.com", "h", "s", Role.Staff, null);
            _repository.SaveUser("boss@x.com", "h", "s", Role.Admin, null);

            Assert.IsTrue(_repository.HasAnyAdmin());
        }

        [TestMethod]
        public void HasAnyAdmin_OnlyCustomerAndStaff_ReturnsFalse()
        {
            _repository.SaveUser("cust@x.com", "h", "s", Role.Customer, null);
            _repository.SaveUser("staff@x.com", "h", "s", Role.Staff, null);

            Assert.IsFalse(_repository.HasAnyAdmin());
        }


        [TestMethod]
        public void SetRole_ExistingUser_ReturnsTrueAndSavesRole()
        {
            int id = _repository.SaveUser("eve@x.com", "h", "s", Role.Customer, null);

            Assert.IsTrue(_repository.SetRole(id, Role.Staff));

            UserRecord? user = _repository.FindByEmail("eve@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual(Role.Staff, user.Role);
        }

        [TestMethod]
        public void SetRole_UnknownUser_ReturnsFalse()
        {
            Assert.IsFalse(_repository.SetRole(99999, Role.Admin));
        }

        [TestMethod]
        public void SetRole_UndefinedRole_ReturnsFalseAndKeepsOldRole()
        {
            int id = _repository.SaveUser("fay@x.com", "h", "s", Role.Customer, null);

            Assert.IsFalse(_repository.SetRole(id, (Role)99));

            UserRecord? user = _repository.FindByEmail("fay@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual(Role.Customer, user.Role);
        }

        [TestMethod]
        public void SetRole_WritesAuditRow()
        {
            int id = _repository.SaveUser("gus@x.com", "h", "s", Role.Customer, null);

            _repository.SetRole(id, Role.Admin);

            Assert.AreEqual(1, _repository.CountAuditRows("role_changed"));
        }

        [TestMethod]
        public void UpdatePassword_SavesNewHashAndSalt()
        {
            int id = _repository.SaveUser("pw@x.com", "oldhash", "oldsalt", Role.Customer, null);

            Assert.IsTrue(_repository.UpdatePassword(id, "newhash", "newsalt"));

            UserRecord? user = _repository.FindByEmail("pw@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual("newhash", user.PasswordHash);
            Assert.AreEqual("newsalt", user.Salt);
        }

        [TestMethod]
        public void UpdatePassword_UnknownUser_ReturnsFalse()
        {
            Assert.IsFalse(_repository.UpdatePassword(99999, "newhash", "newsalt"));
        }

        [TestMethod]
        public void UpdatePassword_ClearsFailedAttemptsAndLock()
        {
            int id = _repository.SaveUser("lock@x.com", "oldhash", "oldsalt", Role.Customer, null);
            _repository.SetFailedAttempts(id, 5, new DateTime(2030, 1, 1, 10, 0, 0));

            Assert.IsTrue(_repository.UpdatePassword(id, "newhash", "newsalt"));

            UserRecord? user = _repository.FindByEmail("lock@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual(0, user.FailedAttempts);
            Assert.IsNull(user.LockedUntil);
        }

        [TestMethod]
        public void UpdatePassword_DoesNotChangeOtherUsers()
        {
            int first_id = _repository.SaveUser("one@x.com", "hash1", "salt1", Role.Customer, null);
            _repository.SaveUser("two@x.com", "hash2", "salt2", Role.Staff, null);

            Assert.IsTrue(_repository.UpdatePassword(first_id, "newhash", "newsalt"));

            UserRecord? other = _repository.FindByEmail("two@x.com");
            Assert.IsNotNull(other);
            Assert.AreEqual("hash2", other.PasswordHash);
            Assert.AreEqual("salt2", other.Salt);
        }

        [TestMethod]
        public void UpdatePassword_EmptyHash_ReturnsFalseAndChangesNothing()
        {
            int id = _repository.SaveUser("eh@x.com", "oldhash", "oldsalt", Role.Customer, null);

            Assert.IsFalse(_repository.UpdatePassword(id, "", "newsalt"));

            UserRecord? user = _repository.FindByEmail("eh@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual("oldhash", user.PasswordHash);
            Assert.AreEqual("oldsalt", user.Salt);
        }

        [TestMethod]
        public void UpdatePassword_EmptySalt_ReturnsFalseAndChangesNothing()
        {
            int id = _repository.SaveUser("es@x.com", "oldhash", "oldsalt", Role.Customer, null);

            Assert.IsFalse(_repository.UpdatePassword(id, "newhash", "  "));

            UserRecord? user = _repository.FindByEmail("es@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual("oldhash", user.PasswordHash);
            Assert.AreEqual("oldsalt", user.Salt);
        }

        [TestMethod]
        public void UpdatePassword_WritesAuditRow()
        {
            int id = _repository.SaveUser("au@x.com", "oldhash", "oldsalt", Role.Customer, null);

            _repository.UpdatePassword(id, "newhash", "newsalt");

            Assert.AreEqual(1, _repository.CountAuditRows("password_changed"));
        }

        [TestMethod]
        public void UpdatePassword_UnknownUser_WritesNoAuditRow()
        {
            _repository.UpdatePassword(99999, "newhash", "newsalt");

            Assert.AreEqual(0, _repository.CountAuditRows("password_changed"));
        }
    }
}