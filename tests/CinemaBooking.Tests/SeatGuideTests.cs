// tests for the seat view text and the arrow key moves on the seat map, card 435 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SeatGuideTests
{
    private const int ROWS = 8;
    private const int SEATS = 12;

    // ---- what a seat is like ----

    [TestMethod]
    public void DescribeView_FrontLeftSeat_SaysCloseToTheScreen()
    {
        Assert.AreEqual("Row A, seat 1: left side, close to the screen, look up a little",
            SeatGuide.DescribeView(0, 0, ROWS, SEATS));
    }

    [TestMethod]
    public void DescribeView_MiddleRowCentreSeat_SaysBestView()
    {
        Assert.AreEqual("Row C, seat 6: centre, best view in the room",
            SeatGuide.DescribeView(2, 5, ROWS, SEATS));
    }

    [TestMethod]
    public void DescribeView_MiddleRowLeftSeat_SaysGoodView()
    {
        Assert.AreEqual("Row C, seat 1: left side, good view",
            SeatGuide.DescribeView(2, 0, ROWS, SEATS));
    }

    [TestMethod]
    public void DescribeView_MiddleRowRightSeat_SaysGoodView()
    {
        // seats 9 to 12 are the right third of a row of 12
        Assert.AreEqual("Row D, seat 9: right side, good view",
            SeatGuide.DescribeView(3, 8, ROWS, SEATS));
    }

    [TestMethod]
    public void DescribeView_BackRowSeat_SaysWholeScreenInView()
    {
        Assert.AreEqual("Row H, seat 12: right side, at the back, the whole screen in view",
            SeatGuide.DescribeView(7, 11, ROWS, SEATS));
    }

    [TestMethod]
    public void DescribeView_LastCentreSeatBeforeTheRightThird_IsCentre()
    {
        Assert.AreEqual("Row E, seat 8: centre, best view in the room",
            SeatGuide.DescribeView(4, 7, ROWS, SEATS));
    }

    [TestMethod]
    public void DescribeView_RowOutsideTheGrid_GivesEmptyText()
    {
        Assert.AreEqual("", SeatGuide.DescribeView(8, 0, ROWS, SEATS));
    }

    [TestMethod]
    public void DescribeView_SeatOutsideTheGrid_GivesEmptyText()
    {
        Assert.AreEqual("", SeatGuide.DescribeView(0, -1, ROWS, SEATS));
    }

    // ---- arrow keys ----

    [TestMethod]
    public void Move_Up_GoesOneRowNearerTheScreen()
    {
        SeatGuide.Move(2, 5, SeatMove.Up, ROWS, SEATS, out int row, out int seat);

        Assert.AreEqual(1, row);
        Assert.AreEqual(5, seat);
    }

    [TestMethod]
    public void Move_Down_GoesOneRowBack()
    {
        SeatGuide.Move(2, 5, SeatMove.Down, ROWS, SEATS, out int row, out int seat);

        Assert.AreEqual(3, row);
        Assert.AreEqual(5, seat);
    }

    [TestMethod]
    public void Move_Left_GoesOneSeatLeft()
    {
        SeatGuide.Move(2, 5, SeatMove.Left, ROWS, SEATS, out int row, out int seat);

        Assert.AreEqual(2, row);
        Assert.AreEqual(4, seat);
    }

    [TestMethod]
    public void Move_Right_GoesOneSeatRight()
    {
        SeatGuide.Move(2, 5, SeatMove.Right, ROWS, SEATS, out int row, out int seat);

        Assert.AreEqual(2, row);
        Assert.AreEqual(6, seat);
    }

    [TestMethod]
    public void Move_UpFromTheFrontRow_StaysInTheFrontRow()
    {
        SeatGuide.Move(0, 3, SeatMove.Up, ROWS, SEATS, out int row, out int seat);

        Assert.AreEqual(0, row);
        Assert.AreEqual(3, seat);
    }

    [TestMethod]
    public void Move_RightFromTheLastSeat_StaysOnTheLastSeat()
    {
        SeatGuide.Move(4, 11, SeatMove.Right, ROWS, SEATS, out int row, out int seat);

        Assert.AreEqual(4, row);
        Assert.AreEqual(11, seat);
    }
}
