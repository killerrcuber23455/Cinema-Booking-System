// tests SaveScreen and LoadScreens against the separate test database (owner: kushal)
// the two screens from cinema.sql (ids 1 and 2) stay in the table, the tests add and remove only their own.
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CinemaBooking.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class DatabaseScreenTests
    {
        private string _connection_string = "";
        private Database _database = null!;

        [TestInitialize]
        public void Setup()
        {
            string? value = Environment.GetEnvironmentVariable("CINEMA_TEST_DB");

            if (string.IsNullOrWhiteSpace(value) || !value.Contains("Test", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Inconclusive("Set CINEMA_TEST_DB to a TEST database (name must contain Test).");
            }

            _connection_string = value;
            _database = new Database(_connection_string);

            Run("DELETE FROM booked_seats; DELETE FROM bookings; DELETE FROM showings; DELETE FROM films; " +
                "DELETE FROM screens WHERE id NOT IN (1, 2);");
            Run("INSERT INTO films VALUES (1, 'Test Film', 'Drama', 2024, 100, 12, 'x', 'x.jpg');");
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

        private int CountScreens()
        {
            return _database.LoadScreens(new HashTable<int, CinemaScreen>());
        }

        [TestMethod]
        public void LoadScreens_ReturnsTheTwoSeedScreens()
        {
            HashTable<int, CinemaScreen> table = new HashTable<int, CinemaScreen>();

            int loaded = _database.LoadScreens(table);

            Assert.AreEqual(2, loaded);
            CinemaScreen? screen_one = table.Find(1);
            Assert.IsNotNull(screen_one);
            Assert.AreEqual("Screen 1", screen_one.Name);
            Assert.AreEqual(8, screen_one.RowCount);
            Assert.AreEqual(12, screen_one.SeatsPerRow);
        }

        [TestMethod]
        public void SaveScreen_NewScreen_ReturnsTrue()
        {
            Assert.IsTrue(_database.SaveScreen(new CinemaScreen(3, "Screen 3", 10, 14)));
        }

        [TestMethod]
        public void SaveScreen_NewScreen_AppearsInLoadScreens()
        {
            _database.SaveScreen(new CinemaScreen(3, "Screen 3", 10, 14));

            HashTable<int, CinemaScreen> table = new HashTable<int, CinemaScreen>();
            int loaded = _database.LoadScreens(table);

            Assert.AreEqual(3, loaded);
            CinemaScreen? screen_three = table.Find(3);
            Assert.IsNotNull(screen_three);
            Assert.AreEqual("Screen 3", screen_three.Name);
            Assert.AreEqual(10, screen_three.RowCount);
            Assert.AreEqual(14, screen_three.SeatsPerRow);
        }

        [TestMethod]
        public void SaveScreen_DuplicateId_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(1, "Another name", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_DuplicateId_KeepsTheOldScreen()
        {
            _database.SaveScreen(new CinemaScreen(1, "Another name", 5, 5));

            HashTable<int, CinemaScreen> table = new HashTable<int, CinemaScreen>();
            _database.LoadScreens(table);

            CinemaScreen? screen_one = table.Find(1);
            Assert.IsNotNull(screen_one);
            Assert.AreEqual("Screen 1", screen_one.Name);
            Assert.AreEqual(8, screen_one.RowCount);
        }

        [TestMethod]
        public void SaveScreen_DuplicateName_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "Screen 1", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_NameWithOtherCase_ReturnsFalse()
        {
            // sql server compares names without caring about upper or lower case
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "screen 1", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_ZeroRows_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "Screen 3", 0, 12)));
        }

        [TestMethod]
        public void SaveScreen_TooManyRows_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "Screen 3", 27, 12)));
        }

        [TestMethod]
        public void SaveScreen_EmptyName_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_NameTooLong_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, new string('x', 51), 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_InvalidScreen_IsNotSaved()
        {
            _database.SaveScreen(new CinemaScreen(3, "Screen 3", 27, 12));

            Assert.AreEqual(2, CountScreens());
        }

        [TestMethod]
        public void SaveScreen_NewScreen_CanBeUsedByAShowing()
        {
            _database.SaveScreen(new CinemaScreen(3, "Screen 3", 10, 14));

            Showing showing = new Showing(1, 1, 3, new DateTime(2026, 12, 1, 18, 0, 0), 8.50m, new SeatGrid(10, 14));

            Assert.IsTrue(_database.SaveShowing(showing));
        }
    }
}