# Timing results: sort and search (card 425)

Author: Irwin Chinathoo (Tester 2)
Card: 425 (AB#425), week 8 algorithms
Run: 26 Nov 2026, commit `cbba8d4` on `feature/irwin` (Kushal's merge sort and binary search from card 424, commit `59dd5ee`)
Test file: `tests/CinemaBooking.Tests/SortPerformanceTests.cs` (14 tests, all pass in Debug and in Release)
Screenshot: `docs/testing/screenshots/425-sort-performance-green.png`

## How the times were measured

- Each test makes n films with random 8-letter titles. The random seed is fixed (42), so every run uses the same films.
- Each piece of work runs once to warm up (not counted), then 5 more times. The tables show the average of those 5 runs.
- One search is too quick to measure, so each search time is for **100 searches** together.
- Binary search runs on a list already sorted by merge sort. The sort is not part of the binary search time.
- Bubble sort is not in the program. It is a local helper in the test file, used only as a slow baseline to compare with merge sort.
- The timing tests use `[DoNotParallelize]`, so no other test shares the processor while they run.
- The tests check that the answers are right (sorted, all found), not that they are fast, so they do not fail on a slower PC.
- Machine: Irwin's laptop (ASUS ROG Zephyrus G16). Run once in a Debug build and once in a Release build (optimised, like the finished program). Times will be different on another PC, but the growth from one size to the next should be similar.

Commands used:

```
dotnet test --filter "FullyQualifiedName~SortPerformanceTests" --logger "console;verbosity=detailed"
dotnet test -c Release --filter "FullyQualifiedName~SortPerformanceTests" --logger "console;verbosity=detailed"
```

## Results (average milliseconds, Debug / Release)

| Operation | Big O | n = 1,000 | n = 10,000 | n = 100,000 |
|---|---|---|---|---|
| Merge sort by title | O(n log n) | 0.6101 / 1.0848 | 9.6715 / 14.6282 | 119.6030 / 98.7994 |
| Bubble sort by title (baseline) | O(n squared) | 13.6700 / 9.5679 | 1701.3211 / 1113.7156 | not run (would take about 2 to 3 minutes per sort) |
| Linear search, 100 searches (FilmService.SearchByTitle) | O(n) each | 10.6273 / 10.6699 | 77.6784 / 49.3607 | 876.5503 / 789.1384 |
| Binary search, 100 searches (SortHelper.BinarySearch) | O(log n) each | 0.0342 / 0.0299 | 0.0261 / 0.0243 | 0.0387 / 0.1079 |
| Hash table lookup by id, 100 lookups | O(1) each | 0.0188 / 0.0175 | 0.0203 / 0.0152 | 0.0074 / 0.0121 |

## What the numbers show

How many times slower each step is (Debug / Release):

| Operation | 1,000 to 10,000 | 10,000 to 100,000 | 1,000 to 100,000 | Expected |
|---|---|---|---|---|
| Merge sort | 15.9 / 13.5 | 12.4 / 6.8 | 196 / 91 | about 13 per step, about 167 from 1,000 to 100,000 |
| Bubble sort | 124 / 116 | not run | not run | about 100 per step |
| Linear search | 7.3 / 4.6 | 11.3 / 16.0 | 82 / 74 | about 10 per step, 100 from 1,000 to 100,000 |
| Binary search | flat | flat | flat | very small rise (about 10, 13 and 17 steps per search) |
| Hash lookup | flat | flat | flat | no change |

Big O check: when n grows 10 times, merge sort gets about 13 times slower on average and bubble sort gets 116 to 124 times slower, close to the 13 times that O(n log n) predicts and the 100 times that O(n squared) predicts. At 10,000 films merge sort is about 76 to 176 times faster than bubble sort, which is why the program uses merge sort.

Searching: linear search grows by roughly 10 times each step (O(n)), while binary search and hash lookup stay at a few hundredths of a millisecond for 100 searches at every size, as O(log n) and O(1) predict. Binary search needs only about 17 steps for 100,000 films.

Why single steps do not match exactly: merge sort grew 15.9 times from 1,000 to 10,000 films in Debug, more than the 13 times O(n log n) predicts. The Debug build (not optimised), the garbage collector (merge sort makes new arrays, so it sometimes stops to clean up memory) and warm-up (the processor speeding up and the caches filling) can explain the difference. The same causes make some Release steps lower than predicted (for example merge sort 6.8 times from 10,000 to 100,000), so the trend over all three sizes matters more than one step.

Limits of these numbers: the binary search and hash times are a few hundredths of a millisecond, close to what the stopwatch can measure, so their small ups and downs (for example binary search at 0.1079 ms at 100,000 in Release, or the hash lookup at 100,000 being quicker than at 1,000) are noise, not a real change. Search times are for 100 searches, so one linear search through 100,000 films takes about 8 to 9 ms and one binary search takes about 0.001 ms or less.
