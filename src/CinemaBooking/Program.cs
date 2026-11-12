using CinemaBooking.DataStructures;

FilmHashTable table = new FilmHashTable();

FilmFileLoader loader = new FilmFileLoader(table);

Console.Write("Enter the film file name: ");
string fileName = Console.ReadLine();

loader.LoadFilms(fileName);

Console.ReadKey();