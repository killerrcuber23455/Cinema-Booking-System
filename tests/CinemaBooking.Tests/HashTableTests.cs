// tests for kushal's film hash table: add, find, remove and collisions (owner: irwin)

using CinemaBooking.DataStructures;

namespace CinemaBooking.Tests;

[TestClass]
public class HashTableTests
{
    // makes a test film with the given id
    private static Film MakeFilm(int id)
    {
        Film film = new Film();
        film.Id = id;
        film.Title = "Film " + id;
        film.Genre = "Drama";
        film.DurationMinutes = 120;
        film.AgeRating = "12";
        film.Description = "test film";
        return film;
    }

    // makes a table where films 1, 17 and 33 all share slot 1 (chain: 33 -> 17 -> 1)
    private static FilmHashTable MakeChainOfThree()
    {
        FilmHashTable table = new FilmHashTable(16);
        table.AddFilm(MakeFilm(1));
        table.AddFilm(MakeFilm(17));
        table.AddFilm(MakeFilm(33));
        return table;
    }

    // HASH-02: an empty table finds nothing
    [TestMethod]
    public void FindFilm_EmptyTable_ReturnsNull()
    {
        FilmHashTable table = new FilmHashTable();

        Film found = table.FindFilm(1);

        Assert.IsNull(found);
    }

    // HASH-03: a film we add can be found again
    [TestMethod]
    public void AddFilm_ThenFind_ReturnsSameFilm()
    {
        FilmHashTable table = new FilmHashTable();

        bool is_added = table.AddFilm(MakeFilm(1));
        Film found = table.FindFilm(1);

        Assert.IsTrue(is_added);
        Assert.IsNotNull(found);
        Assert.AreEqual(1, found.Id);
    }

    // HASH-05: a film that was never added is not found
    [TestMethod]
    public void FindFilm_MissingId_ReturnsNull()
    {
        FilmHashTable table = new FilmHashTable();
        table.AddFilm(MakeFilm(1));
        table.AddFilm(MakeFilm(2));
        table.AddFilm(MakeFilm(3));

        Film found = table.FindFilm(99);

        Assert.IsNull(found);
    }

    // HASH-06: two films in the same slot are both kept
    [TestMethod]
    public void AddFilm_TwoIdsInSameSlot_BothFound()
    {
        FilmHashTable table = new FilmHashTable(16);
        table.AddFilm(MakeFilm(1));
        table.AddFilm(MakeFilm(17));

        Assert.IsNotNull(table.FindFilm(1));
        Assert.IsNotNull(table.FindFilm(17));
    }

    // HASH-07: removing the first film in a chain keeps the others
    [TestMethod]
    public void RemoveFilm_FirstInChain_OthersStillFound()
    {
        FilmHashTable table = MakeChainOfThree();

        bool is_removed = table.RemoveFilm(33);

        Assert.IsTrue(is_removed);
        Assert.IsNull(table.FindFilm(33));
        Assert.IsNotNull(table.FindFilm(17));
        Assert.IsNotNull(table.FindFilm(1));
    }

    // HASH-08: removing the middle film in a chain keeps the others
    [TestMethod]
    public void RemoveFilm_MiddleOfChain_OthersStillFound()
    {
        FilmHashTable table = MakeChainOfThree();

        bool is_removed = table.RemoveFilm(17);

        Assert.IsTrue(is_removed);
        Assert.IsNull(table.FindFilm(17));
        Assert.IsNotNull(table.FindFilm(33));
        Assert.IsNotNull(table.FindFilm(1));
    }

    // HASH-09: removing the last film in a chain keeps the others
    [TestMethod]
    public void RemoveFilm_LastInChain_OthersStillFound()
    {
        FilmHashTable table = MakeChainOfThree();

        bool is_removed = table.RemoveFilm(1);

        Assert.IsTrue(is_removed);
        Assert.IsNull(table.FindFilm(1));
        Assert.IsNotNull(table.FindFilm(33));
        Assert.IsNotNull(table.FindFilm(17));
    }

    // HASH-10: removing a missing film says no and changes nothing
    [TestMethod]
    public void RemoveFilm_MissingId_ReturnsFalse()
    {
        FilmHashTable table = new FilmHashTable();
        table.AddFilm(MakeFilm(1));

        bool is_removed = table.RemoveFilm(99);

        Assert.IsFalse(is_removed);
        Assert.IsNotNull(table.FindFilm(1));
    }

    // HASH-12: a second film with the same id is refused
    [TestMethod]
    public void AddFilm_SameIdTwice_SecondIsRefused()
    {
        FilmHashTable table = new FilmHashTable();
        Film first = MakeFilm(5);
        Film second = MakeFilm(5);
        second.Title = "Copy";

        table.AddFilm(first);
        bool is_added = table.AddFilm(second);

        Assert.IsFalse(is_added);
        Assert.AreEqual("Film 5", table.FindFilm(5).Title);
    }

    // bug: a negative id crashes the table, this test fails until kushal fixes it
    [TestMethod]
    public void AddFilm_NegativeId_IsRefused()
    {
        FilmHashTable table = new FilmHashTable();

        bool is_added = table.AddFilm(MakeFilm(-1));

        Assert.IsFalse(is_added);
    }
}
