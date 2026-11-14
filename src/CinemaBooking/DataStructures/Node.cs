// a node in our own linked list, used by the hash table and booking history (owner: kushal)
// each node holds one value and points to the next node in the chain.
namespace CinemaBooking.DataStructures
{
    public class Node<T>
    {
        public T Value { get; set; }
        public Node<T>? Next { get; set; }

        public Node(T value)
        {
            Value = value;
            Next = null;
        }
    }
}