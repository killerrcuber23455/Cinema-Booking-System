// the name, email and phone of a guest booking, checked before the booking is made (owner: divyesh)
using System;

namespace CinemaBooking.UI
{
    public class GuestDetails
    {
        // stub for card 403, the real code comes in the green step
        public string Name
        {
            get { throw new NotImplementedException(); }
        }

        public string Email
        {
            get { throw new NotImplementedException(); }
        }

        public string Phone
        {
            get { throw new NotImplementedException(); }
        }

        public static bool TryCreate(string name, string email, string phone, out GuestDetails? details, out string error_message)
        {
            throw new NotImplementedException();
        }
    }
}
