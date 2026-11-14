using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

// Create the hash table that will store films by their ID.
HashTable<int, Film> filmTable = new HashTable<int, Film>();

Console.Write("Enter the film file name: ");
string? fileName = Console.ReadLine();

if (string.IsNullOrWhiteSpace(fileName))
{
    Console.WriteLine("No file name entered.");
    return;
}

// Load the films from the CSV file into the hash table.
FileLoader loader = new FileLoader(fileName);
(int loaded, int skipped) = loader.LoadFilms(filmTable);

Console.WriteLine("Films loaded: " + loaded);
Console.WriteLine("Films skipped: " + skipped);

// Print every film that was loaded.
Film[] films = filmTable.ListAll();
for (int i = 0; i < films.Length; i++)
{
    Console.WriteLine(films[i]);
}

Console.ReadKey();