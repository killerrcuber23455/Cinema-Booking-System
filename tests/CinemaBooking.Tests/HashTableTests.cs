// unit tests for kushal's hash table, used as HashTable<int, Film> (owner: irwin)
// checks add, find, remove, count, list all and two films in the same slot.
// test ids (HASH-01...) match docs/testing/test-plan-data-side.md.

using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Tests;

[TestClass]
public class HashTableTests
{
    // makes a simple test film, only the id changes
    private static Film MakeFilm(int film_id)
    {
        // id, title, genre, year, minutes, age rating, description, poster file
        return new Film(film_id, "Film " + film_id, "Drama", 2020, 120, 12, "test film", "");
    }

    // HASH-01: a new table should be empty
    [TestMethod]
    public void NewTable_IsEmpty_CountIsZero()
    {
        // arrange: make an empty table
        HashTable<int, Film> table = new HashTable<int, Film>();

        // assert: nothing inside it
        Assert.AreEqual(0, table.Count);
        Assert.AreEqual(0, table.ListAll().Length);
    }

    // HASH-02: searching an empty table should give nothing, not crash
    [TestMethod]
    public void Find_EmptyTable_ReturnsNull()
    {
        // arrange: empty table
        HashTable<int, Film> table = new HashTable<int, Film>();

        // act: look for film 1 (the ? means the answer can be null)
        Film? found = table.Find(1);

        // assert: null means "not found"
        Assert.IsNull(found);
    }

    // HASH-03: a film we add can be found again
    [TestMethod]
    public void Add_ThenFind_ReturnsSameFilm()
    {
        // arrange: a table and one film
        HashTable<int, Film> table = new HashTable<int, Film>();
        Film film = MakeFilm(1);

        // act: add it, then look for it
        bool is_added = table.Add(1, film);
        Film? found = table.Find(1);

        // assert: add worked and we got the same film back
        Assert.IsTrue(is_added);
        Assert.AreSame(film, found);
    }

    // HASH-04: count goes up by one for each film
    [TestMethod]
    public void Add_ThreeFilms_CountIsThree()
    {
        // arrange: empty table
        HashTable<int, Film> table = new HashTable<int, Film>();

        // act: add three films
        table.Add(1, MakeFilm(1));
        table.Add(2, MakeFilm(2));
        table.Add(3, MakeFilm(3));

        // assert: count is 3
        Assert.AreEqual(3, table.Count);
    }

    // HASH-05: an id that is not in the table gives nothing
    [TestMethod]
    public void Find_MissingId_ReturnsNull()
    {
        // arrange: table with films 1, 2 and 3
        HashTable<int, Film> table = new HashTable<int, Film>();
        table.Add(1, MakeFilm(1));
        table.Add(2, MakeFilm(2));
        table.Add(3, MakeFilm(3));

        // act: look for film 99, which was never added
        Film? found = table.Find(99);

        // assert: not found
        Assert.IsNull(found);
    }

    // HASH-06: two films in the same slot are both kept
    // with 16 slots, 1 % 16 = 1 and 17 % 16 = 1, so both go in slot 1
    [TestMethod]
    public void Add_TwoIdsInSameSlot_BothFound()
    {
        // arrange: 16 slots, so we know where each id goes
        HashTable<int, Film> table = new HashTable<int, Film>(16);

        // act: add two films that land in the same slot
        table.Add(1, MakeFilm(1));
        table.Add(17, MakeFilm(17));

        // assert: both are still there
        Assert.IsNotNull(table.Find(1));
        Assert.IsNotNull(table.Find(17));
        Assert.AreEqual(2, table.Count);
    }

    // HASH-07: remove the first film in a chain
    // ids 1, 17 and 33 share slot 1, new films go to the front: 33 -> 17 -> 1
    [TestMethod]
    public void Remove_FirstInChain_OthersStillFound()
    {
        // arrange: three films in one slot
        HashTable<int, Film> table = MakeChainOfThree();

        // act: remove the front one (33)
        bool is_removed = table.Remove(33);

        // assert: 33 is gone, the other two are still there
        Assert.IsTrue(is_removed);
        Assert.IsNull(table.Find(33));
        Assert.IsNotNull(table.Find(17));
        Assert.IsNotNull(table.Find(1));
        Assert.AreEqual(2, table.Count);
    }

    // HASH-08: remove the middle film in a chain
    [TestMethod]
    public void Remove_MiddleOfChain_OthersStillFound()
    {
        // arrange: chain 33 -> 17 -> 1
        HashTable<int, Film> table = MakeChainOfThree();

        // act: remove the middle one (17)
        bool is_removed = table.Remove(17);

        // assert: 17 is gone, the chain is still joined up
        Assert.IsTrue(is_removed);
        Assert.IsNull(table.Find(17));
        Assert.IsNotNull(table.Find(33));
        Assert.IsNotNull(table.Find(1));
    }

    // HASH-09: remove the last film in a chain
    [TestMethod]
    public void Remove_LastInChain_OthersStillFound()
    {
        // arrange: chain 33 -> 17 -> 1
        HashTable<int, Film> table = MakeChainOfThree();

        // act: remove the last one (1)
        bool is_removed = table.Remove(1);

        // assert: 1 is gone, the other two are still there
        Assert.IsTrue(is_removed);
        Assert.IsNull(table.Find(1));
        Assert.IsNotNull(table.Find(33));
        Assert.IsNotNull(table.Find(17));
    }

    // HASH-10: removing a film that is not there says false, not crash
    [TestMethod]
    public void Remove_MissingId_ReturnsFalse()
    {
        // arrange: table with film 1 only
        HashTable<int, Film> table = new HashTable<int, Film>();
        table.Add(1, MakeFilm(1));

        // act: try to remove film 99
        bool is_removed = table.Remove(99);

        // assert: nothing removed, film 1 still counted
        Assert.IsFalse(is_removed);
        Assert.AreEqual(1, table.Count);
    }

    // HASH-12: the same id cannot be added twice
    [TestMethod]
    public void Add_SameIdTwice_SecondIsRefused()
    {
        // arrange: two different films with the same id 5
        HashTable<int, Film> table = new HashTable<int, Film>();
        Film first = MakeFilm(5);
        Film second = MakeFilm(5);

        // act: add both
        table.Add(5, first);
        bool is_added = table.Add(5, second);

        // assert: second refused, first one kept, only 1 film
        Assert.IsFalse(is_added);
        Assert.AreSame(first, table.Find(5));
        Assert.AreEqual(1, table.Count);
    }

    // HASH-13: list all gives back every film exactly once
    [TestMethod]
    public void ListAll_FiveFilms_ReturnsAllFive()
    {
        // arrange: films 1 to 5
        HashTable<int, Film> table = MakeTableWithFive();

        // act: get them all back as an array
        Film[] films = table.ListAll();

        // assert: 5 films, and each id appears once
        Assert.AreEqual(5, films.Length);
        for (int film_id = 1; film_id <= 5; film_id++)
        {
            Assert.AreEqual(1, CountId(films, film_id));
        }
    }

    // HASH-14: removed films do not come back in list all
    [TestMethod]
    public void ListAll_AfterRemovingTwo_ReturnsOtherThree()
    {
        // arrange: films 1 to 5, then remove 2 and 4
        HashTable<int, Film> table = MakeTableWithFive();
        table.Remove(2);
        table.Remove(4);

        // act: get the films back
        Film[] films = table.ListAll();

        // assert: 3 left, and 2 and 4 are not in the list
        Assert.AreEqual(3, films.Length);
        Assert.AreEqual(0, CountId(films, 2));
        Assert.AreEqual(0, CountId(films, 4));
    }

    // HASH-15: a negative id must not crash the table
    // the old table crashed because -1 % 16 = -1 and there is no slot -1.
    // the new table adds 16 to a negative slot, so -1 goes to slot 15.
    // stopping negative film ids is the Film class's job (IsValidId), not the table's.
    [TestMethod]
    public void Add_NegativeId_DoesNotCrash()
    {
        // arrange: empty table
        HashTable<int, Film> table = new HashTable<int, Film>();

        // act: add a film with id -1
        bool is_added = table.Add(-1, MakeFilm(-1));

        // assert: no crash, and it can be found again
        Assert.IsTrue(is_added);
        Assert.IsNotNull(table.Find(-1));
    }

    // helper: three films that all land in slot 1 (1, 17 and 33)
    private static HashTable<int, Film> MakeChainOfThree()
    {
        HashTable<int, Film> table = new HashTable<int, Film>(16);
        table.Add(1, MakeFilm(1));
        table.Add(17, MakeFilm(17));
        table.Add(33, MakeFilm(33));
        return table;
    }

    // helper: a table with films 1 to 5
    private static HashTable<int, Film> MakeTableWithFive()
    {
        HashTable<int, Film> table = new HashTable<int, Film>();
        for (int film_id = 1; film_id <= 5; film_id++)
        {
            table.Add(film_id, MakeFilm(film_id));
        }
        return table;
    }

    // helper: counts how many times an id appears in the array
    private static int CountId(Film[] films, int film_id)
    {
        int times_found = 0;
        for (int index = 0; index < films.Length; index++)
        {
            // add one each time the id matches
            if (films[index].Id == film_id)
            {
                times_found++;
            }
        }
        return times_found;
    }
}
