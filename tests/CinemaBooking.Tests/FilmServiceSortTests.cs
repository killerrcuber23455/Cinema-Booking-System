// tests SortFilms and FindByExactTitle in FilmService, no database needed (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class FilmServiceSortTests
    {
        private static Film MakeFilm(int id, string title, int year, int age_rating)
        {
            return new Film(id, title, "Drama", year, 120, age_rating, "test film", "");
        }

        // four films added in a mixed order
        private static FilmService MakeService()
        {
            HashTable<int, Film> table = new HashTable<int, Film>();
            table.Add(1, MakeFilm(1, "Matrix", 1999, 15));
            table.Add(2, MakeFilm(2, "Avatar", 2009, 12));
            table.Add(3, MakeFilm(3, "Titanic", 1997, 12));
            table.Add(4, MakeFilm(4, "Inception", 2010, 15));
            return new FilmService(table);
        }

        private static string[] Titles(Film[] films)
        {
            string[] titles = new string[films.Length];
            for (int i = 0; i < films.Length; i++)
            {
                titles[i] = films[i].Title;
            }
            return titles;
        }

        [TestMethod]
        public void SortFilms_ByTitle_SortsAToZ()
        {
            Film[] result = MakeService().SortFilms("title");

            CollectionAssert.AreEqual(new string[] { "Avatar", "Inception", "Matrix", "Titanic" }, Titles(result));
        }

        [TestMethod]
        public void SortFilms_ByYear_SortsOldestFirst()
        {
            Film[] result = MakeService().SortFilms("year");

            CollectionAssert.AreEqual(new string[] { "Titanic", "Matrix", "Avatar", "Inception" }, Titles(result));
        }

        [TestMethod]
        public void SortFilms_ByRating_SortsLowestFirst()
        {
            Film[] result = MakeService().SortFilms("rating");

            Assert.AreEqual(4, result.Length);
            Assert.AreEqual(12, result[0].AgeRating);
            Assert.AreEqual(12, result[1].AgeRating);
            Assert.AreEqual(15, result[2].AgeRating);
            Assert.AreEqual(15, result[3].AgeRating);
        }

        [TestMethod]
        public void SortFilms_EmptyTable_ReturnsEmptyArray()
        {
            FilmService service = new FilmService(new HashTable<int, Film>());

            Assert.AreEqual(0, service.SortFilms("title").Length);
        }

        [TestMethod]
        public void SortFilms_DoesNotChangeTheTable()
        {
            HashTable<int, Film> table = new HashTable<int, Film>();
            table.Add(1, MakeFilm(1, "Matrix", 1999, 15));
            table.Add(2, MakeFilm(2, "Avatar", 2009, 12));
            FilmService service = new FilmService(table);

            service.SortFilms("title");

            Assert.AreEqual(2, table.Count);
            Assert.IsNotNull(table.Find(1));
            Assert.IsNotNull(table.Find(2));
        }

        [TestMethod]
        public void FindByExactTitle_Found_ReturnsTheFilm()
        {
            Film? film = MakeService().FindByExactTitle("Matrix");

            Assert.IsNotNull(film);
            Assert.AreEqual(1, film.Id);
        }

        [TestMethod]
        public void FindByExactTitle_AnyCase_ReturnsTheFilm()
        {
            Film? film = MakeService().FindByExactTitle("tItAnIc");

            Assert.IsNotNull(film);
            Assert.AreEqual(3, film.Id);
        }

        [TestMethod]
        public void FindByExactTitle_NotThere_ReturnsNull()
        {
            Assert.IsNull(MakeService().FindByExactTitle("Shrek"));
        }

        [TestMethod]
        public void FindByExactTitle_OnlyPartOfTitle_ReturnsNull()
        {
            Assert.IsNull(MakeService().FindByExactTitle("Matri"));
        }

        [TestMethod]
        public void FindByExactTitle_EmptyOrNull_ReturnsNull()
        {
            FilmService service = MakeService();

            Assert.IsNull(service.FindByExactTitle(""));
            Assert.IsNull(service.FindByExactTitle(null!));
        }

        [TestMethod]
        public void FindByExactTitle_EmptyTable_ReturnsNull()
        {
            FilmService service = new FilmService(new HashTable<int, Film>());

            Assert.IsNull(service.FindByExactTitle("Matrix"));
        }
    }
}