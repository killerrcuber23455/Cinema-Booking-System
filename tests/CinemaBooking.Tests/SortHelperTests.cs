using CinemaBooking.Algorithms;
using CinemaBooking.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

//tests for merge sort and binary search on films (card 423, owner: akilesh)
//written before the code (red step), the code is card 424 by kushal
namespace CinemaBooking.Tests
{
    [TestClass]
    public class SortHelperTests
    {
        //makes a valid film, only the fields used for sorting change
        private static Film MakeFilm(int id, string title, int year, int age_rating)
        {
            return new Film(id, title, "Drama", year, 120, age_rating, "test film", "");
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

        private static int[] Ids(Film[] films)
        {
            int[] ids = new int[films.Length];
            for (int i = 0; i < films.Length; i++)
            {
                ids[i] = films[i].Id;
            }
            return ids;
        }

        //four films with different titles, years and ratings, not in order
        private static Film[] SampleFilms()
        {
            return new Film[]
            {
                MakeFilm(1, "Matrix", 1999, 15),
                MakeFilm(2, "Avatar", 2009, 12),
                MakeFilm(3, "Titanic", 1997, 12),
                MakeFilm(4, "Inception", 2010, 15)
            };
        }

        // ---------- merge sort ----------

        [TestMethod]
        public void MergeSort_ByTitle_SortsAToZ()
        {
            Film[] result = SortHelper.MergeSort(SampleFilms(), "title");

            CollectionAssert.AreEqual(new string[] { "Avatar", "Inception", "Matrix", "Titanic" }, Titles(result));
        }

        [TestMethod]
        public void MergeSort_ByYear_SortsOldestFirst()
        {
            Film[] result = SortHelper.MergeSort(SampleFilms(), "year");

            CollectionAssert.AreEqual(new int[] { 3, 1, 2, 4 }, Ids(result));
        }

        [TestMethod]
        public void MergeSort_ByRating_SortsLowestRatingFirst()
        {
            Film[] result = SortHelper.MergeSort(SampleFilms(), "rating");

            Assert.AreEqual(12, result[0].AgeRating);
            Assert.AreEqual(12, result[1].AgeRating);
            Assert.AreEqual(15, result[2].AgeRating);
            Assert.AreEqual(15, result[3].AgeRating);
        }

        [TestMethod]
        public void MergeSort_AlreadySorted_StaysTheSame()
        {
            Film[] films = new Film[]
            {
                MakeFilm(1, "Avatar", 1990, 0),
                MakeFilm(2, "Inception", 2000, 12),
                MakeFilm(3, "Matrix", 2010, 15)
            };

            Film[] result = SortHelper.MergeSort(films, "title");

            CollectionAssert.AreEqual(new int[] { 1, 2, 3 }, Ids(result));
        }

        [TestMethod]
        public void MergeSort_ReverseOrder_GetsSorted()
        {
            Film[] films = new Film[]
            {
                MakeFilm(1, "Titanic", 2010, 18),
                MakeFilm(2, "Matrix", 2000, 15),
                MakeFilm(3, "Inception", 1990, 12),
                MakeFilm(4, "Avatar", 1980, 0)
            };

            Film[] result = SortHelper.MergeSort(films, "year");

            CollectionAssert.AreEqual(new int[] { 4, 3, 2, 1 }, Ids(result));
        }

        [TestMethod]
        public void MergeSort_EmptyArray_ReturnsEmptyArray()
        {
            Film[] result = SortHelper.MergeSort(new Film[0], "title");

            Assert.AreEqual(0, result.Length);
        }

        [TestMethod]
        public void MergeSort_OneFilm_ReturnsThatFilm()
        {
            Film[] films = new Film[] { MakeFilm(7, "Avatar", 2009, 12) };

            Film[] result = SortHelper.MergeSort(films, "title");

            Assert.AreEqual(1, result.Length);
            Assert.AreEqual(7, result[0].Id);
        }

        [TestMethod]
        public void MergeSort_EqualYears_KeepsOriginalOrder()
        {
            //ids 1 and 3 have the same year, 2 and 4 have the same year
            Film[] films = new Film[]
            {
                MakeFilm(1, "Matrix", 2010, 15),
                MakeFilm(2, "Avatar", 2005, 12),
                MakeFilm(3, "Titanic", 2010, 12),
                MakeFilm(4, "Inception", 2005, 15)
            };

            Film[] result = SortHelper.MergeSort(films, "year");

            CollectionAssert.AreEqual(new int[] { 2, 4, 1, 3 }, Ids(result));
        }

        [TestMethod]
        public void MergeSort_EqualRatings_KeepsOriginalOrder()
        {
            Film[] films = new Film[]
            {
                MakeFilm(1, "Matrix", 1999, 15),
                MakeFilm(2, "Avatar", 2009, 12),
                MakeFilm(3, "Titanic", 1997, 15),
                MakeFilm(4, "Inception", 2010, 12)
            };

            Film[] result = SortHelper.MergeSort(films, "rating");

            CollectionAssert.AreEqual(new int[] { 2, 4, 1, 3 }, Ids(result));
        }

        [TestMethod]
        public void MergeSort_OriginalArray_IsNotChanged()
        {
            Film[] films = SampleFilms();

            SortHelper.MergeSort(films, "title");

            CollectionAssert.AreEqual(new int[] { 1, 2, 3, 4 }, Ids(films));
        }

        [TestMethod]
        public void MergeSort_Result_IsANewArray()
        {
            Film[] films = SampleFilms();

            Film[] result = SortHelper.MergeSort(films, "title");

            Assert.AreNotSame(films, result);
        }

        // ---------- binary search ----------

        //sorted by title, as binary search needs
        private static Film[] SortedFilms()
        {
            return new Film[]
            {
                MakeFilm(1, "Avatar", 2009, 12),
                MakeFilm(2, "Inception", 2010, 15),
                MakeFilm(3, "Matrix", 1999, 15),
                MakeFilm(4, "Titanic", 1997, 12),
                MakeFilm(5, "Up", 2009, 0)
            };
        }

        [TestMethod]
        public void BinarySearch_TitleInTheMiddle_ReturnsItsIndex()
        {
            Assert.AreEqual(2, SortHelper.BinarySearch(SortedFilms(), "Matrix"));
        }

        [TestMethod]
        public void BinarySearch_FirstTitle_ReturnsZero()
        {
            Assert.AreEqual(0, SortHelper.BinarySearch(SortedFilms(), "Avatar"));
        }

        [TestMethod]
        public void BinarySearch_LastTitle_ReturnsLastIndex()
        {
            Assert.AreEqual(4, SortHelper.BinarySearch(SortedFilms(), "Up"));
        }

        [TestMethod]
        [DataRow("matrix")]
        [DataRow("MATRIX")]
        [DataRow("mAtRiX")]
        public void BinarySearch_AnyCase_FindsTheTitle(string title)
        {
            Assert.AreEqual(2, SortHelper.BinarySearch(SortedFilms(), title));
        }

        [TestMethod]
        public void BinarySearch_TitleNotThere_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, SortHelper.BinarySearch(SortedFilms(), "Shrek"));
        }

        [TestMethod]
        public void BinarySearch_OnlyPartOfTitle_ReturnsMinusOne()
        {
            //it must be the exact title
            Assert.AreEqual(-1, SortHelper.BinarySearch(SortedFilms(), "Matri"));
        }

        [TestMethod]
        public void BinarySearch_EmptyArray_ReturnsMinusOne()
        {
            Assert.AreEqual(-1, SortHelper.BinarySearch(new Film[0], "Avatar"));
        }

        [TestMethod]
        public void BinarySearch_OneFilmFound_ReturnsZero()
        {
            Film[] films = new Film[] { MakeFilm(1, "Avatar", 2009, 12) };

            Assert.AreEqual(0, SortHelper.BinarySearch(films, "Avatar"));
        }

        [TestMethod]
        public void BinarySearch_OneFilmNotFound_ReturnsMinusOne()
        {
            Film[] films = new Film[] { MakeFilm(1, "Avatar", 2009, 12) };

            Assert.AreEqual(-1, SortHelper.BinarySearch(films, "Matrix"));
        }
    }
}
