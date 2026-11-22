// tests that a missing poster gives the placeholder name (owner: kushal)
using CinemaBooking.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class PosterStoreTests
    {
        private string _folder = "";

        [TestInitialize]
        public void Setup()
        {
            _folder = Path.Combine(Path.GetTempPath(), "posters_test_" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(_folder);
            File.WriteAllText(Path.Combine(_folder, "dune.jpg"), "x");
        }

        [TestCleanup]
        public void Cleanup()
        {
            if (Directory.Exists(_folder))
            {
                Directory.Delete(_folder, true);
            }
        }

        [TestMethod]
        public void GetPosterFile_FileExists_ReturnsSameName()
        {
            PosterStore store = new PosterStore(_folder);
            Assert.AreEqual("dune.jpg", store.GetPosterFile("dune.jpg"));
        }

        [TestMethod]
        public void GetPosterFile_FileMissing_ReturnsPlaceholder()
        {
            PosterStore store = new PosterStore(_folder);
            Assert.AreEqual(PosterStore.PLACEHOLDER_FILE, store.GetPosterFile("nothing.jpg"));
        }

        [TestMethod]
        public void GetPosterFile_EmptyName_ReturnsPlaceholder()
        {
            PosterStore store = new PosterStore(_folder);
            Assert.AreEqual(PosterStore.PLACEHOLDER_FILE, store.GetPosterFile(""));
        }

        [TestMethod]
        public void GetPosterFile_NullName_ReturnsPlaceholder()
        {
            PosterStore store = new PosterStore(_folder);
            Assert.AreEqual(PosterStore.PLACEHOLDER_FILE, store.GetPosterFile(null));
        }

        [TestMethod]
        public void GetPosterFile_NameWithPath_ReturnsPlaceholder()
        {
            // only a file name is allowed, so a path like ..\secret.jpg must not reach other folders
            PosterStore store = new PosterStore(_folder);
            Assert.AreEqual(PosterStore.PLACEHOLDER_FILE, store.GetPosterFile("..\\dune.jpg"));
        }

        [TestMethod]
        public void GetPosterFile_MissingFolder_ReturnsPlaceholder()
        {
            PosterStore store = new PosterStore(Path.Combine(_folder, "does_not_exist"));
            Assert.AreEqual(PosterStore.PLACEHOLDER_FILE, store.GetPosterFile("dune.jpg"));
        }
    }
}