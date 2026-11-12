using CinemaBooking.DataStructures;
using System;
using System.IO;

// This class loads film details from a text file chosen by the user.
public class FilmFileLoader
{
    // The custom hash table where loaded films will be stored.
    public FilmHashTable filmTable;

    // Saves the hash table so films can be added to it.
    public FilmFileLoader(FilmHashTable table)
    {
        filmTable = table;
    }

    // Loads films from the file name typed by the user.
    public void LoadFilms(string fileName)
    {
        // Checks that the file exists before trying to open it.
        if (File.Exists(fileName) == false)
        {
            Console.WriteLine("File not found. Please check the file name and try again.");
            return;
        }

        // Reads every line from the film file.
        string[] lines = File.ReadAllLines(fileName);

        // Counts how many valid films were loaded.
        int loadedCount = 0;

        // Reads the file one line at a time.
        for (int i = 0; i < lines.Length; i++)
        {
            string line = lines[i];

            // Skips empty lines.
            if (line == "")
            {
                continue;
            }

            // Splits the line into separate film details.
            string[] parts = line.Split(',');

            // Checks that the line has all six required fields.
            if (parts.Length != 6)
            {
                Console.WriteLine("Warning: line " + (i + 1) + " has the wrong number of fields.");
                continue;
            }

            int filmId;
            int duration;

            // Checks that the film Id and duration are valid numbers.
            bool idValid = int.TryParse(parts[0], out filmId);
            bool durationValid = int.TryParse(parts[3], out duration);

            if (idValid == false || durationValid == false)
            {
                Console.WriteLine("Warning: line " + (i + 1) + " has an invalid number.");
                continue;
            }

            // Creates a new Film object from the line.
            Film film = new Film();
            film.Id = filmId;
            film.Title = parts[1];
            film.Genre = parts[2];
            film.DurationMinutes = duration;
            film.AgeRating = parts[4];
            film.Description = parts[5];

            // Adds the film to the custom hash table.
            bool added = filmTable.AddFilm(film);

            // Counts the film only if it was added successfully.
            if (added)
            {
                loadedCount++;
            }
        }

        // Shows how many films were loaded successfully.
        Console.WriteLine(loadedCount + " film(s) loaded.");
    }
}