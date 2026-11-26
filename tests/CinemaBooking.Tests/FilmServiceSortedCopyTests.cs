// tests that FilmService builds the sorted copy of the films once and reuses it, so FindByExactTitle is a real
// binary search (O(log n)) and not a new sort (O(n log n)) on every call (card 443, owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmServiceSortedCopyTests
{
    private static Film MakeFilm(int id, string title)
    {
        return new Film(id, title, "Drama", 2020, 100, 12, "a test film", "");
    }

    // three films: Alpha, Beta, Gamma
    private static FilmService MakeService(out HashTable<int, Film> table)
    {
        table = new HashTable<int, Film>();
        table.Add(1, MakeFilm(1, "Alpha"));
        table.Add(2, MakeFilm(2, "Beta"));
        table.Add(3, MakeFilm(3, "Gamma"));
        return new FilmService(table);
    }

    [TestMethod]
    public void Nothing_IsBuiltBeforeTheFirstFind()
    {
        FilmService service = MakeService(out _);

        Assert.AreEqual(0, service.SortedCopyBuildCount);
    }

    [TestMethod]
    public void FindByExactTitle_ThreeTimes_BuildsTheSortedCopyOnce()
    {
        FilmService service = MakeService(out _);

        Assert.AreEqual("Alpha", service.FindByExactTitle("Alpha")?.Title);
        Assert.AreEqual("Gamma", service.FindByExactTitle("gamma")?.Title);
        Assert.AreEqual("Beta", service.FindByExactTitle("Beta")?.Title);

        Assert.AreEqual(1, service.SortedCopyBuildCount);
    }

    [TestMethod]
    public void FindByExactTitle_NotFoundTwice_StillBuildsOnce()
    {
        FilmService service = MakeService(out _);

        Assert.IsNull(service.FindByExactTitle("Nothing"));
        Assert.IsNull(service.FindByExactTitle("Nothing again"));

        Assert.AreEqual(1, service.SortedCopyBuildCount);
    }

    [TestMethod]
    public void FindByExactTitle_BlankTitle_DoesNotBuildTheCopy()
    {
        FilmService service = MakeService(out _);

        Assert.IsNull(service.FindByExactTitle(""));
        Assert.IsNull(service.FindByExactTitle("   "));
        Assert.IsNull(service.FindByExactTitle(null!));

        Assert.AreEqual(0, service.SortedCopyBuildCount);
    }

    [TestMethod]
    public void FindByExactTitle_AfterAddFilm_BuildsAgainAndFindsTheNewFilm()
    {
        FilmService service = MakeService(out _);
        Assert.IsNull(service.FindByExactTitle("Delta"));

        bool is_added = service.AddFilm(MakeFilm(4, "Delta"), Role.Admin, out _);

        Assert.IsTrue(is_added);
        Assert.AreEqual("Delta", service.FindByExactTitle("Delta")?.Title);
        Assert.AreEqual(2, service.SortedCopyBuildCount);
    }

    [TestMethod]
    public void FindByExactTitle_AfterRemoveFilm_BuildsAgainAndDoesNotFindIt()
    {
        FilmService service = MakeService(out _);
        Assert.AreEqual("Beta", service.FindByExactTitle("Beta")?.Title);

        bool is_removed = service.RemoveFilm(2, Role.Admin, out _);

        Assert.IsTrue(is_removed);
        Assert.IsNull(service.FindByExactTitle("Beta"));
        Assert.AreEqual(2, service.SortedCopyBuildCount);
    }

    [TestMethod]
    public void FindByExactTitle_AfterAnEdit_FindsTheNewTitleAndNotTheOldOne()
    {
        FilmService service = MakeService(out _);
        Assert.AreEqual("Beta", service.FindByExactTitle("Beta")?.Title);

        bool is_edited = FilmEditor.Update(service, MakeFilm(2, "Omega"), Role.Admin, out _);

        Assert.IsTrue(is_edited);
        Assert.AreEqual("Omega", service.FindByExactTitle("Omega")?.Title);
        Assert.IsNull(service.FindByExactTitle("Beta"));
        Assert.AreEqual(2, service.SortedCopyBuildCount);
    }

    [TestMethod]
    public void FindByExactTitle_FilmAddedStraightToTheSharedTable_IsStillFound()
    {
        // the database load at start-up puts films straight into the shared table, not through AddFilm
        FilmService service = MakeService(out HashTable<int, Film> table);
        Assert.IsNull(service.FindByExactTitle("Sigma"));

        table.Add(9, MakeFilm(9, "Sigma"));

        Assert.AreEqual("Sigma", service.FindByExactTitle("Sigma")?.Title);
        Assert.AreEqual(2, service.SortedCopyBuildCount);
    }
}
