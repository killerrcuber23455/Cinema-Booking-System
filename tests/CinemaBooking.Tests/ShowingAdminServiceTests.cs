// tests for adding and removing showings from the admin page, card 405 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class ShowingAdminServiceTests
{
    private static readonly DateTime SIX_THIRTY = new DateTime(2026, 12, 1, 18, 30, 0);

    // films 1 and 2 exist, nothing else does
    private static bool FilmExists(int film_id)
    {
        return film_id == 1 || film_id == 2;
    }

    private static ShowingAdminService MakeService(ShowingTable table)
    {
        return new ShowingAdminService(table, FilmExists);
    }

    // adds the showing 10 (film 1, screen 1, 18:30) the normal way
    private static void AddFirstShowing(ShowingAdminService service)
    {
        service.AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Admin, out string unused);
    }

    // ---- adding ----

    [TestMethod]
    public void AddShowing_GoodValues_ReturnsTrueAndTheShowingIsInTheTable()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);

        bool is_added = service.AddShowing(10, 2, 1, SIX_THIRTY, 250.50m, Role.Admin, out string error_message);

        Assert.IsTrue(is_added);
        Assert.AreEqual("", error_message);
        Showing? showing = table.FindShowing(10);
        Assert.IsNotNull(showing);
        Assert.AreEqual(2, showing.FilmId);
        Assert.AreEqual(1, showing.ScreenId);
        Assert.AreEqual(SIX_THIRTY, showing.StartsAt);
        Assert.AreEqual(250.50m, showing.Price);
    }

    [TestMethod]
    public void AddShowing_NewShowing_HasAnEightByTwelveGridWithAllSeatsFree()
    {
        ShowingTable table = new ShowingTable();
        AddFirstShowing(MakeService(table));

        SeatGrid grid = table.FindShowing(10)!.SeatGrid;

        Assert.AreEqual(8, grid.Rows);
        Assert.AreEqual(12, grid.SeatsPerRow);
        Assert.AreEqual(0, grid.ListTakenSeats().Length);
    }

    [TestMethod]
    public void AddShowing_StaffRole_IsAllowed()
    {
        ShowingTable table = new ShowingTable();

        bool is_added = MakeService(table).AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Staff, out string error_message);

        Assert.IsTrue(is_added);
    }

    [TestMethod]
    public void AddShowing_CustomerRole_IsRefusedAndNothingIsAdded()
    {
        ShowingTable table = new ShowingTable();

        bool is_added = MakeService(table).AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Customer, out string error_message);

        Assert.IsFalse(is_added);
        Assert.AreEqual("only staff and admins can change showings", error_message);
        Assert.AreEqual(0, table.Count);
    }

    [TestMethod]
    public void AddShowing_GuestRole_IsRefusedAndNothingIsAdded()
    {
        ShowingTable table = new ShowingTable();

        bool is_added = MakeService(table).AddShowing(10, 1, 1, SIX_THIRTY, 250m, Role.Guest, out string error_message);

        Assert.IsFalse(is_added);
        Assert.AreEqual("only staff and admins can change showings", error_message);
        Assert.AreEqual(0, table.Count);
    }

    [TestMethod]
    public void AddShowing_FilmDoesNotExist_IsRefusedAndNothingIsAdded()
    {
        ShowingTable table = new ShowingTable();

        bool is_added = MakeService(table).AddShowing(10, 99, 1, SIX_THIRTY, 250m, Role.Admin, out string error_message);

        Assert.IsFalse(is_added);
        Assert.AreEqual("film 99 does not exist", error_message);
        Assert.AreEqual(0, table.Count);
    }

    [TestMethod]
    public void AddShowing_IdAlreadyUsed_IsRefusedAndTheOldShowingStays()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);

        bool is_added = service.AddShowing(10, 2, 2, SIX_THIRTY.AddDays(1), 300m, Role.Admin, out string error_message);

        Assert.IsFalse(is_added);
        Assert.AreEqual("showing id 10 is already used", error_message);
        Assert.AreEqual(1, table.Count);
        Assert.AreEqual(1, table.FindShowing(10)!.FilmId);
    }

    [TestMethod]
    public void AddShowing_SameScreenAtTheSameStart_IsRefused()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);

        bool is_added = service.AddShowing(11, 2, 1, SIX_THIRTY, 300m, Role.Admin, out string error_message);

        Assert.IsFalse(is_added);
        Assert.AreEqual("screen 1 already has a showing at 2026-12-01 18:30", error_message);
        Assert.AreEqual(1, table.Count);
    }

    [TestMethod]
    public void AddShowing_SameStartOnAnotherScreen_IsAllowed()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);

        bool is_added = service.AddShowing(11, 2, 2, SIX_THIRTY, 300m, Role.Admin, out string error_message);

        Assert.IsTrue(is_added);
        Assert.AreEqual(2, table.Count);
    }

    [TestMethod]
    public void AddShowing_SameScreenAtAnotherStart_IsAllowed()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);

        bool is_added = service.AddShowing(11, 2, 1, SIX_THIRTY.AddHours(4), 300m, Role.Admin, out string error_message);

        Assert.IsTrue(is_added);
        Assert.AreEqual(2, table.Count);
    }

    // ---- removing ----

    [TestMethod]
    public void RemoveShowing_NoSeatsBooked_ReturnsTrueAndTheShowingIsGone()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);

        bool is_removed = service.RemoveShowing(10, Role.Admin, out string error_message);

        Assert.IsTrue(is_removed);
        Assert.AreEqual("", error_message);
        Assert.IsNull(table.FindShowing(10));
    }

    [TestMethod]
    public void RemoveShowing_ShowingNotFound_IsRefused()
    {
        ShowingTable table = new ShowingTable();

        bool is_removed = MakeService(table).RemoveShowing(5, Role.Admin, out string error_message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("showing id 5 was not found", error_message);
    }

    [TestMethod]
    public void RemoveShowing_SeatsAreBooked_IsRefusedAndTheShowingStays()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);
        table.FindShowing(10)!.SeatGrid.BookSeats(new string[] { "C5" });

        bool is_removed = service.RemoveShowing(10, Role.Admin, out string error_message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("showing id 10 has booked seats and cannot be removed", error_message);
        Assert.IsNotNull(table.FindShowing(10));
    }

    [TestMethod]
    public void RemoveShowing_BookedSeatsWereFreedAgain_IsAllowed()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);
        SeatGrid grid = table.FindShowing(10)!.SeatGrid;
        grid.BookSeats(new string[] { "C5" });
        grid.FreeSeats(new string[] { "C5" });

        bool is_removed = service.RemoveShowing(10, Role.Admin, out string error_message);

        Assert.IsTrue(is_removed);
    }

    [TestMethod]
    public void RemoveShowing_CustomerRole_IsRefusedAndTheShowingStays()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);

        bool is_removed = service.RemoveShowing(10, Role.Customer, out string error_message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("only staff and admins can change showings", error_message);
        Assert.IsNotNull(table.FindShowing(10));
    }

    [TestMethod]
    public void RemoveShowing_ThenAddTheSameId_IsAllowed()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        AddFirstShowing(service);
        service.RemoveShowing(10, Role.Admin, out string unused);

        bool is_added = service.AddShowing(10, 2, 2, SIX_THIRTY.AddDays(2), 300m, Role.Admin, out string error_message);

        Assert.IsTrue(is_added);
        Assert.AreEqual(2, table.FindShowing(10)!.FilmId);
    }

    // ---- listing ----

    [TestMethod]
    public void ListShowings_ReturnsTheEarliestFirst()
    {
        ShowingTable table = new ShowingTable();
        ShowingAdminService service = MakeService(table);
        service.AddShowing(1, 1, 1, SIX_THIRTY.AddDays(2), 250m, Role.Admin, out string unused_a);
        service.AddShowing(2, 1, 1, SIX_THIRTY, 250m, Role.Admin, out string unused_b);
        service.AddShowing(3, 2, 2, SIX_THIRTY.AddDays(1), 250m, Role.Admin, out string unused_c);

        Showing[] list = service.ListShowings();

        Assert.AreEqual(3, list.Length);
        Assert.AreEqual(2, list[0].Id);
        Assert.AreEqual(3, list[1].Id);
        Assert.AreEqual(1, list[2].Id);
    }

    [TestMethod]
    public void ListShowings_NoShowings_ReturnsAnEmptyArray()
    {
        ShowingAdminService service = MakeService(new ShowingTable());

        Assert.AreEqual(0, service.ListShowings().Length);
    }
}
