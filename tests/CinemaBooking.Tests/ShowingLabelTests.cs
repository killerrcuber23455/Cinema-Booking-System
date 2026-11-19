// tests for the one line of text shown for each showing on the film detail page (owner: divyesh)
// written before the code for card 402, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class ShowingLabelTests
{
    // a fresh 8x12 grid has 96 free seats
    private static Showing MakeShowing(SeatGrid grid)
    {
        return new Showing(1, 10, 2, new DateTime(2026, 11, 20, 18, 30, 0), 250m, grid);
    }

    [TestMethod]
    public void Describe_EmptyHall_ShowsDateTimeScreenPriceAndSeats()
    {
        Showing showing = MakeShowing(new SeatGrid(8, 12));

        string text = ShowingLabel.Describe(showing);

        Assert.AreEqual("Fri 20 Nov 2026 · 18:30 · Screen 2 · Rs 250.00 · 96 seats left", text);
    }

    [TestMethod]
    public void Describe_SomeSeatsBooked_CountsOnlyFreeSeats()
    {
        SeatGrid grid = new SeatGrid(8, 12);
        grid.BookSeats(new string[] { "C5", "C6" });

        string text = ShowingLabel.Describe(MakeShowing(grid));

        Assert.IsTrue(text.EndsWith("94 seats left"), text);
    }

    [TestMethod]
    public void Describe_OneSeatLeft_UsesSingularWord()
    {
        SeatGrid grid = new SeatGrid(1, 2);
        grid.BookSeats(new string[] { "A1" });

        string text = ShowingLabel.Describe(MakeShowing(grid));

        Assert.IsTrue(text.EndsWith("1 seat left"), text);
    }

    [TestMethod]
    public void Describe_NoSeatsLeft_SaysSoldOut()
    {
        SeatGrid grid = new SeatGrid(1, 2);
        grid.BookSeats(new string[] { "A1", "A2" });

        string text = ShowingLabel.Describe(MakeShowing(grid));

        Assert.IsTrue(text.EndsWith("sold out"), text);
    }

    [TestMethod]
    public void Describe_PriceWithCents_KeepsTwoDecimals()
    {
        Showing showing = new Showing(1, 10, 1, new DateTime(2026, 11, 21, 9, 5, 0), 99.5m, new SeatGrid(8, 12));

        string text = ShowingLabel.Describe(showing);

        Assert.IsTrue(text.Contains("Rs 99.50"), text);
        Assert.IsTrue(text.Contains("09:05"), text);
    }
}
