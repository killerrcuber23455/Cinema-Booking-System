// sends the chat to the ai web api with httpclient and reads the reply (owner: shankar)
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using CinemaBooking.Models;

namespace CinemaBooking.Agent
{
    public class AiClient : IAiClient
    {
        private readonly HttpClient _http_client;
        private readonly string _endpoint;
        private readonly string _model_name;
        private readonly string _api_key;

        public AiClient(string endpoint, string model_name, string api_key, HttpMessageHandler handler)
        {
            _http_client = new HttpClient(handler);
            _endpoint = endpoint;
            _model_name = model_name;
            _api_key = api_key;
        }

        public Task<AiReply> AskAsync(AgentMessage[] messages, CancellationToken cancel_token)
        {
            return Task.FromResult(AiReply.Fail("not built yet"));
        }
    }
}
