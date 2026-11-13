// our own hash table that uses separate chaining and our linked list (owner: kushal)
// it stores a key and a value in each slot. it does not use List, Dictionary,
// HashSet, Queue or LinkedList from .NET.
namespace CinemaBooking.DataStructures
{
    // holds one key and one value inside the hash table
    public class HashTableEntry<TKey, TValue>
    {
        public TKey Key { get; set; }
        public TValue Value { get; set; }

        public HashTableEntry(TKey key, TValue value)
        {
            Key = key;
            Value = value;
        }
    }

    // custom hash table that uses our own linked list for chaining
    public class HashTable<TKey, TValue>
    {
        private LinkedList<HashTableEntry<TKey, TValue>>[] _slots;
        private int _count;

        public int Count
        {
            get { return _count; }
        }

        public HashTable(int initial_slots = 16)
        {
            _slots = new LinkedList<HashTableEntry<TKey, TValue>>[initial_slots];
            _count = 0;
        }

        // works out which slot a key belongs in
        private int calculate_slot(TKey key)
        {
            int hash_code = key.GetHashCode();
            int slot = hash_code % _slots.Length;

            if (slot < 0)
            {
                slot = slot + _slots.Length;
            }

            return slot;
        }

        // adds a new key and value. returns false if the key already exists.
        public bool Add(TKey key, TValue value)
        {
            int slot = calculate_slot(key);

            if (_slots[slot] == null)
            {
                _slots[slot] = new LinkedList<HashTableEntry<TKey, TValue>>();
            }

            HashTableEntry<TKey, TValue>? existing_entry = _slots[slot].Find(entry => entry.Key.Equals(key));

            if (existing_entry != null)
            {
                return false;
            }

            HashTableEntry<TKey, TValue> new_entry = new HashTableEntry<TKey, TValue>(key, value);
            _slots[slot].Add(new_entry);
            _count++;

            return true;
        }

        // finds a value using its key. returns the value or the default value.
        public TValue? Find(TKey key)
        {
            int slot = calculate_slot(key);

            if (_slots[slot] == null)
            {
                return default;
            }

            HashTableEntry<TKey, TValue>? found_entry = _slots[slot].Find(entry => entry.Key.Equals(key));

            if (found_entry == null)
            {
                return default;
            }

            return found_entry.Value;
        }

        // removes a key and its value. returns true if the key was found.
        public bool Remove(TKey key)
        {
            int slot = calculate_slot(key);

            if (_slots[slot] == null)
            {
                return false;
            }

            bool removed = _slots[slot].Remove(entry => entry.Key.Equals(key));

            if (removed)
            {
                _count--;
            }

            return removed;
        }

        // returns every value in a plain array, without using a built-in collection
        public TValue[] ListAll()
        {
            TValue[] values = new TValue[_count];
            int value_index = 0;

            for (int slot = 0; slot < _slots.Length; slot++)
            {
                if (_slots[slot] == null)
                {
                    continue;
                }

                HashTableEntry<TKey, TValue>[] entries = _slots[slot].ListAll();

                for (int entry_index = 0; entry_index < entries.Length; entry_index++)
                {
                    values[value_index] = entries[entry_index].Value;
                    value_index++;
                }
            }

            return values;
        }
    }
}