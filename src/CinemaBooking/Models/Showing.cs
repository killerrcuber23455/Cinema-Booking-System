// one film shown on one screen at one time, with its own seat grid (owner: kushal)
using System;
using CinemaBooking.DataStructures;

namespace CinemaBooking.Models
{
    public class Showing
    {
        public int Id { get; set; }
        public int FilmId { get; set; }
        public int ScreenId { get; set; }
        public DateTime StartsAt { get; set; }
        public decimal Price { get; set; }

        // each showing has its own seat grid
        public SeatGrid SeatGrid { get; set; }

        public Showing(int id, int film_id, int screen_id, DateTime starts_at, decimal price, SeatGrid seat_grid)
        {
            Id = id;
            FilmId = film_id;
            ScreenId = screen_id;
            StartsAt = starts_at;
            Price = price;
            SeatGrid = seat_grid;
        }

        public override string ToString()
        {
            return $"{Id}: film {FilmId}, screen {ScreenId}, {StartsAt:yyyy-MM-dd HH:mm}, price {Price:0.00}";
        }
    }
}