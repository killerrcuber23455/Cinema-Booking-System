// a fake database that keeps everything in memory, so services can be tested without sql server (owner: irwin)
// it copies what the real Database class returns, but has no business rules (no discounts, points or roles).
// idea from week 7 (fakes) and week 10 (depend on IDatabase, not on sql server).

using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Tests.Fakes;

public class FakeDatabase : IDatabase
{
    // the screens are 8 rows by 12 seats (decision 8 nov), the real database reads this from the screens table
    private const int SCREEN_ROWS = 8;
    private const int SCREEN_SEATS_PER_ROW = 12;

    // films by id, members by email, bookings and showings by id, all in our own tables
    private readonly HashTable<int, Film> _films = new HashTable<int, Film>();
    private readonly MemberTable _members = new MemberTable();
    private readonly HashTable<int, Booking> _bookings = new HashTable<int, Booking>();
    private readonly HashTable<int, Showing> _showings = new HashTable<int, Showing>();

    // one entry per booked seat, key "showing id:seat", value the booking reference.
    // Add refuses a key twice, the same as the unique rule on booked_seats (showing_id, seat_code)
    private readonly HashTable<string, string> _booked_seats = new HashTable<string, string>();

    // one entry per showing time, key "screen id:start time", like the unique rule on showings
    private readonly HashTable<string, string> _screen_times = new HashTable<string, string>();

    // the next id to give out, like an identity column in sql server
    private int _next_member_id = 1;
    private int _next_booking_id = 1;

    // ---------- films ----------

    // switches for tests: make the fake fail the way sql server can
    public bool ThrowOnLoad { get; set; }
    public bool FilmSaveFails { get; set; }
    public bool ThrowOnSaveFilm { get; set; }
    public bool FilmUpdateFails { get; set; }
    public bool ThrowOnUpdateFilm { get; set; }
    public bool FilmDeleteFails { get; set; }
    public bool ThrowOnDeleteFilm { get; set; }

    // the real Database.SaveFilm: false if the id is already used
    public bool SaveFilm(Film film)
    {
        if (ThrowOnSaveFilm)
        {
            throw new InvalidOperationException("database is down");
        }

        if (FilmSaveFails)
        {
            return false;
        }

        return _films.Add(film.Id, film);
    }

    // the real Database.UpdateFilm: false if the id is not found
    public bool UpdateFilm(Film film)
    {
        if (ThrowOnUpdateFilm)
        {
            throw new InvalidOperationException("database is down");
        }

        if (FilmUpdateFails || _films.Find(film.Id) == null)
        {
            return false;
        }

        _films.Remove(film.Id);
        return _films.Add(film.Id, film);
    }

    // the real Database.DeleteFilm: false if the id is not found or a showing still uses the film
    public bool DeleteFilm(int film_id)
    {
        if (ThrowOnDeleteFilm)
        {
            throw new InvalidOperationException("database is down");
        }

        if (FilmDeleteFails || _films.Find(film_id) == null)
        {
            return false;
        }

        Showing[] all_showings = _showings.ListAll();
        for (int index = 0; index < all_showings.Length; index++)
        {
            if (all_showings[index].FilmId == film_id)
            {
                return false;
            }
        }

        return _films.Remove(film_id);
    }

    // only for tests: puts a film in the fake (the real database gets films from the sql script)
    public bool AddFilm(Film film)
    {
        return _films.Add(film.Id, film);
    }

    // copies every film into the given table and returns how many were added
    public int LoadFilms(HashTable<int, Film> film_table)
    {
        if (ThrowOnLoad)
        {
            throw new InvalidOperationException("database is down");
        }

        Film[] all_films = _films.ListAll();
        int loaded = 0;

        for (int index = 0; index < all_films.Length; index++)
        {
            if (film_table.Add(all_films[index].Id, all_films[index]))
            {
                loaded++;
            }
        }

        return loaded;
    }

    // ---------- members ----------

    // saves a member and returns the new id, or -1 if the email is already used
    public int SaveMember(Member member)
    {
        // MemberTable trims and lowercases the email, so "Ann@x.com " and "ann@x.com" are the same
        if (_members.FindByEmail(member.Email) != null)
        {
            return -1;
        }

        int new_id = _next_member_id;
        _next_member_id++;

        // we store a copy, so changing the caller's object later does not change our data
        _members.AddMember(CopyMember(member, new_id));
        return new_id;
    }

    // returns copies of every member, like rows read fresh from the database
    public Member[] GetAllMembers()
    {
        Member[] stored = _members.ListAll();
        Member[] copies = new Member[stored.Length];

        for (int index = 0; index < stored.Length; index++)
        {
            copies[index] = CopyMember(stored[index], stored[index].Id);
        }

        return copies;
    }

    // changes a member's points and tier, false if the member id is not found
    public bool UpdateMemberPoints(int member_id, int points, int lifetime_points, Tier tier)
    {
        Member? member = FindMemberById(member_id);
        if (member == null)
        {
            return false;
        }

        member.Points = points;
        member.LifetimePoints = lifetime_points;
        member.Tier = tier;
        return true;
    }

    // ---------- bookings ----------

    // saves a booking and its seats, returns the new id, or -1 if any seat is already taken
    public int SaveBooking(Booking booking)
    {
        // check every seat first, so a taken seat saves nothing (the real database uses a transaction)
        for (int index = 0; index < booking.Seats.Length; index++)
        {
            if (_booked_seats.Find(SeatKey(booking.ShowingId, booking.Seats[index])) != null)
            {
                return -1;
            }
        }

        int new_id = _next_booking_id;
        _next_booking_id++;

        _bookings.Add(new_id, CopyBooking(booking, new_id));

        for (int index = 0; index < booking.Seats.Length; index++)
        {
            _booked_seats.Add(SeatKey(booking.ShowingId, booking.Seats[index]), booking.Reference);
        }

        return new_id;
    }

    // true if a booking already uses this reference
    public bool ReferenceExists(string reference)
    {
        return FindStoredBooking(reference) != null;
    }

    // every booking of one member.
    // honest limit: the real database sorts newest first, the fake does not sort, so tests must not check the order
    public Booking[] GetBookingsForMember(int member_id)
    {
        Booking[] all_bookings = _bookings.ListAll();
        ChainList<Booking> results = new ChainList<Booking>();

        for (int index = 0; index < all_bookings.Length; index++)
        {
            if (all_bookings[index].MemberId == member_id)
            {
                results.Add(CopyBooking(all_bookings[index], all_bookings[index].Id));
            }
        }

        return results.ListAll();
    }

    // finds one booking by its reference, or null
    public Booking? FindBooking(string reference)
    {
        Booking? stored = FindStoredBooking(reference);
        if (stored == null)
        {
            return null;
        }

        return CopyBooking(stored, stored.Id);
    }

    // cancels a booking and frees its seats, false if it does not exist or is already cancelled
    public bool CancelBooking(int booking_id)
    {
        Booking? booking = _bookings.Find(booking_id);
        if (booking == null || booking.Status != BookingStatus.Confirmed)
        {
            return false;
        }

        booking.Status = BookingStatus.Cancelled;

        // free the seats, like deleting the rows from booked_seats
        for (int index = 0; index < booking.Seats.Length; index++)
        {
            _booked_seats.Remove(SeatKey(booking.ShowingId, booking.Seats[index]));
        }

        return true;
    }

    // the seats of the confirmed bookings of one showing.
    // honest limit: the real database sorts the seat codes, the fake does not, so tests must not check the order
    public string[] GetTakenSeats(int showing_id)
    {
        Booking[] all_bookings = _bookings.ListAll();
        ChainList<string> seats = new ChainList<string>();

        for (int index = 0; index < all_bookings.Length; index++)
        {
            Booking booking = all_bookings[index];

            // cancelled bookings no longer hold their seats
            if (booking.ShowingId == showing_id && booking.Status == BookingStatus.Confirmed)
            {
                for (int seat_index = 0; seat_index < booking.Seats.Length; seat_index++)
                {
                    seats.Add(booking.Seats[seat_index]);
                }
            }
        }

        return seats.ListAll();
    }

    // ---------- showings ----------

    // saves a showing, false if the id is used or the screen is already busy at that time
    public bool SaveShowing(Showing showing)
    {
        if (_showings.Find(showing.Id) != null)
        {
            return false;
        }

        if (!_screen_times.Add(ScreenTimeKey(showing.ScreenId, showing.StartsAt), "used"))
        {
            return false;
        }

        _showings.Add(showing.Id, showing);
        return true;
    }

    // puts every showing in the table with a new seat grid that has its taken seats marked
    public int LoadShowings(ShowingTable showing_table)
    {
        if (ThrowOnLoad)
        {
            throw new InvalidOperationException("database is down");
        }

        Showing[] all_showings = _showings.ListAll();
        int added = 0;

        for (int index = 0; index < all_showings.Length; index++)
        {
            Showing stored = all_showings[index];

            SeatGrid grid = new SeatGrid(SCREEN_ROWS, SCREEN_SEATS_PER_ROW);
            string[] taken = GetTakenSeats(stored.Id);
            if (taken.Length > 0)
            {
                grid.BookSeats(taken);
            }

            Showing loaded = new Showing(stored.Id, stored.FilmId, stored.ScreenId, stored.StartsAt, stored.Price, grid);
            if (showing_table.AddShowing(loaded))
            {
                added++;
            }
        }

        return added;
    }

    // ---------- small helpers ----------

    // the key for one seat in one showing, for example "1:C5"
    private static string SeatKey(int showing_id, string seat_code)
    {
        return showing_id + ":" + seat_code;
    }

    // the key for one screen at one time, for example "1:2026-12-01 20:30"
    private static string ScreenTimeKey(int screen_id, DateTime starts_at)
    {
        return screen_id + ":" + starts_at.ToString("yyyy-MM-dd HH:mm");
    }

    // looks through the members for one id (members are stored by email, not by id)
    private Member? FindMemberById(int member_id)
    {
        Member[] stored = _members.ListAll();

        for (int index = 0; index < stored.Length; index++)
        {
            if (stored[index].Id == member_id)
            {
                return stored[index];
            }
        }

        return null;
    }

    // looks through the bookings for one reference
    private Booking? FindStoredBooking(string reference)
    {
        Booking[] all_bookings = _bookings.ListAll();

        for (int index = 0; index < all_bookings.Length; index++)
        {
            if (all_bookings[index].Reference == reference)
            {
                return all_bookings[index];
            }
        }

        return null;
    }

    // a new member with the same details and the given id
    private static Member CopyMember(Member member, int id)
    {
        return new Member(id, member.Name, member.Email, member.Phone, member.Tier,
            member.Points, member.LifetimePoints, member.JoinedAt);
    }

    // a new booking with the same details and the given id
    private static Booking CopyBooking(Booking booking, int id)
    {
        return new Booking(id, booking.Reference, booking.ShowingId, booking.MemberId,
            booking.GuestName, booking.GuestEmail, booking.GuestPhone, booking.Seats,
            booking.Total, booking.PointsEarned, booking.PointsUsed, booking.Status, booking.CreatedAt);
    }
}