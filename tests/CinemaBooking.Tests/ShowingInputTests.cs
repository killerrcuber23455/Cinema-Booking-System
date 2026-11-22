// tests for the boxes of the admin showings page, card 405 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class ShowingInputTests
{
    // the clock is fixed so "in the future" does not depend on the real time
    private static readonly DateTime NOW = new DateTime(2026, 11, 22, 12, 0, 0);

    private const string GOOD_ID = "10";
    private const string GOOD_FILM = "2";
    private const string GOOD_SCREEN = "1";
    private const string GOOD_STARTS = "2026-12-01 18:30";
    private const string GOOD_PRICE = "250";

    private static string[] Check(string id = GOOD_ID, string film = GOOD_FILM, string screen = GOOD_SCREEN,
        string starts = GOOD_STARTS, string price = GOOD_PRICE)
    {
        return ShowingInput.CheckBoxes(id, film, screen, starts, price, NOW);
    }

    [TestMethod]
    public void TryCreate_GoodValues_ReturnsTrueWithNoMessage()
    {
        bool is_good = ShowingInput.TryCreate(GOOD_ID, GOOD_FILM, GOOD_SCREEN, GOOD_STARTS, GOOD_PRICE, NOW,
            out ShowingInput? input, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(input);
    }

    [TestMethod]
    public void TryCreate_GoodValues_KeepsEveryValue()
    {
        ShowingInput.TryCreate(GOOD_ID, GOOD_FILM, GOOD_SCREEN, GOOD_STARTS, "250.50", NOW,
            out ShowingInput? input, out string error_message);

        Assert.AreEqual(10, input!.Id);
        Assert.AreEqual(2, input.FilmId);
        Assert.AreEqual(1, input.ScreenId);
        Assert.AreEqual(new DateTime(2026, 12, 1, 18, 30, 0), input.StartsAt);
        Assert.AreEqual(250.50m, input.Price);
    }

    [TestMethod]
    public void TryCreate_SpacesAroundValues_AreAccepted()
    {
        bool is_good = ShowingInput.TryCreate(" 10 ", " 2 ", " 1 ", "  2026-12-01 18:30  ", " 250 ", NOW,
            out ShowingInput? input, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual(new DateTime(2026, 12, 1, 18, 30, 0), input!.StartsAt);
    }

    [TestMethod]
    public void CheckBoxes_GoodValues_EveryMessageIsEmpty()
    {
        string[] messages = Check();

        for (int box_index = 0; box_index < messages.Length; box_index++)
        {
            Assert.AreEqual("", messages[box_index]);
        }
    }

    [TestMethod]
    public void CheckBoxes_GivesOneMessagePerBox()
    {
        Assert.AreEqual(ShowingInput.BOX_COUNT, Check().Length);
        Assert.AreEqual(5, ShowingInput.BOX_COUNT);
    }

    [TestMethod]
    public void CheckBoxes_NullValues_DoNotCrashAndEveryBoxSaysEmpty()
    {
        string[] messages = ShowingInput.CheckBoxes(null!, null!, null!, null!, null!, NOW);

        Assert.AreEqual("showing id cannot be empty", messages[ShowingInput.BOX_ID]);
        Assert.AreEqual("film id cannot be empty", messages[ShowingInput.BOX_FILM]);
        Assert.AreEqual("screen id cannot be empty", messages[ShowingInput.BOX_SCREEN]);
        Assert.AreEqual("start cannot be empty", messages[ShowingInput.BOX_STARTS]);
        Assert.AreEqual("price cannot be empty", messages[ShowingInput.BOX_PRICE]);
    }

    // ---- showing id ----

    [TestMethod]
    public void CheckBoxes_EmptyId_SaysItIsEmpty()
    {
        Assert.AreEqual("showing id cannot be empty", Check(id: "")[ShowingInput.BOX_ID]);
    }

    [TestMethod]
    public void CheckBoxes_IdIsText_SaysWholeNumber()
    {
        Assert.AreEqual("showing id must be a whole number", Check(id: "abc")[ShowingInput.BOX_ID]);
    }

    [TestMethod]
    public void CheckBoxes_IdIsZero_SaysAboveZero()
    {
        Assert.AreEqual("showing id must be above 0", Check(id: "0")[ShowingInput.BOX_ID]);
    }

    [TestMethod]
    public void CheckBoxes_IdIsNegative_SaysAboveZero()
    {
        Assert.AreEqual("showing id must be above 0", Check(id: "-4")[ShowingInput.BOX_ID]);
    }

    // ---- film id and screen id ----

    [TestMethod]
    public void CheckBoxes_EmptyFilmId_SaysItIsEmpty()
    {
        Assert.AreEqual("film id cannot be empty", Check(film: "")[ShowingInput.BOX_FILM]);
    }

    [TestMethod]
    public void CheckBoxes_FilmIdIsZero_SaysAboveZero()
    {
        Assert.AreEqual("film id must be above 0", Check(film: "0")[ShowingInput.BOX_FILM]);
    }

    [TestMethod]
    public void CheckBoxes_EmptyScreenId_SaysItIsEmpty()
    {
        Assert.AreEqual("screen id cannot be empty", Check(screen: "")[ShowingInput.BOX_SCREEN]);
    }

    [TestMethod]
    public void CheckBoxes_ScreenIdIsZero_SaysAboveZero()
    {
        Assert.AreEqual("screen id must be above 0", Check(screen: "0")[ShowingInput.BOX_SCREEN]);
    }

    // ---- start date and time ----

    [TestMethod]
    public void CheckBoxes_EmptyStart_SaysItIsEmpty()
    {
        Assert.AreEqual("start cannot be empty", Check(starts: "")[ShowingInput.BOX_STARTS]);
    }

    [TestMethod]
    public void CheckBoxes_StartInWords_SaysHowItShouldLook()
    {
        Assert.AreEqual("start must look like 2026-12-01 18:30", Check(starts: "1 Dec 6pm")[ShowingInput.BOX_STARTS]);
    }

    [TestMethod]
    public void CheckBoxes_StartWithoutTime_SaysHowItShouldLook()
    {
        Assert.AreEqual("start must look like 2026-12-01 18:30", Check(starts: "2026-12-01")[ShowingInput.BOX_STARTS]);
    }

    [TestMethod]
    public void CheckBoxes_StartInThePast_SaysItMustBeInTheFuture()
    {
        Assert.AreEqual("start must be in the future", Check(starts: "2026-11-21 18:30")[ShowingInput.BOX_STARTS]);
    }

    [TestMethod]
    public void CheckBoxes_StartIsExactlyNow_SaysItMustBeInTheFuture()
    {
        Assert.AreEqual("start must be in the future", Check(starts: "2026-11-22 12:00")[ShowingInput.BOX_STARTS]);
    }

    [TestMethod]
    public void CheckBoxes_StartOneMinuteAfterNow_IsAccepted()
    {
        Assert.AreEqual("", Check(starts: "2026-11-22 12:01")[ShowingInput.BOX_STARTS]);
    }

    // ---- price ----

    [TestMethod]
    public void CheckBoxes_EmptyPrice_SaysItIsEmpty()
    {
        Assert.AreEqual("price cannot be empty", Check(price: "")[ShowingInput.BOX_PRICE]);
    }

    [TestMethod]
    public void CheckBoxes_PriceIsText_SaysAnAmount()
    {
        Assert.AreEqual("price must be an amount like 12.50", Check(price: "cheap")[ShowingInput.BOX_PRICE]);
    }

    [TestMethod]
    public void CheckBoxes_PriceWithComma_SaysAnAmount()
    {
        Assert.AreEqual("price must be an amount like 12.50", Check(price: "12,50")[ShowingInput.BOX_PRICE]);
    }

    [TestMethod]
    public void CheckBoxes_PriceWithThreeDecimals_SaysTwoDecimalsAtMost()
    {
        Assert.AreEqual("price can have two decimals at most", Check(price: "12.505")[ShowingInput.BOX_PRICE]);
    }

    [TestMethod]
    public void CheckBoxes_PriceIsZero_SaysAboveZero()
    {
        Assert.AreEqual("price must be above 0", Check(price: "0")[ShowingInput.BOX_PRICE]);
    }

    [TestMethod]
    public void CheckBoxes_PriceIsNegative_SaysAboveZero()
    {
        Assert.AreEqual("price must be above 0", Check(price: "-5")[ShowingInput.BOX_PRICE]);
    }

    [TestMethod]
    public void CheckBoxes_PriceOverOneThousand_SaysTooHigh()
    {
        Assert.AreEqual("price must be 1000 or less", Check(price: "1000.01")[ShowingInput.BOX_PRICE]);
    }

    [TestMethod]
    public void CheckBoxes_PriceIsExactlyOneThousand_IsAccepted()
    {
        Assert.AreEqual("", Check(price: "1000")[ShowingInput.BOX_PRICE]);
    }

    // ---- all the boxes together ----

    [TestMethod]
    public void TryCreate_EveryBoxWrong_GivesFiveLinesInPageOrder()
    {
        bool is_good = ShowingInput.TryCreate("0", "0", "0", "soon", "0", NOW,
            out ShowingInput? input, out string error_message);

        Assert.IsFalse(is_good);
        Assert.IsNull(input);
        Assert.AreEqual(
            "showing id must be above 0\n"
            + "film id must be above 0\n"
            + "screen id must be above 0\n"
            + "start must look like 2026-12-01 18:30\n"
            + "price must be above 0",
            error_message);
    }

    [TestMethod]
    public void TryCreate_OnlyOneBoxWrong_GivesOnlyThatLine()
    {
        bool is_good = ShowingInput.TryCreate(GOOD_ID, "0", GOOD_SCREEN, GOOD_STARTS, GOOD_PRICE, NOW,
            out ShowingInput? input, out string error_message);

        Assert.IsFalse(is_good);
        Assert.AreEqual("film id must be above 0", error_message);
    }
}
