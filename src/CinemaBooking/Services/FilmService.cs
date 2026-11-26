//rules to add remove and search films with a role check first (owner: shankar)
using System;
using System.IO;
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Algorithms;

namespace CinemaBooking.Services
{
    public class FilmService
    {
        //each film is stored by its id in the hashtable
        private HashTable<int, Film> _films;

        // the films sorted by title, built when the first exact find needs it and then reused (card 443)
        // it is thrown away when a film is added or removed, and it is also built again when the table has a different
        // number of films than the copy, for example after the start-up load that fills the table without AddFilm
        private Film[]? _sorted_by_title;
        private int _sorted_build_count;
        private readonly object _sorted_lock = new object();

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

            ForgetSortedCopy();

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

            ForgetSortedCopy();
            error_message = "";
            return true;
        }

        //finds one film by its id straight in the hash table, null if there is no such film
        //the hash table is keyed by id, so this is one lookup and not a walk through every film
        public Film? FindById(int film_id)
        {
            return _films.Find(film_id);
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

        //title search and genre filter together, for the films page (added by divyesh for card 347)
        //an empty genre means every genre. the genre must match fully, because it comes from the genre list
        public Film[] Search(string title_text, string genre)
        {
            Film[] title_matches = SearchByTitle(title_text);

            if (string.IsNullOrWhiteSpace(genre))
            {
                return title_matches;
            }

            ChainList<Film> results = new ChainList<Film>();
            string genre_lower = genre.Trim().ToLower();

            for (int film_index = 0; film_index < title_matches.Length; film_index++)
            {
                if (title_matches[film_index].Genre.Trim().ToLower() == genre_lower)
                {
                    results.Add(title_matches[film_index]);
                }
            }
            return results.ListAll();
        }

        //every genre once, for the genre filter on the films page (added by divyesh for card 347)
        //"action" and "Action" count as the same genre, the first spelling found is kept
        public string[] ListGenres()
        {
            ChainList<string> genres = new ChainList<string>();
            Film[] all_films = _films.ListAll();

            for (int film_index = 0; film_index < all_films.Length; film_index++)
            {
                string genre = all_films[film_index].Genre.Trim();
                string genre_lower = genre.ToLower();
                string? already_listed = genres.Find(listed => listed.ToLower() == genre_lower);

                if (already_listed == null)
                {
                    genres.Add(genre);
                }
            }
            return genres.ListAll();
        }

        // returns all films sorted by "title", "year" or "rating" (merge sort, stable)
        public Film[] SortFilms(string sort_by)
        {
            Film[] all_films = _films.ListAll();

            return SortHelper.MergeSort(all_films, sort_by);
        }

        // how many times the sorted copy of the films has been built, so the tests can see that it is built once and reused
        public int SortedCopyBuildCount
        {
            get
            {
                lock (_sorted_lock)
                {
                    return _sorted_build_count;
                }
            }
        }

        // the next exact find builds the sorted copy again
        private void ForgetSortedCopy()
        {
            lock (_sorted_lock)
            {
                _sorted_by_title = null;
            }
        }

        // the sorted copy: built once with the merge sort, then reused until the films change
        private Film[] GetSortedByTitle()
        {
            lock (_sorted_lock)
            {
                if (_sorted_by_title == null || _sorted_by_title.Length != _films.Count)
                {
                    _sorted_by_title = SortHelper.MergeSort(_films.ListAll(), "title");
                    _sorted_build_count++;
                }

                return _sorted_by_title;
            }
        }

        // finds one film by its exact title, any case, using binary search on the sorted copy. null if not found
        // the sort is done once and reused, so each find is O(log n) and only the first one pays the O(n log n) sort
        public Film? FindByExactTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            Film[] sorted_films = GetSortedByTitle();
            int index = SortHelper.BinarySearch(sorted_films, title.Trim());

            return index == -1 ? null : sorted_films[index];
        }
    }
}
