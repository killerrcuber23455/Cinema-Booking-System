// cancel booking tests, written first so they fail until CancelBooking follows the rules (owner: akilesh)
// rules from card 399: who may cancel, seats freed again, points after a cancel
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
[TestCategory("Database")]
public class CancelBookingTests
{
    private const int SHOWING_ID = 1;
    private const string POINTS_NOT_SAVED_MESSAGE =
        "the booking was cancelled but the points could not be updated, please tell staff";

    private Database? _database;
    private ShowingTable _showings = new ShowingTable();
    private readonly List<int> _created_ids = new List<int>();

    // the TEST database connection string is set once on this computer as CINEMA_TEST_DB
    // the real database is never used, so a name without "Test" in it is refused
    private BookingService MakeService()
    {
        string? connection_string = Environment.GetEnvironmentVariable("CINEMA_TEST_DB");

        if (string.IsNullOrWhiteSpace(connection_string))
        {
            Assert.Inconclusive("CINEMA_TEST_DB is not set, the TEST database is needed for this test");
        }
        if (!connection_string.Contains("Test", StringComparison.OrdinalIgnoreCase))
        {
            Assert.Inconclusive("CINEMA_TEST_DB does not look like the TEST database, nothing was run");
        }

        _database = new Database(connection_string);
        _showings = new ShowingTable();
        _showings.CreateShowing(SHOWING_ID, 1, 1, DateTime.Now.AddDays(1), 250m, 8, 12);
        return new BookingService(_database, _showings);
    }

    // frees the seats of every booking the test made, so a rerun never meets a taken seat
    [TestCleanup]
    public void CleanUp()
    {
        if (_database == null)
        {
            return;
        }
        for (int index = 0; index < _created_ids.Count; index++)
        {
            try
            {
                _database.CancelBooking(_created_ids[index]);
            }
            catch (Exception)
            {
                // the database may be down, nothing more can be done here
            }
        }
    }

    private static string NewEmail()
    {
        return "t" + Guid.NewGuid().ToString("N") + "@test.com";
    }

    private static string NewReference()
    {
        return "CB-" + Guid.NewGuid().ToString("N").Substring(0, 6).ToUpper();
    }

    private int AddMember(int points, int lifetime_points, Tier tier = Tier.Standard)
    {
        var member = new Member(0, "Test Member", NewEmail(), "5712 3456", tier, points, lifetime_points,
            DateTime.Now.AddMinutes(-1));
        return _database!.SaveMember(member);
    }

    private Member GetMember(int member_id)
    {
        Member[] members = _database!.GetAllMembers();
        for (int index = 0; index < members.Length; index++)
        {
            if (members[index].Id == member_id)
            {
                return members[index];
            }
        }
        Assert.Fail("the test member was not found");
        return null!;
    }

    // saves a booking straight to the database so the points earned and used are exactly what the test says
    private Booking SaveMemberBooking(int member_id, string seat, int points_earned = 0, int points_used = 0)
    {
        var booking = new Booking(0, NewReference(), SHOWING_ID, member_id, "", "", "",
            new string[] { seat }, 250m, points_earned, points_used, BookingStatus.Confirmed,
            DateTime.Now.AddMinutes(-1));
        return Store(booking, seat);
    }

    private Booking SaveGuestBooking(string email, string seat)
    {
        var booking = new Booking(0, NewReference(), SHOWING_ID, null, "Kevin", email, "5712 3456",
            new string[] { seat }, 250m, 0, 0, BookingStatus.Confirmed, DateTime.Now.AddMinutes(-1));
        return Store(booking, seat);
    }

    private Booking Store(Booking booking, string seat)
    {
        int new_id = _database!.SaveBooking(booking);
        Assert.IsTrue(new_id > 0, "seat " + seat + " is still taken in the TEST database from an earlier run");
        booking.Id = new_id;
        _created_ids.Add(new_id);
        return booking;
    }

    // who may cancel

    [TestMethod]
    public void CancelBooking_CustomerOwnBooking_IsAccepted()
    {
        var service = MakeService();
        int member_id = AddMember(0, 0);
        var booking = SaveMemberBooking(member_id, "B1");

        bool result = service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out string error_message);

        Assert.IsTrue(result, error_message);
        Assert.AreEqual("", error_message);
        Assert.AreEqual(BookingStatus.Cancelled, _database!.FindBooking(booking.Reference)!.Status);
    }

    [TestMethod]
    public void CancelBooking_CustomerSomeoneElsesBooking_IsRefused()
    {
        var service = MakeService();
        int owner_id = AddMember(0, 0);
        int other_id = AddMember(0, 0);
        var booking = SaveMemberBooking(owner_id, "B2");

        bool result = service.CancelBooking(Role.Customer, other_id, "", booking.Reference, out string error_message);

        Assert.IsFalse(result);
        Assert.AreNotEqual("", error_message);
        Assert.AreEqual(BookingStatus.Confirmed, _database!.FindBooking(booking.Reference)!.Status);
    }

    [TestMethod]
    public void CancelBooking_GuestWithMatchingEmail_IsAccepted()
    {
        var service = MakeService();
        string email = NewEmail();
        var booking = SaveGuestBooking(email, "B3");

        bool result = service.CancelBooking(Role.Guest, null, email, booking.Reference, out string error_message);

        Assert.IsTrue(result, error_message);
        Assert.AreEqual(BookingStatus.Cancelled, _database!.FindBooking(booking.Reference)!.Status);
    }

    [TestMethod]
    public void CancelBooking_GuestEmailInOtherCapitalsAndWithSpaces_IsAccepted()
    {
        var service = MakeService();
        string email = NewEmail();
        var booking = SaveGuestBooking(email, "B4");

        bool result = service.CancelBooking(Role.Guest, null, "  " + email.ToUpper() + " ", booking.Reference,
            out string error_message);

        Assert.IsTrue(result, error_message);
    }

    [TestMethod]
    public void CancelBooking_GuestWithWrongEmail_IsRefusedWithTheSameMessageAsNotFound()
    {
        var service = MakeService();
        var booking = SaveGuestBooking(NewEmail(), "B5");

        bool result = service.CancelBooking(Role.Guest, null, NewEmail(), booking.Reference, out string wrong_message);
        service.CancelBooking(Role.Guest, null, NewEmail(), NewReference(), out string not_found_message);

        Assert.IsFalse(result);
        Assert.AreNotEqual("", wrong_message);
        // a wrong email must not tell the person that the reference exists
        Assert.AreEqual(not_found_message, wrong_message);
        Assert.AreEqual(BookingStatus.Confirmed, _database!.FindBooking(booking.Reference)!.Status);
    }

    [TestMethod]
    public void CancelBooking_GuestWithEmptyEmail_IsRefused()
    {
        var service = MakeService();
        var booking = SaveGuestBooking(NewEmail(), "B6");

        bool result = service.CancelBooking(Role.Guest, null, "", booking.Reference, out string error_message);

        Assert.IsFalse(result);
        Assert.AreNotEqual("", error_message);
    }

    [TestMethod]
    public void CancelBooking_GuestOnAMemberBooking_IsRefused()
    {
        var service = MakeService();
        int member_id = AddMember(0, 0);
        var booking = SaveMemberBooking(member_id, "B7");

        bool result = service.CancelBooking(Role.Guest, null, NewEmail(), booking.Reference, out string error_message);

        Assert.IsFalse(result);
        Assert.AreNotEqual("", error_message);
        Assert.AreEqual(BookingStatus.Confirmed, _database!.FindBooking(booking.Reference)!.Status);
    }

    [TestMethod]
    [DataRow(Role.Staff, "B8")]
    [DataRow(Role.Admin, "B9")]
    public void CancelBooking_StaffOrAdmin_CanCancelAnyBooking(Role role, string seat)
    {
        var service = MakeService();
        int member_id = AddMember(0, 0);
        var booking = SaveMemberBooking(member_id, seat);

        bool result = service.CancelBooking(role, null, "", booking.Reference, out string error_message);

        Assert.IsTrue(result, error_message);
        Assert.AreEqual(BookingStatus.Cancelled, _database!.FindBooking(booking.Reference)!.Status);
    }

    // bad references

    [TestMethod]
    public void CancelBooking_UnknownReference_IsRefused()
    {
        var service = MakeService();

        bool result = service.CancelBooking(Role.Admin, null, "", NewReference(), out string error_message);

        Assert.IsFalse(result);
        Assert.AreNotEqual("", error_message);
    }

    [TestMethod]
    [DataRow("")]
    [DataRow("   ")]
    [DataRow("not a reference")]
    [DataRow("CB-12")]
    public void CancelBooking_BadReferenceFormat_IsRefused(string reference)
    {
        var service = MakeService();

        bool result = service.CancelBooking(Role.Admin, null, "", reference, out string error_message);

        Assert.IsFalse(result);
        Assert.AreNotEqual("", error_message);
    }

    [TestMethod]
    public void CancelBooking_AlreadyCancelled_IsRefused()
    {
        var service = MakeService();
        int member_id = AddMember(0, 0);
        var booking = SaveMemberBooking(member_id, "B10");
        service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out _);

        bool result = service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out string error_message);

        Assert.IsFalse(result);
        Assert.AreNotEqual("", error_message);
    }

    // seats

    [TestMethod]
    public void CancelBooking_AfterCancel_SeatsAreFreeAgain()
    {
        var service = MakeService();
        int member_id = AddMember(0, 0);
        bool booked = service.BookSeats(Role.Customer, member_id, "", "", "", SHOWING_ID,
            new string[] { "B11" }, 0, out Booking? booking, out string book_message);
        Assert.IsTrue(booked, book_message);
        _created_ids.Add(booking!.Id);
        Assert.IsFalse(_showings.GetGrid(SHOWING_ID)!.IsSeatFree("B11"));

        service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out _);

        Assert.IsTrue(_showings.GetGrid(SHOWING_ID)!.IsSeatFree("B11"));
        Assert.HasCount(0, _database!.FindBooking(booking.Reference)!.Seats);
    }

    // points: new points = points - earned + used, lifetime points go down by the points earned

    [TestMethod]
    public void CancelBooking_Has80PointsAndBookingEarned10_Leaves70()
    {
        var service = MakeService();
        int member_id = AddMember(80, 80);
        var booking = SaveMemberBooking(member_id, "B12", points_earned: 10, points_used: 0);

        service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out _);

        Member member = GetMember(member_id);
        Assert.AreEqual(70, member.Points);
        Assert.AreEqual(70, member.LifetimePoints);
    }

    [TestMethod]
    public void CancelBooking_BookingUsed20Points_GivesThemBack()
    {
        var service = MakeService();
        int member_id = AddMember(80, 100);
        var booking = SaveMemberBooking(member_id, "B13", points_earned: 10, points_used: 20);

        service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out _);

        Member member = GetMember(member_id);
        Assert.AreEqual(90, member.Points);
        Assert.AreEqual(90, member.LifetimePoints);
    }

    [TestMethod]
    public void CancelBooking_EarnedMoreThanTheMemberHas_NeverGoesBelowZero()
    {
        var service = MakeService();
        int member_id = AddMember(5, 5);
        var booking = SaveMemberBooking(member_id, "B14", points_earned: 10, points_used: 0);

        service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out _);

        Member member = GetMember(member_id);
        Assert.AreEqual(0, member.Points);
        Assert.AreEqual(0, member.LifetimePoints);
    }

    [TestMethod]
    public void CancelBooking_LifetimePointsDropBelow100_TierGoesBackToStandard()
    {
        var service = MakeService();
        int member_id = AddMember(105, 105, Tier.Silver);
        var booking = SaveMemberBooking(member_id, "B15", points_earned: 10, points_used: 0);

        service.CancelBooking(Role.Customer, member_id, "", booking.Reference, out _);

        Member member = GetMember(member_id);
        Assert.AreEqual(95, member.LifetimePoints);
        Assert.AreEqual(Tier.Standard, member.Tier);
    }

    [TestMethod]
    public void CancelBooking_PointsCannotBeSaved_BookingIsStillCancelledWithAMessage()
    {
        var service = MakeService();
        // 100 points, this booking earned 10 and used 50: the new points would be 140 but the new
        // lifetime points only 90, and the members table refuses points above lifetime points
        int member_id = AddMember(100, 100);
        var booking = SaveMemberBooking(member_id, "B16", points_earned: 10, points_used: 50);

        bool result = service.CancelBooking(Role.Admin, null, "", booking.Reference, out string error_message);

        Assert.IsTrue(result);
        Assert.AreEqual(POINTS_NOT_SAVED_MESSAGE, error_message);
        Assert.AreEqual(BookingStatus.Cancelled, _database!.FindBooking(booking.Reference)!.Status);
    }
}
