// tests the memory queue of the AI agent: keeps the last 10 messages, drops the oldest (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class MemoryQueueTests
    {
        // adds "message 1", "message 2" and so on, from first to last
        private void AddMessages(MemoryQueue queue, int first, int last)
        {
            for (int number = first; number <= last; number++)
            {
                queue.Add("user", "message " + number);
            }
        }

        [TestMethod]
        public void NewQueue_IsEmpty()
        {
            MemoryQueue queue = new MemoryQueue();

            Assert.AreEqual(0, queue.Count);
            Assert.IsEmpty(queue.ListAll());
        }

        [TestMethod]
        public void MemorySize_IsTen()
        {
            Assert.AreEqual(10, MemoryQueue.MEMORY_SIZE);
        }

        [TestMethod]
        public void Add_ValidMessage_ReturnsTrue()
        {
            MemoryQueue queue = new MemoryQueue();

            Assert.IsTrue(queue.Add("user", "hello"));
        }

        [TestMethod]
        public void Add_ValidMessage_CountIsOne()
        {
            MemoryQueue queue = new MemoryQueue();

            queue.Add("user", "hello");

            Assert.AreEqual(1, queue.Count);
        }

        [TestMethod]
        public void Add_KeepsRoleAndText()
        {
            MemoryQueue queue = new MemoryQueue();

            queue.Add("assistant", "which film?");

            AgentMessage[] messages = queue.ListAll();
            Assert.HasCount(1, messages);
            Assert.AreEqual("assistant", messages[0].Role);
            Assert.AreEqual("which film?", messages[0].Text);
        }

        [TestMethod]
        public void ListAll_ReturnsOldestFirst()
        {
            MemoryQueue queue = new MemoryQueue();

            AddMessages(queue, 1, 3);

            AgentMessage[] messages = queue.ListAll();
            Assert.HasCount(3, messages);
            Assert.AreEqual("message 1", messages[0].Text);
            Assert.AreEqual("message 2", messages[1].Text);
            Assert.AreEqual("message 3", messages[2].Text);
        }

        [TestMethod]
        public void Add_TenMessages_KeepsAll()
        {
            MemoryQueue queue = new MemoryQueue();

            AddMessages(queue, 1, 10);

            AgentMessage[] messages = queue.ListAll();
            Assert.HasCount(10, messages);
            Assert.AreEqual("message 1", messages[0].Text);
            Assert.AreEqual("message 10", messages[9].Text);
        }

        [TestMethod]
        public void Add_ElevenMessages_DropsTheOldest()
        {
            MemoryQueue queue = new MemoryQueue();

            AddMessages(queue, 1, 11);

            AgentMessage[] messages = queue.ListAll();
            Assert.AreEqual(10, queue.Count);
            Assert.HasCount(10, messages);
            Assert.AreEqual("message 2", messages[0].Text);
            Assert.AreEqual("message 11", messages[9].Text);
        }

        [TestMethod]
        public void Add_ManyMessages_KeepsLastTenInOrder()
        {
            MemoryQueue queue = new MemoryQueue();

            AddMessages(queue, 1, 25);

            AgentMessage[] messages = queue.ListAll();
            Assert.HasCount(10, messages);
            for (int index = 0; index < messages.Length; index++)
            {
                Assert.AreEqual("message " + (16 + index), messages[index].Text);
            }
        }

        [TestMethod]
        public void Add_EmptyRole_ReturnsFalse()
        {
            MemoryQueue queue = new MemoryQueue();

            Assert.IsFalse(queue.Add("", "hello"));
            Assert.AreEqual(0, queue.Count);
        }

        [TestMethod]
        public void Add_EmptyText_ReturnsFalse()
        {
            MemoryQueue queue = new MemoryQueue();

            Assert.IsFalse(queue.Add("user", ""));
            Assert.AreEqual(0, queue.Count);
        }

        [TestMethod]
        public void Add_WhitespaceText_ReturnsFalse()
        {
            MemoryQueue queue = new MemoryQueue();

            Assert.IsFalse(queue.Add("user", "   "));
            Assert.AreEqual(0, queue.Count);
        }

        [TestMethod]
        public void Clear_EmptiesTheQueue()
        {
            MemoryQueue queue = new MemoryQueue();
            AddMessages(queue, 1, 3);
            Assert.AreEqual(3, queue.Count);

            queue.Clear();

            Assert.AreEqual(0, queue.Count);
            Assert.IsEmpty(queue.ListAll());
        }

        [TestMethod]
        public void Add_AfterClear_StartsAgain()
        {
            MemoryQueue queue = new MemoryQueue();
            AddMessages(queue, 1, 3);
            queue.Clear();

            queue.Add("user", "new start");

            AgentMessage[] messages = queue.ListAll();
            Assert.HasCount(1, messages);
            Assert.AreEqual("new start", messages[0].Text);
        }

        [TestMethod]
        public void ListAll_ReturnsACopy()
        {
            MemoryQueue queue = new MemoryQueue();
            queue.Add("user", "original");

            AgentMessage[] first_copy = queue.ListAll();
            first_copy[0] = new AgentMessage("user", "changed");

            AgentMessage[] second_copy = queue.ListAll();
            Assert.AreEqual("original", second_copy[0].Text);
        }

        [TestMethod]
        public void Clear_AfterWrapAround_ThenFillAgain_KeepsLastTen()
        {
            MemoryQueue queue = new MemoryQueue();
            AddMessages(queue, 1, 15);
            queue.Clear();

            AddMessages(queue, 1, 12);

            AgentMessage[] messages = queue.ListAll();
            Assert.HasCount(10, messages);
            Assert.AreEqual("message 3", messages[0].Text);
            Assert.AreEqual("message 12", messages[9].Text);
        }
    }
}