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

        //loads films from the file the user picked, so the file name is never written in the code (added by divyesh for card 347)
        //returns false with a plain reason if the file cannot be read
        public bool LoadFromFile(string file_path, out string message)
        {
            if (string.IsNullOrWhiteSpace(file_path))
            {
                message = "no film file was chosen";
                return false;
            }

            try
            {
                FileLoader file_loader = new FileLoader(file_path);
                (int loaded, int skipped) = file_loader.LoadFilms(_films);
                message = "loaded " + loaded + " films, skipped " + skipped + " rows from " + Path.GetFileName(file_path);
                return true;
            }
            catch (FileNotFoundException)
            {
                message = "the film file was not found: " + Path.GetFileName(file_path);
                return false;
            }
            catch (IOException)
            {
                //the file is open in another program, for example excel
                message = "the film file could not be read, close it in other programs and try again";
                return false;
            }
            catch (UnauthorizedAccessException)
            {
                message = "you do not have permission to read this film file";
                return false;
            }
        }

        // returns all films sorted by "title", "year" or "rating" (merge sort, stable)
        public Film[] SortFilms(string sort_by)
        {
            Film[] all_films = _films.ListAll();

            return SortHelper.MergeSort(all_films, sort_by);
        }

        // finds one film by its exact title, any case, using binary search on a sorted copy. null if not found
        public Film? FindByExactTitle(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
            {
                return null;
            }

            Film[] sorted_films = SortHelper.MergeSort(_films.ListAll(), "title");
            int index = SortHelper.BinarySearch(sorted_films, title.Trim());

            return index == -1 ? null : sorted_films[index];
        }
    }
}
