// tests for FilmService.FindById, the one place that finds a film by its id (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmServiceFindByIdTests
{
    private static Film MakeFilm(int id, string title)
    {
        return new Film(id, title, "Action", 2024, 110, 12, "A short test film", "");
    }

    // two films in the table, 5 and 8
    private static FilmService MakeService(out Film first, out Film second)
    {
        first = MakeFilm(5, "Alpha");
        second = MakeFilm(8, "Beta");
        HashTable<int, Film> films = new HashTable<int, Film>();
        films.Add(first.Id, first);
        films.Add(second.Id, second);
        return new FilmService(films);
    }

    [TestMethod]
    public void FindById_KnownId_ReturnsThatFilm()
    {
        FilmService service = MakeService(out _, out _);

        Film? found = service.FindById(5);

        Assert.IsNotNull(found);
        Assert.AreEqual("Alpha", found.Title);
    }

    [TestMethod]
    public void FindById_AnotherKnownId_ReturnsThatFilm()
    {
        FilmService service = MakeService(out _, out _);

        Film? found = service.FindById(8);

        Assert.IsNotNull(found);
        Assert.AreEqual("Beta", found.Title);
    }

    [TestMethod]
    public void FindById_KnownId_ReturnsTheStoredFilmNotACopy()
    {
        FilmService service = MakeService(out Film first, out _);

        Assert.AreSame(first, service.FindById(5));
    }

    [TestMethod]
    public void FindById_FilmAddedLater_IsFound()
    {
        FilmService service = MakeService(out _, out _);

        bool is_added = service.AddFilm(MakeFilm(20, "Gamma"), Role.Admin, out _);

        Assert.IsTrue(is_added);
        Assert.AreEqual("Gamma", service.FindById(20)?.Title);
    }

    [TestMethod]
    public void FindById_UnknownId_ReturnsNull()
    {
        FilmService service = MakeService(out _, out _);

        Assert.IsNull(service.FindById(99));
    }

    [TestMethod]
    public void FindById_ZeroId_ReturnsNull()
    {
        FilmService service = MakeService(out _, out _);

        Assert.IsNull(service.FindById(0));
    }

    [TestMethod]
    public void FindById_NegativeId_ReturnsNullAndDoesNotCrash()
    {
        FilmService service = MakeService(out _, out _);

        Assert.IsNull(service.FindById(-1));
    }

    [TestMethod]
    public void FindById_AfterTheFilmIsRemoved_ReturnsNull()
    {
        FilmService service = MakeService(out _, out _);

        bool is_removed = service.RemoveFilm(5, Role.Admin, out _);

        Assert.IsTrue(is_removed);
        Assert.IsNull(service.FindById(5));
    }
}
