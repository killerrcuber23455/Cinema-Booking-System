// tests for who sees the "Load film file" button and what the empty films page says (owner: divyesh)
// written before the code for card 437, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmPageRulesTests
{
    // a role number that does not exist, it must never get more than a guest
    private const Role UNKNOWN_ROLE = (Role)99;

    // CanLoadFilms

    [TestMethod]
    public void CanLoadFilms_Guest_IsFalse()
    {
        Assert.IsFalse(FilmPageRules.CanLoadFilms(Role.Guest));
    }

    [TestMethod]
    public void CanLoadFilms_Customer_IsFalse()
    {
        Assert.IsFalse(FilmPageRules.CanLoadFilms(Role.Customer));
    }

    [TestMethod]
    public void CanLoadFilms_Staff_IsTrue()
    {
        Assert.IsTrue(FilmPageRules.CanLoadFilms(Role.Staff));
    }

    [TestMethod]
    public void CanLoadFilms_Admin_IsTrue()
    {
        Assert.IsTrue(FilmPageRules.CanLoadFilms(Role.Admin));
    }

    [TestMethod]
    public void CanLoadFilms_UnknownRole_IsFalse()
    {
        Assert.IsFalse(FilmPageRules.CanLoadFilms(UNKNOWN_ROLE));
    }

    // EmptyFilmsText

    [TestMethod]
    public void EmptyFilmsText_Staff_TellsThemToLoadTheFilmFile()
    {
        StringAssert.Contains(FilmPageRules.EmptyFilmsText(Role.Staff), "Load film file");
    }

    [TestMethod]
    public void EmptyFilmsText_Guest_DoesNotMentionTheButton()
    {
        string text = FilmPageRules.EmptyFilmsText(Role.Guest);

        Assert.IsFalse(text.Contains("Load film file"), "the guest text was: " + text);
        Assert.IsTrue(text.Length > 0);
    }

    [TestMethod]
    public void EmptyFilmsText_Customer_DoesNotMentionTheButton()
    {
        string text = FilmPageRules.EmptyFilmsText(Role.Customer);

        Assert.IsFalse(text.Contains("Load film file"), "the customer text was: " + text);
        Assert.IsTrue(text.Length > 0);
    }
}
