// the memory of the AI agent: our own queue that keeps the last 10 messages (owner: kushal)
// when it is full, adding a message drops the oldest one (the sliding window from week 12).
// no List, Queue or LinkedList from .NET.
using CinemaBooking.Models;

namespace CinemaBooking.DataStructures
{
    public class MemoryQueue
    {
        // how many messages the agent remembers
        public const int MEMORY_SIZE = 10;

        // how many messages are stored now
        public int Count
        {
            get { return 0; }
        }

        // adds a message at the end, drops the oldest when full.
        // returns false if the role or the text is empty (nothing is added)
        public bool Add(string role, string text)
        {
            return false;
        }

        // every message, oldest first. it is a copy, so changing it does not change the queue
        public AgentMessage[] ListAll()
        {
            return new AgentMessage[0];
        }

        // forgets every message (used when the user signs out)
        public void Clear()
        {
        }
    }
}