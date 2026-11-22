// checks a poster picture the admin chose and copies it into the posters folder (owner: divyesh)
// the film window only shows the file picker and the messages, all the rules are here so they can be tested
using System;
using System.IO;

namespace CinemaBooking.Services
{
    public static class PosterImport
    {
        public const int MAX_BYTES = 5 * 1024 * 1024;
        public const int MAX_NAME_LENGTH = 100;

        public const string BLANK_TEXT = "Choose a poster picture first";
        public const string MISSING_TEXT = "That picture file was not found";
        public const string TYPE_TEXT = "The poster must be a .jpg, .jpeg or .png picture";
        public const string BIG_TEXT = "The picture is too big, the limit is 5 MB";
        public const string NAME_TEXT = "The picture file name must be 100 characters or fewer";

        // true when the chosen file can be used as a poster, otherwise false with the reason
        public static bool Check(string? source_path, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(source_path))
            {
                error_message = BLANK_TEXT;
                return false;
            }

            string path = source_path.Trim();
            string extension = Path.GetExtension(path).ToLowerInvariant();

            if (extension != ".jpg" && extension != ".jpeg" && extension != ".png")
            {
                error_message = TYPE_TEXT;
                return false;
            }

            if (!File.Exists(path))
            {
                error_message = MISSING_TEXT;
                return false;
            }

            if (Path.GetFileName(path).Length > MAX_NAME_LENGTH)
            {
                error_message = NAME_TEXT;
                return false;
            }

            if (new FileInfo(path).Length > MAX_BYTES)
            {
                error_message = BIG_TEXT;
                return false;
            }

            error_message = "";
            return true;
        }

        // copies the picture into the posters folder and gives back the file name to store on the film
        // a picture that is already in the folder is not copied again, and a name that is taken gets a number
        public static bool Import(string? source_path, string posters_folder, out string poster_file, out string error_message)
        {
            poster_file = "";

            if (!Check(source_path, out error_message))
            {
                return false;
            }

            string path = Path.GetFullPath(source_path!.Trim());
            string folder = Path.GetFullPath(posters_folder);
            string name = Path.GetFileName(path);

            string? source_folder = Path.GetDirectoryName(path);

            if (string.Equals(source_folder?.TrimEnd(Path.DirectorySeparatorChar), folder.TrimEnd(Path.DirectorySeparatorChar),
                    StringComparison.OrdinalIgnoreCase))
            {
                poster_file = name;
                return true;
            }

            Directory.CreateDirectory(folder);

            string stem = Path.GetFileNameWithoutExtension(name);
            string extension = Path.GetExtension(name);
            string new_name = name;
            int number = 2;

            while (File.Exists(Path.Combine(folder, new_name)))
            {
                new_name = stem + "_" + number + extension;
                number++;
            }

            if (new_name.Length > MAX_NAME_LENGTH)
            {
                error_message = NAME_TEXT;
                return false;
            }

            File.Copy(path, Path.Combine(folder, new_name));
            poster_file = new_name;
            return true;
        }
    }
}
