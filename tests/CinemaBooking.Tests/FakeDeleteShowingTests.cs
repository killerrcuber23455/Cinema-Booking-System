// tests DeleteShowing on the fake database, so the fake follows the same rules as the real one (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class FakeDeleteShowingTests
    {
        private FakeDatabase _database = null!;

        [TestInitialize]
        public void Setup()
        {
            _database = new FakeDatabase();
        }

        private Showing MakeShowing(int id, int hour)
        {
            return new Showing(id, 1, 1, new DateTime(2026, 12, 1, hour, 0, 0), 8.50m, new SeatGrid(8, 12));
        }

        private int SaveBookingFor(string reference, int showing_id, string seat)
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
        }

        [TestMethod]
        public void DeleteShowing_UnknownId_KeepsOtherShowings()
        {
            _database.SaveShowing(MakeShowing(1, 18));

            _database.DeleteShowing(99);

            Assert.AreEqual(1, CountShowings());
        }

        [TestMethod]
        public void DeleteShowing_ShowingWithBooking_ReturnsFalseAndKeepsIt()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            SaveBookingFor("T-0001", 1, "A1");

            Assert.IsFalse(_database.DeleteShowing(1));
            Assert.AreEqual(1, CountShowings());
        }

        [TestMethod]
        public void DeleteShowing_ShowingWithCancelledBooking_ReturnsFalse()
        {
            _database.SaveShowing(MakeShowing(1, 18));
            int booking_id = SaveBookingFor("T-0002", 1, "B2");
            _database.CancelBooking(booking_id);

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
            SaveBookingFor("T-0003", 2, "C3");

            Assert.IsTrue(_database.DeleteShowing(1));
            Assert.AreEqual(1, CountShowings());
        }
    }
}