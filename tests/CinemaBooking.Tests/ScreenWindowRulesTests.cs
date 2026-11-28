// tests for the small rules of the admin screens window, card 406 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class ScreenWindowRulesTests
{
    // the line of one screen in the list

    [TestMethod]
    public void Line_ShowsNameRowsSeatsPerRowAndTotal()
    {
        string text = ScreenLine.Text(new CinemaScreen(1, "Screen 1", 8, 12));

        Assert.AreEqual("Screen 1  -  8 rows x 12 seats  -  96 seats", text);
    }

    [TestMethod]
    public void Line_OneRowOneSeat_UsesTheSingularWords()
    {
        string text = ScreenLine.Text(new CinemaScreen(2, "Tiny", 1, 1));

        Assert.AreEqual("Tiny  -  1 row x 1 seat  -  1 seat", text);
    }

    // the three boxes

    [TestMethod]
    public void CheckBoxes_AllGood_GivesThreeEmptyMessages()
    {
        string[] messages = ScreenInput.CheckBoxes("Screen 3", "10", "14");

        Assert.AreEqual(ScreenInput.BOX_COUNT, messages.Length);
        Assert.AreEqual("", messages[ScreenInput.BOX_NAME]);
        Assert.AreEqual("", messages[ScreenInput.BOX_ROWS]);
        Assert.AreEqual("", messages[ScreenInput.BOX_SEATS]);
    }

    [TestMethod]
    public void CheckBoxes_EmptyName_OnlyTheNameBoxIsWrong()
    {
        string[] messages = ScreenInput.CheckBoxes("  ", "10", "14");

        StringAssert.Contains(messages[ScreenInput.BOX_NAME], "name cannot be empty");
        Assert.AreEqual("", messages[ScreenInput.BOX_ROWS]);
        Assert.AreEqual("", messages[ScreenInput.BOX_SEATS]);
    }

    [TestMethod]
    public void CheckBoxes_NameOver50Characters_IsWrong()
    {
        string[] messages = ScreenInput.CheckBoxes(new string('a', 51), "10", "14");

        StringAssert.Contains(messages[ScreenInput.BOX_NAME], "50 characters");
    }

    [TestMethod]
    public void CheckBoxes_RowsNotANumber_SaysWholeNumber()
    {
        string[] messages = ScreenInput.CheckBoxes("Screen 3", "ten", "14");

        Assert.AreEqual("rows must be a whole number", messages[ScreenInput.BOX_ROWS]);
    }

    [TestMethod]
    public void CheckBoxes_EmptyRows_SaysWholeNumber()
    {
        string[] messages = ScreenInput.CheckBoxes("Screen 3", "", "14");

        Assert.AreEqual("rows must be a whole number", messages[ScreenInput.BOX_ROWS]);
    }

    [TestMethod]
    public void CheckBoxes_ZeroRowsAndTwentySevenRows_AreWrong()
    {
        StringAssert.Contains(ScreenInput.CheckBoxes("S", "0", "14")[ScreenInput.BOX_ROWS], "between 1 and 26");
        StringAssert.Contains(ScreenInput.CheckBoxes("S", "27", "14")[ScreenInput.BOX_ROWS], "between 1 and 26");
    }

    [TestMethod]
    public void CheckBoxes_SeatsNotANumber_SaysWholeNumber()
    {
        string[] messages = ScreenInput.CheckBoxes("Screen 3", "10", "1.5");

        Assert.AreEqual("seats per row must be a whole number", messages[ScreenInput.BOX_SEATS]);
    }

    [TestMethod]
    public void CheckBoxes_ZeroSeatsAndThirtyOneSeats_AreWrong()
    {
        StringAssert.Contains(ScreenInput.CheckBoxes("S", "10", "0")[ScreenInput.BOX_SEATS], "between 1 and 30");
        StringAssert.Contains(ScreenInput.CheckBoxes("S", "10", "31")[ScreenInput.BOX_SEATS], "between 1 and 30");
    }

    [TestMethod]
    public void CheckBoxes_NumbersWithSpaces_AreAccepted()
    {
        string[] messages = ScreenInput.CheckBoxes("Screen 3", " 10 ", " 14 ");

        Assert.AreEqual("", messages[ScreenInput.BOX_ROWS]);
        Assert.AreEqual("", messages[ScreenInput.BOX_SEATS]);
    }

    [TestMethod]
    public void CheckBoxes_EveryBoxWrong_GivesThreeMessages()
    {
        string[] messages = ScreenInput.CheckBoxes("", "x", "y");

        Assert.AreNotEqual("", messages[ScreenInput.BOX_NAME]);
        Assert.AreNotEqual("", messages[ScreenInput.BOX_ROWS]);
        Assert.AreNotEqual("", messages[ScreenInput.BOX_SEATS]);
    }

    [TestMethod]
    public void ReadNumber_WholeNumberWithSpaces_GivesTheNumber()
    {
        Assert.AreEqual(12, ScreenInput.ReadNumber(" 12 "));
    }

    [TestMethod]
    public void ReadNumber_NotANumber_GivesZero()
    {
        Assert.AreEqual(0, ScreenInput.ReadNumber("abc"));
    }

    // the menu

    [TestMethod]
    public void Menu_OnlyAnAdminSeesTheScreensButton()
    {
        Assert.IsTrue(MenuRules.IsVisible(MenuItem.Screens, Role.Admin));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Screens, Role.Staff));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Screens, Role.Customer));
        Assert.IsFalse(MenuRules.IsVisible(MenuItem.Screens, Role.Guest));
    }
}
