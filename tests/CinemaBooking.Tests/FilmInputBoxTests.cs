// tests for the one message per box of the admin film form (owner: divyesh)
// written before the code for card 434, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmInputBoxTests
{
    // a good set of values, each test changes only the one it is about
    private static string[] Check(
        string id_text = "7", string title = "Heat", string genre = "Crime", string year_text = "1995",
        string duration_text = "170", string age_rating_text = "15", string description = "A thief and a cop.",
        string poster_file = "heat.jpg")
    {
        return FilmInput.CheckBoxes(id_text, title, genre, year_text, duration_text, age_rating_text,
            description, poster_file);
    }

    // true when every box except the given one has no message
    private static bool OnlyThisBoxHasAMessage(string[] messages, int box)
    {
        for (int index = 0; index < messages.Length; index++)
        {
            if (index != box && messages[index] != "")
            {
                return false;
            }
        }
        return messages[box] != "";
    }

    [TestMethod]
    public void CheckBoxes_GoodValues_AllMessagesAreEmpty()
    {
        string[] messages = Check();

        Assert.AreEqual(FilmInput.BOX_COUNT, messages.Length);
        for (int index = 0; index < messages.Length; index++)
        {
            Assert.AreEqual("", messages[index]);
        }
    }

    [TestMethod]
    public void CheckBoxes_IdNotANumber_OnlyTheIdBoxHasTheNumberMessage()
    {
        string[] messages = Check(id_text: "abc");

        InputCheck.TryReadWholeNumber("abc", "id", out int _, out string expected);
        Assert.IsTrue(OnlyThisBoxHasAMessage(messages, FilmInput.BOX_ID));
        Assert.AreEqual(expected.TrimEnd('\n'), messages[FilmInput.BOX_ID]);
    }

    [TestMethod]
    public void CheckBoxes_EmptyTitle_OnlyTheTitleBoxHasTheFilmRuleMessage()
    {
        string[] messages = Check(title: "");

        Film.IsValidTitle("", out string expected);
        Assert.IsTrue(OnlyThisBoxHasAMessage(messages, FilmInput.BOX_TITLE));
        Assert.AreEqual(expected.TrimEnd('\n'), messages[FilmInput.BOX_TITLE]);
    }

    [TestMethod]
    public void CheckBoxes_YearNotANumber_OnlyTheYearBoxHasAMessage()
    {
        string[] messages = Check(year_text: "nineteen");

        Assert.IsTrue(OnlyThisBoxHasAMessage(messages, FilmInput.BOX_YEAR));
    }

    [TestMethod]
    public void CheckBoxes_PosterWithAFolder_OnlyThePosterBoxHasTheFilmRuleMessage()
    {
        string[] messages = Check(poster_file: "images/heat.jpg");

        Film.IsValidPosterFile("images/heat.jpg", out string expected);
        Assert.IsTrue(OnlyThisBoxHasAMessage(messages, FilmInput.BOX_POSTER));
        Assert.AreEqual(expected.TrimEnd('\n'), messages[FilmInput.BOX_POSTER]);
    }

    [TestMethod]
    public void CheckBoxes_TwoWrongBoxes_BothHaveAMessageAndTheOthersDoNot()
    {
        string[] messages = Check(genre: "", duration_text: "abc");

        Assert.AreNotEqual("", messages[FilmInput.BOX_GENRE]);
        Assert.AreNotEqual("", messages[FilmInput.BOX_DURATION]);
        Assert.AreEqual("", messages[FilmInput.BOX_ID]);
        Assert.AreEqual("", messages[FilmInput.BOX_TITLE]);
        Assert.AreEqual("", messages[FilmInput.BOX_YEAR]);
        Assert.AreEqual("", messages[FilmInput.BOX_AGE_RATING]);
        Assert.AreEqual("", messages[FilmInput.BOX_DESCRIPTION]);
        Assert.AreEqual("", messages[FilmInput.BOX_POSTER]);
    }

    [TestMethod]
    public void CheckBoxes_MissingValues_DoNotCrashAndTheRequiredBoxesHaveAMessage()
    {
        string[] messages = FilmInput.CheckBoxes(null!, null!, null!, null!, null!, null!, null!, null!);

        // the description and the poster may be empty, so only the other six boxes must complain
        Assert.AreEqual(FilmInput.BOX_COUNT, messages.Length);
        Assert.AreNotEqual("", messages[FilmInput.BOX_ID]);
        Assert.AreNotEqual("", messages[FilmInput.BOX_TITLE]);
        Assert.AreNotEqual("", messages[FilmInput.BOX_GENRE]);
        Assert.AreNotEqual("", messages[FilmInput.BOX_YEAR]);
        Assert.AreNotEqual("", messages[FilmInput.BOX_DURATION]);
        Assert.AreNotEqual("", messages[FilmInput.BOX_AGE_RATING]);
        Assert.AreEqual("", messages[FilmInput.BOX_DESCRIPTION]);
        Assert.AreEqual("", messages[FilmInput.BOX_POSTER]);
    }
}
