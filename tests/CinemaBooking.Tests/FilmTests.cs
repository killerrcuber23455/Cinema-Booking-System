// film validation tests (owner: akilesh)
// written from docs/design/validation-rules.md, card 355
using CinemaBooking.Models;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmTests
{
    // a film that passes every rule, each test changes one field
    private static Film MakeFilm(string title = "Inception", int duration = 148)
    {
        return new Film(1, title, "Sci-Fi", 2010, duration, 12,
            "A thief who steals secrets through dreams", "inception.jpg");
    }

    [TestMethod]
    public void IsValid_ValidFilm_IsAccepted()
    {
        var film = MakeFilm();

        bool result = film.IsValid(out string errors);

        Assert.IsTrue(result);
        Assert.AreEqual("", errors);
    }

    // title

    [TestMethod]
    public void IsValidTitle_Empty_IsRefused()
    {
        bool result = Film.IsValidTitle("", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("title cannot be empty and must be 100 characters or fewer", error_message);
    }

    [TestMethod]
    public void IsValidTitle_OnlySpaces_IsRefused()
    {
        Assert.IsFalse(Film.IsValidTitle("   ", out _));
    }

    [TestMethod]
    public void IsValidTitle_100Characters_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidTitle(new string('a', 100), out _));
    }

    [TestMethod]
    public void IsValidTitle_101Characters_IsRefused()
    {
        Assert.IsFalse(Film.IsValidTitle(new string('a', 101), out _));
    }

    // genre

    [TestMethod]
    public void IsValidGenre_Empty_IsRefused()
    {
        bool result = Film.IsValidGenre("", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("genre cannot be empty and must be maximum 50 in length", error_message);
    }

    [TestMethod]
    public void IsValidGenre_50Characters_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidGenre(new string('g', 50), out _));
    }

    [TestMethod]
    public void IsValidGenre_51Characters_IsRefused()
    {
        Assert.IsFalse(Film.IsValidGenre(new string('g', 51), out _));
    }

    // year, the upper limit moves every January so it is worked out here

    [TestMethod]
    public void IsValidYear_1899_IsRefused()
    {
        Assert.IsFalse(Film.IsValidYear(1899, out _));
    }

    [TestMethod]
    public void IsValidYear_1900_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidYear(1900, out _));
    }

    [TestMethod]
    public void IsValidYear_NextYear_IsAccepted()
    {
        int next_year = DateTime.Now.Year + 1;

        Assert.IsTrue(Film.IsValidYear(next_year, out _));
    }

    [TestMethod]
    public void IsValidYear_YearAfterNextYear_IsRefused()
    {
        int too_late = DateTime.Now.Year + 2;

        Assert.IsFalse(Film.IsValidYear(too_late, out _));
    }

    // duration

    [TestMethod]
    public void IsValidDuration_Zero_IsRefused()
    {
        bool result = Film.IsValidDuration(0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("duration must be between 1 and 300 minutes", error_message);
    }

    [TestMethod]
    public void IsValidDuration_Negative_IsRefused()
    {
        Assert.IsFalse(Film.IsValidDuration(-1, out _));
    }

    [TestMethod]
    public void IsValidDuration_1_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidDuration(1, out _));
    }

    [TestMethod]
    public void IsValidDuration_300_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidDuration(300, out _));
    }

    [TestMethod]
    public void IsValidDuration_301_IsRefused()
    {
        Assert.IsFalse(Film.IsValidDuration(301, out _));
    }

    // age rating

    [TestMethod]
    [DataRow(0)]
    [DataRow(12)]
    [DataRow(15)]
    [DataRow(18)]
    public void IsValidAgeRating_AllowedValue_IsAccepted(int rating)
    {
        Assert.IsTrue(Film.IsValidAgeRating(rating, out _));
    }

    [TestMethod]
    [DataRow(10)]
    [DataRow(-1)]
    [DataRow(21)]
    public void IsValidAgeRating_OtherValue_IsRefused(int rating)
    {
        bool result = Film.IsValidAgeRating(rating, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("age rating must be 0, 12, 15 or 18", error_message);
    }

    // description is optional

    [TestMethod]
    public void IsValidDescription_NullOrEmpty_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidDescription(null!, out _));
        Assert.IsTrue(Film.IsValidDescription("", out _));
    }

    [TestMethod]
    public void IsValidDescription_500Characters_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidDescription(new string('d', 500), out _));
    }

    [TestMethod]
    public void IsValidDescription_501Characters_IsRefused()
    {
        bool result = Film.IsValidDescription(new string('d', 501), out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("description must be 500 characters or fewer", error_message);
    }

    // poster is optional, a file name only

    [TestMethod]
    [DataRow(null)]
    [DataRow("")]
    [DataRow("   ")]
    public void IsValidPosterFile_Empty_IsAccepted(string? poster)
    {
        Assert.IsTrue(Film.IsValidPosterFile(poster!, out _));
    }

    [TestMethod]
    public void IsValidPosterFile_FileName_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidPosterFile("inception.jpg", out _));
    }

    [TestMethod]
    [DataRow("images/a.jpg")]
    [DataRow("images\\a.jpg")]
    public void IsValidPosterFile_WithFolderPath_IsRefused(string poster)
    {
        bool result = Film.IsValidPosterFile(poster, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("poster must be a file name only, up to 100 characters", error_message);
    }

    [TestMethod]
    public void IsValidPosterFile_101Characters_IsRefused()
    {
        Assert.IsFalse(Film.IsValidPosterFile(new string('p', 101), out _));
    }

    // id, the service gives the id before IsValid so 0 means it was never given

    [TestMethod]
    public void IsValidId_Zero_IsRefused()
    {
        bool result = Film.IsValidId(0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("film id must be a positive number", error_message);
    }

    [TestMethod]
    public void IsValidId_1_IsAccepted()
    {
        Assert.IsTrue(Film.IsValidId(1, out _));
    }

    // whole film

    [TestMethod]
    public void IsValid_TwoBadFields_GivesOneLineEach()
    {
        var film = MakeFilm(title: "", duration: 0);

        bool result = film.IsValid(out string errors);

        Assert.IsFalse(result);
        Assert.Contains("title cannot be empty", errors);
        Assert.Contains("duration must be between 1 and 300 minutes", errors);
        Assert.HasCount(2, errors.Split('\n', StringSplitOptions.RemoveEmptyEntries));
    }
}
