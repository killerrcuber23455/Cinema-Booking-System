// keeps track of which free seats the user has picked on the seat map, it never changes the seat grid (owner: divyesh)
// the window draws the seats, this class decides what a click on a seat does
using System;
using System.Globalization;
using CinemaBooking.DataStructures;

namespace CinemaBooking.UI
{
    public class SeatSelection
    {
        private readonly SeatGrid _grid;

        // true for every seat the user has picked, same size as the grid
        private readonly bool[,] _picked;

        private int _count;

        public SeatSelection(SeatGrid grid)
        {
            _grid = grid;
            _picked = new bool[grid.Rows, grid.SeatsPerRow];
        }

        // how many seats are picked now
        public int Count
        {
            get { return _count; }
        }

        // the code of a seat from its place in the grid, row 0 seat 0 is A1
        public static string MakeSeatCode(int row_index, int seat_index)
        {
            return ((char)('A' + row_index)).ToString() + (seat_index + 1);
        }

        // a click on a seat: a picked seat is unpicked, a free seat is picked, a taken seat is refused
        public bool TryToggle(string seat_code, out string error_message)
        {
            if (!TryGetPosition(seat_code, out int row_index, out int seat_index))
            {
                error_message = "seat " + seat_code + " is not on this screen";
                return false;
            }

            // a picked seat can always be unpicked, even if somebody else booked it meanwhile
            if (_picked[row_index, seat_index])
            {
                _picked[row_index, seat_index] = false;
                _count--;
                error_message = "";
                return true;
            }

            if (!_grid.IsSeatFree(seat_code))
            {
                error_message = "seat " + seat_code + " is already taken";
                return false;
            }

            _picked[row_index, seat_index] = true;
            _count++;
            error_message = "";
            return true;
        }

        public bool IsSelected(string seat_code)
        {
            if (!TryGetPosition(seat_code, out int row_index, out int seat_index))
            {
                return false;
            }

            return _picked[row_index, seat_index];
        }

        // the picked seats in screen order (A1, A2 ... H12), this is what goes to the booking
        public string[] GetSelectedSeats()
        {
            string[] seats = new string[_count];
            int next_place = 0;

            for (int row_index = 0; row_index < _grid.Rows; row_index++)
            {
                for (int seat_index = 0; seat_index < _grid.SeatsPerRow; seat_index++)
                {
                    if (_picked[row_index, seat_index])
                    {
                        seats[next_place] = MakeSeatCode(row_index, seat_index);
                        next_place++;
                    }
                }
            }

            return seats;
        }

        // one line of text for the window, for example: chosen: A1, C5
        public string Describe()
        {
            if (_count == 0)
            {
                return "no seats chosen";
            }

            return "chosen: " + string.Join(", ", GetSelectedSeats());
        }

        public void Clear()
        {
            Array.Clear(_picked);
            _count = 0;
        }

        // turns a code like C5 into a row and seat place, false if the code is not exactly a seat on this screen
        private bool TryGetPosition(string seat_code, out int row_index, out int seat_index)
        {
            row_index = 0;
            seat_index = 0;

            if (string.IsNullOrEmpty(seat_code) || seat_code.Length < 2)
            {
                return false;
            }

            row_index = seat_code[0] - 'A';

            // none style: digits only, no sign and no spaces
            if (!int.TryParse(seat_code.Substring(1), NumberStyles.None, CultureInfo.InvariantCulture, out int seat_number))
            {
                return false;
            }

            seat_index = seat_number - 1;

            if (row_index < 0 || row_index >= _grid.Rows || seat_index < 0 || seat_index >= _grid.SeatsPerRow)
            {
                return false;
            }

            // A01 reads as seat 1 but it is not the code we use, so the code must match exactly
            return seat_code == MakeSeatCode(row_index, seat_index);
        }
    }
}
