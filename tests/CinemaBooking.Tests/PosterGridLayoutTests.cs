// tests for where the poster cards go on the films page when the window changes size (owner: divyesh)
// written before the code of UI upgrade step 2, so they fail first (red) and pass after (green)
using System.Drawing;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class PosterGridLayoutTests
{
    private const int MIN_CARD = 180;
    private const int GAP = 24;

    [TestMethod]
    public void ColumnsFor_NormalWindow_FitsFiveCards()
    {
        Assert.AreEqual(5, PosterGridLayout.ColumnsFor(1000, MIN_CARD, GAP));
    }

    [TestMethod]
    public void ColumnsFor_WiderWindow_AddsColumns()
    {
        Assert.AreEqual(7, PosterGridLayout.ColumnsFor(1600, MIN_CARD, GAP));
    }

    [TestMethod]
    public void ColumnsFor_NarrowerThanOneCard_IsStillOneColumn()
    {
        Assert.AreEqual(1, PosterGridLayout.ColumnsFor(100, MIN_CARD, GAP));
    }

    // a page that is not shown yet has a width of 0
    [TestMethod]
    public void ColumnsFor_ZeroWidth_IsOneColumn()
    {
        Assert.AreEqual(1, PosterGridLayout.ColumnsFor(0, MIN_CARD, GAP));
    }

    // 1000 wide, 5 cards and 4 gaps of 24: (1000 - 96) / 5 = 180.8, rounded down
    [TestMethod]
    public void CardWidth_CardsGrowToFillTheRow()
    {
        Assert.AreEqual(180, PosterGridLayout.CardWidth(1000, 5, GAP));
    }

    [TestMethod]
    public void CardWidth_OneColumn_IsTheWholeWidth()
    {
        Assert.AreEqual(300, PosterGridLayout.CardWidth(300, 1, GAP));
    }

    [TestMethod]
    public void PosterHeight_IsTwoWideForThreeHigh()
    {
        Assert.AreEqual(270, PosterGridLayout.PosterHeight(180));
    }

    [TestMethod]
    public void RowsFor_SixFilmsInFiveColumns_IsTwoRows()
    {
        Assert.AreEqual(2, PosterGridLayout.RowsFor(6, 5));
    }

    [TestMethod]
    public void RowsFor_NoFilms_IsNoRows()
    {
        Assert.AreEqual(0, PosterGridLayout.RowsFor(0, 5));
    }

    // the sixth film (index 5) starts the second row
    [TestMethod]
    public void CardBounds_SixthFilm_StartsTheSecondRow()
    {
        Rectangle bounds = PosterGridLayout.CardBounds(5, 5, 180, 334, GAP);

        Assert.AreEqual(new Rectangle(0, 358, 180, 334), bounds);
    }

    [TestMethod]
    public void IndexAt_InsideTheThirdCard_IsIndexTwo()
    {
        Point inside_third = new Point((2 * (180 + GAP)) + 10, 10);

        Assert.AreEqual(2, PosterGridLayout.IndexAt(inside_third, 6, 5, 180, 334, GAP));
    }

    // a click between two cards opens nothing
    [TestMethod]
    public void IndexAt_InTheGapBetweenCards_IsMinusOne()
    {
        Point in_gap = new Point(180 + 5, 10);

        Assert.AreEqual(-1, PosterGridLayout.IndexAt(in_gap, 6, 5, 180, 334, GAP));
    }

    // 6 films in 5 columns: the second row only has one card, the fourth place of that row is empty
    [TestMethod]
    public void IndexAt_PlaceAfterTheLastFilm_IsMinusOne()
    {
        Point after_last = new Point((3 * (180 + GAP)) + 10, 358 + 10);

        Assert.AreEqual(-1, PosterGridLayout.IndexAt(after_last, 6, 5, 180, 334, GAP));
    }

    // two rows of 334 with one gap of 24 between them
    [TestMethod]
    public void TotalHeight_TwoRows_AddsOneGap()
    {
        Assert.AreEqual(692, PosterGridLayout.TotalHeight(6, 5, 334, GAP));
    }
}
