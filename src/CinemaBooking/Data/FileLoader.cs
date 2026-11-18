// loads film records from a csv file into the hash table (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using System;
using System.IO;

namespace CinemaBooking.Data
{
    // reads the csv file, validates each film and adds the valid ones to the hash table
    public class FileLoader
    {
        // the path of the csv file that contains the film records
        private readonly string _file_path;

        // creates a new file loader using the given csv file path
        public FileLoader(string file_path)
        {
            _file_path = file_path;
        }

        // reads the csv file, creates Film objects, validates them and adds valid films to the table.
        // returns how many films were loaded and how many were skipped.
        // throws FileNotFoundException if the csv file does not exist.
        public (int loaded, int skipped) LoadFilms(HashTable<int, Film> film_table)
        {
            int loaded = 0;
            int skipped = 0;

            // a missing file is an error the window can show, not a silent 0 and 0
            if (!File.Exists(_file_path))
            {
                throw new FileNotFoundException("The film file was not found: " + _file_path, _file_path);
            }

            string[] lines = File.ReadAllLines(_file_path);

            // start at line 1 because line 0 is the csv header
            for (int i = 1; i < lines.Length; i++)
            {
                string line = lines[i];

                // ignore empty lines
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // descriptions cannot contain commas, so a line must have exactly 8 columns
                string[] parts = line.Split(',');

                if (parts.Length != 8)
                {
                    skipped++;
                    continue;
                }

                int id;
                int year;
                int duration_minutes;
                int age_rating;

                bool is_id_parsed = int.TryParse(parts[0].Trim(), out id);
                bool is_year_parsed = int.TryParse(parts[3].Trim(), out year);
                bool is_duration_parsed = int.TryParse(parts[4].Trim(), out duration_minutes);
                bool is_age_rating_parsed = int.TryParse(parts[5].Trim(), out age_rating);

                if (!is_id_parsed || !is_year_parsed || !is_duration_parsed || !is_age_rating_parsed)
                {
                    skipped++;
                    continue;
                }

                string title = parts[1].Trim();
                string genre = parts[2].Trim();
                string description = parts[6].Trim();
                string poster_file = parts[7].Trim();

                // create the film using Shankar's Film class
                Film film = new Film(id, title, genre, year, duration_minutes, age_rating, description, poster_file);

                // validate the film
                string errors;
                bool is_valid = film.IsValid(out errors);

                if (!is_valid)
                {
                    skipped++;
                    continue;
                }

                // Add returns false if the film id already exists, so it is skipped as a duplicate
                bool is_added = film_table.Add(id, film);

                if (!is_added)
                {
                    skipped++;
                    continue;
                }

                loaded++;
            }

            return (loaded, skipped);
        }
    }
}