// tests GetTakenSeats, SaveShowing, LoadShowings and DeleteShowing against the separate test database (owner: kushal)
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
    public class DatabaseShowingTests
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

            Run("DELETE FROM booked_seats; DELETE FROM bookings; DELETE FROM showings; DELETE FROM films;");
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

        private Showing MakeShowing(int id, int hour)
        {
            return new Showing(id, 1, 1, new DateTime(2026, 12, 1, hour, 0, 0), 8.50m, new SeatGrid(8, 12));
        }

        private int SaveConfirmedBooking(string reference, int showing_id, string seat)
        {
            Booking booking = new Booking(0, reference, showing_id, null, "Guest", "g@x.com", "5712345",
                new string[] { seat }, 8.50m, 0, 0, BookingStatus.Confirmed, DateTime.Now.AddMinutes(-1));
            return _database.SaveBooking(booking);
        }

        private int CountShowings()
        {
            return _database.LoadShowings(new ShowingTable());
        }

        [TestMethod]
        public void SaveShowing_New_ReturnsTrue()
        {
            Assert.IsTrue(_database.SaveShowing(MakeShowing(1, 18)));
        }

        [TestMethod]
        public void SaveShowing_DuplicateId_ReturnsFalse()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            Assert.IsFalse(_database.SaveShowing(MakeShowing(1, 20)));
        }

        [TestMethod]
        public void SaveShowing_SameScreenSameTime_ReturnsFalse()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            Assert.IsFalse(_database.SaveShowing(MakeShowing(2, 18)));
        }

        [TestMethod]
        public void GetTakenSeats_NoBookings_ReturnsEmpty()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            Assert.IsEmpty(_database.GetTakenSeats(1));
        }

        [TestMethod]
        public void GetTakenSeats_ReturnsConfirmedSeats()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            SaveConfirmedBooking("T-0001", 1, "A1");
            SaveConfirmedBooking("T-0002", 1, "C5");

            string[] taken = _database.GetTakenSeats(1);

            Assert.HasCount(2, taken);
            Assert.AreEqual("A1", taken[0]);
            Assert.AreEqual("C5", taken[1]);
        }

        [TestMethod]
        public void GetTakenSeats_CancelledBookingSeatIsFree()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            int id = SaveConfirmedBooking("T-0003", 1, "B2");
            _database.CancelBooking(id);

            Assert.IsEmpty(_database.GetTakenSeats(1));
        }

        [TestMethod]
        public void LoadShowings_FillsTableAndMarksTakenSeats()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            SaveConfirmedBooking("T-0004", 1, "D4");

            ShowingTable table = new ShowingTable();
            int added = _database.LoadShowings(table);

            Assert.AreEqual(1, added);
            SeatGrid? grid = table.GetGrid(1);
            Assert.IsNotNull(grid);
            Assert.IsFalse(grid.IsSeatFree("D4"));
            Assert.IsTrue(grid.IsSeatFree("D5"));
        }

        [TestMethod]
        public void DeleteShowing_ExistingShowing_ReturnsTrue()
        {
            _database.SaveShowing(MakeShowing(1, 18));

            Assert.IsTrue(_database.DeleteShowing(1));
        }

        [TestMethod]
        public void DeleteShowing_RemovesTheShowing()
        {
            _database.SaveShowing(MakeShowing(1, 18));

            _database.DeleteShowing(1);

            Assert.AreEqual(0, CountShowings());
        }

        [TestMethod]
        public void DeleteShowing_UnknownId_ReturnsFalse()
        {
            _database.SaveShowing(MakeShowing(1, 18));

            Assert.IsFalse(_database.DeleteShowing(99));
            Assert.AreEqual(1, CountShowings());
        }

        [TestMethod]
        public void DeleteShowing_ShowingWithBooking_ReturnsFalseAndKeepsIt()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            SaveConfirmedBooking("T-0005", 1, "A1");

            Assert.IsFalse(_database.DeleteShowing(1));
            Assert.AreEqual(1, CountShowings());
        }

        [TestMethod]
        public void DeleteShowing_ShowingWithCancelledBooking_ReturnsFalse()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            int id = SaveConfirmedBooking("T-0006", 1, "B2");
            _database.CancelBooking(id);

            Assert.IsFalse(_database.DeleteShowing(1));
        }

        [TestMethod]
        public void DeleteShowing_FreesTheScreenTime()
        {
            _database.SaveShowing(MakeShowing(1, 18));

            _database.DeleteShowing(1);

            Assert.IsTrue(_database.SaveShowing(MakeShowing(2, 18)));
        }

        [TestMethod]
        public void DeleteShowing_BookingOnAnotherShowing_DoesNotBlock()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            _database.SaveShowing(MakeShowing(2, 20));
            SaveConfirmedBooking("T-0007", 2, "C3");

            Assert.IsTrue(_database.DeleteShowing(1));
            Assert.AreEqual(1, CountShowings());
        }
    }
}