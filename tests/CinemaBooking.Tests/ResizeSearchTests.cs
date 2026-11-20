// unit tests for the hash table resize and for film search by title and genre (owner: irwin)
// resize is kushal's HashTable, search is shankar's FilmService (card 305).
// test ids (RES-01..., SRCH-01...)

using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class ResizeSearchTests
{
    // makes a test film, only the id changes
    private static Film MakeFilm(int film_id)
    {
        // id, title, genre, year, minutes, age rating, description, poster file
        return new Film(film_id, "Film " + film_id, "Drama", 2020, 120, 12, "test film", "");
    }

    // helper: a table with films 1 up to how_many
    private static HashTable<int, Film> MakeTable(int how_many)
    {
        HashTable<int, Film> table = new HashTable<int, Film>(16);
        for (int film_id = 1; film_id <= how_many; film_id++)
        {
            table.Add(film_id, MakeFilm(film_id));
        }
        return table;
    }

    // ---------- resize ----------

    // RES-01 and RES-02: one resize
    // 16 slots, 75% of 16 is 12, so the 13th film makes the table grow to 32 slots.
    // the slot count is private, so we check the result instead: every film still found.
    [TestMethod]
    public void Add_ThirteenFilms_AllFoundAfterOneResize()
    {
        // arrange and act: 13 films, enough to force one resize
        HashTable<int, Film> table = MakeTable(13);

        // assert: count is right and every film can still be found
        Assert.AreEqual(13, table.Count);
        for (int film_id = 1; film_id <= 13; film_id++)
        {
            Assert.IsNotNull(table.Find(film_id));
        }
    }

    // RES-03: a big load, 1000 films, forces many resizes (16 to 32 to 64 ... to 2048)
    [TestMethod]
    public void Add_ThousandFilms_AllFound()
    {
        // arrange and act: add 1000 films
        HashTable<int, Film> table = MakeTable(1000);

        // assert: count is 1000 and none were lost
        Assert.AreEqual(1000, table.Count);
        for (int film_id = 1; film_id <= 1000; film_id++)
        {
            Assert.IsNotNull(table.Find(film_id));
        }
    }

    // RES-04: after resizing, list all has each film exactly once
    [TestMethod]
    public void ListAll_ThousandFilms_EachIdOnce()
    {
        // arrange: 1000 films
        HashTable<int, Film> table = MakeTable(1000);

        // act: get them all back
        Film[] films = table.ListAll();

        // assert: 1000 films
        Assert.AreEqual(1000, films.Length);

        // tick off each id in a plain bool array (seen[5] = true means film 5 was found)
        bool[] seen = new bool[1001];
        for (int index = 0; index < films.Length; index++)
        {
            int film_id = films[index].Id;

            // if we already ticked this id, it is a copy
            Assert.IsFalse(seen[film_id], "film " + film_id + " appears twice");
            seen[film_id] = true;
        }
    }

    // RES-05: remove still works after the table has grown
    [TestMethod]
    public void Remove_HalfOfThousand_OtherHalfStillFound()
    {
        // arrange: 1000 films
        HashTable<int, Film> table = MakeTable(1000);

        // act: remove every even id (2, 4, 6 ... 1000)
        for (int film_id = 2; film_id <= 1000; film_id = film_id + 2)
        {
            table.Remove(film_id);
        }

        // assert: 500 left, even ids gone, odd ids still there
        Assert.AreEqual(500, table.Count);
        for (int film_id = 1; film_id <= 1000; film_id++)
        {
            bool is_even = film_id % 2 == 0;
            if (is_even)
            {
                Assert.IsNull(table.Find(film_id));
            }
            else
            {
                Assert.IsNotNull(table.Find(film_id));
            }
        }
    }

    // RES-06: boundary, 12 films is exactly 75% so no resize yet, the 13th film triggers it
    [TestMethod]
    public void Add_TwelveThenThirteen_CountStaysRight()
    {
        // arrange: 12 films, the table is exactly 75% full
        HashTable<int, Film> table = MakeTable(12);
        Assert.AreEqual(12, table.Count);

        // act: the 13th film pushes it over 75% and the table grows
        table.Add(13, MakeFilm(13));

        // assert: count went up by one and the new film is there
        Assert.AreEqual(13, table.Count);
        Assert.IsNotNull(table.Find(13));
    }

    // RES-07: the table still takes new films after it has grown
    [TestMethod]
    public void Add_AfterResize_NewFilmFound()
    {
        // arrange: 100 films, the table has grown several times
        HashTable<int, Film> table = MakeTable(100);

        // act: add one more
        bool is_added = table.Add(101, MakeFilm(101));

        // assert: it was added and can be found
        Assert.IsTrue(is_added);
        Assert.IsNotNull(table.Find(101));
        Assert.AreEqual(101, table.Count);
    }

    // RES-08: the same id is still refused after the table has grown
    [TestMethod]
    public void Add_SameIdAfterResize_IsRefused()
    {
        // arrange: 100 films
        HashTable<int, Film> table = MakeTable(100);

        // act: try to add film 50 again
        bool is_added = table.Add(50, MakeFilm(50));

        // assert: refused, count unchanged
        Assert.IsFalse(is_added);
        Assert.AreEqual(100, table.Count);
    }

    // RES-09: remove every film after resizing, the table ends empty
    [TestMethod]
    public void Remove_AllThousand_TableIsEmpty()
    {
        // arrange: 1000 films
        HashTable<int, Film> table = MakeTable(1000);

        // act: remove all of them
        for (int film_id = 1; film_id <= 1000; film_id++)
        {
            table.Remove(film_id);
        }

        // assert: nothing left
        Assert.AreEqual(0, table.Count);
        Assert.AreEqual(0, table.ListAll().Length);
    }

    // ---------- search ----------

    // helper: a film service holding six real films to search through
    private static FilmService MakeServiceWithFilms()
    {
        HashTable<int, Film> table = new HashTable<int, Film>();
        table.Add(1, new Film(1, "Inception", "Sci-Fi", 2010, 148, 12, "dreams", ""));
        table.Add(2, new Film(2, "Dune: Part Two", "Sci-Fi", 2024, 166, 12, "desert", ""));
        table.Add(3, new Film(3, "The Dark Knight", "Action", 2008, 152, 12, "batman", ""));
        table.Add(4, new Film(4, "The Lion King", "Animation", 1994, 88, 0, "lions", ""));
        table.Add(5, new Film(5, "Ocean's Eleven", "Crime", 2001, 116, 12, "heist", ""));
        table.Add(6, new Film(6, "Paddington", "Family", 2014, 95, 0, "bear", ""));
        return new FilmService(table);
    }

    // helper: true if a film with this title is in the results
    private static bool HasTitle(Film[] films, string title)
    {
        for (int index = 0; index < films.Length; index++)
        {
            if (films[index].Title == title)
            {
                return true;
            }
        }
        return false;
    }

    // SRCH-01: the full title finds that one film
    [TestMethod]
    public void SearchByTitle_FullTitle_FindsOneFilm()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("Inception");

        Assert.AreEqual(1, results.Length);
        Assert.IsTrue(HasTitle(results, "Inception"));
    }

    // SRCH-02: part of a title is enough
    [TestMethod]
    public void SearchByTitle_PartOfTitle_FindsFilm()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("incep");

        Assert.IsTrue(HasTitle(results, "Inception"));
    }

    // SRCH-03: capital letters do not matter
    [TestMethod]
    public void SearchByTitle_DifferentCase_FindsFilm()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("INCEPTION");

        Assert.IsTrue(HasTitle(results, "Inception"));
    }

    // SRCH-04: text from the middle of a title also matches
    [TestMethod]
    public void SearchByTitle_TextInsideTitle_FindsFilm()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("part");

        Assert.AreEqual(1, results.Length);
        Assert.IsTrue(HasTitle(results, "Dune: Part Two"));
    }

    // SRCH-05: a word in several titles finds all of them, once each
    [TestMethod]
    public void SearchByTitle_SeveralMatches_FindsAll()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("the");

        // "the" is only in The Dark Knight and The Lion King
        Assert.AreEqual(2, results.Length);
        Assert.IsTrue(HasTitle(results, "The Dark Knight"));
        Assert.IsTrue(HasTitle(results, "The Lion King"));
    }

    // SRCH-06: no match gives an empty list, not a crash
    [TestMethod]
    public void SearchByTitle_NoMatch_ReturnsEmpty()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("zzzz");

        Assert.AreEqual(0, results.Length);
    }

    // SRCH-07: empty search text gives every film (every title "contains" nothing)
    [TestMethod]
    public void SearchByTitle_EmptyText_ReturnsAllFilms()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("");

        Assert.AreEqual(6, results.Length);
    }

    // SRCH-08: search by genre finds only that genre
    [TestMethod]
    public void SearchByGenre_SciFi_FindsBothSciFiFilms()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByGenre("Sci-Fi");

        Assert.AreEqual(2, results.Length);
        Assert.IsTrue(HasTitle(results, "Inception"));
        Assert.IsTrue(HasTitle(results, "Dune: Part Two"));
    }

    // SRCH-09: a genre with no films gives an empty list
    [TestMethod]
    public void SearchByGenre_NoFilms_ReturnsEmpty()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByGenre("Western");

        Assert.AreEqual(0, results.Length);
    }

    // SRCH-10: an apostrophe in the search text works
    [TestMethod]
    public void SearchByTitle_Apostrophe_FindsFilm()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByTitle("Ocean's");

        Assert.AreEqual(1, results.Length);
        Assert.IsTrue(HasTitle(results, "Ocean's Eleven"));
    }

    // SRCH-11: genre search also ignores capital letters
    [TestMethod]
    public void SearchByGenre_LowerCase_FindsFilms()
    {
        FilmService service = MakeServiceWithFilms();

        Film[] results = service.SearchByGenre("sci-fi");

        Assert.AreEqual(2, results.Length);
    }

    // ---------- search after resize ----------

    // SRCH-12: search still finds the right film after the table has grown to 1000 films
    [TestMethod]
    public void SearchByTitle_AfterResize_FindsFilm()
    {
        // arrange: 1000 plain films plus one real title, so the table has resized many times
        HashTable<int, Film> table = MakeTable(1000);
        table.Add(1001, new Film(1001, "Inception", "Sci-Fi", 2010, 148, 12, "dreams", ""));
        FilmService service = new FilmService(table);

        // act: search for it
        Film[] results = service.SearchByTitle("incep");

        // assert: exactly that one film comes back
        Assert.AreEqual(1, results.Length);
        Assert.IsTrue(HasTitle(results, "Inception"));
    }

    // SRCH-13: genre search after resize finds every film of that genre
    [TestMethod]
    public void SearchByGenre_AfterResize_FindsAllThousand()
    {
        // arrange: 1000 films, all "Drama" (see MakeFilm)
        HashTable<int, Film> table = MakeTable(1000);
        FilmService service = new FilmService(table);

        // act: search by genre
        Film[] results = service.SearchByGenre("drama");

        // assert: all 1000 come back, none lost in the resize
        Assert.AreEqual(1000, results.Length);
    }

    // ---------- performance ----------

    // PERF-01: adding and finding 1000 films is quick (Stopwatch measures the time)
    // the 1 second limit is only a safety line, the real time goes in the test plan
    [TestMethod]
    public void AddAndFind_ThousandFilms_UnderOneSecond()
    {
        // arrange: start the clock
        System.Diagnostics.Stopwatch timer = System.Diagnostics.Stopwatch.StartNew();

        // act: add 1000 films, then find each one
        HashTable<int, Film> table = MakeTable(1000);
        for (int film_id = 1; film_id <= 1000; film_id++)
        {
            table.Find(film_id);
        }
        timer.Stop();

        // assert: well under one second
        Assert.IsTrue(timer.ElapsedMilliseconds < 1000, "took " + timer.ElapsedMilliseconds + " ms");
    }
}
