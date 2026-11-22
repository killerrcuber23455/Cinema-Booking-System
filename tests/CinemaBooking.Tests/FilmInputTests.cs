// tests for turning what the admin typed into a film (owner: divyesh)
// written before the code for card 351, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmInputTests
{
    // a good set of values, each test changes only the one it is about
    private static bool Read(out Film? film, out string error_message,
        string id_text = "7", string title = "Heat", string genre = "Crime", string year_text = "1995",
        string duration_text = "170", string age_rating_text = "15", string description = "A thief and a cop.",
        string poster_file = "heat.jpg")
    {
        return FilmInput.TryRead(id_text, title, genre, year_text, duration_text, age_rating_text,
            description, poster_file, out film, out error_message);
    }

    // good film
    [TestMethod]
    public void TryRead_GoodValues_ReturnsTrueWithNoMessage()
    {
        bool is_good = Read(out Film? film, out string error_message);

        Assert.IsTrue(is_good);
        Assert.AreEqual("", error_message);
        Assert.IsNotNull(film);
    }

    [TestMethod]
    public void TryRead_GoodValues_KeepsEveryField()
    {
        Read(out Film? film, out string error_message);

        Assert.AreEqual(7, film!.Id);
        Assert.AreEqual("Heat", film.Title);
        Assert.AreEqual("Crime", film.Genre);
        Assert.AreEqual(1995, film.Year);
        Assert.AreEqual(170, film.DurationMinutes);
        Assert.AreEqual(15, film.AgeRating);
        Assert.AreEqual("A thief and a cop.", film.Description);
        Assert.AreEqual("heat.jpg", film.PosterFile);
    }

    [TestMethod]
    public void TryRead_SpacesAroundText_AreRemoved()
    {
        Read(out Film? film, out string error_message, id_text: " 7 ", title: "  Heat ", genre: " Crime ",
            description: " A thief. ", poster_file: " heat.jpg ");

        Assert.AreEqual(7, film!.Id);
        Assert.AreEqual("Heat", film.Title);
        Assert.AreEqual("Crime", film.Genre);
        Assert.AreEqual("A thief.", film.Description);
        Assert.AreEqual("heat.jpg", film.PosterFile);
    }

    [TestMethod]
    public void TryRead_EmptyDescriptionAndPoster_AreAllowed()
    {
        bool is_good = Read(out Film? film, out string error_message, description: "", poster_file: "");

        Assert.IsTrue(is_good);
        Assert.AreEqual("", film!.PosterFile);
    }

    // numbers that cannot be read
    [TestMethod]
    public void TryRead_IdNotANumber_ReturnsFalseAndSaysId()
    {
        bool is_good = Read(out Film? film, out string error_message, id_text: "abc");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "id must be a whole number");
    }

    [TestMethod]
    public void TryRead_EmptyId_ReturnsFalse()
    {
        bool is_good = Read(out Film? film, out string error_message, id_text: "");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "id cannot be empty");
    }

    [TestMethod]
    public void TryRead_YearNotANumber_SaysYear()
    {
        bool is_good = Read(out Film? film, out string error_message, year_text: "19x5");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "year must be a whole number");
    }

    [TestMethod]
    public void TryRead_DurationWithDecimal_IsRefused()
    {
        bool is_good = Read(out Film? film, out string error_message, duration_text: "12.5");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "duration must be a whole number");
    }

    [TestMethod]
    public void TryRead_AgeRatingNotANumber_SaysAgeRating()
    {
        bool is_good = Read(out Film? film, out string error_message, age_rating_text: "PG");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "age rating must be a whole number");
    }

    [TestMethod]
    public void TryRead_SeveralNumbersWrong_ShowsEveryOne()
    {
        Read(out Film? film, out string error_message, id_text: "x", year_text: "y");

        StringAssert.Contains(error_message, "id must be a whole number");
        StringAssert.Contains(error_message, "year must be a whole number");
    }

    [TestMethod]
    public void TryRead_EverythingEmpty_ShowsEveryBox()
    {
        Read(out Film? film, out string error_message, id_text: "", title: "", genre: "", year_text: "",
            duration_text: "", age_rating_text: "");

        StringAssert.Contains(error_message, "id cannot be empty");
        StringAssert.Contains(error_message, "title");
        StringAssert.Contains(error_message, "genre");
        StringAssert.Contains(error_message, "year cannot be empty");
        StringAssert.Contains(error_message, "duration cannot be empty");
        StringAssert.Contains(error_message, "age rating cannot be empty");
    }

    [TestMethod]
    public void TryRead_EverythingEmpty_GivesOneLinePerBox()
    {
        // description and poster file may be empty, so six boxes are wrong
        Read(out Film? film, out string error_message, id_text: "", title: "", genre: "", year_text: "",
            duration_text: "", age_rating_text: "");

        Assert.AreEqual(6, error_message.Split('\n').Length);
    }

    [TestMethod]
    public void TryRead_MessagesAreInPageOrder()
    {
        Read(out Film? film, out string error_message, id_text: "", title: "", genre: "", year_text: "",
            duration_text: "", age_rating_text: "");

        int id_place = error_message.IndexOf("id cannot be empty");
        int title_place = error_message.IndexOf("title");
        int genre_place = error_message.IndexOf("genre");
        int year_place = error_message.IndexOf("year cannot be empty");
        int duration_place = error_message.IndexOf("duration cannot be empty");
        int age_place = error_message.IndexOf("age rating cannot be empty");

        Assert.IsTrue(id_place < title_place);
        Assert.IsTrue(title_place < genre_place);
        Assert.IsTrue(genre_place < year_place);
        Assert.IsTrue(year_place < duration_place);
        Assert.IsTrue(duration_place < age_place);
    }

    [TestMethod]
    public void TryRead_BadNumberAndBadTitle_ShowsBoth()
    {
        Read(out Film? film, out string error_message, id_text: "x", title: "");

        StringAssert.Contains(error_message, "id must be a whole number");
        StringAssert.Contains(error_message, "title");
    }

    [TestMethod]
    public void TryRead_OnlyTheYearWrong_ShowsOnlyOneLine()
    {
        Read(out Film? film, out string error_message, year_text: "abc");

        Assert.AreEqual(1, error_message.Split('\n').Length);
        StringAssert.Contains(error_message, "year must be a whole number");
    }

    // numbers that are read but the film rules refuse
    [TestMethod]
    public void TryRead_IdZero_UsesTheFilmRuleMessage()
    {
        bool is_good = Read(out Film? film, out string error_message, id_text: "0");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "film id must be a positive number");
    }

    [TestMethod]
    public void TryRead_AgeRatingThirteen_UsesTheFilmRuleMessage()
    {
        bool is_good = Read(out Film? film, out string error_message, age_rating_text: "13");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "age rating must be 0, 12, 15 or 18");
    }

    [TestMethod]
    public void TryRead_EmptyTitle_SaysTitle()
    {
        bool is_good = Read(out Film? film, out string error_message, title: "   ");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "title cannot be empty");
    }

    [TestMethod]
    public void TryRead_PosterWithAFolder_IsRefused()
    {
        bool is_good = Read(out Film? film, out string error_message, poster_file: "C:\\posters\\heat.jpg");

        Assert.IsFalse(is_good);
        StringAssert.Contains(error_message, "file name only");
    }

    [TestMethod]
    public void TryRead_SeveralFilmRulesBroken_ShowsAllMessagesAtOnce()
    {
        Read(out Film? film, out string error_message, title: "", genre: "");

        StringAssert.Contains(error_message, "title");
        StringAssert.Contains(error_message, "genre");
    }

    // what comes back when it fails
    [TestMethod]
    public void TryRead_BadValues_GiveNoFilm()
    {
        Read(out Film? film, out string error_message, title: "");

        Assert.IsNull(film);
    }

    [TestMethod]
    public void TryRead_NullText_IsRefusedWithoutCrashing()
    {
        bool is_good = FilmInput.TryRead(null!, null!, null!, null!, null!, null!, null!, null!,
            out Film? film, out string error_message);

        Assert.IsFalse(is_good);
        Assert.IsNull(film);
    }
}
