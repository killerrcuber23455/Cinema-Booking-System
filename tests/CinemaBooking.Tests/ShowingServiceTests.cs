// tests for finding the showings of a film, card 402 (owner: shankar)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class ShowingServiceTests
{
    private static Showing MakeShowing(int showing_id, int film_id, DateTime starts_at)
    {
        return new Showing(showing_id, film_id, 1, starts_at, 250m, new SeatGrid(8, 12));
    }

    // saves the showings in the fake, loads them into a table, and gives the service that table
    private static ShowingService MakeService(params Showing[] showings)
    {
        FakeDatabase fake_database = new FakeDatabase();
        for (int index = 0; index < showings.Length; index++)
        {
            fake_database.SaveShowing(showings[index]);
        }

        ShowingTable showing_table = new ShowingTable();
        fake_database.LoadShowings(showing_table);
        return new ShowingService(showing_table);
    }

    [TestMethod]
    public void GetShowingsForFilm_FilmHasShowings_ReturnsSortedByStartTime()
    {
        // saved out of order on purpose: 20:00, 14:00, 17:30
        ShowingService service = MakeService(
            MakeShowing(1, 1, new DateTime(2026, 12, 1, 20, 0, 0)),
            MakeShowing(2, 1, new DateTime(2026, 12, 1, 14, 0, 0)),
            MakeShowing(3, 1, new DateTime(2026, 12, 1, 17, 30, 0)));

        Showing[] result = service.GetShowingsForFilm(1);

        Assert.AreEqual(3, result.Length);
        Assert.AreEqual(2, result[0].Id);
        Assert.AreEqual(3, result[1].Id);
        Assert.AreEqual(1, result[2].Id);
    }

    [TestMethod]
    public void GetShowingsForFilm_NoShowings_ReturnsEmptyArray()
    {
        ShowingService service = MakeService(
            MakeShowing(1, 1, new DateTime(2026, 12, 1, 20, 0, 0)));

        Showing[] result = service.GetShowingsForFilm(99);

        Assert.IsNotNull(result);
        Assert.AreEqual(0, result.Length);
    }

    [TestMethod]
    public void GetShowingsForFilm_OtherFilmsShowings_AreNotIncluded()
    {
        ShowingService service = MakeService(
            MakeShowing(1, 1, new DateTime(2026, 12, 1, 20, 0, 0)),
            MakeShowing(2, 2, new DateTime(2026, 12, 1, 18, 0, 0)));

        Showing[] result = service.GetShowingsForFilm(1);

        Assert.AreEqual(1, result.Length);
        Assert.AreEqual(1, result[0].FilmId);
    }
}
