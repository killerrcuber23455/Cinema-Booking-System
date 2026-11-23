// the database methods the services use, so a fake database can replace sql server in tests (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Data
{
    public interface IDatabase
    {
        // reads every film into the hash table, returns how many were added
        int LoadFilms(HashTable<int, Film> film_table);

        // saves a film with the id it already has, false if the id exists
        bool SaveFilm(Film film);

        // returns the new id, or -1 if the email already exists
        int SaveMember(Member member);

        // returns every member with tier, points, lifetime points and join date
        Member[] GetAllMembers();

        // updates points and tier, false if the member is not found
        bool UpdateMemberPoints(int member_id, int points, int lifetime_points, Tier tier);

        // saves the booking and its seats in one transaction, -1 if a seat is taken
        int SaveBooking(Booking booking);

        // true if the booking reference is already used
        bool ReferenceExists(string reference);

        // returns the bookings of one member, newest first
        Booking[] GetBookingsForMember(int member_id);

        // finds a booking by its reference, null if there is none
        Booking? FindBooking(string reference);

        // sets the status to Cancelled and frees the seats in one transaction
        bool CancelBooking(int booking_id);

        // seat codes of the confirmed bookings of one showing
        string[] GetTakenSeats(int showing_id);

        // saves a showing, false if the id exists or the screen is busy at that time
        bool SaveShowing(Showing showing);

        // loads every showing into the table with its taken seats marked
        int LoadShowings(ShowingTable showing_table);
    }
}