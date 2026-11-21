// rules to find the showings of a film (owner: shankar)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Services
{
    public class ShowingService
    {
        //same table the booking service uses so everyone sees same showings
        private readonly ShowingTable _showing_table;

        public ShowingService(ShowingTable showing_table)
        {
            _showing_table = showing_table;
        }

        // showings of one film with earliest first and an empty array if the film has none
        // the table is keyed by showing id, so every showing is checked
        public Showing[] GetShowingsForFilm(int film_id)
        { 
            ChainList<Showing> matches = new ChainList<Showing>();
            Showing[] all_showings = _showing_table.ListAll();

            for (int showing_index = 0; showing_index < all_showings.Length; showing_index++)
            {
                if (all_showings[showing_index].FilmId == film_id)
                {
                    matches.Add(all_showings[showing_index]);
                }
            }

            Showing[] found = matches.ListAll();

            // insertion sort by start time, a film has only a few showings
            for (int sorted_end = 1; sorted_end < found.Length; sorted_end++)
            {
                Showing current = found[sorted_end];
                int compare_index = sorted_end - 1;

                while (compare_index >= 0 && found[compare_index].StartsAt > current.StartsAt)
                {
                    found[compare_index + 1] = found[compare_index];
                    compare_index--;
                }
                
            }
            return found;
        }
    }
}
