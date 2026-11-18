// reads the connection string from appsettings.json (owner: kushal)
using System;
using System.IO;
using System.Text.Json;

namespace CinemaBooking.Data
{
    // reads settings from the appsettings.json file next to the program
    public class AppConfig
    {
        // the name of the config file
        private const string CONFIG_FILE_NAME = "appsettings.json";

        // the full path of the config file
        private readonly string _file_path;

        // uses appsettings.json in the folder where the program runs
        public AppConfig()
        {
            _file_path = Path.Combine(AppContext.BaseDirectory, CONFIG_FILE_NAME);
        }

        // uses a given path, so a test can point to its own file
        public AppConfig(string file_path)
        {
            _file_path = file_path;
        }

        // returns the sql server connection string from the config file.
        // throws FileNotFoundException if the file is missing,
        // and InvalidOperationException if the connection string is missing or empty.
        public string GetConnectionString()
        {
            if (!File.Exists(_file_path))
            {
                throw new FileNotFoundException(
                    "appsettings.json was not found. Copy appsettings.example.json to appsettings.json and fill in your values.",
                    _file_path);
            }

            string json_text = File.ReadAllText(_file_path);

            using (JsonDocument document = JsonDocument.Parse(json_text))
            {
                JsonElement connection_strings;
                JsonElement connection_string;

                if (!document.RootElement.TryGetProperty("ConnectionStrings", out connection_strings)
                    || !connection_strings.TryGetProperty("CinemaDb", out connection_string))
                {
                    throw new InvalidOperationException("ConnectionStrings:CinemaDb is missing in appsettings.json.");
                }

                string? result = connection_string.GetString();

                if (string.IsNullOrWhiteSpace(result))
                {
                    throw new InvalidOperationException("ConnectionStrings:CinemaDb is empty in appsettings.json.");
                }

                return result;
            }
        }
    }
}