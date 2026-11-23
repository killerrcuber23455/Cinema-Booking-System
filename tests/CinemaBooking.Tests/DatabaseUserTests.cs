// integration tests for the users and audit_log tables on the TEST database (owner: irwin)
// card 337. kushal's UserRepositoryTests check the normal cases; these add read-back, bad data,
// security text, database down and a timing.

using System.Diagnostics;
using CinemaBooking.Data;
using CinemaBooking.Models;
using Microsoft.Data.SqlClient;

namespace CinemaBooking.Tests;

// these tests share the same tables, so they must not run at the same time as each other
[DoNotParallelize]
[TestClass]
public class DatabaseUserTests
{
    private string _connection_string = "";
    private UserRepository _repository = null!;

    // runs before every test: find the test database, then empty the two tables
    [TestInitialize]
    public void Setup()
    {
        string? value = Environment.GetEnvironmentVariable("CINEMA_TEST_DB");

        // safety: only ever run on a database whose name has "Test" in it, never the real one
        if (string.IsNullOrWhiteSpace(value) || !value.Contains("Test", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Inconclusive("Set CINEMA_TEST_DB to a TEST database (name must contain Test).");
        }

        _connection_string = value;
        _repository = new UserRepository(_connection_string);

        // audit_log first, because its rows point at users
        RunSql("DELETE FROM audit_log; DELETE FROM users;");
    }

    // helper: runs one sql command on the test database (only used to set up and check)
    private void RunSql(string sql)
    {
        using (SqlConnection connection = new SqlConnection(_connection_string))
        using (SqlCommand command = new SqlCommand(sql, connection))
        {
            connection.Open();
            command.ExecuteNonQuery();
        }
    }

    // helper: runs a sql query that gives back one value
    private object? ReadOneValue(string sql)
    {
        using (SqlConnection connection = new SqlConnection(_connection_string))
        using (SqlCommand command = new SqlCommand(sql, connection))
        {
            connection.Open();
            return command.ExecuteScalar();
        }
    }

    // helper: adds a member row and gives back its id (a random email, so it never clashes)
    private int AddMemberRow()
    {
        string email = "member_" + Guid.NewGuid().ToString("N") + "@x.com";

        using (SqlConnection connection = new SqlConnection(_connection_string))
        using (SqlCommand command = new SqlCommand(
            "INSERT INTO members (name, email, phone) OUTPUT INSERTED.id VALUES (@name, @email, @phone)",
            connection))
        {
            // parameters, never joined text, even in tests (team rule)
            command.Parameters.AddWithValue("@name", "Ann");
            command.Parameters.AddWithValue("@email", email);
            command.Parameters.AddWithValue("@phone", "5712 3456");

            connection.Open();
            return Convert.ToInt32(command.ExecuteScalar());
        }
    }

    // ---------- save and read back ----------

    // USER-01: a customer linked to a member reads back with every field the same
    [TestMethod]
    public void SaveUser_CustomerWithMember_ReadsBackAllFields()
    {
        // arrange: a member row for the customer to point at
        int member_id = AddMemberRow();

        // act: save, then read back
        int user_id = _repository.SaveUser("Ann@Mail.com", "hash123", "salt123", Role.Customer, member_id);
        UserRecord? user = _repository.FindByEmail("ann@mail.com");

        // assert: same id, email stored in lowercase, everything else unchanged
        Assert.IsNotNull(user);
        Assert.AreEqual(user_id, user.Id);
        Assert.AreEqual("ann@mail.com", user.Email);
        Assert.AreEqual("hash123", user.PasswordHash);
        Assert.AreEqual("salt123", user.Salt);
        Assert.AreEqual(Role.Customer, user.Role);
        Assert.AreEqual(member_id, user.MemberId);
    }

    // ---------- audit log ----------

    // AUD-01: saving a user writes an audit row with the user id, the details and the time
    [TestMethod]
    public void SaveUser_WritesAuditRowWithTime()
    {
        // act: save one admin
        int user_id = _repository.SaveUser("boss@x.com", "h", "s", Role.Admin, null);

        // assert: one audit row for that user, with the email in the details
        object? row_user_id = ReadOneValue("SELECT user_id FROM audit_log WHERE action = 'user_created'");
        object? details = ReadOneValue("SELECT details FROM audit_log WHERE action = 'user_created'");
        object? created_at = ReadOneValue("SELECT created_at FROM audit_log WHERE action = 'user_created'");

        Assert.AreEqual(user_id, Convert.ToInt32(row_user_id));
        Assert.AreEqual("boss@x.com as Admin", details);

        // the time is set by the database, so it should be within the last minute
        TimeSpan age = DateTime.Now - Convert.ToDateTime(created_at);
        Assert.IsTrue(age.TotalMinutes < 1, "audit time was " + created_at);
    }

    // AUD-03: an audit row for a real user keeps the user id
    [TestMethod]
    public void WriteAudit_WithUser_KeepsUserId()
    {
        int user_id = _repository.SaveUser("staff@x.com", "h", "s", Role.Staff, null);

        bool is_written = _repository.WriteAudit(user_id, "film_added", "film 5");

        Assert.IsTrue(is_written);
        object? row_user_id = ReadOneValue("SELECT user_id FROM audit_log WHERE action = 'film_added'");
        Assert.AreEqual(user_id, Convert.ToInt32(row_user_id));
    }

    // AUD-04: an empty action is refused and nothing is written
    [TestMethod]
    public void WriteAudit_EmptyAction_ReturnsFalse()
    {
        bool is_written = _repository.WriteAudit(null, "   ", "details");

        Assert.IsFalse(is_written);
        Assert.AreEqual(0, Convert.ToInt32(ReadOneValue("SELECT COUNT(*) FROM audit_log")));
    }

    // ---------- security ----------

    // SEC-03: sql injection text in the email is stored as plain text, the users table is still there
    [TestMethod]
    public void SaveUser_SqlInjectionEmail_StoredAsText()
    {
        string attack_email = "x'); DROP TABLE users;--@x.com";

        int user_id = _repository.SaveUser(attack_email, "h", "s", Role.Customer, null);
        UserRecord? user = _repository.FindByEmail(attack_email);

        // parameterised sql treats it as text: saved, found, and the table was not dropped
        Assert.IsTrue(user_id > 0);
        Assert.IsNotNull(user);
        Assert.AreEqual(1, Convert.ToInt32(ReadOneValue("SELECT COUNT(*) FROM users")));
    }

    // SEC-04: an apostrophe in the email is saved and read back correctly
    [TestMethod]
    public void SaveUser_ApostropheEmail_ReadsBack()
    {
        _repository.SaveUser("o'brien@x.com", "h", "s", Role.Customer, null);

        UserRecord? user = _repository.FindByEmail("o'brien@x.com");

        Assert.IsNotNull(user);
        Assert.AreEqual("o'brien@x.com", user.Email);
    }

    // ---------- boundary and negative ----------

    // BND-05: an empty or blank email is refused with -1
    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    public void SaveUser_BlankEmail_ReturnsMinusOne(string email)
    {
        int user_id = _repository.SaveUser(email, "h", "s", Role.Customer, null);

        Assert.AreEqual(-1, user_id);
    }

    // BND-06: finding a blank email gives null, not a crash
    [TestMethod]
    public void FindByEmail_BlankEmail_ReturnsNull()
    {
        Assert.IsNull(_repository.FindByEmail("  "));
    }

    // BND-07: guests have no account, so a Guest user must be refused with -1, not a crash
    // (the users table only allows Customer, Staff and Admin)
    [TestMethod]
    public void SaveUser_GuestRole_ReturnsMinusOne()
    {
        int user_id = _repository.SaveUser("guest@x.com", "h", "s", Role.Guest, null);

        Assert.AreEqual(-1, user_id);
    }

    // BND-08: a negative number of failed attempts must be refused with false, not a crash
    [TestMethod]
    public void SetFailedAttempts_Negative_ReturnsFalse()
    {
        int user_id = _repository.SaveUser("cy@x.com", "h", "s", Role.Customer, null);

        bool is_saved = _repository.SetFailedAttempts(user_id, -1, null);

        Assert.IsFalse(is_saved);
    }

    // BND-09: an email longer than the column (100 letters) must be refused with -1, not a crash
    [TestMethod]
    public void SaveUser_EmailTooLong_ReturnsMinusOne()
    {
        // 95 letters + "@x.com" = 101 letters
        string long_email = new string('a', 95) + "@x.com";

        int user_id = _repository.SaveUser(long_email, "h", "s", Role.Customer, null);

        Assert.AreEqual(-1, user_id);
    }

    // BND-10: a member id that does not exist must be refused with -1, not a crash
    [TestMethod]
    public void SaveUser_MissingMemberId_ReturnsMinusOne()
    {
        int user_id = _repository.SaveUser("ann@x.com", "h", "s", Role.Customer, 999999);

        Assert.AreEqual(-1, user_id);
    }

    // ---------- database down ----------

    // DB-12b: with no server, the repository gives a clear message instead of a raw sql error
    [TestMethod]
    public void FindByEmail_DatabaseDown_ClearMessage()
    {
        // arrange: a server name that does not exist, and a short time limit so the test is quick
        UserRepository down_repository = new UserRepository(
            "Server=localhost\\NoSuchServerTest;Database=CinemaBookingTestDb;" +
            "Trusted_Connection=True;TrustServerCertificate=True;Connect Timeout=2;");

        // act: try to read, and catch the error
        string error_text = "";
        try
        {
            down_repository.FindByEmail("ann@x.com");
        }
        catch (InvalidOperationException error)
        {
            error_text = error.Message;
        }

        // assert: the plain message from UserRepository, not a crash
        Assert.AreEqual("The database could not be reached. Check that SQL Server is running and appsettings.json is correct.", error_text);
    }

    // ---------- performance ----------

    // PERF-04: save 50 users and find each one, the time is printed for the test plan
    [TestMethod]
    public void SaveAndFind_FiftyUsers_UnderTenSeconds()
    {
        Stopwatch timer = Stopwatch.StartNew();

        for (int number = 1; number <= 50; number++)
        {
            _repository.SaveUser("user" + number + "@x.com", "h", "s", Role.Customer, null);
        }
        for (int number = 1; number <= 50; number++)
        {
            Assert.IsNotNull(_repository.FindByEmail("user" + number + "@x.com"));
        }
        timer.Stop();

        Console.WriteLine("50 saves and 50 finds took " + timer.ElapsedMilliseconds + " ms");
        Assert.IsTrue(timer.ElapsedMilliseconds < 10000);
    }
}
