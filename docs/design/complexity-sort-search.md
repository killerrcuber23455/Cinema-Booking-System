\# Complexity of sort and search



Owner: Kushal. Week 8. n is the number of films.



| Algorithm | Best time | Average time | Worst time | Extra space |

|---|---|---|---|---|

| Merge sort | O(n log n) | O(n log n) | O(n log n) | O(n) |

| Binary search | O(1) | O(log n) | O(log n) | O(1) |

| Linear contains search | O(1) | O(n) | O(n) | O(n) for the result array |

| Bubble sort (timing baseline only) | O(n) | O(n squared) | O(n squared) | O(1) |

| Hash table lookup (ids, emails) | O(1) | O(1) | O(n) | O(1) |



Binary search on a sorted array with equal titles returns the index of one of them, not always the first.



\## Why merge sort



Merge sort takes O(n log n) time in every case, so a film list in any starting order sorts in the same time. It is stable: films with equal keys (for example two films from the same year) keep their original order, so a second sort by year does not scramble an earlier sort by title. The price is O(n) extra space for the merged arrays. Bubble sort is O(n squared) and is used only as a timing baseline. Quicksort is usually fast but can fall to O(n squared) on bad input, which merge sort never does.



\## Why binary search needs a sorted array



Binary search halves the range each step, so 100,000 films take about 17 steps. It only works on an array sorted by title, so we sort a copy first with merge sort. Partial title search stays a linear search, because a partial text can match anywhere in a title, and a sorted order does not help.

