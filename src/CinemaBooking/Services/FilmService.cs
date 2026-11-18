//rules to add remove and search films with a role check first (owner: shankar)
using System;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class FilmService
    {
        //each film is stored by its id in the hashtable
        private HashTable<int, Film> _films;

        //uses the table it is given so everyone shares the same films
        public FilmService(HashTable<int, Film> films)
        {
            _films = films;
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

        //search by title, partial and any case
        //hash only works with id so every film is checked
        public Film[] SearchByTitle(string search_text)
        {
            ChainList<Film> results = new ChainList<Film>();

            if (search_text == null)
            {
                search_text = "";
            }
            string text_lower = search_text.ToLower();
            Film[] all_films = _films.ListAll();

            for (int film_index = 0; film_index < all_films.Length; film_index++)
            {
                if (all_films[film_index].Title.ToLower().Contains(text_lower))
                {
                    results.Add(all_films[film_index]);
                }
            }
            return results.ListAll();
        }

        //searching by genre, partial and any case
        public Film[] SearchByGenre(string genre)
        {
            ChainList<Film> results = new ChainList<Film>();

            if (genre == null)
            {
                genre = "";
            }
            string genre_lower = genre.ToLower();
            Film[] all_films = _films.ListAll();

            for (int film_index = 0; film_index < all_films.Length; film_index++)
            {
                if (all_films[film_index].Genre.ToLower().Contains(genre_lower))
                {
                    results.Add(all_films[film_index]);
                }
            }
            return results.ListAll();
        }

        //stubs for card 347, the real code comes in the green step
        public Film[] Search(string title_text, string genre)
        {
            throw new NotImplementedException();
        }

        public string[] ListGenres()
        {
            throw new NotImplementedException();
        }

        public bool LoadFromFile(string file_path, out string message)
        {
            throw new NotImplementedException();
        }
    }
}
