// tests for the films page search and the genre list (owner: divyesh)
// written before the code for card 347, so they fail first (red) and pass after (green)
using System.IO;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmSearchTests
{
    // three films with two spellings of the same genre, so the genre list must not repeat it
    private static FilmService MakeService()
    {
        HashTable<int, Film> films = new HashTable<int, Film>();
        films.Add(1, new Film(1, "The Last Voyage", "Adventure", 2023, 128, 12, "A crew sails to the edge of the map", "last_voyage.jpg"));
        films.Add(2, new Film(2, "Iron Harbour", "Action", 2024, 115, 15, "A dockworker uncovers a smuggling ring", "iron_harbour.jpg"));
        films.Add(3, new Film(3, "Harbour Lights", "action", 2021, 101, 12, "Two sisters run a lighthouse", ""));
        return new FilmService(films);
    }

    // search: title and genre together

    [TestMethod]
    public void Search_PartOfTitleAnyCase_FindsBothHarbourFilms()
    {
        FilmService film_service = MakeService();

        Film[] results = film_service.Search("hARBour", "");

        Assert.AreEqual(2, results.Length);
    }

    [TestMethod]
    public void Search_TitleAndGenre_KeepsOnlyThatGenre()
    {
        FilmService film_service = MakeService();

        Film[] results = film_service.Search("voyage", "Adventure");

        Assert.AreEqual(1, results.Length);
        Assert.AreEqual("The Last Voyage", results[0].Title);
    }

    [TestMethod]
    public void Search_GenreAnyCase_MatchesBothSpellings()
    {
        FilmService film_service = MakeService();

        Film[] results = film_service.Search("", "ACTION");

        Assert.AreEqual(2, results.Length);
    }

    [TestMethod]
    public void Search_PartOfGenre_DoesNotMatch()
    {
        FilmService film_service = MakeService();

        Film[] results = film_service.Search("", "Act");

        Assert.AreEqual(0, results.Length);
    }

    [TestMethod]
    public void Search_EmptyTitleAndNoGenre_ReturnsEveryFilm()
    {
        FilmService film_service = MakeService();

        Film[] results = film_service.Search("", "");

        Assert.AreEqual(3, results.Length);
    }

    [TestMethod]
    public void Search_NoFilmMatches_ReturnsEmpty()
    {
        FilmService film_service = MakeService();

        Film[] results = film_service.Search("zzz", "");

        Assert.AreEqual(0, results.Length);
    }

    // genre list for the filter

    [TestMethod]
    public void ListGenres_SameGenreTwoSpellings_ListedOnce()
    {
        FilmService film_service = MakeService();

        string[] genres = film_service.ListGenres();

        Assert.AreEqual(2, genres.Length);
    }

    [TestMethod]
    public void ListGenres_NoFilms_ReturnsEmpty()
    {
        FilmService film_service = new FilmService(new HashTable<int, Film>());

        string[] genres = film_service.ListGenres();

        Assert.AreEqual(0, genres.Length);
    }
}
