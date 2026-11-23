// tests for finding a guest booking by reference and email (owner: divyesh)
// written before the code for card 404, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class BookingLookupTests
{
    private const string REFERENCE = "CB-TEST-0001";
    private const string EMAIL = "kevin@example.com";

    private FakeDatabase _database = new FakeDatabase();

    [TestInitialize]
    public void SetUp()
    {
        _database = new FakeDatabase();
    }

    private void SaveBooking(string reference, int? member_id, string email, BookingStatus status, string seat)
    {
        Booking booking = new Booking(0, reference, 1, member_id, member_id == null ? "Kevin" : "", email, "5712 3456",
            new string[] { seat }, 250m, 0, 0, status, new DateTime(2026, 11, 20, 10, 0, 0));
        _database.SaveBooking(booking);
    }

    [TestMethod]
    public void Find_RightReferenceAndEmail_GivesTheBooking()
    {
        SaveBooking(REFERENCE, null, EMAIL, BookingStatus.Confirmed, "C5");

        Booking? booking = BookingLookup.Find(_database, REFERENCE, EMAIL, out string error);

        Assert.IsNotNull(booking);
        Assert.AreEqual(REFERENCE, booking.Reference);
        Assert.AreEqual("", error);
    }

    [TestMethod]
    public void Find_EmailInOtherCaseWithSpaces_StillMatches()
    {
        SaveBooking(REFERENCE, null, EMAIL, BookingStatus.Confirmed, "C5");

        Booking? booking = BookingLookup.Find(_database, REFERENCE, "  KEVIN@Example.com ", out _);

        Assert.IsNotNull(booking);
    }

    [TestMethod]
    public void Find_ReferenceWithSpaces_IsTrimmed()
    {
        SaveBooking(REFERENCE, null, EMAIL, BookingStatus.Confirmed, "C5");

        Booking? booking = BookingLookup.Find(_database, "  " + REFERENCE + "  ", EMAIL, out _);

        Assert.IsNotNull(booking);
    }

    [TestMethod]
    public void Find_WrongEmail_GivesNothingWithTheNotFoundMessage()
    {
        SaveBooking(REFERENCE, null, EMAIL, BookingStatus.Confirmed, "C5");

        Booking? booking = BookingLookup.Find(_database, REFERENCE, "someone.else@example.com", out string error);

        Assert.IsNull(booking);
        Assert.AreEqual(BookingLookup.NOT_FOUND_TEXT, error);
    }

    [TestMethod]
    public void Find_UnknownReference_GivesTheSameMessageAsAWrongEmail()
    {
        Booking? booking = BookingLookup.Find(_database, "CB-NOTHING", EMAIL, out string error);

        Assert.IsNull(booking);
        Assert.AreEqual(BookingLookup.NOT_FOUND_TEXT, error);
    }

    [TestMethod]
    public void Find_MemberBooking_IsNotShownToAGuest()
    {
        SaveBooking(REFERENCE, 7, "", BookingStatus.Confirmed, "C5");

        Booking? booking = BookingLookup.Find(_database, REFERENCE, EMAIL, out string error);

        Assert.IsNull(booking);
        Assert.AreEqual(BookingLookup.NOT_FOUND_TEXT, error);
    }

    [TestMethod]
    public void Find_BlankEmail_AsksForTheEmail()
    {
        SaveBooking(REFERENCE, null, EMAIL, BookingStatus.Confirmed, "C5");

        Booking? booking = BookingLookup.Find(_database, REFERENCE, "  ", out string error);

        Assert.IsNull(booking);
        Assert.AreEqual(BookingLookup.EMAIL_TEXT, error);
    }

    [TestMethod]
    public void Find_BlankReference_GivesTheBookingReferenceMessage()
    {
        Booking.IsValidReference("", out string expected);

        Booking? booking = BookingLookup.Find(_database, "", EMAIL, out string error);

        Assert.IsNull(booking);
        Assert.AreEqual(expected, error);
    }

    [TestMethod]
    public void Find_CancelledBooking_IsStillFoundSoThePageCanSayCancelled()
    {
        SaveBooking(REFERENCE, null, EMAIL, BookingStatus.Cancelled, "C5");

        Booking? booking = BookingLookup.Find(_database, REFERENCE, EMAIL, out _);

        Assert.IsNotNull(booking);
        Assert.AreEqual(BookingStatus.Cancelled, booking.Status);
    }
}
