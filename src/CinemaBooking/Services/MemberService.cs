//rules for registering and listing members with their membership tier (owner: shankar)
using System;
using CinemaBooking.Models;
using CinemaBooking.Data;

namespace CinemaBooking.Services
{
   public class MemberService
   {
        //database used to save and read members
        private Database _database;

        //uses the database given so everyone shares the same data
        public MemberService(Database database)
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

   }
}
