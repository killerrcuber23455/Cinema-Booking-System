// tests UpdatePassword on the fake user repository, no database needed (owner: kushal)
using CinemaBooking.Data;
using CinemaBooking.Models;
using CinemaBooking.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class FakeUpdatePasswordTests
    {
        [TestMethod]
        public void UpdatePassword_SavesNewHashAndSalt()
        {
            FakeUserRepository repository = new FakeUserRepository();
            int id = repository.SaveUser("pw@x.com", "oldhash", "oldsalt", Role.Customer, null);

            Assert.IsTrue(repository.UpdatePassword(id, "newhash", "newsalt"));

            UserRecord? user = repository.FindByEmail("pw@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual("newhash", user.PasswordHash);
            Assert.AreEqual("newsalt", user.Salt);
        }

        [TestMethod]
        public void UpdatePassword_UnknownUser_ReturnsFalse()
        {
            FakeUserRepository repository = new FakeUserRepository();

            Assert.IsFalse(repository.UpdatePassword(99999, "newhash", "newsalt"));
        }

        [TestMethod]
        public void UpdatePassword_ClearsFailedAttemptsAndLock()
        {
            FakeUserRepository repository = new FakeUserRepository();
            int id = repository.SaveUser("lock@x.com", "oldhash", "oldsalt", Role.Customer, null);
            repository.SetFailedAttempts(id, 5, new DateTime(2030, 1, 1, 10, 0, 0));

            Assert.IsTrue(repository.UpdatePassword(id, "newhash", "newsalt"));

            UserRecord? user = repository.FindByEmail("lock@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual(0, user.FailedAttempts);
            Assert.IsNull(user.LockedUntil);
        }

        [TestMethod]
        public void UpdatePassword_DoesNotChangeOtherUsers()
        {
            FakeUserRepository repository = new FakeUserRepository();
            int first_id = repository.SaveUser("one@x.com", "hash1", "salt1", Role.Customer, null);
            repository.SaveUser("two@x.com", "hash2", "salt2", Role.Staff, null);

            Assert.IsTrue(repository.UpdatePassword(first_id, "newhash", "newsalt"));

            UserRecord? other = repository.FindByEmail("two@x.com");
            Assert.IsNotNull(other);
            Assert.AreEqual("hash2", other.PasswordHash);
            Assert.AreEqual("salt2", other.Salt);
        }

        [TestMethod]
        public void UpdatePassword_EmptyHash_ReturnsFalseAndChangesNothing()
        {
            FakeUserRepository repository = new FakeUserRepository();
            int id = repository.SaveUser("eh@x.com", "oldhash", "oldsalt", Role.Customer, null);

            Assert.IsFalse(repository.UpdatePassword(id, "", "newsalt"));

            UserRecord? user = repository.FindByEmail("eh@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual("oldhash", user.PasswordHash);
            Assert.AreEqual("oldsalt", user.Salt);
        }

        [TestMethod]
        public void UpdatePassword_EmptySalt_ReturnsFalseAndChangesNothing()
        {
            FakeUserRepository repository = new FakeUserRepository();
            int id = repository.SaveUser("es@x.com", "oldhash", "oldsalt", Role.Customer, null);

            Assert.IsFalse(repository.UpdatePassword(id, "newhash", "  "));

            UserRecord? user = repository.FindByEmail("es@x.com");
            Assert.IsNotNull(user);
            Assert.AreEqual("oldhash", user.PasswordHash);
            Assert.AreEqual("oldsalt", user.Salt);
        }

        [TestMethod]
        public void UpdatePassword_WritesAuditRowWithoutThePassword()
        {
            FakeUserRepository repository = new FakeUserRepository();
            int id = repository.SaveUser("au@x.com", "oldhash", "oldsalt", Role.Customer, null);

            repository.UpdatePassword(id, "newhash", "newsalt");

            int found = 0;

            for (int index = 0; index < repository.AuditRows.Count; index++)
            {
                AuditRow row = repository.AuditRows[index];

                if (row.Action == "password_changed")
                {
                    found++;
                    Assert.AreEqual(id, row.UserId);
                    Assert.DoesNotContain("newhash", row.Details);
                    Assert.DoesNotContain("newsalt", row.Details);
                    Assert.DoesNotContain("oldhash", row.Details);
                }
            }

            Assert.AreEqual(1, found);
        }

        [TestMethod]
        public void UpdatePassword_UnknownUser_WritesNoAuditRow()
        {
            FakeUserRepository repository = new FakeUserRepository();

            repository.UpdatePassword(99999, "newhash", "newsalt");

            int found = 0;

            for (int index = 0; index < repository.AuditRows.Count; index++)
            {
                if (repository.AuditRows[index].Action == "password_changed")
                {
                    found++;
                }
            }

            Assert.AreEqual(0, found);
        }
    }
}