// tests for the films page search, genre list and loading the film file the user picks (owner: divyesh)
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

    // loading the file the user picked

    [TestMethod]
    public void LoadFromFile_GoodFile_LoadsFilmsAndSaysHowMany()
    {
        FilmService film_service = new FilmService(new HashTable<int, Film>());
        string file_path = Path.GetTempFileName();
        File.WriteAllLines(file_path, new string[]
        {
            "id,title,genre,year,duration_minutes,age_rating,description,poster_file",
            "10,Night Train,Thriller,2020,110,15,A train that never stops,night_train.jpg",
            "11,Sun Garden,Drama,2019,95,0,Two neighbours share a garden,",
            "not a film row"
        });

        bool is_loaded = film_service.LoadFromFile(file_path, out string message);
        File.Delete(file_path);

        Assert.IsTrue(is_loaded);
        Assert.IsTrue(message.Contains("loaded 2 films, skipped 1 rows"), message);
        Assert.AreEqual(2, film_service.Search("", "").Length);
    }

    [TestMethod]
    public void LoadFromFile_MissingFile_ReturnsFalseWithReason()
    {
        FilmService film_service = new FilmService(new HashTable<int, Film>());

        bool is_loaded = film_service.LoadFromFile("no_such_folder/no_such_file.csv", out string message);

        Assert.IsFalse(is_loaded);
        Assert.IsTrue(message.Contains("not found"), message);
    }

    [TestMethod]
    public void LoadFromFile_EmptyPath_ReturnsFalseWithReason()
    {
        FilmService film_service = new FilmService(new HashTable<int, Film>());

        bool is_loaded = film_service.LoadFromFile("", out string message);

        Assert.IsFalse(is_loaded);
        Assert.AreEqual("no film file was chosen", message);
    }
}
