// tests the checks of CinemaScreen: name not empty (50 characters or fewer), rows 1 to 26, seats per row 1 to 30 (owner: kushal)
using CinemaBooking.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class CinemaScreenTests
    {
        // ---------- id ----------

        [TestMethod]
        public void IsValidId_Positive_ReturnsTrue()
        {
            Assert.IsTrue(CinemaScreen.IsValidId(1, out string error));
            Assert.AreEqual("", error);
        }

        [TestMethod]
        public void IsValidId_Zero_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidId(0, out string error));
            Assert.AreNotEqual("", error);
        }

        [TestMethod]
        public void IsValidId_Negative_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidId(-1, out string error));
            Assert.AreNotEqual("", error);
        }

        // ---------- name ----------

        [TestMethod]
        public void IsValidName_Normal_ReturnsTrue()
        {
            Assert.IsTrue(CinemaScreen.IsValidName("Screen 3", out string error));
            Assert.AreEqual("", error);
        }

        [TestMethod]
        public void IsValidName_Empty_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidName("", out string error));
            Assert.AreNotEqual("", error);
        }

        [TestMethod]
        public void IsValidName_Whitespace_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidName("   ", out string error));
            Assert.AreNotEqual("", error);
        }

        [TestMethod]
        public void IsValidName_FiftyCharacters_ReturnsTrue()
        {
            Assert.IsTrue(CinemaScreen.IsValidName(new string('x', 50), out string error));
        }

        [TestMethod]
        public void IsValidName_FiftyOneCharacters_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidName(new string('x', 51), out string error));
            Assert.AreNotEqual("", error);
        }

        // ---------- rows ----------

        [TestMethod]
        public void IsValidRowCount_Zero_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidRowCount(0, out string error));
            Assert.AreNotEqual("", error);
        }

        [TestMethod]
        public void IsValidRowCount_One_ReturnsTrue()
        {
            Assert.IsTrue(CinemaScreen.IsValidRowCount(1, out string error));
        }

        [TestMethod]
        public void IsValidRowCount_TwentySix_ReturnsTrue()
        {
            Assert.IsTrue(CinemaScreen.IsValidRowCount(26, out string error));
        }

        [TestMethod]
        public void IsValidRowCount_TwentySeven_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidRowCount(27, out string error));
            Assert.AreNotEqual("", error);
        }

        // ---------- seats per row ----------

        [TestMethod]
        public void IsValidSeatsPerRow_Zero_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidSeatsPerRow(0, out string error));
            Assert.AreNotEqual("", error);
        }

        [TestMethod]
        public void IsValidSeatsPerRow_One_ReturnsTrue()
        {
            Assert.IsTrue(CinemaScreen.IsValidSeatsPerRow(1, out string error));
        }

        [TestMethod]
        public void IsValidSeatsPerRow_Thirty_ReturnsTrue()
        {
            Assert.IsTrue(CinemaScreen.IsValidSeatsPerRow(30, out string error));
        }

        [TestMethod]
        public void IsValidSeatsPerRow_ThirtyOne_ReturnsFalse()
        {
            Assert.IsFalse(CinemaScreen.IsValidSeatsPerRow(31, out string error));
            Assert.AreNotEqual("", error);
        }

        // ---------- the whole screen ----------

        [TestMethod]
        public void IsValid_GoodScreen_ReturnsTrueWithNoErrors()
        {
            CinemaScreen screen = new CinemaScreen(3, "Screen 3", 8, 12);

            Assert.IsTrue(screen.IsValid(out string errors));
            Assert.AreEqual("", errors);
        }

        [TestMethod]
        public void IsValid_EmptyName_ReturnsFalseWithNameError()
        {
            CinemaScreen screen = new CinemaScreen(3, "", 8, 12);

            Assert.IsFalse(screen.IsValid(out string errors));
            Assert.Contains("name", errors);
        }

        [TestMethod]
        public void IsValid_TooManyRows_ReturnsFalseWithRowsError()
        {
            CinemaScreen screen = new CinemaScreen(3, "Screen 3", 27, 12);

            Assert.IsFalse(screen.IsValid(out string errors));
            Assert.Contains("rows", errors);
        }

        [TestMethod]
        public void IsValid_TooManySeats_ReturnsFalseWithSeatsError()
        {
            CinemaScreen screen = new CinemaScreen(3, "Screen 3", 8, 31);

            Assert.IsFalse(screen.IsValid(out string errors));
            Assert.Contains("seats per row", errors);
        }

        [TestMethod]
        public void IsValid_TwoBadFields_ReportsBoth()
        {
            CinemaScreen screen = new CinemaScreen(3, "Screen 3", 0, 0);

            Assert.IsFalse(screen.IsValid(out string errors));
            Assert.Contains("rows", errors);
            Assert.Contains("seats per row", errors);
        }

        [TestMethod]
        public void Constructor_BadValues_DoesNotThrow()
        {
            CinemaScreen screen = new CinemaScreen(0, "", 0, 0);

            Assert.AreEqual(0, screen.Id);
            Assert.AreEqual("", screen.Name);
            Assert.AreEqual(0, screen.RowCount);
            Assert.AreEqual(0, screen.SeatsPerRow);
        }
    }
}