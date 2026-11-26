# Timing results: sort and search (card 425)

Author: Irwin Chinathoo (Tester 2)
Card: 425 (AB#425), week 8 algorithms
Run: 26 Nov 2026, commit `cbba8d4` on `feature/irwin` (Kushal's merge sort and binary search from card 424, commit `59dd5ee`)
Test file: `tests/CinemaBooking.Tests/SortPerformanceTests.cs` (14 tests, all pass)
Screenshot: `docs/testing/screenshots/425-sort-performance-green.png`

## How the times were measured

- Each test makes n films with random 8-letter titles. The random seed is fixed (42), so every run uses the same films.
- Each piece of work runs once to warm up (not counted), then 5 more times. The table shows the average of those 5 runs.
- One search is too quick to measure, so each search time is for **100 searches** together.
- Binary search runs on a list already sorted by merge sort. The sort is not part of the binary search time.
- Bubble sort is not in the program. It is a local helper in the test file, used only as a slow baseline to compare with merge sort.
- The timing tests use `[DoNotParallelize]`, so no other test shares the processor while they run.
- Machine: Irwin's laptop (ASUS ROG Zephyrus G16), Debug build, `dotnet test`. Times will be different on another PC, but the growth from one size to the next should be similar.

Command used:

```
dotnet test --filter "FullyQualifiedName~SortPerformanceTests" --logger "console;verbosity=detailed"
```

## Results (average milliseconds)

| Operation | Big O | n = 1,000 | n = 10,000 | n = 100,000 |
|---|---|---|---|---|
| Merge sort by title | O(n log n) | 0.6101 | 9.6715 | 119.6030 |
| Bubble sort by title (baseline) | O(n squared) | 13.6700 | 1701.3211 | not run (would take about 3 minutes per sort) |
| Linear search, 100 searches (FilmService.SearchByTitle) | O(n) each | 10.6273 | 77.6784 | 876.5503 |
| Binary search, 100 searches (SortHelper.BinarySearch) | O(log n) each | 0.0342 | 0.0261 | 0.0387 |
| Hash table lookup by id, 100 lookups | O(1) each | 0.0188 | 0.0203 | 0.0074 |

## What the numbers show

| Operation | 1,000 to 10,000 | 10,000 to 100,000 | Expected when n is 10 times bigger |
|---|---|---|---|
| Merge sort | 15.9 times slower | 12.4 times slower | about 12.5 to 13.3 times (10 times n, times a slightly bigger log n) |
| Bubble sort | 124 times slower | not run | about 100 times (10 squared) |
| Linear search | 7.3 times slower | 11.3 times slower | about 10 times |
| Binary search | no real change | no real change | very small rise (about 10, 13 and 17 steps per search) |
| Hash lookup | no real change | no real change | no change |

Big O check: when n grows 10 times, merge sort gets about 12 to 16 times slower, close to the 12.5 to 13.3 times that O(n log n) predicts, while bubble sort gets 124 times slower, close to the 100 times that O(n squared) predicts. At 10,000 films merge sort is about 176 times faster than bubble sort (9.67 ms against 1701 ms), which is why the program uses merge sort.

Searching: linear search grows about 10 times each time n grows 10 times, as O(n) predicts. Binary search and hash lookup stay around 0.01 to 0.04 ms for 100 searches at every size, so they do not grow in a way we can see. Binary search is O(log n), so it only needs about 17 steps for 100,000 films, and the hash lookup is O(1).

Limits of these numbers: the binary search and hash times are a few hundredths of a millisecond, which is close to what the stopwatch can measure, so small ups and downs (for example the hash lookup at 100,000 being quicker than at 1,000) are noise, not a real speed-up. Search times are for 100 searches, so one linear search through 100,000 films takes about 8.8 ms and one binary search takes well under 0.001 ms.
