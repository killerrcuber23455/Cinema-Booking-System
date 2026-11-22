// tests for the "starts in" text and how full a showing is, card 435 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class ShowingTimesTests
{
    private static readonly DateTime NOW = new DateTime(2026, 11, 22, 16, 0, 0);

    private static Showing MakeShowing()
    {
        return new Showing(1, 1, 1, NOW.AddHours(2), 250m, new SeatGrid(8, 12));
    }

    // ---- starts in ----

    [TestMethod]
    public void StartsIn_LaterToday_GivesHoursAndMinutes()
    {
        Assert.AreEqual("starts in 2 h 15 min", ShowingTimes.StartsIn(NOW.AddMinutes(135), NOW));
    }

    [TestMethod]
    public void StartsIn_LessThanAnHour_GivesOnlyMinutes()
    {
        Assert.AreEqual("starts in 45 min", ShowingTimes.StartsIn(NOW.AddMinutes(45), NOW));
    }

    [TestMethod]
    public void StartsIn_WholeHours_GivesOnlyHours()
    {
        Assert.AreEqual("starts in 3 h", ShowingTimes.StartsIn(NOW.AddHours(3), NOW));
    }

    [TestMethod]
    public void StartsIn_ThirtySecondsLeft_RoundsUpToOneMinute()
    {
        Assert.AreEqual("starts in 1 min", ShowingTimes.StartsIn(NOW.AddSeconds(30), NOW));
    }

    [TestMethod]
    public void StartsIn_AnotherDay_GivesEmptyText()
    {
        Assert.AreEqual("", ShowingTimes.StartsIn(NOW.AddDays(1), NOW));
    }

    [TestMethod]
    public void StartsIn_AlreadyStarted_SaysStarted()
    {
        Assert.AreEqual("started", ShowingTimes.StartsIn(NOW.AddMinutes(-10), NOW));
    }

    [TestMethod]
    public void StartsIn_ExactlyNow_SaysStarted()
    {
        Assert.AreEqual("started", ShowingTimes.StartsIn(NOW, NOW));
    }

    // ---- how full ----

    [TestMethod]
    public void FillFraction_NoSeatsTaken_IsZero()
    {
        Assert.AreEqual(0.0, ShowingTimes.FillFraction(MakeShowing()), 0.0001);
    }

    [TestMethod]
    public void FillFraction_TwentyFourOfNinetySixTaken_IsOneQuarter()
    {
        Showing showing = MakeShowing();
        showing.SeatGrid.BookSeats(new string[]
        {
            "A1", "A2", "A3", "A4", "A5", "A6", "A7", "A8", "A9", "A10", "A11", "A12",
            "B1", "B2", "B3", "B4", "B5", "B6", "B7", "B8", "B9", "B10", "B11", "B12"
        });

        Assert.AreEqual(0.25, ShowingTimes.FillFraction(showing), 0.0001);
    }

    [TestMethod]
    public void FillFraction_OneSeatTaken_IsOneOutOfNinetySix()
    {
        Showing showing = MakeShowing();
        showing.SeatGrid.BookSeats(new string[] { "C5" });

        Assert.AreEqual(1.0 / 96.0, ShowingTimes.FillFraction(showing), 0.0001);
    }
}
