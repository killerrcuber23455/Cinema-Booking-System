// film service and member service tests that need no database (owner: akilesh)
// cards 355 and 358
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class ServiceTests
{
    private static Film MakeFilm(int id = 1, string title = "Inception", string genre = "Sci-Fi")
    {
        return new Film(id, title, genre, 2010, 148, 12, "A film", "poster.jpg");
    }

    private static FilmService MakeFilmService()
    {
        return new FilmService(new HashTable<int, Film>());
    }

    // add film, admin only

    [TestMethod]
    public void AddFilm_AdminWithValidFilm_IsAccepted()
    {
        var service = MakeFilmService();

        bool result = service.AddFilm(MakeFilm(), Role.Admin, out string error_message);

        Assert.IsTrue(result);
        Assert.AreEqual("", error_message);
    }

    [TestMethod]
    [DataRow(Role.Guest)]
    [DataRow(Role.Customer)]
    [DataRow(Role.Staff)]
    public void AddFilm_NotAnAdmin_IsRefused(Role role)
    {
        var service = MakeFilmService();

        bool result = service.AddFilm(MakeFilm(), role, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("Only an admin can add a film", error_message);
    }

    [TestMethod]
    public void AddFilm_NullFilm_IsRefused()
    {
        var service = MakeFilmService();

        Assert.IsFalse(service.AddFilm(null!, Role.Admin, out _));
    }

    [TestMethod]
    public void AddFilm_InvalidFilm_IsRefusedWithTheFieldMessage()
    {
        var service = MakeFilmService();

        bool result = service.AddFilm(MakeFilm(title: ""), Role.Admin, out string error_message);

        Assert.IsFalse(result);
        Assert.Contains("title cannot be empty", error_message);
    }

    [TestMethod]
    public void AddFilm_SameIdTwice_SecondIsRefused()
    {
        var service = MakeFilmService();
        service.AddFilm(MakeFilm(id: 5), Role.Admin, out _);

        bool result = service.AddFilm(MakeFilm(id: 5, title: "Other"), Role.Admin, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("Film with id 5 already exists", error_message);
    }

    // remove film, admin only

    [TestMethod]
    public void RemoveFilm_AdminExistingFilm_IsAccepted()
    {
        var service = MakeFilmService();
        service.AddFilm(MakeFilm(id: 3), Role.Admin, out _);

        Assert.IsTrue(service.RemoveFilm(3, Role.Admin, out _));
        Assert.HasCount(0, service.SearchByTitle(""));
    }

    [TestMethod]
    public void RemoveFilm_FilmDoesNotExist_IsRefused()
    {
        var service = MakeFilmService();

        bool result = service.RemoveFilm(99, Role.Admin, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("No film with id 99 was found", error_message);
    }

    [TestMethod]
    public void RemoveFilm_Staff_IsRefused()
    {
        var service = MakeFilmService();
        service.AddFilm(MakeFilm(id: 3), Role.Admin, out _);

        bool result = service.RemoveFilm(3, Role.Staff, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("Only an admin can remove a film", error_message);
        Assert.HasCount(1, service.SearchByTitle(""));
    }

    // search

    private static FilmService MakeServiceWithThreeFilms()
    {
        var service = MakeFilmService();
        service.AddFilm(MakeFilm(1, "The Last Voyage", "Adventure"), Role.Admin, out _);
        service.AddFilm(MakeFilm(2, "Last Night", "Drama"), Role.Admin, out _);
        service.AddFilm(MakeFilm(3, "Space Run", "Sci-Fi"), Role.Admin, out _);
        return service;
    }

    [TestMethod]
    public void SearchByTitle_PartOfTitleAnyCase_FindsTheFilms()
    {
        var service = MakeServiceWithThreeFilms();

        Assert.HasCount(2, service.SearchByTitle("LAST"));
    }

    [TestMethod]
    public void SearchByTitle_NoMatch_GivesEmptyArray()
    {
        var service = MakeServiceWithThreeFilms();

        Assert.HasCount(0, service.SearchByTitle("zzz"));
    }

    [TestMethod]
    public void SearchByTitle_NullOrEmpty_GivesEveryFilm()
    {
        var service = MakeServiceWithThreeFilms();

        Assert.HasCount(3, service.SearchByTitle(null!));
        Assert.HasCount(3, service.SearchByTitle(""));
    }

    [TestMethod]
    public void SearchByGenre_PartOfGenreAnyCase_FindsTheFilm()
    {
        var service = MakeServiceWithThreeFilms();

        Assert.HasCount(1, service.SearchByGenre("sci"));
    }

    [TestMethod]
    public void SearchByGenre_NoMatch_GivesEmptyArray()
    {
        var service = MakeServiceWithThreeFilms();

        Assert.HasCount(0, service.SearchByGenre("horror"));
    }

    // member service, these methods never touch the database so none is given

    [TestMethod]
    public void TryBuildNewMember_ValidInput_BuildsAStandardMemberWithNoPoints()
    {
        var service = new MemberService(null!);

        bool result = service.TryBuildNewMember("Priya", "priya@mail.com", "5712 3456",
            out Member? member, out string errors);

        Assert.IsTrue(result);
        Assert.AreEqual("", errors);
        Assert.IsNotNull(member);
        Assert.AreEqual(Tier.Standard, member.Tier);
        Assert.AreEqual(0, member.Points);
        Assert.AreEqual(0, member.LifetimePoints);
    }

    [TestMethod]
    public void TryBuildNewMember_AllFieldsBad_GivesOneLineEachAndNoMember()
    {
        var service = new MemberService(null!);

        bool result = service.TryBuildNewMember("", "nope", "abc", out Member? member, out string errors);

        Assert.IsFalse(result);
        Assert.IsNull(member);
        Assert.HasCount(3, errors.Split('\n', StringSplitOptions.RemoveEmptyEntries));
    }

    [TestMethod]
    [DataRow(Role.Guest)]
    [DataRow(Role.Customer)]
    public void GetAllMembers_NotStaffOrAdmin_GivesEmptyArrayWithAMessage(Role role)
    {
        var service = new MemberService(null!);

        Member[] members = service.GetAllMembers(role, out string error_message);

        Assert.HasCount(0, members);
        Assert.AreEqual("Only staff or admin can view members", error_message);
    }
}
