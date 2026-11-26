//rules for registering and listing members with their membership tier (owner: shankar)
using System;
using CinemaBooking.Models;
using CinemaBooking.Data;

namespace CinemaBooking.Services
{
    public class MemberService
    {
        //database used to save and read members
        private IDatabase _database;

        //the words agreed with Divyesh for every "no match"
        private const string MEMBER_NOT_FOUND_MESSAGE = "no member found with this email";

        //uses the database given so everyone shares the same data
        public MemberService(IDatabase database)
        {
            _database = database;
        }


        //from input builds a new member
        //if bad input returns false with reasons
        public bool TryBuildNewMember(string name, string email, string phone,
            out Member? new_member, out string errors)
        {
            string all_errors = "";
            string error_message;

            //id isnt created yet and check what user input
            if (!Member.IsValidName(name, out error_message)) all_errors += error_message + "\n";
            if (!Member.IsValidEmail(email, out error_message)) all_errors += error_message + "\n";
            if (!Member.IsValidPhone(phone, out error_message)) all_errors += error_message + "\n";

            if (all_errors != "")
            {
                new_member = null;
                errors = all_errors;
                return false;
            }

            //id 0 for now, database gives real one when it saves
            new_member = new Member(0, name, email, phone, Tier.Standard, 0, 0, DateTime.Now);
            errors = "";
            return true;
        }

        //anyone can register to become a new member so no role check
        //returns the new id or -1 with a reason if refused
        public int RegisterMember(string name, string email, string phone, out string error_message)
        {
            Member? new_member;
            string build_errors;

            //check the input and build a standard member
            if (!TryBuildNewMember(name, email, phone, out new_member, out build_errors) || new_member == null)
            {
                error_message = build_errors;
                return -1;
            }

            //-1 means that the email is already registered in the database
            int new_id = _database.SaveMember(new_member);
            if (new_id == -1)
            {
                error_message = "a member with this email address is already registered";
                return -1;
            }

            error_message = "";
            return new_id;
        }

        //only staff or admin can view members
        //if refused an empty array with a reason is returned
        public Member[] GetAllMembers(Role role, out string error_message)
        {
            if (role != Role.Staff && role != Role.Admin)
            {
                error_message = "Only staff or admin can view members";
                return new Member[0];
            }
            error_message = "";
            return _database.GetAllMembers();
        }

        //reads one member again from the database by id, so a window can show the points as they are now
        //null if there is no such member. any signed in user may read their own member, so there is no role check
        //(the green step will use the FindMemberById that login already uses)
        //STUB for the red step: nothing is found yet, so the tests fail first
        public Member? ReloadMember(int member_id)
        {
            return null;
        }

        //finding a member by email, only staff or admin can do it
        //returns the member, or null with a reason
        public Member? FindMember(Role role, string email, out string error_message)
        {
            //GetAllMembers already holds the rule "only staff and admin", so we reuse it
            //this way the role rule and its message stay in one place
            Member[] all_members = GetAllMembers(role, out error_message);

            if (error_message != "")
            {
                return null;
            }

            //a null email counts as blank, a blank email never matches anything
            string wanted = (email ?? "").Trim().ToLowerInvariant();

            if (wanted == "")
            {
                error_message = MEMBER_NOT_FOUND_MESSAGE;
                return null;
            }

            //the whole email must match, ignoring upper and lower case
            for (int index = 0; index < all_members.Length; index++)
            {
                if (all_members[index].Email.Trim().ToLowerInvariant() == wanted)
                {
                    error_message = "";
                    return all_members[index];
                }
            }

            error_message = MEMBER_NOT_FOUND_MESSAGE;
            return null;
        }

        // used by login to get the member of a customer, no role check because the user is already known
        internal Member? FindMemberById(int member_id)
        {
            Member[] all_members = _database.GetAllMembers();

            for (int index = 0; index < all_members.Length; index++)
            {
                if (all_members[index].Id == member_id)
                {
                    return all_members[index];
                }
            }

            return null;
        }

        // checks name, email and phone of a new member without saving anything
        internal bool IsValidNewMember(string name, string email, string phone, out string error_message)
        {
            return TryBuildNewMember(name, email, phone, out _, out error_message);
        }

        // true if a member already has this email (trimmed, ignoring case)
        internal bool IsEmailUsed(string email)
        {
            string wanted = email.Trim().ToLowerInvariant();

            foreach (Member member in _database.GetAllMembers())
            {
                if (member.Email.Trim().ToLowerInvariant() == wanted)
                {
                    return true;
                }
            }

            return false;
        }

    }
}
