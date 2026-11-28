// what any ai client must do: take the chat and give back a reply (owner: shankar)
using System.Threading;
using System.Threading.Tasks;
using CinemaBooking.Models;

namespace CinemaBooking.Agent
{
    public interface IAiClient
    {
        Task<AiReply> AskAsync(AgentMessage[] messages, CancellationToken cancel_token);
    }
}
