// tests for mixing colours and finding the main colour of a poster, card 435 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using System.Drawing;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class ColorMathTests
{
    private static readonly Color BLACK = Color.FromArgb(0, 0, 0);
    private static readonly Color WHITE = Color.FromArgb(255, 255, 255);

    private static void AssertSameColour(Color expected, Color actual)
    {
        Assert.AreEqual(expected.R, actual.R, "red");
        Assert.AreEqual(expected.G, actual.G, "green");
        Assert.AreEqual(expected.B, actual.B, "blue");
    }

    [TestMethod]
    public void Blend_AmountZero_GivesTheFirstColour()
    {
        AssertSameColour(BLACK, ColorMath.Blend(BLACK, WHITE, 0));
    }

    [TestMethod]
    public void Blend_AmountOne_GivesTheSecondColour()
    {
        AssertSameColour(WHITE, ColorMath.Blend(BLACK, WHITE, 1));
    }

    [TestMethod]
    public void Blend_Half_GivesTheMiddleRoundedUp()
    {
        AssertSameColour(Color.FromArgb(128, 128, 128), ColorMath.Blend(BLACK, WHITE, 0.5));
    }

    [TestMethod]
    public void Blend_AmountAboveOne_StopsAtTheSecondColour()
    {
        AssertSameColour(WHITE, ColorMath.Blend(BLACK, WHITE, 1.5));
    }

    [TestMethod]
    public void Blend_AmountBelowZero_StopsAtTheFirstColour()
    {
        AssertSameColour(BLACK, ColorMath.Blend(BLACK, WHITE, -1));
    }

    [TestMethod]
    public void Blend_ResultIsNotSeeThrough()
    {
        Assert.AreEqual(255, ColorMath.Blend(Color.FromArgb(0, 10, 20, 30), WHITE, 0.2).A);
    }

    [TestMethod]
    public void Average_HalfRedHalfBluePicture_GivesPurple()
    {
        using (Bitmap picture = new Bitmap(2, 2))
        {
            picture.SetPixel(0, 0, Color.FromArgb(255, 0, 0));
            picture.SetPixel(1, 0, Color.FromArgb(255, 0, 0));
            picture.SetPixel(0, 1, Color.FromArgb(0, 0, 255));
            picture.SetPixel(1, 1, Color.FromArgb(0, 0, 255));

            AssertSameColour(Color.FromArgb(128, 0, 128), ColorMath.Average(picture));
        }
    }

    [TestMethod]
    public void Average_OneColourPicture_GivesThatColour()
    {
        using (Bitmap picture = new Bitmap(30, 20))
        {
            using (Graphics graphics = Graphics.FromImage(picture))
            {
                graphics.Clear(Color.FromArgb(200, 120, 40));
            }

            AssertSameColour(Color.FromArgb(200, 120, 40), ColorMath.Average(picture));
        }
    }
}
