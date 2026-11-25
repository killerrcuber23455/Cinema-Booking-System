// tests for the sizes the forms share (owner: divyesh)
// written before the code for card 445, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class FormMetricsTests
{
    [TestMethod]
    public void BoxHeight_IsAtLeastFortyPixels()
    {
        Assert.IsGreaterThanOrEqualTo(40, FormParts.BOX_HEIGHT);
    }

    [TestMethod]
    public void RowHeight_HoldsTheLabelTheBoxAndTheMessage()
    {
        Assert.IsGreaterThanOrEqualTo(FormParts.LABEL_HEIGHT + FormParts.BOX_HEIGHT + FormParts.MESSAGE_HEIGHT, FormParts.ROW_HEIGHT);
    }

    [TestMethod]
    public void HeightForRows_GrowsByOneRowHeightForEachRow()
    {
        Assert.AreEqual(FormParts.ROW_HEIGHT, FormParts.HeightForRows(3) - FormParts.HeightForRows(2));
        Assert.AreEqual(FormParts.ROW_HEIGHT, FormParts.HeightForRows(5) - FormParts.HeightForRows(4));
    }

    [TestMethod]
    public void HeightForRows_HoldsTheRowsAndTheFullWidthButton()
    {
        for (int row_count = 1; row_count <= 5; row_count++)
        {
            Assert.IsGreaterThan(FormParts.TopUnderRows(row_count) + FormParts.BUTTON_HEIGHT, FormParts.HeightForRows(row_count), "rows " + row_count);
        }
    }

    [TestMethod]
    public void LandingCard_UsesTheSameFormHeight()
    {
        for (int row_count = 1; row_count <= 5; row_count++)
        {
            Assert.AreEqual(FormParts.HeightForRows(row_count), LandingLayout.FormHeight(row_count), "rows " + row_count);
        }
    }
}
