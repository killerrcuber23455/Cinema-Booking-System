
// tests for several threads booking the same seat at the same moment, card 412 (owner: shankar)
// a fake database is used on purpose: the real UNIQUE rule would refuse the second booking
// and hide a missing lock, so here only the lock in BookingService can stop a double booking
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using System.Threading;

namespace CinemaBooking.Tests;

[TestClass]
public class BookingConcurrencyTests
{
    private const int SHOWING_ID = 1;
    private const int ROWS = 8;
    private const int SEATS_PER_ROW = 12;

    // many rounds, because a race only shows up now and then
    private const int ROUNDS = 20;
    private const int MANY_THREADS = 8;

    // a thread that has not finished after this long is probably stuck (deadlock)
    private const int JOIN_TIMEOUT_MS = 30000;

    private const string GUEST_NAME = "Kevin";
    private const string GUEST_EMAIL = "kevin@mail.com";
    private const string GUEST_PHONE = "5712 3456";

    // a tiny database that accepts everything, only for this test file
    // SaveBooking is called from many threads, so the counter uses Interlocked
    private class FakeDatabase : IDatabase
    {
        private int _last_id = 0;

        // how many bookings were saved
        public int SaveCount
        {
            get { return Volatile.Read(ref _last_id); }
        }

        public int SaveBooking(Booking booking)
        {
            return Interlocked.Increment(ref _last_id);
        }

        public bool ReferenceExists(string reference)
        {
            return false;
        }

        public int LoadFilms(HashTable<int, Film> film_table)
        {
            return 0;
        }

        public int SaveMember(Member member)
        {
            return 1;
        }

        public Member[] GetAllMembers()
        {
            return new Member[0];
        }

        public bool UpdateMemberPoints(int member_id, int points, int lifetime_points, Tier tier)
        {
            return true;
        }

        public Booking[] GetBookingsForMember(int member_id)
        {
            return new Booking[0];
        }

        public Booking? FindBooking(string reference)
        {
            return null;
        }

        public bool CancelBooking(int booking_id)
        {
            return true;
        }

        public string[] GetTakenSeats(int showing_id)
        {
            return new string[0];
        }

        public bool SaveShowing(Showing showing)
        {
            return true;
        }

        public int LoadShowings(ShowingTable showing_table)
        {
            return 0;
        }

        // the concurrency tests do not use films, this only has to compile since IDatabase has SaveFilm
        public bool SaveFilm(Film film)
        {
            return true;
        }
    }

    // what one thread got back from BookSeats
    private class Attempt
    {
        public bool IsBooked;
        public string ErrorMessage = "";
        public Exception? Crash;
    }

    // starts one thread per seat list, lets them all go at the same moment, waits for all of them
    private static Attempt[] BookTogether(BookingService service, string[][] seats_per_thread)
    {
        int thread_count = seats_per_thread.Length;
        Attempt[] attempts = new Attempt[thread_count];
        Thread[] threads = new Thread[thread_count];

        // the barrier is the start line, nobody books until every thread is ready
        using (Barrier start_line = new Barrier(thread_count))
        {
            for (int index = 0; index < thread_count; index++)
            {
                // each thread gets its own copy of the number and its own result object
                int thread_number = index;
                Attempt attempt = new Attempt();
                attempts[thread_number] = attempt;

                threads[thread_number] = new Thread(() =>
                {
                    try
                    {
                        start_line.SignalAndWait();

                        attempt.IsBooked = service.BookSeats(Role.Guest, null, GUEST_NAME, GUEST_EMAIL,
                            GUEST_PHONE, SHOWING_ID, seats_per_thread[thread_number], 0,
                            out Booking? booking, out string error_message);

                        attempt.ErrorMessage = error_message;
                    }
                    catch (Exception error)
                    {
                        // an exception must not escape a thread, it would stop the whole test run
                        attempt.Crash = error;
                    }
                });
                threads[thread_number].Start();
            }

            for (int index = 0; index < thread_count; index++)
            {
                if (!threads[index].Join(JOIN_TIMEOUT_MS))
                {
                    Assert.Fail("thread " + index + " did not finish, is it stuck?");
                }
            }
        }

        return attempts;
    }

    // one round: fresh showing, grid and fake database, all threads book together, then check
    private static void RunRound(int round, string[][] seats_per_thread, int expected_winners)
    {
        FakeDatabase database = new FakeDatabase();
        ShowingTable showings = new ShowingTable();
        showings.CreateShowing(SHOWING_ID, 1, 1, new DateTime(2026, 12, 1, 18, 0, 0), 10m, ROWS, SEATS_PER_ROW);
        BookingService service = new BookingService(database, showings);

        Attempt[] attempts = BookTogether(service, seats_per_thread);

        int winners = 0;
        for (int index = 0; index < attempts.Length; index++)
        {
            if (attempts[index].Crash != null)
            {
                Assert.Fail("round " + round + ": a thread crashed: " + attempts[index].Crash);
            }
            if (attempts[index].IsBooked)
            {
                winners++;
            }
        }

        Assert.AreEqual(expected_winners, winners,
            "round " + round + ": wrong number of successful bookings");

        // a booking that was refused must never have reached the database
        Assert.AreEqual(expected_winners, database.SaveCount,
            "round " + round + ": wrong number of bookings saved");

        // every seat of a winner must be taken in the grid
        SeatGrid? grid = showings.GetGrid(SHOWING_ID);
        Assert.IsNotNull(grid);
        for (int index = 0; index < attempts.Length; index++)
        {
            if (!attempts[index].IsBooked)
            {
                continue;
            }
            for (int seat = 0; seat < seats_per_thread[index].Length; seat++)
            {
                Assert.IsFalse(grid.IsSeatFree(seats_per_thread[index][seat]),
                    "round " + round + ": seat " + seats_per_thread[index][seat] +
                    " was booked but the grid shows it free");
            }
        }
    }

    [TestMethod]
    public void BookSeats_TwoThreadsSameSeat_OnlyOneSucceeds()
    {
        for (int round = 0; round < ROUNDS; round++)
        {
            string[][] wanted = { new string[] { "C5" }, new string[] { "C5" } };

            RunRound(round, wanted, 1);
        }
    }

    [TestMethod]
    public void BookSeats_ManyThreadsSameSeat_OnlyOneSucceeds()
    {
        for (int round = 0; round < ROUNDS; round++)
        {
            string[][] wanted = new string[MANY_THREADS][];
            for (int index = 0; index < MANY_THREADS; index++)
            {
                wanted[index] = new string[] { "C5" };
            }

            RunRound(round, wanted, 1);
        }
    }

    [TestMethod]
    public void BookSeats_TwoThreadsDifferentSeats_BothSucceed()
    {
        // the lock must not refuse people who want different seats
        for (int round = 0; round < ROUNDS; round++)
        {
            string[][] wanted = { new string[] { "C5" }, new string[] { "C6" } };

            RunRound(round, wanted, 2);
        }
    }
}
