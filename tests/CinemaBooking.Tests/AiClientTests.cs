// tests for the ai client, using a fake http handler so no real api is called (owner: shankar)
// written before the code for card 372, so they fail first (red) and pass after (green)
using System.Net;
using CinemaBooking.Agent;
using CinemaBooking.Models;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class AiClientTests
{
    private const string ENDPOINT = "https://example.com/fake-ai-endpoint";
    private const string MODEL = "FAKE-MODEL-NAME";
    private const string KEY = "test-secret-123";
    private const string PLACEHOLDER_KEY = "FAKE-API-KEY-REPLACE-ME";

    private const string GOOD_JSON = """{"choices":[{"message":{"role":"assistant","content":"Dune is on at 18:00"}}]}""";

    private const string NO_KEY_MESSAGE = "the AI assistant is off: no API key in appsettings.json";
    private const string BUSY_MESSAGE = "the AI is busy, please wait a minute and try again";
    private const string TIMEOUT_MESSAGE = "the AI did not answer within 20 seconds, please try again";
    private const string NO_NETWORK_MESSAGE = "the AI could not be reached, check the internet connection";
    private const string BAD_JSON_MESSAGE = "the AI sent an answer we could not read";
    private const string STOPPED_MESSAGE = "the request was stopped";

    private static AgentMessage[] OneMessage()
    {
        return new AgentMessage[] { new AgentMessage("user", "what is on tonight?") };
    }

    private static AiClient MakeClient(FakeHttpHandler handler, string api_key)
    {
        return new AiClient(ENDPOINT, MODEL, api_key, handler);
    }

    //AICLIENT-01
    [TestMethod]
    public async Task AskAsync_GoodReply_ReturnsText()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, GOOD_JSON);

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.IsTrue(reply.IsOk);
        Assert.AreEqual("Dune is on at 18:00", reply.Text);
    }

    // AICLIENT-02
    [TestMethod]
    public async Task AskAsync_BlankKey_IsRefusedAndSendsNothing()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, GOOD_JSON);

        AiReply reply = await MakeClient(handler, "  ").AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(NO_KEY_MESSAGE, reply.ErrorMessage);
        Assert.AreEqual(0, handler.CallCount);
    }

    // AICLIENT-03
    [TestMethod]
    public async Task AskAsync_PlaceholderKey_IsRefusedAndSendsNothing()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, GOOD_JSON);

        AiReply reply = await MakeClient(handler, PLACEHOLDER_KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(NO_KEY_MESSAGE, reply.ErrorMessage);
        Assert.AreEqual(0, handler.CallCount);
    }

    // AICLIENT-04
    [TestMethod]
    public async Task AskAsync_SendsKeyInBearerHeader()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, GOOD_JSON);

        await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.IsNotNull(handler.LastRequest);
        Assert.IsNotNull(handler.LastRequest.Headers.Authorization);
        Assert.AreEqual("Bearer", handler.LastRequest.Headers.Authorization.Scheme);
        Assert.AreEqual(KEY, handler.LastRequest.Headers.Authorization.Parameter);
    }

    // AICLIENT-05
    [TestMethod]
    public async Task AskAsync_ServerError500_GivesClearMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.InternalServerError, "");

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.IsFalse(reply.IsOk);
        Assert.AreEqual("the AI service returned an error (status 500)", reply.ErrorMessage);
    }

    // AICLIENT-06
    [TestMethod]
    public async Task AskAsync_RateLimit429_GivesBusyMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.TooManyRequests, "");

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(BUSY_MESSAGE, reply.ErrorMessage);
    }

    // AICLIENT-07
    [TestMethod]
    public async Task AskAsync_Timeout_GivesClearMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(new TaskCanceledException());

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(TIMEOUT_MESSAGE, reply.ErrorMessage);
    }

    // AICLIENT-08
    [TestMethod]
    public async Task AskAsync_NoNetwork_GivesClearMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(new HttpRequestException("no such host"));

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(NO_NETWORK_MESSAGE, reply.ErrorMessage);
    }

    // AICLIENT-09
    [TestMethod]
    public async Task AskAsync_EmptyBody_GivesClearMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, "");

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(BAD_JSON_MESSAGE, reply.ErrorMessage);
    }

    // AICLIENT-10
    [TestMethod]
    public async Task AskAsync_InvalidJson_GivesClearMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, "{not json");

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(BAD_JSON_MESSAGE, reply.ErrorMessage);
    }

    // AICLIENT-11
    [TestMethod]
    public async Task AskAsync_JsonWithoutChoices_GivesClearMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, """{"id":"abc"}""");

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), CancellationToken.None);

        Assert.AreEqual(BAD_JSON_MESSAGE, reply.ErrorMessage);
    }

    // AICLIENT-12
    [TestMethod]
    public async Task AskAsync_ErrorMessages_NeverContainKey()
    {
        FakeHttpHandler[] handlers = new FakeHttpHandler[]
        {
            new FakeHttpHandler(HttpStatusCode.Unauthorized, "invalid key " + KEY),
            new FakeHttpHandler(HttpStatusCode.TooManyRequests, KEY),
            new FakeHttpHandler(new TaskCanceledException(KEY)),
            new FakeHttpHandler(new HttpRequestException("failed for " + KEY)),
            new FakeHttpHandler(HttpStatusCode.OK, "{bad json " + KEY)
        };

        for (int index = 0; index < handlers.Length; index++)
        {
            AiReply reply = await MakeClient(handlers[index], KEY).AskAsync(OneMessage(), CancellationToken.None);

            Assert.IsFalse(reply.IsOk);
            Assert.DoesNotContain(KEY, reply.ErrorMessage);
        }
    }

    // AICLIENT-13
    [TestMethod]
    public async Task AskAsync_UserCancels_GivesStoppedMessage()
    {
        FakeHttpHandler handler = new FakeHttpHandler(HttpStatusCode.OK, GOOD_JSON);
        CancellationTokenSource cancel_source = new CancellationTokenSource();
        cancel_source.Cancel();

        AiReply reply = await MakeClient(handler, KEY).AskAsync(OneMessage(), cancel_source.Token);

        Assert.AreEqual(STOPPED_MESSAGE, reply.ErrorMessage);
    }
}
