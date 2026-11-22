// checks that a film's poster file exists and gives a placeholder when it does not (owner: kushal)
using System;
using System.IO;

namespace CinemaBooking.Data
{
    public class PosterStore
    {
        // the file shown when a film has no poster or the file is missing
        public const string PLACEHOLDER_FILE = "placeholder.jpg";

        private readonly string _folder;

        public PosterStore(string folder)
        {
            _folder = folder;
        }

        // returns the poster file name if the file is in the posters folder, otherwise the placeholder name.
        // we only accept a plain file name, so a name like "..\x.jpg" cannot reach another folder
        public string GetPosterFile(string? poster_file)
        {
            if (string.IsNullOrWhiteSpace(poster_file))
            {
                return PLACEHOLDER_FILE;
            }

            string clean_name = poster_file.Trim();

            if (clean_name != Path.GetFileName(clean_name))
            {
                return PLACEHOLDER_FILE;
            }

            if (!Directory.Exists(_folder))
            {
                return PLACEHOLDER_FILE;
            }

            bool is_found = File.Exists(Path.Combine(_folder, clean_name));

            return is_found ? clean_name : PLACEHOLDER_FILE;
        }
    }
}