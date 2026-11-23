// loads films and showings from the database at startup and saves added, edited and removed films to it (owner: divyesh)
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class CatalogStore
    {
        public const string DATABASE_DOWN_TEXT = "Could not reach the database. Only the films already in the program are shown.";
        public const string SAVE_FAILED_TEXT = "The film could not be saved to the database, so it was not added.";
        public const string UPDATE_FAILED_TEXT = "The changes could not be saved to the database, so the film was not changed.";
        public const string DELETE_FAILED_TEXT = "The film could not be removed from the database (a showing may still use it), so it was not removed.";

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
            try
            {
                _database.LoadFilms(_films);
                _database.LoadShowings(_showings);
            }
            catch (Exception)
            {
                message = DATABASE_DOWN_TEXT;
                return false;
            }

            message = "";
            return true;
        }

        // adds a film (admin only) and saves it to the database. if the save fails the film is taken back out
        public bool AddFilm(Film film, Role role, out string error_message)
        {
            // FilmService checks the role and the film first, nothing is saved if it refuses
            if (!_film_service.AddFilm(film, role, out error_message))
            {
                return false;
            }

            bool is_saved;
            try
            {
                is_saved = _database.SaveFilm(film);
            }
            catch (Exception)
            {
                is_saved = false;
            }

            if (!is_saved)
            {
                // the database did not keep it, so the program must not show it either
                _film_service.RemoveFilm(film.Id, Role.Admin, out _);
                error_message = SAVE_FAILED_TEXT;
                return false;
            }

            error_message = "";
            return true;
        }

        // red stub: says yes and does nothing, on purpose. the real code comes in the green step
        public bool RemoveFilm(int film_id, Role role, out string error_message)
        {
            error_message = "";
            return true;
        }

        // red stub: says yes and does nothing, on purpose. the real code comes in the green step
        public bool EditFilm(Film edited_film, Role role, out string error_message)
        {
            error_message = "";
            return true;
        }
    }
}
