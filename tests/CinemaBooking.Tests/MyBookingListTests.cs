// tests for the list of a member's bookings, card 349 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class MyBookingListTests
{
    private FakeDatabase _database = null!;

    [TestInitialize]
    public void Setup()
    {
        _database = new FakeDatabase();
    }

    // saves a booking with one seat, each booking uses its own seat so the fake accepts them all
    private void Save(string reference, int? member_id, string seat, DateTime created_at,
        BookingStatus status = BookingStatus.Confirmed)
    {
        Booking booking = new Booking(0, reference, 1, member_id, member_id == null ? "Guest" : "",
            member_id == null ? "guest@mail.com" : "", member_id == null ? "5712 3456" : "",
            new string[] { seat }, 300m, 30, 0, status, created_at);
        _database.SaveBooking(booking);
    }

    [TestMethod]
    public void ForMember_NotSignedIn_GivesNothingAndTheSignInMessage()
    {
        Booking[] bookings = MyBookingList.ForMember(_database, null, out string message);

        Assert.AreEqual(0, bookings.Length);
        Assert.AreEqual(MyBookingList.SIGN_IN_TEXT, message);
    }

    [TestMethod]
    public void ForMember_NoBookings_GivesNothingAndTheNoBookingsMessage()
    {
        Booking[] bookings = MyBookingList.ForMember(_database, 1, out string message);

        Assert.AreEqual(0, bookings.Length);
        Assert.AreEqual(MyBookingList.NONE_TEXT, message);
    }

    [TestMethod]
    public void ForMember_GivesOnlyThisMembersBookings()
    {
        Save("AAA111", 1, "A1", new DateTime(2026, 11, 1));
        Save("BBB222", 1, "A2", new DateTime(2026, 11, 2));
        Save("CCC333", 2, "A3", new DateTime(2026, 11, 3));
        Save("DDD444", null, "A4", new DateTime(2026, 11, 4));

        Booking[] bookings = MyBookingList.ForMember(_database, 1, out string message);

        Assert.AreEqual(2, bookings.Length);
        Assert.AreEqual("", message);
        Assert.AreEqual(1, bookings[0].MemberId);
        Assert.AreEqual(1, bookings[1].MemberId);
    }

    [TestMethod]
    public void ForMember_NewestBookingComesFirst()
    {
        Save("AAA111", 1, "A1", new DateTime(2026, 11, 1));
        Save("CCC333", 1, "A3", new DateTime(2026, 11, 3));
        Save("BBB222", 1, "A2", new DateTime(2026, 11, 2));

        Booking[] bookings = MyBookingList.ForMember(_database, 1, out _);

        Assert.AreEqual(3, bookings.Length);
        Assert.AreEqual("CCC333", bookings[0].Reference);
        Assert.AreEqual("BBB222", bookings[1].Reference);
        Assert.AreEqual("AAA111", bookings[2].Reference);
    }

    [TestMethod]
    public void ForMember_CancelledBookingsAreStillListed()
    {
        Save("AAA111", 1, "A1", new DateTime(2026, 11, 1), BookingStatus.Cancelled);

        Booking[] bookings = MyBookingList.ForMember(_database, 1, out _);

        Assert.AreEqual(1, bookings.Length);
        Assert.AreEqual(BookingStatus.Cancelled, bookings[0].Status);
    }

    [TestMethod]
    public void ForMember_DatabaseDown_GivesNothingAndTheDatabaseMessage()
    {
        _database.ThrowOnLoadBookings = true;

        Booking[] bookings = MyBookingList.ForMember(_database, 1, out string message);

        Assert.AreEqual(0, bookings.Length);
        Assert.AreEqual(MyBookingList.DATABASE_DOWN_TEXT, message);
    }
}
