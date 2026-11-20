// performance, security and boundary tests that run without sql server (owner: irwin)
// card 418. the times are printed, so they show in Test Explorer under "Standard Output".
// test ids (PERF-02, SEC-01...)

using System.Diagnostics;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class PerformanceSecurityTests
{
    // the text an attacker might type to try to delete the films table
    private const string SQL_INJECTION_TEXT = "x'); DROP TABLE films;--";

    // a film service holding films 1 to how_many, all "Drama"
    private static FilmService MakeServiceWithFilms(int how_many, out HashTable<int, Film> table)
    {
        table = new HashTable<int, Film>();
        for (int film_id = 1; film_id <= how_many; film_id++)
        {
            table.Add(film_id, new Film(film_id, "Film " + film_id, "Drama", 2020, 120, 12, "test film", ""));
        }
        return new FilmService(table);
    }

    // a booking service with one showing, using the fake database (no sql server)
    private static BookingService MakeBookingService(out FakeDatabase fake_database)
    {
        fake_database = new FakeDatabase();
        fake_database.SaveShowing(new Showing(1, 1, 1, new DateTime(2026, 12, 1, 20, 30, 0), 250m, new SeatGrid(8, 12)));
        ShowingTable showing_table = new ShowingTable();
        fake_database.LoadShowings(showing_table);
        return new BookingService(fake_database, showing_table);
    }

    // ---------- performance ----------

    // PERF-02: search by title through 1000 films
    [TestMethod]
    public void SearchByTitle_ThousandFilms_UnderOneSecond()
    {
        // arrange: 1000 films
        FilmService service = MakeServiceWithFilms(1000, out HashTable<int, Film> table);

        // act: time one search ("film 99" matches 99 and 990 to 999)
        Stopwatch timer = Stopwatch.StartNew();
        Film[] results = service.SearchByTitle("film 99");
        timer.Stop();

        // print the time so we can copy it into the test plan
        Console.WriteLine("search through 1000 films took " + timer.ElapsedMilliseconds + " ms");

        // assert: right answer, and well under the 1 second safety line
        Assert.AreEqual(11, results.Length);
        Assert.IsTrue(timer.ElapsedMilliseconds < 1000);
    }

    // PERF-03: one booking through BookingService, with the fake database
    [TestMethod]
    public void BookSeats_OneBooking_UnderOneSecond()
    {
        // arrange: a service with one showing
        BookingService service = MakeBookingService(out FakeDatabase fake_database);

        // act: time one guest booking of two seats
        Stopwatch timer = Stopwatch.StartNew();
        bool is_booked = service.BookSeats(Role.Guest, null, "Kevin", "kevin@mail.com", "5712 3456",
            1, new string[] { "C5", "C6" }, 0, out Booking? booking, out string error_message);
        timer.Stop();

        Console.WriteLine("one booking took " + timer.ElapsedMilliseconds + " ms");

        // assert: booked, and quick
        Assert.IsTrue(is_booked, error_message);
        Assert.IsTrue(timer.ElapsedMilliseconds < 1000);
    }

    // ---------- security ----------

    // SEC-01: sql injection text in the search box finds nothing and changes nothing
    [TestMethod]
    public void SearchByTitle_SqlInjectionText_NothingFoundNothingChanged()
    {
        // arrange: 1000 films
        FilmService service = MakeServiceWithFilms(1000, out HashTable<int, Film> table);

        // act: search with the attack text
        Film[] results = service.SearchByTitle(SQL_INJECTION_TEXT);

        // assert: treated as plain text, so no match, and all 1000 films are still there
        Assert.AreEqual(0, results.Length);
        Assert.AreEqual(1000, table.Count);
    }

    // SEC-02: sql injection text as a guest name does not crash the booking service
    [TestMethod]
    public void BookSeats_SqlInjectionGuestName_NoCrash()
    {
        // arrange: a service with one showing
        BookingService service = MakeBookingService(out FakeDatabase fake_database);

        // act: book with the attack text as the name
        bool is_booked = service.BookSeats(Role.Guest, null, SQL_INJECTION_TEXT, "kevin@mail.com", "5712 3456",
            1, new string[] { "C5" }, 0, out Booking? booking, out string error_message);

        // assert: either refused with a message, or saved as plain text. never a crash.
        if (is_booked)
        {
            Assert.AreEqual(SQL_INJECTION_TEXT, booking!.GuestName);
        }
        else
        {
            Assert.AreNotEqual("", error_message);
        }
    }

    // ---------- boundary and negative ----------

    // BND-01: a very long search text (10,000 letters) does not crash
    [TestMethod]
    public void SearchByTitle_VeryLongText_ReturnsEmpty()
    {
        FilmService service = MakeServiceWithFilms(10, out HashTable<int, Film> table);

        Film[] results = service.SearchByTitle(new string('a', 10000));

        Assert.AreEqual(0, results.Length);
    }

    // BND-02: a null search text is treated as empty, so every film comes back
    [TestMethod]
    public void SearchByTitle_NullText_ReturnsAllFilms()
    {
        FilmService service = MakeServiceWithFilms(10, out HashTable<int, Film> table);

        Film[] results = service.SearchByTitle(null!);

        Assert.AreEqual(10, results.Length);
    }

    // BND-03: booking a seat in a showing that does not exist is refused with a message
    [TestMethod]
    public void BookSeats_MissingShowing_IsRefused()
    {
        BookingService service = MakeBookingService(out FakeDatabase fake_database);

        bool is_booked = service.BookSeats(Role.Guest, null, "Kevin", "kevin@mail.com", "5712 3456",
            99, new string[] { "C5" }, 0, out Booking? booking, out string error_message);

        Assert.IsFalse(is_booked);
        Assert.AreEqual("this showing does not exist", error_message);
    }

    // BND-04: a seat that is already taken is refused by the service, the fake keeps one booking
    [TestMethod]
    public void BookSeats_TakenSeat_IsRefused()
    {
        // arrange: C5 is booked once
        BookingService service = MakeBookingService(out FakeDatabase fake_database);
        service.BookSeats(Role.Guest, null, "Kevin", "kevin@mail.com", "5712 3456",
            1, new string[] { "C5" }, 0, out Booking? first, out string first_message);

        // act: book C5 again
        bool is_booked = service.BookSeats(Role.Guest, null, "Ann", "ann@mail.com", "5712 3457",
            1, new string[] { "C5" }, 0, out Booking? second, out string error_message);

        // assert: refused with a clear message, still only one seat taken
        Assert.IsFalse(is_booked);
        Assert.AreEqual("seat C5 is already taken", error_message);
        Assert.AreEqual(1, fake_database.GetTakenSeats(1).Length);
    }
}
