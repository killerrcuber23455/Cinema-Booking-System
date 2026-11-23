// tests for adding a showing so that the database keeps it too (owner: divyesh)
// written before the code for card 405, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class ShowingStoreTests
{
    private static readonly DateTime SIX_THIRTY = new DateTime(2026, 12, 1, 18, 30, 0);

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

    // reads the showing from the fake database, null if the database does not have it
    private Showing? ShowingInDatabase(int showing_id)
    {
        ShowingTable table = new ShowingTable();
        _database.LoadShowings(table);
        return table.FindShowing(showing_id);
    }

    [TestMethod]
    public void AddShowing_Staff_AddsToTheTableAndTheDatabase()
    {
        bool is_added = _store.AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Staff, out string message);

        Assert.IsTrue(is_added);
        Assert.AreEqual("", message);
        Assert.IsNotNull(_table.FindShowing(10));
        Assert.IsNotNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void AddShowing_Admin_AddsToTheTableAndTheDatabase()
    {
        bool is_added = _store.AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Admin, out string message);

        Assert.IsTrue(is_added);
        Assert.IsNotNull(_table.FindShowing(10));
        Assert.IsNotNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void AddShowing_Customer_ReturnsFalseAndSavesNothing()
    {
        bool is_added = _store.AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Customer, out string message);

        Assert.IsFalse(is_added);
        Assert.AreEqual("only staff and admins can change showings", message);
        Assert.IsNull(_table.FindShowing(10));
        Assert.IsNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void AddShowing_FilmDoesNotExist_ReturnsFalseAndSavesNothing()
    {
        bool is_added = _store.AddShowing(10, 9, 1, SIX_THIRTY, 250m, Role.Admin, out string message);

        Assert.IsFalse(is_added);
        Assert.AreEqual("film 9 does not exist", message);
        Assert.IsNull(_table.FindShowing(10));
        Assert.IsNull(ShowingInDatabase(10));
    }

    [TestMethod]
    public void AddShowing_DatabaseRefuses_TakesTheShowingBackOut()
    {
        _database.ShowingSaveFails = true;

        bool is_added = _store.AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Admin, out string message);

        Assert.IsFalse(is_added);
        Assert.AreEqual(ShowingStore.SAVE_FAILED_TEXT, message);
        Assert.IsNull(_table.FindShowing(10));
    }

    [TestMethod]
    public void AddShowing_DatabaseThrows_TakesTheShowingBackOut()
    {
        _database.ThrowOnSaveShowing = true;

        bool is_added = _store.AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Admin, out string message);

        Assert.IsFalse(is_added);
        Assert.AreEqual(ShowingStore.SAVE_FAILED_TEXT, message);
        Assert.IsNull(_table.FindShowing(10));
    }
}
