// tests for the seat picking on the seat map: what a click on a seat does (owner: divyesh)
// written before the code for card 348, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SeatSelectionTests
{
    // a screen is 8 rows (A to H) of 12 seats
    private static SeatGrid MakeGrid()
    {
        return new SeatGrid(8, 12);
    }

    [TestMethod]
    public void NewSelection_HasNoSeatsChosen()
    {
        SeatSelection selection = new SeatSelection(MakeGrid());

        Assert.AreEqual(0, selection.Count);
        Assert.AreEqual(0, selection.GetSelectedSeats().Length);
        Assert.AreEqual("no seats chosen", selection.Describe());
    }

    [TestMethod]
    public void TryToggle_FreeSeat_IsChosen()
    {
        SeatSelection selection = new SeatSelection(MakeGrid());

        bool is_done = selection.TryToggle("C5", out string message);

        Assert.IsTrue(is_done, message);
        Assert.IsTrue(selection.IsSelected("C5"));
        Assert.AreEqual(1, selection.Count);
    }

    [TestMethod]
    public void TryToggle_ChosenSeatClickedAgain_IsNotChosenAnyMore()
    {
        SeatSelection selection = new SeatSelection(MakeGrid());
        selection.TryToggle("C5", out _);

        bool is_done = selection.TryToggle("C5", out string message);

        Assert.IsTrue(is_done, message);
        Assert.IsFalse(selection.IsSelected("C5"));
        Assert.AreEqual(0, selection.Count);
    }

    [TestMethod]
    public void TryToggle_TakenSeat_IsRefusedAndNotChosen()
    {
        SeatGrid grid = MakeGrid();
        grid.BookSeats(new string[] { "C5" });
        SeatSelection selection = new SeatSelection(grid);

        bool is_done = selection.TryToggle("C5", out string message);

        Assert.IsFalse(is_done);
        Assert.AreEqual("seat C5 is already taken", message);
        Assert.IsFalse(selection.IsSelected("C5"));
        Assert.AreEqual(0, selection.Count);
    }

    [TestMethod]
    [DataRow("Z9")]
    [DataRow("A13")]
    [DataRow("A0")]
    [DataRow("A01")]
    [DataRow("AA1")]
    public void TryToggle_SeatNotOnTheScreen_IsRefused(string seat_code)
    {
        SeatSelection selection = new SeatSelection(MakeGrid());

        bool is_done = selection.TryToggle(seat_code, out string message);

        Assert.IsFalse(is_done);
        Assert.AreEqual("seat " + seat_code + " is not on this screen", message);
        Assert.AreEqual(0, selection.Count);
    }

    [TestMethod]
    public void TryToggle_ChosenSeatThatGotTakenMeanwhile_CanStillBeUnchosen()
    {
        SeatGrid grid = MakeGrid();
        SeatSelection selection = new SeatSelection(grid);
        selection.TryToggle("C5", out _);

        // somebody else books the seat while the window is open
        grid.BookSeats(new string[] { "C5" });
        bool is_done = selection.TryToggle("C5", out string message);

        Assert.IsTrue(is_done, message);
        Assert.IsFalse(selection.IsSelected("C5"));
    }

    [TestMethod]
    public void GetSelectedSeats_GivesSeatsInScreenOrder()
    {
        SeatSelection selection = new SeatSelection(MakeGrid());
        selection.TryToggle("C5", out _);
        selection.TryToggle("A12", out _);
        selection.TryToggle("A1", out _);

        string[] seats = selection.GetSelectedSeats();

        Assert.AreEqual(3, seats.Length);
        Assert.AreEqual("A1", seats[0]);
        Assert.AreEqual("A12", seats[1]);
        Assert.AreEqual("C5", seats[2]);
    }

    [TestMethod]
    public void Describe_ChosenSeats_ListsThem()
    {
        SeatSelection selection = new SeatSelection(MakeGrid());
        selection.TryToggle("C5", out _);
        selection.TryToggle("A1", out _);

        Assert.AreEqual("chosen: A1, C5", selection.Describe());
    }

    [TestMethod]
    public void Clear_RemovesEveryChosenSeat()
    {
        SeatSelection selection = new SeatSelection(MakeGrid());
        selection.TryToggle("C5", out _);
        selection.TryToggle("C6", out _);

        selection.Clear();

        Assert.AreEqual(0, selection.Count);
        Assert.IsFalse(selection.IsSelected("C5"));
    }

    [TestMethod]
    public void TryToggle_ChoosingASeat_DoesNotBookItInTheGrid()
    {
        SeatGrid grid = MakeGrid();
        SeatSelection selection = new SeatSelection(grid);

        selection.TryToggle("C5", out _);

        // only the booking service books seats, the window only picks them
        Assert.IsTrue(grid.IsSeatFree("C5"));
    }
}
