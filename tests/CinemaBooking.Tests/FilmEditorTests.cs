// tests for editing a film: who may edit, the film must exist, the new details must be valid (owner: divyesh)
// written before the code for card 351, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class FilmEditorTests
{
    // a service with films 1, 2 and 3
    private static FilmService MakeService()
    {
        HashTable<int, Film> table = new HashTable<int, Film>();
        table.Add(1, new Film(1, "Alpha", "Drama", 2020, 100, 12, "first", "a.jpg"));
        table.Add(2, new Film(2, "Beta", "Comedy", 2021, 110, 15, "second", "b.jpg"));
        table.Add(3, new Film(3, "Gamma", "Action", 2022, 120, 18, "third", "c.jpg"));
        return new FilmService(table);
    }

    private static Film? FindById(FilmService service, int id)
    {
        Film[] all = service.Search("", "");

        for (int index = 0; index < all.Length; index++)
        {
            if (all[index].Id == id)
            {
                return all[index];
            }
        }

        return null;
    }

    [TestMethod]
    public void Update_AdminChangesAnExistingFilm_TheNewDetailsAreSaved()
    {
        FilmService service = MakeService();
        Film edited = new Film(2, "Beta Reborn", "Thriller", 2023, 95, 12, "changed", "new.jpg");

        bool ok = FilmEditor.Update(service, edited, Role.Admin, out string error);

        Assert.IsTrue(ok, error);
        Film? found = FindById(service, 2);
        Assert.IsNotNull(found);
        Assert.AreEqual("Beta Reborn", found.Title);
        Assert.AreEqual("Thriller", found.Genre);
        Assert.AreEqual("new.jpg", found.PosterFile);
    }

    [TestMethod]
    public void Update_AdminChangesAFilm_OtherFilmsAndTheCountStayTheSame()
    {
        FilmService service = MakeService();

        FilmEditor.Update(service, new Film(2, "Beta Reborn", "Thriller", 2023, 95, 12, "changed", "new.jpg"), Role.Admin, out _);

        Assert.AreEqual(3, service.Search("", "").Length);
        Assert.AreEqual("Alpha", FindById(service, 1)!.Title);
        Assert.AreEqual("Gamma", FindById(service, 3)!.Title);
    }

    [TestMethod]
    public void Update_Staff_IsRefusedAndTheFilmIsNotChanged()
    {
        FilmService service = MakeService();

        bool ok = FilmEditor.Update(service, new Film(2, "Hacked", "Drama", 2023, 95, 12, "x", ""), Role.Staff, out string error);

        Assert.IsFalse(ok);
        Assert.AreEqual(FilmEditor.NOT_ADMIN_TEXT, error);
        Assert.AreEqual("Beta", FindById(service, 2)!.Title);
    }

    [TestMethod]
    public void Update_Customer_IsRefused()
    {
        FilmService service = MakeService();

        bool ok = FilmEditor.Update(service, new Film(2, "Hacked", "Drama", 2023, 95, 12, "x", ""), Role.Customer, out string error);

        Assert.IsFalse(ok);
        Assert.AreEqual(FilmEditor.NOT_ADMIN_TEXT, error);
    }

    [TestMethod]
    public void Update_FilmIdThatDoesNotExist_IsRefusedAndNothingIsAdded()
    {
        FilmService service = MakeService();

        bool ok = FilmEditor.Update(service, new Film(99, "Ghost", "Drama", 2023, 95, 12, "x", ""), Role.Admin, out string error);

        Assert.IsFalse(ok);
        Assert.AreEqual(FilmEditor.NotFoundText(99), error);
        Assert.AreEqual(3, service.Search("", "").Length);
        Assert.IsNull(FindById(service, 99));
    }

    [TestMethod]
    public void Update_InvalidNewDetails_IsRefusedAndTheOldFilmStays()
    {
        FilmService service = MakeService();
        Film bad = new Film(2, "", "Comedy", 2021, 110, 15, "second", "b.jpg");

        bool ok = FilmEditor.Update(service, bad, Role.Admin, out string error);

        Assert.IsFalse(ok);
        Assert.AreNotEqual("", error);
        Assert.AreEqual("Beta", FindById(service, 2)!.Title);
        Assert.AreEqual(3, service.Search("", "").Length);
    }

    [TestMethod]
    public void Update_NoFilmGiven_IsRefused()
    {
        FilmService service = MakeService();

        bool ok = FilmEditor.Update(service, null, Role.Admin, out string error);

        Assert.IsFalse(ok);
        Assert.AreEqual(FilmEditor.NO_FILM_GIVEN_TEXT, error);
    }
}
