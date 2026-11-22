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
        // this is only a stub for the red step, it accepts everything
        public static bool Check(string? source_path, out string error_message)
        {
            error_message = "";
            return true;
        }

        // copies the picture into the posters folder and gives back the file name to store on the film
        // this is only a stub for the red step, it copies nothing
        public static bool Import(string? source_path, string posters_folder, out string poster_file, out string error_message)
        {
            poster_file = "";
            error_message = "";
            return false;
        }
    }
}
