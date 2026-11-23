// loads films and showings from the database at startup and saves a new film to it (owner: divyesh)
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class CatalogStore
    {
        public const string DATABASE_DOWN_TEXT = "Could not reach the database. Only the films already in the program are shown.";
        public const string SAVE_FAILED_TEXT = "The film could not be saved to the database, so it was not added.";

        private readonly IDatabase _database;
        private readonly HashTable<int, Film> _films;
        private readonly ShowingTable _showings;
        private readonly FilmService _film_service;

        public CatalogStore(IDatabase database, HashTable<int, Film> films, ShowingTable showings, FilmService film_service)
        {
            _database = database;
            _films = films;
            _showings = showings;
            _film_service = film_service;
        }

        // fills the film and showing tables from the database. false with a message if the database fails
        public bool LoadAll(out string message)
        {
            // red stub: wrong on purpose
            message = "";
            return false;
        }

        // adds a film (admin only) and saves it to the database. if the save fails the film is taken back out
        public bool AddFilm(Film film, Role role, out string error_message)
        {
            // red stub: wrong on purpose
            error_message = "";
            return false;
        }
    }
}
