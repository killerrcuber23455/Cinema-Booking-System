// a fake http handler for tests: gives back a set answer and remembers the request, never uses the internet (owner: shankar)
using System.Net;
using System.Security.Cryptography;

namespace CinemaBooking.Tests.Fakes;

public class FakeHttpHandler : HttpMessageHandler
{
    private readonly HttpStatusCode _status_code;
    private readonly string _body;
    private readonly Exception? _error_to_throw;

    // how many times a request reached this handler
    public int CallCount { get; private set; }

    // the last request, so a test can look at its headers
    public HttpRequestMessage? LastRequest { get; private set; }

    //answers every request with the status and body
    public FakeHttpHandler(HttpStatusCode status_code, string body)
    {
        _status_code = status_code;
        _body = body;
        _error_to_throw = null;
    }

    // throws this error instead of answering such as no network and timeout
    public FakeHttpHandler(Exception error_to_throw)
    {
        _status_code = HttpStatusCode.OK;
        _body = "";
        _error_to_throw = error_to_throw;
    }

    protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancel_token)
    {
        CallCount = CallCount + 1;
        LastRequest = request;

        if (_error_to_throw != null)
        {
            throw _error_to_throw;
        }

        HttpResponseMessage response = new HttpResponseMessage(_status_code);
        response.Content = new StringContent(_body);
        return Task.FromResult(response);
    }

}