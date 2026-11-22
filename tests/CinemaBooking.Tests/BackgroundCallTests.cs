// tests for the background call helper: the value comes back, errors become one clear sentence (owner: divyesh)
// written before the code for card 415, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class BackgroundCallTests
{
    private const string DATABASE_DOWN_TEXT =
        "The database could not be reached. Check that SQL Server is running and appsettings.json is correct.";

    // RunAsync

    [TestMethod]
    public async Task RunAsync_WorkSucceeds_ReturnsTheValue()
    {
        CallResult<int> result = await BackgroundCall.RunAsync(() => 42);

        Assert.IsTrue(result.IsOk);
        Assert.AreEqual(42, result.Value);
    }

    [TestMethod]
    public async Task RunAsync_WorkSucceeds_HasNoErrorText()
    {
        CallResult<string> result = await BackgroundCall.RunAsync(() => "fine");

        Assert.AreEqual("", result.ErrorText);
    }

    [TestMethod]
    public async Task RunAsync_DatabaseDown_GivesItsClearSentence()
    {
        CallResult<int> result = await BackgroundCall.RunAsync<int>(
            () => throw new InvalidOperationException(DATABASE_DOWN_TEXT));

        Assert.IsFalse(result.IsOk);
        Assert.AreEqual(DATABASE_DOWN_TEXT, result.ErrorText);
    }

    [TestMethod]
    public async Task RunAsync_UnexpectedError_GivesTheGeneralTextAndHidesTheDetail()
    {
        CallResult<int> result = await BackgroundCall.RunAsync<int>(
            () => throw new ArgumentException("secret technical detail"));

        Assert.IsFalse(result.IsOk);
        Assert.AreEqual(BackgroundCall.GENERIC_ERROR, result.ErrorText);
        Assert.IsFalse(result.ErrorText.Contains("secret"));
    }

    [TestMethod]
    public async Task RunAsync_RunsTheWorkOnAnotherThread()
    {
        int calling_thread = Environment.CurrentManagedThreadId;

        CallResult<int> result = await BackgroundCall.RunAsync(() => Environment.CurrentManagedThreadId);

        Assert.AreNotEqual(calling_thread, result.Value);
    }

    // ErrorTextFor

    [TestMethod]
    public void ErrorTextFor_DatabaseDown_IsItsClearSentence()
    {
        string text = BackgroundCall.ErrorTextFor(new InvalidOperationException(DATABASE_DOWN_TEXT));

        Assert.AreEqual(DATABASE_DOWN_TEXT, text);
    }

    [TestMethod]
    public void ErrorTextFor_AnyOtherError_IsTheGeneralText()
    {
        string text = BackgroundCall.ErrorTextFor(new NullReferenceException("object not set"));

        Assert.AreEqual(BackgroundCall.GENERIC_ERROR, text);
    }
}
