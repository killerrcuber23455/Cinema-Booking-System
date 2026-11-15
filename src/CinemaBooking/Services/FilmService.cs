//rules to add and remove films with a role check first (owner: shankar)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class FilmService
    {
        //each film is stored by its id in the hashtable
        private HashTable<int, Film> _films;

        public FilmService()
        {
            _films = new HashTable<int, Film>();
        }

        //adding a film but only an admin can add
        //if refused code returns false with a reason
        public bool AddFilm(Film film, Role role, out string error_message)
        {
            //check if admin is the user
            if (role != Role.Admin)
            {
                error_message = "Only an admin can add a film";
                return false;
            }

            if (film == null)
            {
                error_message = "No film was given";
                return false;
            }

            //check all films and then give all errors
            string film_errors;
            if (!film.IsValid(out film_errors))
            {
                error_message = film_errors;
                return false;
            }

            //check if id is already in use and return false
            if (!_films.Add(film.Id, film))
            {
                error_message = "Film with id " + film.Id + " already exists";
                return false;
            }

            error_message = "";
            return true;
        }

        //remove a film can only be done by an admin
        //if refused false is returned alongside a reason
        public bool RemoveFilm(int film_id, Role role, out string error_message)
        {
            if (role != Role.Admin)
            {
                error_message = "Only an admin can remove a film";
                return false;
            }

            if (!_films.Remove(film_id))
            {
                error_message = "No film with id " + film_id + " was found";
                return false;
            }
            error_message = "";
            return true;
        }
    }
}
