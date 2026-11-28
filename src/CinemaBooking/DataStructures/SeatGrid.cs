// the seat grid of one showing: 8 rows x 12 seats, codes A1 to H12 (owner: kushal)
using System;

namespace CinemaBooking.DataStructures
{
    public class SeatGrid
    {
        // true means the seat is taken
        private readonly bool[,] _taken;
        private readonly int _rows;
        private readonly int _seats_per_row;

        // creates a grid where every seat is free
        public SeatGrid(int rows, int seats_per_row)
        {
            _rows = rows;
            _seats_per_row = seats_per_row;
            _taken = new bool[rows, seats_per_row];
        }

        public int Rows
        {
            get { return _rows; }
        }

        public int SeatsPerRow
        {
            get { return _seats_per_row; }
        }

        // turns a code like C5 into row and seat indexes.
        // returns false for a bad format or a seat outside the grid.
        private bool TryGetPosition(string? seat_code, out int row_index, out int seat_index)
        {
            row_index = -1;
            seat_index = -1;

            if (string.IsNullOrEmpty(seat_code) || seat_code.Length < 2)
            {
                return false;
            }

            char row_letter = seat_code[0];

            if (row_letter < 'A' || row_letter > 'Z' || seat_code[1] == '0')
            {
                return false;
            }

            for (int i = 1; i < seat_code.Length; i++)
            {
                if (!char.IsAsciiDigit(seat_code[i]))
                {
                    return false;
                }
            }

            int seat_number;

            if (!int.TryParse(seat_code.Substring(1), out seat_number))
            {
                return false;
            }

            row_index = row_letter - 'A';
            seat_index = seat_number - 1;

            return row_index < _rows && seat_index >= 0 && seat_index < _seats_per_row;
        }

        // true only if the code is valid, inside the grid, and the seat is not taken
        public bool IsSeatFree(string seat_code)
        {
            int row_index;
            int seat_index;

            if (!TryGetPosition(seat_code, out row_index, out seat_index))
            {
                return false;
            }

            return !_taken[row_index, seat_index];
        }

        // books all the seats or none. false if any seat is bad, taken, or listed twice.
        public bool BookSeats(string[] seat_codes)
        {
            if (seat_codes == null || seat_codes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < seat_codes.Length; i++)
            {
                if (!IsSeatFree(seat_codes[i]))
                {
                    return false;
                }

                for (int j = i + 1; j < seat_codes.Length; j++)
                {
                    if (seat_codes[i] == seat_codes[j])
                    {
                        return false;
                    }
                }
            }

            for (int i = 0; i < seat_codes.Length; i++)
            {
                int row_index;
                int seat_index;
                TryGetPosition(seat_codes[i], out row_index, out seat_index);
                _taken[row_index, seat_index] = true;
            }

            return true;
        }

        // frees all the seats or none. false if any code is bad.
        public bool FreeSeats(string[] seat_codes)
        {
            if (seat_codes == null || seat_codes.Length == 0)
            {
                return false;
            }

            for (int i = 0; i < seat_codes.Length; i++)
            {
                int row_index;
                int seat_index;

                if (!TryGetPosition(seat_codes[i], out row_index, out seat_index))
                {
                    return false;
                }
            }

            for (int i = 0; i < seat_codes.Length; i++)
            {
                int row_index;
                int seat_index;
                TryGetPosition(seat_codes[i], out row_index, out seat_index);
                _taken[row_index, seat_index] = false;
            }

            return true;
        }

        // returns the codes of all taken seats, in order A1, A2, ... H12
        public string[] ListTakenSeats()
        {
            int count = 0;

            for (int r = 0; r < _rows; r++)
            {
                for (int s = 0; s < _seats_per_row; s++)
                {
                    if (_taken[r, s])
                    {
                        count++;
                    }
                }
            }

            string[] result = new string[count];
            int index = 0;

            for (int r = 0; r < _rows; r++)
            {
                for (int s = 0; s < _seats_per_row; s++)
                {
                    if (_taken[r, s])
                    {
                        result[index] = ((char)('A' + r)).ToString() + (s + 1);
                        index++;
                    }
                }
            }

            return result;
        }
    }
}