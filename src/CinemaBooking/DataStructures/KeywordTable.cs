// the arm table of the AI agent: our own hash table from the arm name to its allowed roles and its handler (owner: kushal)
// the name is not case sensitive and spaces around it are ignored. no Dictionary from .NET.
using CinemaBooking.Models;

namespace CinemaBooking.DataStructures
{
    // one arm: its name, the roles that may use it and the handler that runs it
    public class KeywordEntry<THandler>
    {
        public string Name { get; }
        public Role[] AllowedRoles { get; }
        public THandler Handler { get; }

        public KeywordEntry(string name, Role[] allowed_roles, THandler handler)
        {
            Name = name;
            AllowedRoles = allowed_roles;
            Handler = handler;
        }
    }

    public class KeywordTable<THandler>
    {
        // how many arms are stored
        public int Count
        {
            get { return 0; }
        }

        // adds an arm. returns false if the name is empty or already used (stub for now)
        public bool Add(string name, Role[] allowed_roles, THandler handler)
        {
            return false;
        }

        // true if the arm is in the table (stub for now)
        public bool Contains(string name)
        {
            return false;
        }

        // finds an arm, null if it is not in the table (stub for now)
        public KeywordEntry<THandler>? Find(string name)
        {
            return null;
        }

        // true only if the arm is in the table and the role is in its list of allowed roles (stub for now)
        public bool IsAllowed(string name, Role role)
        {
            return false;
        }

        // the names of all the arms (stub for now)
        public string[] ListNames()
        {
            return new string[0];
        }
    }
}