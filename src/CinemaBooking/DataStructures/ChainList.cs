// our own linked list for the hash table chain and booking history (owner: kushal)
// it uses our own nodes, not List, Dictionary, HashSet, Queue or LinkedList from .NET.
// adding at the front is O(1), which keeps hash table inserts fast.
namespace CinemaBooking.DataStructures
{
    public class ChainList<T>
    {
        private Node<T>? _head;
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        // add a new item at the front of the chain
        public void Add(T value)
        {
            Node<T> new_node = new Node<T>(value);
            new_node.Next = _head;
            _head = new_node;
            _count++;
        }

        // find the first item that matches the rule given by the caller
        public T? Find(Func<T, bool> matches)
        {
            Node<T>? current = _head;

            while (current != null)
            {
                if (matches(current.Value))
                {
                    return current.Value;
                }

                current = current.Next;
            }

            return default;
        }

        // remove the first item that matches the rule given by the caller
        public bool Remove(Func<T, bool> matches)
        {
            Node<T>? current = _head;
            Node<T>? previous = null;

            while (current != null)
            {
                if (matches(current.Value))
                {
                    if (previous == null)
                    {
                        // the matching item is the first node
                        _head = current.Next;
                    }
                    else
                    {
                        // join the node before it to the node after it
                        previous.Next = current.Next;
                    }

                    _count--;
                    return true;
                }

                previous = current;
                current = current.Next;
            }

            return false;
        }

        // return every item in a plain array, without using a built-in collection
        public T[] ListAll()
        {
            T[] items = new T[_count];
            Node<T>? current = _head;
            int index = 0;

            while (current != null)
            {
                items[index] = current.Value;
                index++;
                current = current.Next;
            }

            return items;
        }
    }
}