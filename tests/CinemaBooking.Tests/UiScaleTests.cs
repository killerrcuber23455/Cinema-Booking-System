// tests for how the text and spaces grow when the window is made wider (owner: divyesh)
// written before the code of the UI upgrade, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class UiScaleTests
{
    [TestMethod]
    public void StepFor_StartingWindowWidth_IsTheSmallStep()
    {
        Assert.AreEqual(UiScale.SMALL_STEP, UiScale.StepFor(1100));
    }

    [TestMethod]
    public void StepFor_JustUnderMedium_IsStillTheSmallStep()
    {
        Assert.AreEqual(UiScale.SMALL_STEP, UiScale.StepFor(1399));
    }

    [TestMethod]
    public void StepFor_MediumWidth_IsTheMediumStep()
    {
        Assert.AreEqual(UiScale.MEDIUM_STEP, UiScale.StepFor(1400));
    }

    [TestMethod]
    public void StepFor_JustUnderLarge_IsStillTheMediumStep()
    {
        Assert.AreEqual(UiScale.MEDIUM_STEP, UiScale.StepFor(1799));
    }

    // a full HD screen maximised is about 1920 wide
    [TestMethod]
    public void StepFor_MaximisedOnFullHd_IsTheLargeStep()
    {
        Assert.AreEqual(UiScale.LARGE_STEP, UiScale.StepFor(1920));
    }

    // a 4K screen does not make the text huge, the large step is the last one
    [TestMethod]
    public void StepFor_VeryWideWindow_StopsAtTheLargeStep()
    {
        Assert.AreEqual(UiScale.LARGE_STEP, UiScale.StepFor(3840));
    }

    // a minimised window has a width of 0, that must not shrink anything
    [TestMethod]
    public void StepFor_ZeroOrNegativeWidth_IsTheSmallStep()
    {
        Assert.AreEqual(UiScale.SMALL_STEP, UiScale.StepFor(0));
        Assert.AreEqual(UiScale.SMALL_STEP, UiScale.StepFor(-50));
    }

    [TestMethod]
    public void Size_SmallStep_KeepsTheSize()
    {
        Assert.AreEqual(10.5f, UiScale.Size(10.5f, UiScale.SMALL_STEP));
    }

    // 10.5 x 1.15 is 12.075, rounded to the nearest half point
    [TestMethod]
    public void Size_MediumStep_RoundsToHalfAPoint()
    {
        Assert.AreEqual(12f, UiScale.Size(10.5f, UiScale.MEDIUM_STEP));
    }

    [TestMethod]
    public void Size_LargeStep_GrowsThePageTitle()
    {
        Assert.AreEqual(26f, UiScale.Size(20f, UiScale.LARGE_STEP));
    }

    // 48 x 1.3 is 62.4, rounded to a whole pixel
    [TestMethod]
    public void Pixels_LargeStep_RoundsToAWholePixel()
    {
        Assert.AreEqual(62, UiScale.Pixels(48, UiScale.LARGE_STEP));
    }

    [TestMethod]
    public void Pixels_Zero_StaysZero()
    {
        Assert.AreEqual(0, UiScale.Pixels(0, UiScale.LARGE_STEP));
    }
}
