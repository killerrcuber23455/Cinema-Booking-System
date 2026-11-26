// tests that only staff and admin can load the film file, checked in the service and not only by hiding the button (owner: divyesh)
// written before the code for card 442 (HT-45: a guest can load the film file), so the refusal tests fail first (red)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class CatalogStoreFileRoleTests
{
    private const string HEADER = "id,title,genre,year,duration_minutes,age_rating,description,poster_file";

    private FakeDatabase _database = null!;
    private HashTable<int, Film> _films = null!;
    private CatalogStore _store = null!;
    private string _file_path = "";

    [TestInitialize]
    public void Setup()
    {
        _database = new FakeDatabase();
        _films = new HashTable<int, Film>();
        _store = new CatalogStore(_database, _films, new ShowingTable(), new FilmService(_films));
        _file_path = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString() + ".csv");
        File.WriteAllLines(_file_path, new string[] { HEADER, "1,Alpha,Drama,2020,100,12,A film,a.jpg" });
    }

    [TestCleanup]
    public void Cleanup()
    {
        if (File.Exists(_file_path))
        {
            File.Delete(_file_path);
        }
    }

    // true when the fake database holds a film with this id
    private bool IsInDatabase(int film_id)
    {
        HashTable<int, Film> table = new HashTable<int, Film>();
        _database.LoadFilms(table);
        return table.Find(film_id) != null;
    }

    // checks one refused role: false, the refusal text, nothing in the program and nothing in the database
    private void AssertRefused(Role role)
    {
        bool is_loaded = _store.LoadFilmFile(_file_path, role, out string message);

        Assert.IsFalse(is_loaded);
        Assert.AreEqual(CatalogStore.LOAD_NOT_ALLOWED_TEXT, message);
        Assert.IsNull(_films.Find(1));
        Assert.IsFalse(IsInDatabase(1));
    }

    [TestMethod]
    public void LoadFilmFile_Guest_IsRefusedAndNothingIsSaved()
    {
        AssertRefused(Role.Guest);
    }

    [TestMethod]
    public void LoadFilmFile_Customer_IsRefusedAndNothingIsSaved()
    {
        AssertRefused(Role.Customer);
    }

    [TestMethod]
    public void LoadFilmFile_UnknownRole_IsTreatedLikeAGuest()
    {
        AssertRefused((Role)99);
    }

    [TestMethod]
    public void LoadFilmFile_GuestAndMissingFile_TheRoleIsCheckedFirst()
    {
        bool is_loaded = _store.LoadFilmFile(Path.Combine(Path.GetTempPath(), "no-such-film-file.csv"), Role.Guest, out string message);

        Assert.IsFalse(is_loaded);
        Assert.AreEqual(CatalogStore.LOAD_NOT_ALLOWED_TEXT, message);
    }

    [TestMethod]
    public void LoadFilmFile_Admin_LoadsTheFilm()
    {
        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Admin, out _);

        Assert.IsTrue(is_loaded);
        Assert.IsNotNull(_films.Find(1));
        Assert.IsTrue(IsInDatabase(1));
    }

    [TestMethod]
    public void LoadFilmFile_Staff_LoadsTheFilm()
    {
        bool is_loaded = _store.LoadFilmFile(_file_path, Role.Staff, out _);

        Assert.IsTrue(is_loaded);
        Assert.IsNotNull(_films.Find(1));
        Assert.IsTrue(IsInDatabase(1));
    }
}
