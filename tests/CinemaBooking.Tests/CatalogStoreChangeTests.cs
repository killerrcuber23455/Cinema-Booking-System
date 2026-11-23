// tests for removing and editing a film so that the database changes too (owner: divyesh)
// written before the code for card 444, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class CatalogStoreChangeTests
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

        // film 1 is in the program and in the database, like after LoadAll at startup
        Film film = MakeFilm(1, "Alpha");
        _film_service.AddFilm(film, Role.Admin, out _);
        _database.AddFilm(film);
    }

    // reads the film from the fake database, null if the database does not have it
    private Film? FilmInDatabase(int film_id)
    {
        HashTable<int, Film> table = new HashTable<int, Film>();
        _database.LoadFilms(table);
        return table.Find(film_id);
    }

    // ---------- remove ----------

    [TestMethod]
    public void RemoveFilm_Admin_RemovesFromTheProgramAndTheDatabase()
    {
        bool is_removed = _store.RemoveFilm(1, Role.Admin, out string message);

        Assert.IsTrue(is_removed);
        Assert.AreEqual("", message);
        Assert.IsNull(_films.Find(1));
        Assert.IsNull(FilmInDatabase(1));
    }

    [TestMethod]
    public void RemoveFilm_NotAdmin_ReturnsFalseAndKeepsTheFilm()
    {
        bool is_removed = _store.RemoveFilm(1, Role.Customer, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("Only an admin can remove a film", message);
        Assert.IsNotNull(_films.Find(1));
        Assert.IsNotNull(FilmInDatabase(1));
    }

    [TestMethod]
    public void RemoveFilm_UnknownId_ReturnsFalseWithTheNotFoundMessage()
    {
        bool is_removed = _store.RemoveFilm(99, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual("No film with id 99 was found", message);
    }

    [TestMethod]
    public void RemoveFilm_DatabaseRefuses_KeepsTheFilmInTheProgram()
    {
        _database.FilmDeleteFails = true;

        bool is_removed = _store.RemoveFilm(1, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual(CatalogStore.DELETE_FAILED_TEXT, message);
        Assert.IsNotNull(_films.Find(1));
    }

    [TestMethod]
    public void RemoveFilm_DatabaseThrows_KeepsTheFilmInTheProgram()
    {
        _database.ThrowOnDeleteFilm = true;

        bool is_removed = _store.RemoveFilm(1, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual(CatalogStore.DELETE_FAILED_TEXT, message);
        Assert.IsNotNull(_films.Find(1));
    }

    [TestMethod]
    public void RemoveFilm_AShowingStillUsesIt_KeepsTheFilm()
    {
        Showing showing = new Showing(1, 1, 1, new DateTime(2026, 12, 1, 18, 0, 0), 150m, new SeatGrid(8, 12));
        _database.SaveShowing(showing);

        bool is_removed = _store.RemoveFilm(1, Role.Admin, out string message);

        Assert.IsFalse(is_removed);
        Assert.AreEqual(CatalogStore.DELETE_FAILED_TEXT, message);
        Assert.IsNotNull(_films.Find(1));
        Assert.IsNotNull(FilmInDatabase(1));
    }

    // ---------- edit ----------

    [TestMethod]
    public void EditFilm_Admin_ChangesTheProgramAndTheDatabase()
    {
        bool is_edited = _store.EditFilm(MakeFilm(1, "Alpha Reloaded"), Role.Admin, out string message);

        Assert.IsTrue(is_edited);
        Assert.AreEqual("", message);
        Assert.AreEqual("Alpha Reloaded", _films.Find(1)!.Title);
        Assert.AreEqual("Alpha Reloaded", FilmInDatabase(1)!.Title);
    }

    [TestMethod]
    public void EditFilm_NotAdmin_ReturnsFalseAndChangesNothing()
    {
        bool is_edited = _store.EditFilm(MakeFilm(1, "Hacked"), Role.Guest, out string message);

        Assert.IsFalse(is_edited);
        Assert.AreEqual(FilmEditor.NOT_ADMIN_TEXT, message);
        Assert.AreEqual("Alpha", _films.Find(1)!.Title);
        Assert.AreEqual("Alpha", FilmInDatabase(1)!.Title);
    }

    [TestMethod]
    public void EditFilm_UnknownId_ReturnsFalseWithTheNotFoundMessage()
    {
        bool is_edited = _store.EditFilm(MakeFilm(99, "Ghost"), Role.Admin, out string message);

        Assert.IsFalse(is_edited);
        Assert.AreEqual(FilmEditor.NotFoundText(99), message);
    }

    [TestMethod]
    public void EditFilm_BadDetails_ReturnsFalseAndChangesNothing()
    {
        bool is_edited = _store.EditFilm(MakeFilm(1, ""), Role.Admin, out string message);

        Assert.IsFalse(is_edited);
        Assert.AreNotEqual("", message);
        Assert.AreEqual("Alpha", _films.Find(1)!.Title);
        Assert.AreEqual("Alpha", FilmInDatabase(1)!.Title);
    }

    [TestMethod]
    public void EditFilm_DatabaseRefuses_PutsTheOldFilmBack()
    {
        _database.FilmUpdateFails = true;

        bool is_edited = _store.EditFilm(MakeFilm(1, "Alpha Reloaded"), Role.Admin, out string message);

        Assert.IsFalse(is_edited);
        Assert.AreEqual(CatalogStore.UPDATE_FAILED_TEXT, message);
        Assert.AreEqual("Alpha", _films.Find(1)!.Title);
    }

    [TestMethod]
    public void EditFilm_DatabaseThrows_PutsTheOldFilmBack()
    {
        _database.ThrowOnUpdateFilm = true;

        bool is_edited = _store.EditFilm(MakeFilm(1, "Alpha Reloaded"), Role.Admin, out string message);

        Assert.IsFalse(is_edited);
        Assert.AreEqual(CatalogStore.UPDATE_FAILED_TEXT, message);
        Assert.AreEqual("Alpha", _films.Find(1)!.Title);
    }
}
