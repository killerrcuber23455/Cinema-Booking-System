// week 8 sorting and searching on our own arrays (owner: kushal)
using CinemaBooking.Models;
using System;

namespace CinemaBooking.Algorithms
{
    public static class SortHelper
    {
        // merge sort: O(n log n) in every case and stable, so films with equal keys keep their order
        // returns a new sorted array and leaves the original alone
        // sort_by is "title", "year" or "rating"
        public static Film[] MergeSort(Film[] films, string sort_by)
        {
            if (films.Length <= 1)
            {
                return CopyRange(films, 0, films.Length);
            }

            int middle = films.Length / 2;
            Film[] left = CopyRange(films, 0, middle);
            Film[] right = CopyRange(films, middle, films.Length);

            Film[] sorted_left = MergeSort(left, sort_by);
            Film[] sorted_right = MergeSort(right, sort_by);

            return Merge(sorted_left, sorted_right, sort_by);
        }

        // binary search: O(log n) on an array already sorted by title
        // returns the index of the exact title (any case), or -1 if it is not there
        public static int BinarySearch(Film[] sorted_films, string title)
        {
            int low = 0;
            int high = sorted_films.Length - 1;

            while (low <= high)
            {
                int middle = (low + high) / 2;
                int result = string.Compare(title, sorted_films[middle].Title, StringComparison.OrdinalIgnoreCase);

                if (result == 0)
                {
                    return middle;
                }

                if (result < 0)
                {
                    high = middle - 1;
                }
                else
                {
                    low = middle + 1;
                }
            }

            return -1;
        }

        // copies films from start up to (not including) end into a new array
        private static Film[] CopyRange(Film[] films, int start, int end)
        {
            Film[] copy = new Film[end - start];

            for (int i = start; i < end; i++)
            {
                copy[i - start] = films[i];
            }

            return copy;
        }

        // joins two sorted arrays into one sorted array
        private static Film[] Merge(Film[] left, Film[] right, string sort_by)
        {
            Film[] result = new Film[left.Length + right.Length];
            int left_index = 0;
            int right_index = 0;
            int result_index = 0;

            while (left_index < left.Length && right_index < right.Length)
            {
                // we take from the left when equal, so equal films keep their order (stable)
                if (Compare(left[left_index], right[right_index], sort_by) <= 0)
                {
                    result[result_index] = left[left_index];
                    left_index++;
                }
                else
                {
                    result[result_index] = right[right_index];
                    right_index++;
                }

                result_index++;
            }

            while (left_index < left.Length)
            {
                result[result_index] = left[left_index];
                left_index++;
                result_index++;
            }

            while (right_index < right.Length)
            {
                result[result_index] = right[right_index];
                right_index++;
                result_index++;
            }

            return result;
        }

        // negative if film_a comes first, 0 if equal, positive if film_b comes first
        private static int Compare(Film film_a, Film film_b, string sort_by)
        {
            if (sort_by == "year")
            {
                return film_a.Year.CompareTo(film_b.Year);
            }

            if (sort_by == "rating")
            {
                return film_a.AgeRating.CompareTo(film_b.AgeRating);
            }

            return string.Compare(film_a.Title, film_b.Title, StringComparison.OrdinalIgnoreCase);
        }
    }
}