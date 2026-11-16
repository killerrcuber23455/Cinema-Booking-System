// the members hash table, keyed by email (owner: kushal)
using CinemaBooking.Models;

namespace CinemaBooking.DataStructures
{
    public class MemberTable
    {
        private readonly HashTable<string, Member> _members = new HashTable<string, Member>();

        public int Count
        {
            get { return _members.Count; }
        }

        // emails are trimmed and made lowercase, so A@x.com and a@x.com are the same member
        private static string MakeKey(string email)
        {
            return email.Trim().ToLowerInvariant();
        }

        // false if the email is null or already used
        public bool AddMember(Member member)
        {
            if (member == null || member.Email == null)
            {
                return false;
            }

            return _members.Add(MakeKey(member.Email), member);
        }

        // returns the member, or null if the email is not found
        public Member? FindByEmail(string email)
        {
            if (email == null)
            {
                return null;
            }

            return _members.Find(MakeKey(email));
        }

        public Member[] ListAll()
        {
            return _members.ListAll();
        }
    }
}