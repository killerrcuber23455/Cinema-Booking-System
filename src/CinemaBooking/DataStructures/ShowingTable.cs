// the showings hash table, keyed by showing id (owner: kushal)
using CinemaBooking.Models;

namespace CinemaBooking.DataStructures
{
    public class ShowingTable
    {
        private readonly HashTable<int, Showing> _showings = new HashTable<int, Showing>();

        public int Count
        {
            get { return _showings.Count; }
        }

        // creates a showing with its own seat grid. returns false if the id already exists.
        public bool CreateShowing(int showing_id, int film_id, int screen_id, System.DateTime starts_at,
            decimal price, int rows, int seats_per_row)
        {
            if (_showings.Find(showing_id) != null)
            {
                return false;
            }

            SeatGrid grid = new SeatGrid(rows, seats_per_row);
            Showing showing = new Showing(showing_id, film_id, screen_id, starts_at, price, grid);

            return _showings.Add(showing_id, showing);
        }

        // adds a showing that already exists (for example loaded from the database)
        public bool AddShowing(Showing showing)
        {
            return _showings.Add(showing.Id, showing);
        }

        // returns the showing, or null if it does not exist
        public Showing? FindShowing(int showing_id)
        {
            return _showings.Find(showing_id);
        }

        // returns the seat grid of a showing or null if the showing does not exist
        public SeatGrid? GetGrid(int showing_id)
        {
            Showing? showing = _showings.Find(showing_id);

            return showing == null ? null : showing.SeatGrid;
        }

        // removes a showing returns false if it does not exist
        // EMPTY STUB for the red step
        public bool RemoveShowing(int showing_id)
        {
            return false;
        }

        public Showing[] ListAll()
        {
            return _showings.ListAll();
        }
    }
}