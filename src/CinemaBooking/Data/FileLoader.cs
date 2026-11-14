using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using System;
using System.Collections.Generic;
using System.IO;

namespace CinemaBooking.Data
{
    // Loads film records from a CSV file and adds valid films to the hash table.
    public class FileLoader
    {
        // The path of the CSV file that contains the film records.
        private readonly string filePath;

        // Creates a new file loader using the given CSV file path.
        public FileLoader(string filePath)
        {
            this.filePath = filePath;
        }

        // Reads the CSV file, creates Film objects, validates them,
        // and adds valid films to the hash table.
        // Returns the number of films loaded and the number skipped.
        public (int loaded, int skipped) LoadFilms(HashTable<int, Film> filmTable)
        {
            // Counts how many valid films were added to the hash table.
            int loaded = 0;

            // Counts how many films were rejected or could not be read correctly.
            int skipped = 0;

            // If the CSV file does not exist, return zero loaded and zero skipped.
            if (!File.Exists(filePath))
            {
                return (loaded, skipped);
            }

            // Read every line from the CSV file.
            string[] lines = File.ReadAllLines(filePath);

            // Start at line 1 because line 0 is the CSV header.
            for (int i = 1; i < lines.Length; i++)
            {
                // Get the current film record.
                string line = lines[i];

                // Ignore empty lines.
                if (string.IsNullOrWhiteSpace(line))
                {
                    continue;
                }

                // Split the line into its eight CSV columns.
                string[] parts = line.Split(',');

                // A valid film record must have exactly eight columns.
                if (parts.Length != 8)
                {
                    skipped++;
                    continue;
                }

                // Variables used to hold the numeric CSV values.
                int id;
                int year;
                int durationMinutes;
                int ageRating;

                // Convert the numeric text values into integers.
                bool idParsed = int.TryParse(parts[0].Trim(), out id);
                bool yearParsed = int.TryParse(parts[3].Trim(), out year);
                bool durationParsed = int.TryParse(parts[4].Trim(), out durationMinutes);
                bool ageRatingParsed = int.TryParse(parts[5].Trim(), out ageRating);

                // Skip the record if any numeric value cannot be converted.
                if (!idParsed || !yearParsed || !durationParsed || !ageRatingParsed)
                {
                    skipped++;
                    continue;
                }

                // Read the text values from the CSV columns.
                string title = parts[1].Trim();
                string genre = parts[2].Trim();
                string description = parts[6].Trim();
                string posterFile = parts[7].Trim();

                // Create a new Film object using Shankar's Film class.
                Film film = new Film(id, title, genre, year, durationMinutes, ageRating, description, posterFile);

                // Validate the film using the IsValid method.
                string errors;
                if (!film.IsValid(out errors))
                {
                    skipped++;
                    continue;
                }

                // Add the film to the hash table.
                // Add returns false if the film ID already exists,
                // so the record is skipped as a duplicate.
                bool added = filmTable.Add(id, film);

                if (!added)
                {
                    skipped++;
                    continue;
                }

                // The film was valid and its ID was not already used.
                loaded++;
            }

            // Return how many films were loaded and how many were skipped.
            return (loaded, skipped);
        }
    }
}