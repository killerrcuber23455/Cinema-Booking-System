// tests for choosing a poster picture: what is accepted and how the file is copied (owner: divyesh)
// written before the code for card 351, so they fail first (red) and pass after (green)
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class PosterImportTests
{
    private string _work = "";

    [TestInitialize]
    public void MakeFolder()
    {
        _work = Path.Combine(Path.GetTempPath(), "poster_import_" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(_work);
    }

    [TestCleanup]
    public void DeleteFolder()
    {
        if (Directory.Exists(_work))
        {
            Directory.Delete(_work, true);
        }
    }

    private string MakeFile(string folder, string name, int bytes = 10)
    {
        Directory.CreateDirectory(folder);
        string path = Path.Combine(folder, name);
        File.WriteAllBytes(path, new byte[bytes]);
        return path;
    }

    // Check

    [TestMethod]
    public void Check_JpgThatExists_IsAccepted()
    {
        string path = MakeFile(_work, "film.jpg");

        Assert.IsTrue(PosterImport.Check(path, out string error));
        Assert.AreEqual("", error);
    }

    [TestMethod]
    public void Check_UpperCasePng_IsAccepted()
    {
        string path = MakeFile(_work, "FILM.PNG");

        Assert.IsTrue(PosterImport.Check(path, out _));
    }

    [TestMethod]
    public void Check_BlankPath_IsRefused()
    {
        Assert.IsFalse(PosterImport.Check("  ", out string error));
        Assert.AreEqual(PosterImport.BLANK_TEXT, error);
    }

    [TestMethod]
    public void Check_WrongFileType_IsRefused()
    {
        string path = MakeFile(_work, "notes.txt");

        Assert.IsFalse(PosterImport.Check(path, out string error));
        Assert.AreEqual(PosterImport.TYPE_TEXT, error);
    }

    [TestMethod]
    public void Check_FileThatDoesNotExist_IsRefused()
    {
        Assert.IsFalse(PosterImport.Check(Path.Combine(_work, "nothing.jpg"), out string error));
        Assert.AreEqual(PosterImport.MISSING_TEXT, error);
    }

    [TestMethod]
    public void Check_FileOverFiveMegabytes_IsRefused()
    {
        string path = MakeFile(_work, "big.jpg", PosterImport.MAX_BYTES + 1);

        Assert.IsFalse(PosterImport.Check(path, out string error));
        Assert.AreEqual(PosterImport.BIG_TEXT, error);
    }

    [TestMethod]
    public void Check_FileOfExactlyFiveMegabytes_IsAccepted()
    {
        string path = MakeFile(_work, "limit.jpg", PosterImport.MAX_BYTES);

        Assert.IsTrue(PosterImport.Check(path, out _));
    }

    [TestMethod]
    public void Check_FileNameTooLong_IsRefused()
    {
        string path = MakeFile(_work, new string('a', PosterImport.MAX_NAME_LENGTH) + ".jpg");

        Assert.IsFalse(PosterImport.Check(path, out string error));
        Assert.AreEqual(PosterImport.NAME_TEXT, error);
    }

    // Import

    [TestMethod]
    public void Import_CopiesThePictureIntoTheFolder()
    {
        string path = MakeFile(Path.Combine(_work, "downloads"), "dune.jpg");
        string posters = Path.Combine(_work, "posters");

        bool ok = PosterImport.Import(path, posters, out string poster_file, out string error);

        Assert.IsTrue(ok, error);
        Assert.AreEqual("dune.jpg", poster_file);
        Assert.IsTrue(File.Exists(Path.Combine(posters, "dune.jpg")));
    }

    [TestMethod]
    public void Import_NameAlreadyTaken_GivesANewNameAndKeepsTheOldPicture()
    {
        string posters = Path.Combine(_work, "posters");
        string old_picture = MakeFile(posters, "dune.jpg", 5);
        string path = MakeFile(Path.Combine(_work, "downloads"), "dune.jpg", 20);

        bool ok = PosterImport.Import(path, posters, out string poster_file, out _);

        Assert.IsTrue(ok);
        Assert.AreEqual("dune_2.jpg", poster_file);
        Assert.AreEqual(5, new FileInfo(old_picture).Length);
        Assert.AreEqual(20, new FileInfo(Path.Combine(posters, "dune_2.jpg")).Length);
    }

    [TestMethod]
    public void Import_PictureAlreadyInTheFolder_IsNotCopiedAgain()
    {
        string posters = Path.Combine(_work, "posters");
        string path = MakeFile(posters, "dune.jpg");

        bool ok = PosterImport.Import(path, posters, out string poster_file, out _);

        Assert.IsTrue(ok);
        Assert.AreEqual("dune.jpg", poster_file);
        Assert.AreEqual(1, Directory.GetFiles(posters).Length);
    }

    [TestMethod]
    public void Import_WrongFileType_CopiesNothing()
    {
        string path = MakeFile(Path.Combine(_work, "downloads"), "notes.txt");
        string posters = Path.Combine(_work, "posters");

        bool ok = PosterImport.Import(path, posters, out string poster_file, out string error);

        Assert.IsFalse(ok);
        Assert.AreEqual("", poster_file);
        Assert.AreEqual(PosterImport.TYPE_TEXT, error);
        Assert.IsFalse(Directory.Exists(posters));
    }
}
