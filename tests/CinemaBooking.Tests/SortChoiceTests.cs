// tests for the sort dropdown choices on the films page (owner: divyesh)
// written before the code for card 441, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SortChoiceTests
{
    [TestMethod]
    public void Labels_HasThreeChoicesWithTitleFirst()
    {
        Assert.AreEqual(3, SortChoice.LABELS.Length);
        Assert.AreEqual(SortChoice.TITLE_LABEL, SortChoice.LABELS[0]);
    }

    [TestMethod]
    public void KeyFor_TitleLabel_IsTitle()
    {
        Assert.AreEqual("title", SortChoice.KeyFor(SortChoice.TITLE_LABEL));
    }

    [TestMethod]
    public void KeyFor_YearLabel_IsYear()
    {
        Assert.AreEqual("year", SortChoice.KeyFor(SortChoice.YEAR_LABEL));
    }

    [TestMethod]
    public void KeyFor_RatingLabel_IsRating()
    {
        Assert.AreEqual("rating", SortChoice.KeyFor(SortChoice.RATING_LABEL));
    }

    [TestMethod]
    public void KeyFor_NullEmptyOrUnknown_IsTitle()
    {
        Assert.AreEqual("title", SortChoice.KeyFor(null));
        Assert.AreEqual("title", SortChoice.KeyFor(""));
        Assert.AreEqual("title", SortChoice.KeyFor("Newest first"));
    }
}
