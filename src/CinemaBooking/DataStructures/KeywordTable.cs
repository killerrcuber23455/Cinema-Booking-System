// the arm table of the AI agent: our own hash table from the arm name to its allowed roles and its handler (owner: kushal)
// the name is not case sensitive and spaces around it are ignored. no Dictionary from .NET.
using CinemaBooking.Models;

namespace CinemaBooking.DataStructures
{
    // one arm: its name, the roles that may use it and the handler that runs it
    public class KeywordEntry<THandler>
    {
        private readonly Role[] _allowed_roles;

        public string Name { get; }
        public THandler Handler { get; }

        // the entry keeps its own copy of the roles, so nobody can change them from outside
        public KeywordEntry(string name, Role[] allowed_roles, THandler handler)
        {
            Name = name;
            Handler = handler;
            _allowed_roles = CopyRoles(allowed_roles);
        }

        // a copy of the allowed roles
        public Role[] AllowedRoles
        {
            get { return CopyRoles(_allowed_roles); }
        }

        // true if the role is in the list of allowed roles
        public bool Allows(Role role)
        {
            for (int index = 0; index < _allowed_roles.Length; index++)
            {
                if (_allowed_roles[index] == role)
                {
                    return true;
                }
            }

            return false;
        }

        private static Role[] CopyRoles(Role[] roles)
        {
            Role[] copy = new Role[roles.Length];

            for (int index = 0; index < roles.Length; index++)
            {
                copy[index] = roles[index];
            }

            return copy;
        }
    }

    public class KeywordTable<THandler>
    {
        // the arms by name (lowercase, no spaces around it)
        private readonly HashTable<string, KeywordEntry<THandler>> _entries = new HashTable<string, KeywordEntry<THandler>>();

        // how many arms are stored
        public int Count
        {
            get { return _entries.Count; }
        }

        // adds an arm. returns false if the name is empty, the roles are missing or the name is already used
        public bool Add(string name, Role[] allowed_roles, THandler handler)
        {
            string key = MakeKey(name);

            if (key == "" || allowed_roles == null)
            {
                return false;
            }

            return _entries.Add(key, new KeywordEntry<THandler>(key, allowed_roles, handler));
        }

        // true if the arm is in the table
        public bool Contains(string name)
        {
            return Find(name) != null;
        }

        // finds an arm, null if it is not in the table
        public KeywordEntry<THandler>? Find(string name)
        {
            string key = MakeKey(name);

            if (key == "")
            {
                return null;
            }

            return _entries.Find(key);
        }

        // true only if the arm is in the table and the role is in its list of allowed roles.
        // an arm name that is not in the table is refused for every role
        public bool IsAllowed(string name, Role role)
        {
            KeywordEntry<THandler>? entry = Find(name);

            if (entry == null)
            {
                return false;
            }

            return entry.Allows(role);
        }

        // the names of all the arms (lowercase)
        public string[] ListNames()
        {
            KeywordEntry<THandler>[] all_entries = _entries.ListAll();
            string[] names = new string[all_entries.Length];

            for (int index = 0; index < all_entries.Length; index++)
            {
                names[index] = all_entries[index].Name;
            }

            return names;
        }

        // the name without spaces around it, in lowercase. empty text if there is no name
        private static string MakeKey(string name)
        {
            if (string.IsNullOrWhiteSpace(name))
            {
                return "";
            }

            return name.Trim().ToLowerInvariant();
        }
    }
}