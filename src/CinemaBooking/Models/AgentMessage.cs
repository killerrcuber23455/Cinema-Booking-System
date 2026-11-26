// one message in the AI agent memory: who said it and what was said (owner: kushal)
namespace CinemaBooking.Models
{
    public class AgentMessage
    {
        public string Role { get; }
        public string Text { get; }

        public AgentMessage(string role, string text)
        {
            Role = role;
            Text = text;
        }
    }
}