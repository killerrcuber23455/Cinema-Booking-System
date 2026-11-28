// tests for removing a showing so that the database forgets it too, and it does not come back after a restart (owner: kushal)
// written before the code (card 405), so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class ShowingStoreRemoveTests
{
    private static readonly DateTime SIX_THIRTY = new DateTime(2026, 12, 1, 18, 30, 0);
    private static readonly DateTime EIGHT_THIRTY = new DateTime(2026, 12, 1, 20, 30, 0);

    private FakeDatabase _database = null!;
    private ShowingTable _table = null!;
    private ShowingStore _store = null!;

    // only film 1 exists
    private static bool FilmExists(int film_id)
    {
        return film_id == 1;
    }

    [TestInitialize]
    public void Setup()
    {
        _database = new FakeDatabase();
        _table = new ShowingTable();
        ShowingAdminService service = new ShowingAdminService(_table, FilmExists);
        _store = new ShowingStore(_database, service, _table);
    }

    // adds a showing the normal way, so it is in the table and in the database
    private void AddShowing(int showing_id, DateTime starts_at)
    {
        bool is_added = _store.AddShowing(showing_id, 1, 1, starts_at, 250m, Role.Admin, out string message);
        Assert.IsTrue(is_added, message);
    }

    // reads the showing from the fake database like a restart does: a fresh table filled from the database
    private Showing? ShowingInDatabase(int showing_id)
    {
        ShowingTable table = new ShowingTable();
        _database.LoadShowings(table);
        return table.FindShowing(showing_id);
    }

    [TestMethod]
    public void RemoveShowing_Staff_RemovesFromTableAndDatabase()
    {
        AddShowing(10, SIX_THIRTY);

        bool is_removed = _store.RemoveShowing(10, Role.Staff, out string message);

        Assert.IsTrue(is_removed);
        Assert.AreEqual("", message);
        Assert.IsNull(_table.FindShowing(10));
        Assert.IsNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void RemoveShowing_Admin_RemovesFromTableAndDatabase()
    {
        AddShowing(10, SIX_THIRTY);

        bool is_removed = _store.RemoveShowing(10, Role.Admin, out string message);

        Assert.IsTrue(is_removed);
        Assert.IsNull(_table.FindShowing(10));
        Assert.IsNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void RemoveShowing_Customer_ReturnsFalseAndKeepsIt()
    {
        AddShowing(10, SIX_THIRTY);

        bool is_removed = _store.RemoveShowing(10, Role.Customer, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("only staff and admins can change showings", message);
        Assert.IsNotNull(_table.FindShowing(10));
        Assert.IsNotNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void RemoveShowing_UnknownId_ReturnsFalse()
    {
        bool is_removed = _store.RemoveShowing(99, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("showing id 99 was not found", message);
    }

    [TestMethod]
    public void RemoveShowing_ShowingWithBookedSeats_ReturnsFalseAndKeepsIt()
    {
        AddShowing(10, SIX_THIRTY);
        SeatGrid? grid = _table.GetGrid(10);
        Assert.IsNotNull(grid);
        grid.BookSeats(new string[] { "A1" });

        bool is_removed = _store.RemoveShowing(10, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("showing id 10 has booked seats and cannot be removed", message);
        Assert.IsNotNull(_table.FindShowing(10));
        Assert.IsNotNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void RemoveShowing_DatabaseRefuses_PutsTheShowingBack()
    {
        // a cancelled booking frees its seats, so the program allows the removal,
        // but the database still has the booking, so it refuses (the same as the foreign key in sql server)
        AddShowing(10, SIX_THIRTY);
        Booking booking = new Booking(0, "T-0001", 10, null, "Guest", "g@x.com", "5712345",
            new string[] { "A1" }, 250m, 0, 0, BookingStatus.Confirmed, DateTime.Now.AddMinutes(-1));
        int booking_id = _database.SaveBooking(booking);
        _database.CancelBooking(booking_id);

        bool is_removed = _store.RemoveShowing(10, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual(ShowingStore.REMOVE_FAILED_TEXT, message);
        Assert.IsNotNull(_table.FindShowing(10));
        Assert.IsNotNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void RemoveShowing_DatabaseThrows_PutsTheShowingBack()
    {
        AddShowing(10, SIX_THIRTY);
        _database.ThrowOnDeleteShowing = true;

        bool is_removed = _store.RemoveShowing(10, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual(ShowingStore.REMOVE_FAILED_TEXT, message);
        Assert.IsNotNull(_table.FindShowing(10));
    }

    [TestMethod]
    public void RemoveShowing_AfterRemoving_SameIdAndTimeCanBeAddedAgain()
    {
        AddShowing(10, SIX_THIRTY);
        _store.RemoveShowing(10, Role.Admin, out string message);

        bool is_added = _store.AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Admin, out message);

        Assert.IsTrue(is_added);
    }

    [TestMethod]
    public void RemoveShowing_OtherShowingsStay()
    {
        AddShowing(10, SIX_THIRTY);
        AddShowing(11, EIGHT_THIRTY);

        _store.RemoveShowing(10, Role.Admin, out string message);

        Assert.IsNotNull(_table.FindShowing(11));
        Assert.IsNotNull(ShowingInDatabase(11));
    }
}