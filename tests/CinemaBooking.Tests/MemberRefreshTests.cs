// tests for reading a member again after a booking or a cancel, so the points on the screen are the real ones (owner: divyesh)
// written before the code for card 349 (the Settings page kept the old points after a cancel), so they fail first (red)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class MemberRefreshTests
{
    private const int SHOWING_ID = 1;

    private FakeDatabase _database = null!;
    private BookingService _booking_service = null!;
    private MemberService _member_service = null!;
    private int _member_id;
    private Member _signed_in_copy = null!;

    [TestInitialize]
    public void Setup()
    {
        _database = new FakeDatabase();
        ShowingTable showings = new ShowingTable();
        showings.CreateShowing(SHOWING_ID, 1, 1, new DateTime(2026, 12, 1, 18, 0, 0), 250m, 8, 12);
        _booking_service = new BookingService(_database, showings);
        _member_service = new MemberService(_database);

        _member_id = _database.SaveMember(new Member(0, "Ann", "ann@x.com", "5712 3456", Tier.Standard, 0, 0, new DateTime(2026, 1, 1)));

        // the window keeps the member it got at sign in, like MainForm does
        _signed_in_copy = _database.GetAllMembers()[0];
    }

    // books one seat as the member and returns the booking
    private Booking BookOneSeat()
    {
        bool is_booked = _booking_service.BookSeats(Role.Customer, _member_id, "", "", "", SHOWING_ID,
            new string[] { "C5" }, 0, out Booking? booking, out string error_message);

        Assert.IsTrue(is_booked, error_message);
        Assert.IsNotNull(booking);
        return booking!;
    }

    [TestMethod]
    public void ReloadMember_KnownMember_ReturnsThatMember()
    {
        Member? found = _member_service.ReloadMember(_member_id);

        Assert.IsNotNull(found);
        Assert.AreEqual("Ann", found!.Name);
    }

    [TestMethod]
    public void ReloadMember_UnknownId_ReturnsNull()
    {
        Assert.IsNull(_member_service.ReloadMember(999));
    }

    [TestMethod]
    public void ReloadMember_ZeroId_ReturnsNull()
    {
        Assert.IsNull(_member_service.ReloadMember(0));
    }

    [TestMethod]
    public void ReloadMember_AfterBooking_HasTheEarnedPoints()
    {
        Booking booking = BookOneSeat();

        Member? found = _member_service.ReloadMember(_member_id);

        Assert.IsNotNull(found);
        Assert.IsGreaterThan(0, booking.PointsEarned);
        Assert.AreEqual(booking.PointsEarned, found!.Points);
    }

    [TestMethod]
    public void ReloadMember_AfterCancel_HasThePointsTakenBack()
    {
        Booking booking = BookOneSeat();

        bool is_cancelled = _booking_service.CancelBooking(Role.Customer, _member_id, "", booking.Reference, out string error_message);
        Member? found = _member_service.ReloadMember(_member_id);

        Assert.IsTrue(is_cancelled, error_message);
        Assert.IsNotNull(found);
        Assert.AreEqual(0, found!.Points);
        Assert.AreEqual(0, found.LifetimePoints);
    }

    [TestMethod]
    public void ReloadMember_AfterBooking_TheCopyKeptAtSignInIsOldButTheNewReadIsRight()
    {
        Booking booking = BookOneSeat();

        Member? found = _member_service.ReloadMember(_member_id);

        // this is the bug of card 349: the copy a window keeps shows old points, so the window must read again
        Assert.AreEqual(0, _signed_in_copy.Points);
        Assert.IsNotNull(found);
        Assert.AreEqual(booking.PointsEarned, found!.Points);
    }
}
