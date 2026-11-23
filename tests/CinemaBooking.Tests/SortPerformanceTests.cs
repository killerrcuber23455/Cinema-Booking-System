// week 8 timing tests: merge sort, bubble sort, linear search, binary search and hash lookup (owner: irwin)
// card 425. each test makes n random films with a fixed seed, runs the work 5 times and prints the average.
// the times show in Test Explorer under "Standard Output" and are copied into docs/testing/timing-results.md.

using System.Diagnostics;
using CinemaBooking.Algorithms;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

// timing tests must not share the processor with other tests, or the numbers would be wrong
[DoNotParallelize]
[TestClass]
public class SortPerformanceTests
{
    // the same seed gives the same random films every time, so runs can be compared
    private const int SEED = 42;

    // each piece of work is timed this many times and the average is used
    private const int RUNS = 5;

    // one search is too quick to measure, so each search test times this many searches together
    private const int SEARCHES = 100;

    // letters in each random title
    private const int TITLE_LENGTH = 8;

    // ---------- helpers ----------

    // makes how_many films with ids 1 to how_many and random 8-letter titles
    private static Film[] MakeRandomFilms(int how_many)
    {
        Random random = new Random(SEED);
        Film[] films = new Film[how_many];

        for (int index = 0; index < how_many; index++)
        {
            films[index] = new Film(index + 1, MakeRandomTitle(random), "Drama", 2020, 120, 12, "test film", "");
        }

        return films;
    }

    // a title of random lowercase letters, for example "qhxbtmza"
    private static string MakeRandomTitle(Random random)
    {
        char[] letters = new char[TITLE_LENGTH];

        for (int index = 0; index < TITLE_LENGTH; index++)
        {
            letters[index] = (char)('a' + random.Next(26));
        }

        return new string(letters);
    }

    // runs the work once to warm up, then RUNS more times, and gives back the average time in milliseconds
    // (the warm-up run is not counted, because the first run of any code is slower while .NET prepares it)
    private static double AverageMilliseconds(Action work)
    {
        work();

        double total_milliseconds = 0;
        for (int run = 1; run <= RUNS; run++)
        {
            Stopwatch timer = Stopwatch.StartNew();
            work();
            timer.Stop();
            total_milliseconds += timer.Elapsed.TotalMilliseconds;
        }

        return total_milliseconds / RUNS;
    }

    // prints one line of results in the same format for every test
    private static void PrintTime(string operation, int how_many, double milliseconds)
    {
        Console.WriteLine(operation + ", n = " + how_many + ": " + milliseconds.ToString("0.0000") + " ms (average of " + RUNS + ")");
    }

    // a new array with the same films, so a sort never changes the original
    private static Film[] CopyFilms(Film[] films)
    {
        Film[] copy = new Film[films.Length];

        for (int index = 0; index < films.Length; index++)
        {
            copy[index] = films[index];
        }

        return copy;
    }

    // true if every title comes before or equals the next one (any case)
    private static bool IsSortedByTitle(Film[] films)
    {
        for (int index = 0; index < films.Length - 1; index++)
        {
            if (string.Compare(films[index].Title, films[index + 1].Title, StringComparison.OrdinalIgnoreCase) > 0)
            {
                return false;
            }
        }

        return true;
    }

    // bubble sort by title, only used here as a slow baseline to compare with merge sort (week 8).
    // O(n squared): each pass compares neighbours and swaps them, and there are n passes
    private static Film[] BubbleSort(Film[] films)
    {
        Film[] sorted = CopyFilms(films);

        for (int pass = 0; pass < sorted.Length - 1; pass++)
        {
            for (int index = 0; index < sorted.Length - 1 - pass; index++)
            {
                if (string.Compare(sorted[index].Title, sorted[index + 1].Title, StringComparison.OrdinalIgnoreCase) > 0)
                {
                    Film swap = sorted[index];
                    sorted[index] = sorted[index + 1];
                    sorted[index + 1] = swap;
                }
            }
        }

        return sorted;
    }

    // ---------- sorting ----------

    // TIME-01 to 03: merge sort by title at 1,000, 10,000 and 100,000 films
    [TestMethod]
    [DataRow(1000)]
    [DataRow(10000)]
    [DataRow(100000)]
    public void MergeSort_RandomFilms_SortedAndTimed(int how_many)
    {
        // arrange
        Film[] films = MakeRandomFilms(how_many);
        Film[] sorted = new Film[0];

        // act: time the sort (MergeSort gives back a new array, so the original stays unsorted every run)
        double milliseconds = AverageMilliseconds(() => sorted = SortHelper.MergeSort(films, "title"));
        PrintTime("merge sort", how_many, milliseconds);

        // assert: every film is still there, in title order
        Assert.AreEqual(how_many, sorted.Length);
        Assert.IsTrue(IsSortedByTitle(sorted), "merge sort did not sort the titles");
    }

    // TIME-04 and 05: bubble sort by title at 1,000 and 10,000 films (100,000 would take too long)
    [TestMethod]
    [DataRow(1000)]
    [DataRow(10000)]
    public void BubbleSort_RandomFilms_SortedAndTimed(int how_many)
    {
        Film[] films = MakeRandomFilms(how_many);
        Film[] sorted = new Film[0];

        double milliseconds = AverageMilliseconds(() => sorted = BubbleSort(films));
        PrintTime("bubble sort", how_many, milliseconds);

        Assert.AreEqual(how_many, sorted.Length);
        Assert.IsTrue(IsSortedByTitle(sorted), "bubble sort did not sort the titles");
    }

    // ---------- searching ----------

    // TIME-06 to 08: linear search (FilmService.SearchByTitle checks every film), 100 searches
    [TestMethod]
    [DataRow(1000)]
    [DataRow(10000)]
    [DataRow(100000)]
    public void LinearSearch_RandomFilms_FoundAndTimed(int how_many)
    {
        // arrange: all films in the hash table that FilmService searches
        Film[] films = MakeRandomFilms(how_many);
        HashTable<int, Film> table = new HashTable<int, Film>();
        for (int index = 0; index < films.Length; index++)
        {
            table.Add(films[index].Id, films[index]);
        }
        FilmService service = new FilmService(table);
        int found_count = 0;

        // act: search for 100 titles spread across the films
        double milliseconds = AverageMilliseconds(() =>
        {
            found_count = 0;
            for (int search = 0; search < SEARCHES; search++)
            {
                string title = films[search * how_many / SEARCHES].Title;
                if (service.SearchByTitle(title).Length > 0)
                {
                    found_count++;
                }
            }
        });
        PrintTime("linear search x" + SEARCHES, how_many, milliseconds);

        // assert: every title was found
        Assert.AreEqual(SEARCHES, found_count);
    }

    // TIME-09 to 11: binary search for an exact title on the merge-sorted films, 100 searches
    [TestMethod]
    [DataRow(1000)]
    [DataRow(10000)]
    [DataRow(100000)]
    public void BinarySearch_SortedFilms_FoundAndTimed(int how_many)
    {
        // arrange: binary search only works on films already sorted by title (the sort is not timed here)
        Film[] films = MakeRandomFilms(how_many);
        Film[] sorted = SortHelper.MergeSort(films, "title");
        int found_count = 0;

        // act
        double milliseconds = AverageMilliseconds(() =>
        {
            found_count = 0;
            for (int search = 0; search < SEARCHES; search++)
            {
                string title = films[search * how_many / SEARCHES].Title;
                int position = SortHelper.BinarySearch(sorted, title);
                if (position >= 0 && sorted[position].Title == title)
                {
                    found_count++;
                }
            }
        });
        PrintTime("binary search x" + SEARCHES, how_many, milliseconds);

        // assert: every title was found at the right place
        Assert.AreEqual(SEARCHES, found_count);
    }

    // TIME-12 to 14: hash table lookup by id, 100 lookups
    [TestMethod]
    [DataRow(1000)]
    [DataRow(10000)]
    [DataRow(100000)]
    public void HashLookup_RandomFilms_FoundAndTimed(int how_many)
    {
        Film[] films = MakeRandomFilms(how_many);
        HashTable<int, Film> table = new HashTable<int, Film>();
        for (int index = 0; index < films.Length; index++)
        {
            table.Add(films[index].Id, films[index]);
        }
        int found_count = 0;

        double milliseconds = AverageMilliseconds(() =>
        {
            found_count = 0;
            for (int search = 0; search < SEARCHES; search++)
            {
                int film_id = films[search * how_many / SEARCHES].Id;
                if (table.Find(film_id) != null)
                {
                    found_count++;
                }
            }
        });
        PrintTime("hash lookup x" + SEARCHES, how_many, milliseconds);

        Assert.AreEqual(SEARCHES, found_count);
    }
}
