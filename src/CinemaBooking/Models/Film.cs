using System;

//the movies and details for the cinema (owner: shankar)
namespace CinemaBooking.Models
{
    public class Film
    {
        //validation rules constant values
        private const int FIRST_FILM_YEAR = 1900;
        private const int MAX_DURATION_IN_MINS = 300; //5 hours
        private const int MAX_TITLE_LENGTH = 100;
        private const int MAX_GENRE_LENGTH = 50;
        private const int MAX_DESCRIPTION_LENGTH = 500;
        private const int MAX_POSTER_FILE_LENGTH = 100;


        //properties of film
        public int Id { get; set; }
        public string Title { get; set; }
        public string Genre { get; set; }
        public int Year { get; set; }
        public int DurationMinutes { get; set; }
        public int AgeRating { get; set; }
        public string Description { get; set; }
        public string PosterFile { get; set; }

        //constructor
        public Film(int id, string title, string genre, int year, int duration_minutes, int age_rating,
            string description, string poster_file)
        {
            Id = id;
            Title = title;
            Genre = genre;
            Year = year;
            DurationMinutes = duration_minutes;
            AgeRating = age_rating;
            Description = description;
            PosterFile = poster_file;

        }

        //validation method it returns false with a reason
        public static bool IsValidId(int id, out string error_message)
        {
            if (id <= 0)
            {
                error_message = "film id must be a positive number";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidTitle(string title, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(title) || title.Length > MAX_TITLE_LENGTH)
            {
                error_message = "title cannot be empty and must be " + MAX_TITLE_LENGTH + " characters or fewer";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidGenre(string genre, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(genre) || genre.Length > MAX_GENRE_LENGTH)
            {
                error_message = "genre cannot be empty and must be maximum " + MAX_GENRE_LENGTH + " in length";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidYear(int year, out string error_message)
        {
            //we allow next year movies as upcoming movies
            int last_year_allowed = DateTime.Now.Year + 1;
            if (year < FIRST_FILM_YEAR || year > last_year_allowed)
            {
                error_message = "year must be between " + FIRST_FILM_YEAR + " and " + last_year_allowed;
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidDuration(int duration_minutes, out string error_message)
        {
            if (duration_minutes <= 0 || duration_minutes > MAX_DURATION_IN_MINS)
            {
                error_message = "duration must be between 1 and " + MAX_DURATION_IN_MINS + " minutes ";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidAgeRating(int age_rating, out string error_message)
        {
            if (age_rating != 0 && age_rating != 12 && age_rating != 15 && age_rating != 18)
            {
                error_message = "age rating must be 0, 12, 15 or 18";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidDescription(string description, out string error_message)
        {
            if (description != null && description.Length > MAX_DESCRIPTION_LENGTH)
            {
                error_message = "description must be " + MAX_DESCRIPTION_LENGTH + " characters or fewer";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidPosterFile(string poster_file, out string error_message)
        {
            //poster can be empty, it displays a card with the title only
            if (string.IsNullOrWhiteSpace(poster_file))
            {
                error_message = "";
                return true;
            }
            //file name is stored not the path
            if (poster_file.Length > MAX_POSTER_FILE_LENGTH || poster_file != System.IO.Path.GetFileName(poster_file))
            {
                error_message = "poster must be a file made only, up of " + MAX_POSTER_FILE_LENGTH + " characters";
                return false;
            }
            error_message = "";
            return true;
        }

        //checks the whole film properties
        public bool IsValid(out string errors)
        {
            string all_errors = "";
            string error_message;
            //all errors found are gathered in all_errors so user sees all errors at once
            if (!IsValidId(Id, out error_message)) all_errors += error_message + "\n";
            if (!IsValidTitle(Title, out error_message)) all_errors += error_message + "\n";
            if (!IsValidGenre(Genre, out error_message)) all_errors += error_message + "\n";
            if (!IsValidYear(Year, out error_message)) all_errors += error_message + "\n";
            if (!IsValidDuration(DurationMinutes, out error_message)) all_errors += error_message + "\n";
            if (!IsValidAgeRating(AgeRating, out error_message)) all_errors += error_message + "\n";
            if (!IsValidDescription(Description, out error_message)) all_errors += error_message + "\n";
            if (!IsValidPosterFile(PosterFile, out error_message)) all_errors += error_message + "\n";

            errors = all_errors;
            return all_errors == "";
        }

        // one readable line per film
        public override string ToString()
        {
            return $"{Id}: {Title} ({Year}) - {Genre}, {DurationMinutes} min, age {AgeRating}";
        }
    }
}


