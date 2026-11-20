// tests for the fake database: it must answer like the real Database class (owner: irwin)
// written first (red), then FakeDatabase was filled in until they pass (green), card 432.
// test ids (FAKE-01...) match docs/testing/test-plan-data-side.md.

using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class FakeDatabaseTests
{
    // ---------- helpers ----------

    // a member with a given email, id 0 until the database gives one
    private static Member MakeMember(string email)
    {
        return new Member(0, "Ann", email, "5712 3456", Tier.Standard, 0, 0, DateTime.Now);
    }

    // a confirmed guest booking for one showing and some seats
    private static Booking MakeBooking(string reference, int showing_id, string[] seats)
    {
        return new Booking(0, reference, showing_id, null, "Kevin", "kevin@mail.com", "5712 3456",
            seats, 500m, 0, 0, BookingStatus.Confirmed, DateTime.Now);
    }

    // a booking that belongs to a member
    private static Booking MakeMemberBooking(string reference, int member_id, string[] seats)
    {
        return new Booking(0, reference, 1, member_id, "", "", "",
            seats, 500m, 50, 0, BookingStatus.Confirmed, DateTime.Now);
    }

    // a showing on a screen at a time, with an empty 8 by 12 grid
    private static Showing MakeShowing(int showing_id, int screen_id, DateTime starts_at)
    {
        return new Showing(showing_id, 1, screen_id, starts_at, 250m, new SeatGrid(8, 12));
    }

    // true if a seat code is in the list (the order is not checked, see the fake's comments)
    private static bool HasSeat(string[] seats, string seat_code)
    {
        for (int index = 0; index < seats.Length; index++)
        {
            if (seats[index] == seat_code)
            {
                return true;
            }
        }
        return false;
    }

    // ---------- films ----------

    // FAKE-01: a film put in the fake comes back from LoadFilms
    [TestMethod]
    public void LoadFilms_AfterAddFilm_FilmIsLoaded()
    {
        // arrange: a fake with one film
        FakeDatabase database = new FakeDatabase();
        database.AddFilm(new Film(1, "Inception", "Sci-Fi", 2010, 148, 12, "dreams", ""));
        HashTable<int, Film> film_table = new HashTable<int, Film>();

        // act: load the films into the table
        int loaded = database.LoadFilms(film_table);

        // assert: one film loaded and it is the right one
        Assert.AreEqual(1, loaded);
        Assert.AreEqual("Inception", film_table.Find(1)!.Title);
    }

    // ---------- members ----------

    // FAKE-02: a new member gets an id above 0
    [TestMethod]
    public void SaveMember_NewEmail_ReturnsNewId()
    {
        FakeDatabase database = new FakeDatabase();

        int new_id = database.SaveMember(MakeMember("ann@mail.com"));

        Assert.IsTrue(new_id > 0);
    }

    // FAKE-03: the same email with capitals and spaces is refused with -1
    [TestMethod]
    public void SaveMember_SameEmailOtherCaseAndSpaces_ReturnsMinusOne()
    {
        // arrange: ann is already saved
        FakeDatabase database = new FakeDatabase();
        database.SaveMember(MakeMember("ann@mail.com"));

        // act: save "ANN@mail.com " (capitals and a space)
        int new_id = database.SaveMember(MakeMember(" ANN@mail.com "));

        // assert: refused, like the unique rule on members.email
        Assert.AreEqual(-1, new_id);
    }

    // FAKE-04: a saved member is in GetAllMembers with its new id
    [TestMethod]
    public void GetAllMembers_AfterSave_HasMemberWithId()
    {
        FakeDatabase database = new FakeDatabase();
        int new_id = database.SaveMember(MakeMember("ann@mail.com"));

        Member[] members = database.GetAllMembers();

        Assert.AreEqual(1, members.Length);
        Assert.AreEqual(new_id, members[0].Id);
        Assert.AreEqual("ann@mail.com", members[0].Email);
    }

    // FAKE-05: points and tier are saved for an existing member
    [TestMethod]
    public void UpdateMemberPoints_ExistingMember_SavesPoints()
    {
        // arrange: one member
        FakeDatabase database = new FakeDatabase();
        int member_id = database.SaveMember(MakeMember("ann@mail.com"));

        // act: give them 120 points and Silver
        bool is_updated = database.UpdateMemberPoints(member_id, 120, 120, Tier.Silver);

        // assert: saved, and reading the members again shows the new values
        Member saved = database.GetAllMembers()[0];
        Assert.IsTrue(is_updated);
        Assert.AreEqual(120, saved.Points);
        Assert.AreEqual(Tier.Silver, saved.Tier);
    }

    // FAKE-06: updating a member that does not exist says false
    [TestMethod]
    public void UpdateMemberPoints_MissingMember_ReturnsFalse()
    {
        FakeDatabase database = new FakeDatabase();

        bool is_updated = database.UpdateMemberPoints(99, 10, 10, Tier.Standard);

        Assert.IsFalse(is_updated);
    }

    // ---------- bookings ----------

    // FAKE-07: a booking on free seats is saved and its reference now exists
    [TestMethod]
    public void SaveBooking_FreeSeats_ReturnsIdAndReferenceExists()
    {
        FakeDatabase database = new FakeDatabase();

        int new_id = database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5", "C6" }));

        Assert.IsTrue(new_id > 0);
        Assert.IsTrue(database.ReferenceExists("CB-AAAAAA"));
    }

    // FAKE-08: the same seat in the same showing is refused with -1 and nothing is saved
    [TestMethod]
    public void SaveBooking_TakenSeat_ReturnsMinusOne()
    {
        // arrange: C5 is booked for showing 1
        FakeDatabase database = new FakeDatabase();
        database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5" }));

        // act: a second booking wants C4 and C5
        int new_id = database.SaveBooking(MakeBooking("CB-BBBBBB", 1, new string[] { "C4", "C5" }));

        // assert: refused, and C4 was not taken either (all or nothing)
        Assert.AreEqual(-1, new_id);
        Assert.IsFalse(database.ReferenceExists("CB-BBBBBB"));
        Assert.IsFalse(HasSeat(database.GetTakenSeats(1), "C4"));
    }

    // FAKE-09: the same seat in a different showing is fine
    [TestMethod]
    public void SaveBooking_SameSeatOtherShowing_IsSaved()
    {
        FakeDatabase database = new FakeDatabase();
        database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5" }));

        int new_id = database.SaveBooking(MakeBooking("CB-BBBBBB", 2, new string[] { "C5" }));

        Assert.IsTrue(new_id > 0);
    }

    // FAKE-10: GetTakenSeats lists the seats of a saved booking
    [TestMethod]
    public void GetTakenSeats_AfterSave_ReturnsSeats()
    {
        FakeDatabase database = new FakeDatabase();
        database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5", "C6" }));

        string[] taken = database.GetTakenSeats(1);

        Assert.AreEqual(2, taken.Length);
        Assert.IsTrue(HasSeat(taken, "C5"));
        Assert.IsTrue(HasSeat(taken, "C6"));
    }

    // FAKE-11: cancelling frees the seats, so they can be booked again
    [TestMethod]
    public void CancelBooking_Confirmed_FreesSeats()
    {
        // arrange: C5 booked
        FakeDatabase database = new FakeDatabase();
        int booking_id = database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5" }));

        // act: cancel it
        bool is_cancelled = database.CancelBooking(booking_id);

        // assert: cancelled, C5 is free and can be booked again
        Assert.IsTrue(is_cancelled);
        Assert.AreEqual(0, database.GetTakenSeats(1).Length);
        Assert.IsTrue(database.SaveBooking(MakeBooking("CB-BBBBBB", 1, new string[] { "C5" })) > 0);
    }

    // FAKE-12: cancelling twice is refused the second time (no double cancel)
    [TestMethod]
    public void CancelBooking_Twice_SecondReturnsFalse()
    {
        FakeDatabase database = new FakeDatabase();
        int booking_id = database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5" }));
        database.CancelBooking(booking_id);

        bool is_cancelled = database.CancelBooking(booking_id);

        Assert.IsFalse(is_cancelled);
    }

    // FAKE-13: cancelling a booking that does not exist says false
    [TestMethod]
    public void CancelBooking_MissingId_ReturnsFalse()
    {
        FakeDatabase database = new FakeDatabase();

        bool is_cancelled = database.CancelBooking(99);

        Assert.IsFalse(is_cancelled);
    }

    // FAKE-14: FindBooking gives back the booking with its seats
    [TestMethod]
    public void FindBooking_Saved_HasSeatsAndStatus()
    {
        FakeDatabase database = new FakeDatabase();
        database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5", "C6" }));

        Booking? found = database.FindBooking("CB-AAAAAA");

        Assert.IsNotNull(found);
        Assert.AreEqual(2, found.Seats.Length);
        Assert.AreEqual(BookingStatus.Confirmed, found.Status);
    }

    // FAKE-15: a reference that was never saved gives null
    [TestMethod]
    public void FindBooking_MissingReference_ReturnsNull()
    {
        FakeDatabase database = new FakeDatabase();

        Booking? found = database.FindBooking("CB-ZZZZZZ");

        Assert.IsNull(found);
    }

    // FAKE-16: GetBookingsForMember only returns that member's bookings
    [TestMethod]
    public void GetBookingsForMember_TwoMembers_OnlyTheirOwn()
    {
        // arrange: member 1 has two bookings, member 2 has one
        FakeDatabase database = new FakeDatabase();
        database.SaveBooking(MakeMemberBooking("CB-AAAAAA", 1, new string[] { "A1" }));
        database.SaveBooking(MakeMemberBooking("CB-BBBBBB", 1, new string[] { "A2" }));
        database.SaveBooking(MakeMemberBooking("CB-CCCCCC", 2, new string[] { "A3" }));

        // act
        Booking[] bookings = database.GetBookingsForMember(1);

        // assert: only member 1's two bookings
        Assert.AreEqual(2, bookings.Length);
    }

    // ---------- showings ----------

    // FAKE-17: the same showing id twice is refused
    [TestMethod]
    public void SaveShowing_SameIdTwice_SecondReturnsFalse()
    {
        FakeDatabase database = new FakeDatabase();
        database.SaveShowing(MakeShowing(1, 1, new DateTime(2026, 12, 1, 20, 30, 0)));

        bool is_saved = database.SaveShowing(MakeShowing(1, 2, new DateTime(2026, 12, 2, 20, 30, 0)));

        Assert.IsFalse(is_saved);
    }

    // FAKE-18: two showings on the same screen at the same time are refused (decision 16 nov)
    [TestMethod]
    public void SaveShowing_SameScreenSameTime_SecondReturnsFalse()
    {
        FakeDatabase database = new FakeDatabase();
        DateTime starts_at = new DateTime(2026, 12, 1, 20, 30, 0);
        database.SaveShowing(MakeShowing(1, 1, starts_at));

        bool is_saved = database.SaveShowing(MakeShowing(2, 1, starts_at));

        Assert.IsFalse(is_saved);
    }

    // FAKE-19: LoadShowings marks the booked seats as taken in the grid
    [TestMethod]
    public void LoadShowings_WithBooking_SeatIsTakenInGrid()
    {
        // arrange: showing 1 with C5 booked
        FakeDatabase database = new FakeDatabase();
        database.SaveShowing(MakeShowing(1, 1, new DateTime(2026, 12, 1, 20, 30, 0)));
        database.SaveBooking(MakeBooking("CB-AAAAAA", 1, new string[] { "C5" }));
        ShowingTable showing_table = new ShowingTable();

        // act: load the showings into a table
        int added = database.LoadShowings(showing_table);

        // assert: one showing, C5 is taken and C6 is free
        SeatGrid grid = showing_table.GetGrid(1)!;
        Assert.AreEqual(1, added);
        Assert.IsFalse(grid.IsSeatFree("C5"));
        Assert.IsTrue(grid.IsSeatFree("C6"));
    }

    // ---------- a service using the fake ----------

    // FAKE-20: BookingService works with the fake, no sql server needed
    // this is how akilesh and shankar build a service in their tests
    [TestMethod]
    public void BookingService_WithFake_GuestBookingIsSaved()
    {
        // arrange: a fake with showing 1, loaded into the table the service uses
        FakeDatabase fake_database = new FakeDatabase();
        fake_database.SaveShowing(MakeShowing(1, 1, new DateTime(2026, 12, 1, 20, 30, 0)));
        ShowingTable showing_table = new ShowingTable();
        fake_database.LoadShowings(showing_table);
        BookingService service = new BookingService(fake_database, showing_table);

        // act: a guest books C5 and C6
        bool is_booked = service.BookSeats(Role.Guest, null, "Kevin", "kevin@mail.com", "5712 3456",
            1, new string[] { "C5", "C6" }, 0, out Booking? booking, out string error_message);

        // assert: booked, and the fake now holds the two seats
        Assert.IsTrue(is_booked, error_message);
        Assert.IsNotNull(booking);
        Assert.AreEqual(2, fake_database.GetTakenSeats(1).Length);
    }
}
