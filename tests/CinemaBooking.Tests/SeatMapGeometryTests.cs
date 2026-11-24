// tests for the sizes of the seat map when the window changes size (owner: divyesh)
// written before the code of UI upgrade step 3, so they fail first (red) and pass after (green)
using System.Drawing;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SeatMapGeometryTests
{
    private const int ROWS = 8;
    private const int SEATS = 12;

    // 1000 wide: (1000 - 48 - 30 - 11 gaps of 8) / 12 = 69.5, rounded down
    [TestMethod]
    public void Make_BigWindow_SeatWidthFitsTheWidth()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        Assert.AreEqual(69, geometry.SeatWidth);
    }

    // 69 x 85 / 100 = 58.65, rounded down
    [TestMethod]
    public void Make_BigWindow_SeatHeightIs85PercentOfTheWidth()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        Assert.AreEqual(58, geometry.SeatHeight);
    }

    // a wide but short window: the height only has room for seats 43 wide
    [TestMethod]
    public void Make_ShortWindow_TheHeightLimitsTheSeats()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1600, 500, ROWS, SEATS);

        Assert.AreEqual(43, geometry.SeatWidth);
        Assert.AreEqual(36, geometry.SeatHeight);
    }

    // a window that is too small does not make seats smaller than a number can be read
    [TestMethod]
    public void Make_TinyWindow_KeepsTheMinimumSeatWidth()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(200, 200, ROWS, SEATS);

        Assert.AreEqual(SeatMapGeometry.MIN_SEAT_WIDTH, geometry.SeatWidth);
        Assert.AreEqual(20, geometry.SeatHeight);
    }

    // a 4K screen does not make seats as big as posters
    [TestMethod]
    public void Make_HugeWindow_StopsAtTheMaximumSeatWidth()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(3840, 2000, ROWS, SEATS);

        Assert.AreEqual(SeatMapGeometry.MAX_SEAT_WIDTH, geometry.SeatWidth);
        Assert.AreEqual(61, geometry.SeatHeight);
    }

    // 12 seats of 69 and 11 gaps of 8 are 916 wide, the seats and the row letters are centred together
    [TestMethod]
    public void Make_BigWindow_TheGridIsCentred()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        Assert.AreEqual(916, geometry.GridWidth);
        Assert.AreEqual(57, geometry.GridLeft);
        Assert.AreEqual(1000 - (geometry.GridLeft + geometry.GridWidth), geometry.GridLeft - SeatMapGeometry.ROW_LABEL_WIDTH);
    }

    // 8 rows of 58 and 7 gaps of 8 are 520 high, under the room kept for the screen
    [TestMethod]
    public void Make_BigWindow_TheGridStartsUnderTheScreen()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        Assert.AreEqual(SeatMapGeometry.TOP_ROOM, geometry.GridTop);
        Assert.AreEqual(520, geometry.GridHeight);
    }

    [TestMethod]
    public void SeatArea_RowOneSeatTwo_IsInTheRightPlace()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        Assert.AreEqual(new Rectangle(211, 162, 69, 58), geometry.SeatArea(1, 2));
    }

    [TestMethod]
    public void FindSeat_InsideASeat_GivesItsRowAndSeat()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        bool found = geometry.FindSeat(new Point(216, 167), ROWS, SEATS, out int row_index, out int seat_index);

        Assert.IsTrue(found);
        Assert.AreEqual(1, row_index);
        Assert.AreEqual(2, seat_index);
    }

    // a click between two seats picks nothing
    [TestMethod]
    public void FindSeat_InTheGapBetweenSeats_IsFalse()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        bool found = geometry.FindSeat(new Point(57 + 69 + 3, 100), ROWS, SEATS, out int row_index, out int seat_index);

        Assert.IsFalse(found);
    }

    [TestMethod]
    public void FindSeat_OutsideTheGrid_IsFalse()
    {
        SeatMapGeometry geometry = SeatMapGeometry.Make(1000, 700, ROWS, SEATS);

        Assert.IsFalse(geometry.FindSeat(new Point(10, 100), ROWS, SEATS, out int row_left, out int seat_left));
        Assert.IsFalse(geometry.FindSeat(new Point(300, 621), ROWS, SEATS, out int row_below, out int seat_below));
    }
}
