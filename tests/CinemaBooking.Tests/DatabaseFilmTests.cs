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


        [TestMethod]
        public void UpdateFilm_Existing_ReturnsTrueAndSavesChanges()
        {
            _database.SaveFilm(MakeFilm(11));
            Film changed = new Film(11, "New Title", "Comedy", 2020, 90, 15, "new text", "new.jpg");

            Assert.IsTrue(_database.UpdateFilm(changed));

            HashTable<int, Film> table = new HashTable<int, Film>();
            _database.LoadFilms(table);
            Film? film = table.Find(11);
            Assert.IsNotNull(film);
            Assert.AreEqual("New Title", film.Title);
            Assert.AreEqual("Comedy", film.Genre);
            Assert.AreEqual(2020, film.Year);
            Assert.AreEqual("new.jpg", film.PosterFile);
        }

        [TestMethod]
        public void UpdateFilm_UnknownId_ReturnsFalse()
        {
            Assert.IsFalse(_database.UpdateFilm(MakeFilm(999)));
        }

        [TestMethod]
        public void DeleteFilm_Existing_ReturnsTrueAndRemovesIt()
        {
            _database.SaveFilm(MakeFilm(12));

            Assert.IsTrue(_database.DeleteFilm(12));

            HashTable<int, Film> table = new HashTable<int, Film>();
            _database.LoadFilms(table);
            Assert.IsNull(table.Find(12));
        }

        [TestMethod]
        public void DeleteFilm_UnknownId_ReturnsFalse()
        {
            Assert.IsFalse(_database.DeleteFilm(999));
        }

        [TestMethod]
        public void DeleteFilm_UsedByAShowing_ReturnsFalse()
        {
            _database.SaveFilm(MakeFilm(13));
            _database.SaveShowing(new Showing(1, 13, 1, new DateTime(2026, 12, 1, 18, 0, 0), 8.50m, new SeatGrid(8, 12)));

            Assert.IsFalse(_database.DeleteFilm(13));
        }
    }
}