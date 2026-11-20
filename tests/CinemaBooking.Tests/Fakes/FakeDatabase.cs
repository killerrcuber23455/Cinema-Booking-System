// a fake database that keeps everything in memory, so services can be tested without sql server (owner: irwin)
// red step: every method is empty for now, so the tests compile and fail. the green commit fills them in.

using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Tests.Fakes;

public class FakeDatabase : IDatabase
{
    public bool AddFilm(Film film)
    {
        throw new NotImplementedException();
    }

    public int LoadFilms(HashTable<int, Film> film_table)
    {
        throw new NotImplementedException();
    }

    public int SaveMember(Member member)
    {
        throw new NotImplementedException();
    }

    public Member[] GetAllMembers()
    {
        throw new NotImplementedException();
    }

    public bool UpdateMemberPoints(int member_id, int points, int lifetime_points, Tier tier)
    {
        throw new NotImplementedException();
    }

    public int SaveBooking(Booking booking)
    {
        throw new NotImplementedException();
    }

    public bool ReferenceExists(string reference)
    {
        throw new NotImplementedException();
    }

    public Booking[] GetBookingsForMember(int member_id)
    {
        throw new NotImplementedException();
    }

    public Booking? FindBooking(string reference)
    {
        throw new NotImplementedException();
    }

    public bool CancelBooking(int booking_id)
    {
        throw new NotImplementedException();
    }

    public string[] GetTakenSeats(int showing_id)
    {
        throw new NotImplementedException();
    }

    public bool SaveShowing(Showing showing)
    {
        throw new NotImplementedException();
    }

    public int LoadShowings(ShowingTable showing_table)
    {
        throw new NotImplementedException();
    }
}
