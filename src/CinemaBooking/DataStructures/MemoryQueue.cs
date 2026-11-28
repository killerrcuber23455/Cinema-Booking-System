// the memory of the AI agent: our own queue that keeps the last 10 messages (owner: kushal)
// when it is full, adding a message drops the oldest one (the sliding window from week 12).
// it is an array of 10 slots used in a circle, so dropping the oldest never moves the other messages.
// no List, Queue or LinkedList from .NET.
using CinemaBooking.Models;

namespace CinemaBooking.DataStructures
{
    public class MemoryQueue
    {
        // how many messages the agent remembers
        public const int MEMORY_SIZE = 10;

        private readonly AgentMessage[] _messages = new AgentMessage[MEMORY_SIZE];

        // the slot that holds the oldest message
        private int _start;

        // how many messages are stored now
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        // adds a message at the end, drops the oldest when full.
        // returns false if the role or the text is empty (nothing is added)
        public bool Add(string role, string text)
        {
            if (string.IsNullOrWhiteSpace(role) || string.IsNullOrWhiteSpace(text))
            {
                return false;
            }

            AgentMessage message = new AgentMessage(role, text);

            if (_count < MEMORY_SIZE)
            {
                // there is still a free slot after the newest message
                _messages[(_start + _count) % MEMORY_SIZE] = message;
                _count++;
            }
            else
            {
                // full: the slot of the oldest message is reused for the newest one,
                // and the next slot becomes the oldest
                _messages[_start] = message;
                _start = (_start + 1) % MEMORY_SIZE;
            }

            return true;
        }

        // every message, oldest first. it is a copy, so changing it does not change the queue
        public AgentMessage[] ListAll()
        {
            AgentMessage[] copy = new AgentMessage[_count];

            for (int index = 0; index < _count; index++)
            {
                copy[index] = _messages[(_start + index) % MEMORY_SIZE];
            }

            return copy;
        }

        // forgets every message (used when the user signs out)
        public void Clear()
        {
            for (int index = 0; index < MEMORY_SIZE; index++)
            {
                _messages[index] = null!;
            }

            _start = 0;
            _count = 0;
        }
    }
}