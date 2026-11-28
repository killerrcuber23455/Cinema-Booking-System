// the database methods the services use, so a fake database can replace sql server in tests (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Data
{
    public interface IDatabase
    {
        int LoadFilms(HashTable<int, Film> film_table);
        bool SaveFilm(Film film);

        int SaveMember(Member member);
        Member[] GetAllMembers();
        bool UpdateMemberPoints(int member_id, int points, int lifetime_points, Tier tier);

        int SaveBooking(Booking booking);
        bool ReferenceExists(string reference);
        Booking[] GetBookingsForMember(int member_id);
        Booking? FindBooking(string reference);
        bool CancelBooking(int booking_id);

        string[] GetTakenSeats(int showing_id);
        bool SaveShowing(Showing showing);
        int LoadShowings(ShowingTable showing_table);
    }
}