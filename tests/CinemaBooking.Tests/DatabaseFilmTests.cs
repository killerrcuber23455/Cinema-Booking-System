// tests SaveFilm on the separate test database (owner: kushal)
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using Microsoft.Data.SqlClient;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CinemaBooking.Tests
{
    [DoNotParallelize]
    [TestClass]
    public class DatabaseFilmTests
    {
        private string _connection_string = "";
        private Database _database = null!;

        [TestInitialize]
        public void Setup()
        {
            string? value = Environment.GetEnvironmentVariable("CINEMA_TEST_DB");

            if (string.IsNullOrWhiteSpace(value) || !value.Contains("Test", StringComparison.OrdinalIgnoreCase))
            {
                Assert.Inconclusive("Set CINEMA_TEST_DB to a TEST database (name must contain Test).");
            }

            _connection_string = value;
            _database = new Database(_connection_string);

            using (SqlConnection connection = new SqlConnection(_connection_string))
            using (SqlCommand command = new SqlCommand(
                "DELETE FROM booked_seats; DELETE FROM bookings; DELETE FROM showings; DELETE FROM films;", connection))
            {
                connection.Open();
                command.ExecuteNonQuery();
            }
        }

        private static Film MakeFilm(int id)
        {
            return new Film(id, "Test Film " + id, "Drama", 2024, 100, 12, "a test film", "test.jpg");
        }

        [TestMethod]
        public void SaveFilm_New_ReturnsTrue()
        {
            Assert.IsTrue(_database.SaveFilm(MakeFilm(5)));
        }

        [TestMethod]
        public void SaveFilm_DuplicateId_ReturnsFalse()
        {
            _database.SaveFilm(MakeFilm(5));
            Assert.IsFalse(_database.SaveFilm(MakeFilm(5)));
        }

        [TestMethod]
        public void SaveFilm_ThenLoadFilms_ReturnsSavedValues()
        {
            _database.SaveFilm(MakeFilm(7));

            HashTable<int, Film> table = new HashTable<int, Film>();
            _database.LoadFilms(table);

            Film? film = table.Find(7);
            Assert.IsNotNull(film);
            Assert.AreEqual("Test Film 7", film.Title);
            Assert.AreEqual(2024, film.Year);
            Assert.AreEqual("test.jpg", film.PosterFile);
        }

        [TestMethod]
        public void SaveFilm_EmptyDescriptionAndPoster_AreSaved()
        {
            Film film = new Film(9, "No Extras", "Drama", 2024, 100, 12, "", "");

            Assert.IsTrue(_database.SaveFilm(film));
        }

        [TestMethod]
        public void IDatabase_Has_SaveFilm()
        {
            Assert.IsNotNull(typeof(IDatabase).GetMethod("SaveFilm"));
        }
    }
}