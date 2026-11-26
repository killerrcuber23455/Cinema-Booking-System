// tests for loading the film file so that the films are saved to the database too (owner: divyesh)
// written before the code for the bug "films loaded from the film file are not saved", so they fail first (red)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class CatalogStoreFileTests
{
    private const string HEADER = "id,title,genre,year,duration_minutes,age_rating,description,poster_file";

    private FakeDatabase _database = null!;
    private HashTable<int, Film> _films = null!;
    private FilmService _film_service = null!;
    private CatalogStore _store = null!;
    private string _file_path = "";

    [TestInitialize]
    public void Setup()
    {
        _database = new FakeDatabase();
        _films = new HashTable<int, Film>();
        _film_service = new FilmService(_films);
        _store = new CatalogStore(_database, _films, new ShowingTable(), _film_service);
        _file_path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_file_path))
        {
            File.Delete(_file_path);
        }
    }

    // writes a film file with the given rows under the header
    private void WriteFile(params string[] rows)
    {
        File.WriteAllLines(_file_path, new string[] { HEADER }.Concat(rows));
    }

    // reads the film from the fake database, null if the database does not have it
    private Film? FilmInDatabase(int film_id)
    {
        HashTable<int, Film> table = new HashTable<int, Film>();
        _database.LoadFilms(table);
        return table.Find(film_id);
    }

    [TestMethod]
    public void LoadFilmFile_GoodFile_FilmsAreInTheProgramAndTheDatabase()
    {
        WriteFile("1,Alpha,Drama,2020,100,12,A film,a.jpg", "2,Beta,Comedy,2021,90,0,Another film,b.jpg");

        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Admin, out string message);

        Assert.IsTrue(is_loaded);
        StringAssert.Contains(message, "loaded 2 films");
        Assert.IsNotNull(_films.Find(1));
        Assert.IsNotNull(_films.Find(2));
        Assert.IsNotNull(FilmInDatabase(1));
        Assert.IsNotNull(FilmInDatabase(2));
    }

    [TestMethod]
    public void LoadFilmFile_MissingFile_ReturnsFalseAndSavesNothing()
    {
        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Admin, out string message);

        Assert.IsFalse(is_loaded);
        StringAssert.Contains(message, "not found");
        Assert.IsNull(FilmInDatabase(1));
    }

    [TestMethod]
    public void LoadFilmFile_FilmAlreadyKnown_OnlyTheNewFilmIsSaved()
    {
        Film existing = new Film(1, "Alpha", "Drama", 2020, 100, 12, "A film", "a.jpg");
        _film_service.AddFilm(existing, Role.Admin, out _);
        _database.AddFilm(existing);
        WriteFile("1,Changed Title,Drama,2020,100,12,A film,a.jpg", "2,Beta,Comedy,2021,90,0,Another film,b.jpg");

        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Admin, out string message);

        Assert.IsTrue(is_loaded);
        StringAssert.Contains(message, "loaded 1 films");
        Assert.AreEqual("Alpha", _films.Find(1)!.Title);
        Assert.AreEqual("Alpha", FilmInDatabase(1)!.Title);
        Assert.IsNotNull(FilmInDatabase(2));
    }

    [TestMethod]
    public void LoadFilmFile_DatabaseRefuses_FilmsAreTakenBackOutAndTheMessageSaysSo()
    {
        _database.FilmSaveFails = true;
        WriteFile("1,Alpha,Drama,2020,100,12,A film,a.jpg", "2,Beta,Comedy,2021,90,0,Another film,b.jpg");

        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Admin, out string message);

        Assert.IsFalse(is_loaded);
        StringAssert.Contains(message, "could not be saved to the database");
        Assert.IsNull(_films.Find(1));
        Assert.IsNull(_films.Find(2));
    }

    [TestMethod]
    public void LoadFilmFile_DatabaseThrows_FilmsAreTakenBackOutAndTheMessageSaysSo()
    {
        _database.ThrowOnSaveFilm = true;
        WriteFile("1,Alpha,Drama,2020,100,12,A film,a.jpg");

        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Admin, out string message);

        Assert.IsFalse(is_loaded);
        StringAssert.Contains(message, "could not be saved to the database");
        Assert.IsNull(_films.Find(1));
    }

    [TestMethod]
    public void LoadFilmFile_BadRowIsSkippedAndTheGoodOnesAreSaved()
    {
        WriteFile("1,Alpha,Drama,2020,100,12,A film,a.jpg", "not a film row");

        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Admin, out string message);

        Assert.IsTrue(is_loaded);
        StringAssert.Contains(message, "skipped 1 rows");
        Assert.IsNotNull(FilmInDatabase(1));
    }
}
