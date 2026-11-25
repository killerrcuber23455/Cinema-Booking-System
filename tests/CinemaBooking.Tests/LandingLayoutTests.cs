// tests for the sizes of the landing window, so no form of the card needs a scroll bar (owner: divyesh)
// written before the code for card 445, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class LandingLayoutTests
{
    [TestMethod]
    public void SignUpForm_FitsWithoutScrolling()
    {
        Assert.IsTrue(LandingLayout.FormFitsWithoutScrolling(SignUpInput.BOX_COUNT));
    }

    [TestMethod]
    public void SignInForm_FitsWithoutScrolling()
    {
        Assert.IsTrue(LandingLayout.FormFitsWithoutScrolling(LoginInput.BOX_COUNT));
    }

    [TestMethod]
    public void FirstAdminForm_FitsWithoutScrolling()
    {
        Assert.IsTrue(LandingLayout.FormFitsWithoutScrolling(FirstAdminInput.BOX_COUNT));
    }

    [TestMethod]
    public void CardHeight_GrowsWithTheNumberOfRows()
    {
        Assert.IsTrue(LandingLayout.CardHeightForForm(SignUpInput.BOX_COUNT) > LandingLayout.CardHeightForForm(LoginInput.BOX_COUNT));
    }

    [TestMethod]
    public void CardHeight_HoldsTheRowsAndStaysInsideTheWindow()
    {
        for (int row_count = 1; row_count <= SignUpInput.BOX_COUNT; row_count++)
        {
            int height = LandingLayout.CardHeightForForm(row_count);

            Assert.IsTrue(height > LandingLayout.FORM_TOP + FormParts.TopUnderRows(row_count), "rows " + row_count);
            Assert.IsTrue(height <= LandingLayout.WINDOW_HEIGHT - (2 * LandingLayout.WINDOW_MARGIN), "rows " + row_count);
        }
    }
}
