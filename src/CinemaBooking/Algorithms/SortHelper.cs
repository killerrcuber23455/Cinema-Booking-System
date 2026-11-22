// week 8 sorting and searching on our own arrays (owner: kushal)
using CinemaBooking.Models;

namespace CinemaBooking.Algorithms
{
    public static class SortHelper
    {
        // merge sort: O(n log n) in every case and stable, so films with equal keys keep their order
        // returns a new sorted array and leaves the original alone
        // sort_by is "title", "year" or "rating"
        public static Film[] MergeSort(Film[] films, string sort_by)
        {
            return films;
        }

        // binary search: O(log n) on an array already sorted by title
        // returns the index of the exact title (any case), or -1 if it is not there
        public static int BinarySearch(Film[] sorted_films, string title)
        {
            return -1;
        }
    }
}