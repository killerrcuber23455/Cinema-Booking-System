// tests SaveScreen and LoadScreens on the fake database, so the fake follows the same rules as the real one (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.Tests.Fakes;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class FakeScreenTests
    {
        private FakeDatabase _database = null!;

        [TestInitialize]
        public void Setup()
        {
            _database = new FakeDatabase();
        }

        private int CountScreens()
        {
            return _database.LoadScreens(new HashTable<int, CinemaScreen>());
        }

        [TestMethod]
        public void LoadScreens_NewFake_HasTheTwoSeedScreens()
        {
            Assert.AreEqual(2, CountScreens());
        }

        [TestMethod]
        public void LoadScreens_SeedScreensHaveTheirSize()
        {
            HashTable<int, CinemaScreen> table = new HashTable<int, CinemaScreen>();
            _database.LoadScreens(table);

            CinemaScreen? screen_two = table.Find(2);

            Assert.IsNotNull(screen_two);
            Assert.AreEqual("Screen 2", screen_two.Name);
            Assert.AreEqual(8, screen_two.RowCount);
            Assert.AreEqual(12, screen_two.SeatsPerRow);
        }

        [TestMethod]
        public void SaveScreen_NewScreen_ReturnsTrue()
        {
            Assert.IsTrue(_database.SaveScreen(new CinemaScreen(3, "Screen 3", 10, 14)));
        }

        [TestMethod]
        public void SaveScreen_NewScreen_AppearsInLoadScreens()
        {
            _database.SaveScreen(new CinemaScreen(3, "Screen 3", 10, 14));

            HashTable<int, CinemaScreen> table = new HashTable<int, CinemaScreen>();
            int loaded = _database.LoadScreens(table);

            Assert.AreEqual(3, loaded);
            CinemaScreen? screen_three = table.Find(3);
            Assert.IsNotNull(screen_three);
            Assert.AreEqual("Screen 3", screen_three.Name);
            Assert.AreEqual(10, screen_three.RowCount);
            Assert.AreEqual(14, screen_three.SeatsPerRow);
        }

        [TestMethod]
        public void SaveScreen_DuplicateId_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(1, "Another name", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_DuplicateId_KeepsTheOldScreen()
        {
            _database.SaveScreen(new CinemaScreen(1, "Another name", 5, 5));

            HashTable<int, CinemaScreen> table = new HashTable<int, CinemaScreen>();
            _database.LoadScreens(table);

            CinemaScreen? screen_one = table.Find(1);
            Assert.IsNotNull(screen_one);
            Assert.AreEqual("Screen 1", screen_one.Name);
            Assert.AreEqual(8, screen_one.RowCount);
        }

        [TestMethod]
        public void SaveScreen_DuplicateName_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "Screen 1", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_NameWithOtherCase_ReturnsFalse()
        {
            // sql server compares names without caring about upper or lower case
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "screen 1", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_ZeroRows_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "Screen 3", 0, 12)));
        }

        [TestMethod]
        public void SaveScreen_TooManyRows_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "Screen 3", 27, 12)));
        }

        [TestMethod]
        public void SaveScreen_TooManySeatsPerRow_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "Screen 3", 8, 31)));
        }

        [TestMethod]
        public void SaveScreen_EmptyName_ReturnsFalse()
        {
            Assert.IsFalse(_database.SaveScreen(new CinemaScreen(3, "", 8, 12)));
        }

        [TestMethod]
        public void SaveScreen_InvalidScreen_IsNotSaved()
        {
            _database.SaveScreen(new CinemaScreen(3, "Screen 3", 27, 12));

            Assert.AreEqual(2, CountScreens());
        }
    }
}