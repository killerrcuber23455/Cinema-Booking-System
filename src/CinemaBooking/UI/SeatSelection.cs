// keeps track of which free seats the user has picked on the seat map (owner: divyesh)
using System;
using CinemaBooking.DataStructures;

namespace CinemaBooking.UI
{
    public class SeatSelection
    {
        // stubs for card 348, the real code comes in the green step
        public SeatSelection(SeatGrid grid)
        {
        }

        public int Count
        {
            get { throw new NotImplementedException(); }
        }

        public bool TryToggle(string seat_code, out string error_message)
        {
            throw new NotImplementedException();
        }

        public bool IsSelected(string seat_code)
        {
            throw new NotImplementedException();
        }

        public string[] GetSelectedSeats()
        {
            throw new NotImplementedException();
        }

        public string Describe()
        {
            throw new NotImplementedException();
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }
    }
}
