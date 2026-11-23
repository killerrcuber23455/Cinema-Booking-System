using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class CatalogStoreTests
{
    private FakeDatabase _database = null!;
    private HashTable<int, Film> _films = null!;
    private ShowingTable _showings = null!;
    private FilmService _film_service = null!;
    private CatalogStore _store = null!;

    private static Film MakeFilm(int id, string title)
    {
        return new Film(id, title, "Drama", 2020, 100, 12, "a film", "a.jpg");
    }

    [TestInitialize]
    public void Setup()
    {
        _database = new FakeDatabase();
        _films = new HashTable<int, Film>();
        _showings = new ShowingTable();
        _film_service = new FilmService(_films);
        _store = new CatalogStore(_database, _films, _showings, _film_service);
    }

    [TestMethod]
    public void LoadAll_DatabaseWorks_ReturnsTrue()
    {
        bool is_ok = _store.LoadAll(out string message);

        Assert.IsTrue(is_ok);
        Assert.AreEqual("", message);
    }

    [TestMethod]
    public void LoadAll_FillsTheFilmTable()
    {
        _database.AddFilm(MakeFilm(1, "Alpha"));
        _database.AddFilm(MakeFilm(2, "Beta"));

        _store.LoadAll(out _);

        Assert.IsNotNull(_films.Find(1));
        Assert.IsNotNull(_films.Find(2));
    }

    [TestMethod]
    public void LoadAll_FillsTheShowingTable()
    {
        _database.AddFilm(MakeFilm(1, "Alpha"));
        Showing showing = new Showing(1, 1, 1, new DateTime(2026, 12, 1, 18, 0, 0), 150m, new SeatGrid(8, 12));
        _database.SaveShowing(showing);

        _store.LoadAll(out _);

        Assert.IsNotNull(_showings.FindShowing(1));
    }

    [TestMethod]
    public void LoadAll_DatabaseThrows_ReturnsFalseWithTheDownMessage()
    {
        _database.ThrowOnLoad = true;

        bool is_ok = _store.LoadAll(out string message);

        Assert.IsFalse(is_ok);
        Assert.AreEqual(CatalogStore.DATABASE_DOWN_TEXT, message);
    }

    [TestMethod]
    public void AddFilm_Admin_SavesToTheDatabase()
    {
        bool is_added = _store.AddFilm(MakeFilm(7, "Gamma"), Role.Admin, out string error);

        Assert.IsTrue(is_added, error);
        HashTable<int, Film> from_database = new HashTable<int, Film>();
        _database.LoadFilms(from_database);
        Assert.IsNotNull(from_database.Find(7));
    }

    [TestMethod]
    public void AddFilm_Admin_FilmIsInTheFilmService()
    {
        _store.AddFilm(MakeFilm(7, "Gamma"), Role.Admin, out _);

        Assert.AreEqual(1, _film_service.SearchByTitle("gamma").Length);
    }

    [TestMethod]
    public void AddFilm_NotAdmin_RefusedAndNotSaved()
    {
        bool is_added = _store.AddFilm(MakeFilm(7, "Gamma"), Role.Customer, out string error);

        Assert.IsFalse(is_added);
        Assert.AreEqual("Only an admin can add a film", error);
        HashTable<int, Film> from_database = new HashTable<int, Film>();
        _database.LoadFilms(from_database);
        Assert.IsNull(from_database.Find(7));
    }

    [TestMethod]
    public void AddFilm_SaveRefused_FilmIsTakenBackOut()
    {
        _database.FilmSaveFails = true;

        bool is_added = _store.AddFilm(MakeFilm(7, "Gamma"), Role.Admin, out string error);

        Assert.IsFalse(is_added);
        Assert.AreEqual(CatalogStore.SAVE_FAILED_TEXT, error);
        Assert.AreEqual(0, _film_service.SearchByTitle("gamma").Length);
    }

    [TestMethod]
    public void AddFilm_DatabaseThrows_FilmIsTakenBackOut()
    {
        _database.ThrowOnSaveFilm = true;

        bool is_added = _store.AddFilm(MakeFilm(7, "Gamma"), Role.Admin, out string error);

        Assert.IsFalse(is_added);
        Assert.AreEqual(CatalogStore.SAVE_FAILED_TEXT, error);
        Assert.AreEqual(0, _film_service.SearchByTitle("gamma").Length);
    }
}
