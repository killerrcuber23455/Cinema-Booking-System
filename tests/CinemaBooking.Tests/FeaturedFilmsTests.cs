// tests for which films are shown big at the top of the films page (owner: divyesh)
// written before the code of UI upgrade step 2, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class FeaturedFilmsTests
{
    private static Film MakeFilm(int id, string title, int year)
    {
        return new Film(id, title, "Drama", year, 120, 12, "a film", "");
    }

    [TestMethod]
    public void FeaturedFilms_NewestFilmComesFirst()
    {
        Film[] films = { MakeFilm(1, "Old", 2019), MakeFilm(2, "New", 2025), MakeFilm(3, "Middle", 2022) };

        Film[] featured = FilmPageRules.FeaturedFilms(films, 5);

        Assert.AreEqual("New", featured[0].Title);
        Assert.AreEqual("Middle", featured[1].Title);
        Assert.AreEqual("Old", featured[2].Title);
    }

    [TestMethod]
    public void FeaturedFilms_SameYear_AreInTitleOrder()
    {
        Film[] films = { MakeFilm(1, "Zebra", 2024), MakeFilm(2, "Apple", 2024) };

        Film[] featured = FilmPageRules.FeaturedFilms(films, 5);

        Assert.AreEqual("Apple", featured[0].Title);
        Assert.AreEqual("Zebra", featured[1].Title);
    }

    [TestMethod]
    public void FeaturedFilms_KeepsAtMostTheMaximum()
    {
        Film[] films = { MakeFilm(1, "A", 2020), MakeFilm(2, "B", 2021), MakeFilm(3, "C", 2022), MakeFilm(4, "D", 2023) };

        Film[] featured = FilmPageRules.FeaturedFilms(films, 2);

        Assert.AreEqual(2, featured.Length);
        Assert.AreEqual("D", featured[0].Title);
    }

    [TestMethod]
    public void FeaturedFilms_NullList_GivesNoFilms()
    {
        Film[] featured = FilmPageRules.FeaturedFilms(null!, 5);

        Assert.AreEqual(0, featured.Length);
    }

    // the films page still needs the full list in its own order
    [TestMethod]
    public void FeaturedFilms_DoesNotChangeTheListItWasGiven()
    {
        Film[] films = { MakeFilm(1, "Old", 2019), MakeFilm(2, "New", 2025) };

        FilmPageRules.FeaturedFilms(films, 5);

        Assert.AreEqual("Old", films[0].Title);
        Assert.AreEqual("New", films[1].Title);
    }
}
