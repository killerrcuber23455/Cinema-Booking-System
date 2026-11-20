# Test plan: data side

Author: Irwin Chinathoo (Tester 2)
Card: 354 (AB#354), data-side section for Akilesh to merge into `test-plan.md`
Status: draft 6, 20 Nov 2026. Every table now uses the new columns. Cards 356, 409, 361 and 432 run and passing. Section 11 has the extra test types (card 418) and the summary of the whole file.

This section covers Kushal's code: our own hash tables and linked list, the seat grid, search, the file loader and the database. All tables use the columns agreed on 18 Nov: Test ID, Test type, Mode, Area, What is tested, Preconditions, Test data, Post-conditions, Expected result, Actual result, Pass/Fail, Executed date, Tester, Bug ID. Rows that cannot run yet say why in Actual result and are marked Not run.

Default sizes used below come from the AI context file (section 6 and 8): seats are rows A to H and seats 1 to 12 (96 seats), and the hash table grows at about 75% full.

## 1. Hash table (films by ID, card 356)

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| HASH-01 | Boundary/negative | Automatic | Hash table | Empty table | Empty HashTable<int, Film> (16 slots) | New table | - | Count is 0, list all is empty | Count 0, list all empty | Pass | 15 Nov 2026 | Irwin | - |
| HASH-02 | Boundary/negative | Automatic | Hash table | Find on an empty table | Empty HashTable<int, Film> (16 slots) | Find ID 1 | - | Not found, no crash | Returns null, no crash | Pass | 15 Nov 2026 | Irwin | - |
| HASH-03 | White-box | Automatic | Hash table | Add then find | Empty HashTable<int, Film> (16 slots) | Add film ID 1, find ID 1 | - | Same film comes back | Same film returned | Pass | 15 Nov 2026 | Irwin | - |
| HASH-04 | White-box | Automatic | Hash table | Count goes up | Empty HashTable<int, Film> (16 slots) | Add 3 films | - | Count is 3 | Count is 3 | Pass | 15 Nov 2026 | Irwin | - |
| HASH-05 | Boundary/negative | Automatic | Hash table | Find a missing ID | Empty HashTable<int, Film> (16 slots) | Table has IDs 1 to 3, find ID 99 | - | Not found, no crash | Returns null, no crash | Pass | 15 Nov 2026 | Irwin | - |
| HASH-06 | White-box | Automatic | Hash table | Two films in the same slot | Empty HashTable<int, Film> (16 slots) | Two IDs that land in the same slot (picked from Kushal's hash function) | - | Both can be found, count is 2 | Both found, count is 2 | Pass | 15 Nov 2026 | Irwin | - |
| HASH-07 | White-box | Automatic | Hash table | Remove the first item in a chain | Empty HashTable<int, Film> (16 slots) | Three films in one slot, remove the first | - | Other two still found, count goes down by 1 | Other two found, count is 2 | Pass | 15 Nov 2026 | Irwin | - |
| HASH-08 | White-box | Automatic | Hash table | Remove from the middle of a chain | Empty HashTable<int, Film> (16 slots) | Three films in one slot, remove the middle one | - | Other two still found | Other two found | Pass | 15 Nov 2026 | Irwin | - |
| HASH-09 | White-box | Automatic | Hash table | Remove the last item in a chain | Empty HashTable<int, Film> (16 slots) | Three films in one slot, remove the last | - | Other two still found | Other two found | Pass | 15 Nov 2026 | Irwin | - |
| HASH-10 | Boundary/negative | Automatic | Hash table | Remove a missing ID | Empty HashTable<int, Film> (16 slots) | Remove ID 99 | - | Refused or returns false, count unchanged, no crash | Returns false, count still 1 | Pass | 15 Nov 2026 | Irwin | - |
| HASH-11 | White-box | Automatic | Hash table | Remove then find | Empty HashTable<int, Film> (16 slots) | Add ID 1, remove ID 1, find ID 1 | - | Not found | Removed film not found (checked in the three remove tests) | Pass | 15 Nov 2026 | Irwin | - |
| HASH-12 | Boundary/negative | Automatic | Hash table | Add the same ID twice | Empty HashTable<int, Film> (16 slots) | Add ID 1 twice | - | Second is refused (Add returns false), first film kept | Second add refused, first film kept, count 1 | Pass | 15 Nov 2026 | Irwin | - |
| HASH-13 | White-box | Automatic | Hash table | List all | Empty HashTable<int, Film> (16 slots) | Add 5 films | - | List all returns exactly those 5, no repeats | 5 films, each ID once | Pass | 15 Nov 2026 | Irwin | - |
| HASH-14 | White-box | Automatic | Hash table | List all after remove | Empty HashTable<int, Film> (16 slots) | Add 5, remove 2 | - | List all returns the other 3 | 3 films left, 2 and 4 not in the list | Pass | 15 Nov 2026 | Irwin | - |
| HASH-15 | Boundary/negative | Automatic | Hash table | Negative ID | Empty HashTable<int, Film> (16 slots) | Add film ID -1 | - | No crash, film can be found. (Refusing a negative film ID is the Film class's job: IsValidId, tested by Akilesh) | Added and found, no crash. The old FilmHashTable crashed here; Kushal's new table moves a negative slot back into range | Pass | 15 Nov 2026 | Irwin | BUG-04 (closed) |

### Hash table tests in code (card 356)

Run on 15 Nov 2026 against Kushal's `HashTable<int, Film>` (branch `feature/kushal`) with Shankar's `Film` from `CinemaBooking.Models`. File: `tests/CinemaBooking.Tests/HashTableTests.cs`. Result: 14 tests, 14 passed.

History: on 12 Nov the old `FilmHashTable` crashed on a negative ID (HASH-15). Kushal replaced it with the generic table, and the test now passes.

| Row | Test method |
|---|---|
| HASH-01 | NewTable_IsEmpty_CountIsZero |
| HASH-02 | Find_EmptyTable_ReturnsNull |
| HASH-03 | Add_ThenFind_ReturnsSameFilm |
| HASH-04 | Add_ThreeFilms_CountIsThree |
| HASH-05 | Find_MissingId_ReturnsNull |
| HASH-06 | Add_TwoIdsInSameSlot_BothFound |
| HASH-07 | Remove_FirstInChain_OthersStillFound |
| HASH-08 | Remove_MiddleOfChain_OthersStillFound |
| HASH-09 | Remove_LastInChain_OthersStillFound |
| HASH-10 | Remove_MissingId_ReturnsFalse |
| HASH-12 | Add_SameIdTwice_SecondIsRefused |
| HASH-13 | ListAll_FiveFilms_ReturnsAllFive |
| HASH-14 | ListAll_AfterRemovingTwo_ReturnsOtherThree |
| HASH-15 | Add_NegativeId_DoesNotCrash |

## 2. Resize (card 409)

MSTest file: `tests/CinemaBooking.Tests/ResizeSearchTests.cs`. The table starts with 16 slots and doubles when it is more than 75% full. The slot count is private, so the tests check the result of a resize: every film is still found and the count is right.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| RES-01 | White-box | Automatic | Resize | One resize keeps every film (method Add_ThirteenFilms_AllFoundAfterOneResize) | Empty table, 16 slots | Films 1 to 13 | Table has grown to 32 slots | Count 13, all 13 found | Count 13, all found | Pass | 20 Nov 2026 | Irwin | - |
| RES-03 | White-box | Automatic | Resize | Large load (Add_ThousandFilms_AllFound) | Empty table, 16 slots | Films 1 to 1000 | Table has grown to 2048 slots | Count 1000, all 1000 found | Count 1000, all found | Pass | 20 Nov 2026 | Irwin | - |
| RES-04 | White-box | Automatic | Resize | No copies after resize (ListAll_ThousandFilms_EachIdOnce) | 1000 films added | List all | No change | 1000 films, each ID once | 1000 films, each once | Pass | 20 Nov 2026 | Irwin | - |
| RES-05 | White-box | Automatic | Resize | Remove after resize (Remove_HalfOfThousand_OtherHalfStillFound) | 1000 films added | Remove every even ID | 500 films left | Count 500, odd IDs found, even IDs not found | As expected | Pass | 20 Nov 2026 | Irwin | - |
| RES-06 | Boundary/negative | Automatic | Resize | The 75% line: 12 films is exactly 75%, the 13th triggers the resize (Add_TwelveThenThirteen_CountStaysRight) | Empty table, 16 slots | Films 1 to 12, then film 13 | Table has grown | Count 12, then 13, film 13 found | As expected | Pass | 20 Nov 2026 | Irwin | - |
| RES-07 | White-box | Automatic | Resize | Add after resize (Add_AfterResize_NewFilmFound) | 100 films added | Film 101 | 101 films | Added and found, count 101 | As expected | Pass | 20 Nov 2026 | Irwin | - |
| RES-08 | Boundary/negative | Automatic | Resize | Same ID after resize is refused (Add_SameIdAfterResize_IsRefused) | 100 films added | Film 50 again | No change | Refused, count stays 100 | As expected | Pass | 20 Nov 2026 | Irwin | - |
| RES-09 | White-box | Automatic | Resize | Remove everything after resize (Remove_AllThousand_TableIsEmpty) | 1000 films added | Remove IDs 1 to 1000 | Table empty | Count 0, list all empty | As expected | Pass | 20 Nov 2026 | Irwin | - |
| PERF-01 | Performance | Automatic | Resize | Add 1000 films and find each one (AddAndFind_ThousandFilms_UnderOneSecond) | Empty table, 16 slots | Films 1 to 1000 | 1000 films | Under 1 second (safety limit) | 1 ms | Pass | 20 Nov 2026 | Irwin | - |

RES-02 (nothing lost after one resize) is covered by RES-01.

## 3. Showings table and members by email (card 361)

MSTest file: `tests/CinemaBooking.Tests/SeatGridTests.cs` (same file as the seat grid). The tests cover every branch of `ShowingTable` and `MemberTable`. SHOWDS-02 is now "each showing has its own grid"; the old "showings for one film" row was dropped because `ShowingTable` has no such method (the services do that).

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| SHOWDS-01 | White-box | Automatic | Showings | Create then find (CreateShowing_ThenFind_ReturnsShowing) | Empty ShowingTable | Showing 10, 8 x 12 | 1 showing | Found, empty grid | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SHOWDS-02 | White-box | Automatic | Showings | Each showing has its own grid (BookSeat_InOneShowing_OtherShowingStillFree) | Showings 10 and 11 | Book C5 in 10 | C5 taken in 10 only | C5 taken in 10, free in 11 | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SHOWDS-03 | Boundary/negative | Automatic | Showings | Missing showing (FindShowing_MissingId_ReturnsNull) | Empty ShowingTable | ID 99 | No change | Null showing and null grid | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SHOWDS-04 | Boundary/negative | Automatic | Showings | Same ID twice (CreateShowing_SameIdTwice_SecondRefused) | Showing 10 exists | Showing 10 again | Still 1 showing | False, count 1 | As expected | Pass | 20 Nov 2026 | Irwin | - |
| MEMB-01 | White-box | Automatic | Members | Find by email (AddMember_ThenFindByEmail_ReturnsMember) | Empty MemberTable | priya@example.com | 1 member | Member 1 found | As expected | Pass | 20 Nov 2026 | Irwin | - |
| MEMB-02 | White-box | Automatic | Members | Capitals and spaces ignored (FindByEmail_OtherCaseAndSpaces_FindsMember) | priya@example.com added | "  PRIYA@Example.com " | No change | Found (the table trims and lowercases) | Found | Pass | 20 Nov 2026 | Irwin | - |
| MEMB-03 | Boundary/negative | Automatic | Members | Email not in the table (FindByEmail_Missing_ReturnsNull) | priya@example.com added | nobody@example.com | No change | Null | Null | Pass | 20 Nov 2026 | Irwin | - |
| MEMB-04 | Boundary/negative | Automatic | Members | Same email, other capitals (AddMember_SameEmailOtherCase_SecondRefused) | priya@example.com added | PRIYA@example.com | Still 1 member | False, count 1 | As expected | Pass | 20 Nov 2026 | Irwin | - |
| MEMB-05 | Boundary/negative | Automatic | Members | Empty email (FindByEmail_EmptyText_ReturnsNull) | priya@example.com added | "" | No change | Null, no crash | Null | Pass | 20 Nov 2026 | Irwin | - |
| MEMB-06 | Boundary/negative | Automatic | Members | Adding nothing (AddMember_Null_IsRefused) | Empty MemberTable | null | No change | False, count 0 | As expected | Pass | 20 Nov 2026 | Irwin | - |

## 4. Seat grid (one per showing, card 361)

MSTest file: `tests/CinemaBooking.Tests/SeatGridTests.cs`. The grid is a plain 8 x 12 bool array. The tests cover every branch of `SeatGrid`: free and taken seats, all or nothing booking and freeing, codes outside the grid and badly written codes.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| SEAT-01 | White-box | Automatic | Seat grid | New grid is all free (NewGrid_AllSeatsFree) | Empty 8 x 12 SeatGrid | - | No change | No taken seats, A1 and H12 free | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-02 | White-box | Automatic | Seat grid | Book one seat (BookSeats_OneSeat_SeatIsTaken) | Empty 8 x 12 SeatGrid | C5 | C5 taken | True, C5 taken, 1 taken seat | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-03 | White-box | Automatic | Seat grid | Book two seats (BookSeats_TwoSeats_BothTaken) | Empty 8 x 12 SeatGrid | C5, C6 | Both taken | True, both taken | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-04 | Boundary/negative | Automatic | Seat grid | Taken seat refused (BookSeats_TakenSeat_IsRefused) | C5 taken | C5 | No change | False, still 1 taken seat | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-05 | Boundary/negative | Automatic | Seat grid | All or nothing (BookSeats_OneTakenInGroup_NoneBooked) | C6 taken | C5, C6 | No change | False, C5 still free | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-06 | White-box | Automatic | Seat grid | Free a taken seat (FreeSeats_TakenSeat_IsFreeAgain) | C5 taken | Free C5 | C5 free | True, C5 free | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-07 | Boundary/negative | Automatic | Seat grid | Free a seat that is already free (FreeSeats_AlreadyFree_NoChange) | Empty 8 x 12 SeatGrid | Free D1 | No change | True (no change), D1 free | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-08 | Boundary/negative | Automatic | Seat grid | Corner seats (BookSeats_FourCorners_AllTaken) | Empty 8 x 12 SeatGrid | A1, A12, H1, H12 | 4 taken | True, 4 taken | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-09 | Boundary/negative | Automatic | Seat grid | Row outside the grid (BookSeats_RowOutsideGrid_IsRefused) | Empty 8 x 12 SeatGrid | I1 | No change | False | False | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-10 | Boundary/negative | Automatic | Seat grid | Seat number outside the grid (BookSeats_SeatNumberOutsideGrid_IsRefused, 2 inputs) | Empty 8 x 12 SeatGrid | A13, A0 | No change | False for both | False for both | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-11 | Boundary/negative | Automatic | Seat grid | Bad seat codes (BookSeats_BadCode_IsRefused, 4 inputs) | Empty 8 x 12 SeatGrid | "", "5C", "ZZ", "C" | No change | False, no crash | False for all 4 | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-12 | Boundary/negative | Automatic | Seat grid | Lower case code (BookSeats_LowerCaseCode_IsRefused) | Empty 8 x 12 SeatGrid | "c5" | No change | False (only capital row letters, answered from the code) | False | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-13 | Boundary/negative | Automatic | Seat grid | Full grid (BookSeats_FullGrid_NextSeatRefused) | Empty 8 x 12 SeatGrid | All 96 seats, then D6 | 96 taken | 96 taken, D6 refused | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-14 | White-box | Automatic | Seat grid | Taken list in order (ListTakenSeats_ThreeBooked_ReturnsThemInOrder) | Empty 8 x 12 SeatGrid | H12, B2, C5 | 3 taken | B2, C5, H12 in that order | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-15 | Boundary/negative | Automatic | Seat grid | Same seat twice in one request (BookSeats_SameSeatTwiceInRequest_IsRefused) | Empty 8 x 12 SeatGrid | C5, C5 | No change | False, C5 still free | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-16 | Boundary/negative | Automatic | Seat grid | Empty list (BookSeats_EmptyList_IsRefused) | Empty 8 x 12 SeatGrid | No seats | No change | False | False | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-17 | Boundary/negative | Automatic | Seat grid | Free with one bad code (FreeSeats_OneBadCode_NoneFreed) | C5 taken | C5, Z99 | No change | False, C5 still taken | As expected | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-18 | Boundary/negative | Automatic | Seat grid | Free an empty list (FreeSeats_EmptyList_IsRefused) | Empty 8 x 12 SeatGrid | No seats | No change | False | False | Pass | 20 Nov 2026 | Irwin | - |
| SEAT-19 | Boundary/negative | Automatic | Seat grid | Is a bad code free (IsSeatFree_BadCode_ReturnsFalse, 3 inputs) | Empty 8 x 12 SeatGrid | "", I1, A13 | No change | False, no crash | False for all 3 | Pass | 20 Nov 2026 | Irwin | - |
| DB-06b | Integration | Automatic | Database | Same seat booked twice, real database (card 361) | Local CinemaBookingTest database with showing 10 | C5 for showing 10, twice | 1 booking | Second save returns -1 (unique key uq_booked_seats_showing_seat) | Not run yet: waits for the CinemaBookingTest database. Covered in memory by SEAT-04 and in the fake by FAKE-08 | Not run | - | Irwin | - |

Summary (card 361, run on 20 Nov 2026): 29 test methods (35 runs, because SEAT-10, SEAT-11 and SEAT-19 have several inputs), all automatic. White-box 9, Boundary/negative 20. Passed 29, failed 0. DB-06b (real database) not run yet. Pass rate 100% of what was run. Open bugs: none. Screenshot: `361-seat-grid-tests-green.png`.

## 5. Search (card 409)

Search is in Shankar's `FilmService` (card 305). It checks every film, because the hash table can only look up by ID.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| SRCH-01 | White-box | Automatic | Search | Full title (SearchByTitle_FullTitle_FindsOneFilm) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "Inception" | No change | Inception only | Inception only | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-02 | White-box | Automatic | Search | Part of a title (SearchByTitle_PartOfTitle_FindsFilm) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "incep" | No change | Inception | Inception | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-03 | White-box | Automatic | Search | Different case (SearchByTitle_DifferentCase_FindsFilm) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "INCEPTION" | No change | Inception | Inception | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-04 | White-box | Automatic | Search | Text inside the title (SearchByTitle_TextInsideTitle_FindsFilm) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "part" | No change | Dune: Part Two only | Dune: Part Two only | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-05 | White-box | Automatic | Search | Several matches (SearchByTitle_SeveralMatches_FindsAll) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "the" | No change | The Dark Knight and The Lion King | Both, no repeats | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-06 | Boundary/negative | Automatic | Search | No match (SearchByTitle_NoMatch_ReturnsEmpty) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "zzzz" | No change | Empty result, no crash | Empty result | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-07 | Boundary/negative | Automatic | Search | Empty search text (SearchByTitle_EmptyText_ReturnsAllFilms) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "" | No change | All 6 films (every title contains empty text) | All 6 films | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-08 | White-box | Automatic | Search | By genre (SearchByGenre_SciFi_FindsBothSciFiFilms) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "Sci-Fi" | No change | Inception and Dune: Part Two | Both | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-09 | Boundary/negative | Automatic | Search | Genre with no films (SearchByGenre_NoFilms_ReturnsEmpty) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "Western" | No change | Empty result, no crash | Empty result | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-10 | Security | Automatic | Search | Apostrophe in the search text (SearchByTitle_Apostrophe_FindsFilm) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "Ocean's" | No change | Ocean's Eleven, no crash | Ocean's Eleven | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-11 | White-box | Automatic | Search | Genre in lower case (SearchByGenre_LowerCase_FindsFilms) | FilmService with 6 films: Inception, Dune: Part Two (Sci-Fi); The Dark Knight (Action); The Lion King (Animation); Ocean's Eleven (Crime); Paddington (Family) | "sci-fi" | No change | 2 films | 2 films | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-12 | White-box | Automatic | Search | Title search after resize (SearchByTitle_AfterResize_FindsFilm) | 1000 plain films plus Inception (ID 1001) | "incep" | No change | Inception only | Inception only | Pass | 20 Nov 2026 | Irwin | - |
| SRCH-13 | White-box | Automatic | Search | Genre search after resize (SearchByGenre_AfterResize_FindsAllThousand) | 1000 Drama films | "drama" | No change | All 1000 films | 1000 films | Pass | 20 Nov 2026 | Irwin | - |

Summary (card 409, run on 20 Nov 2026):

| Test type | Tests | Passed | Failed | Not run |
|---|---|---|---|---|
| White-box | 15 | 15 | 0 | 0 |
| Boundary/negative | 5 | 5 | 0 | 0 |
| Security | 1 | 1 | 0 | 0 |
| Performance | 1 | 1 | 0 | 0 |
| **Total** | **22** | **22** | **0** | **0** |

All 22 are automatic (MSTest). Pass rate 100%. Open bugs: none. Screenshot: `docs/testing/screenshots/409-resize-search-tests-green.png`.

## 6. File loader

Sample files are kept in the test folder. The format is the CSV from the AI context file: `id,title,genre,year,duration_minutes,age_rating,description,poster_file`.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| LOAD-01 | White-box | Automatic | File loader | Good file | FileLoader, a CSV file the test makes | 10 good lines | - | Loaded 10, skipped 0 | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-02 | White-box | Automatic | File loader | Some bad lines | FileLoader, a CSV file the test makes | `data/films_sample.csv`: 10 good lines, 2 bad (letters in the year, extra commas) | - | Loaded 10, skipped 2 | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-03 | Boundary/negative | Automatic | File loader | Letters in a number column | FileLoader, a CSV file the test makes | Year "20x0" | - | That line skipped, others loaded | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-04 | Boundary/negative | Automatic | File loader | Missing columns | FileLoader, a CSV file the test makes | A line with only 3 values | - | That line skipped | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-05 | Boundary/negative | Automatic | File loader | Blank line | FileLoader, a CSV file the test makes | An empty line in the middle | - | Skipped or ignored, no crash | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-06 | Boundary/negative | Automatic | File loader | Header only | FileLoader, a CSV file the test makes | File with the header and no films | - | Loaded 0, skipped 0, no crash | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-07 | Boundary/negative | Automatic | File loader | Empty file | FileLoader, a CSV file the test makes | 0 bytes | - | Loaded 0, skipped 0, no crash (the window should then say no films were loaded) | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-08 | Boundary/negative | Automatic | File loader | File does not exist | FileLoader, a CSV file the test makes | Name of a missing file | - | Loader throws FileNotFoundException; the window catches it and shows a clear message, no crash | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-09 | Boundary/negative | Automatic | File loader | Same ID twice in the file | FileLoader, a CSV file the test makes | Two lines with ID 5 | - | Second one skipped and counted (confirmed: Kushal's loader refuses a duplicate ID) | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-10 | Boundary/negative | Automatic | File loader | Comma inside the description | FileLoader, a CSV file the test makes | Description with a comma | - | Line skipped and counted (decided: descriptions cannot contain commas) | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |
| LOAD-11 | Boundary/negative | Automatic | File loader | Very long title | FileLoader, a CSV file the test makes | 500-character title | - | Skipped or cut, no crash | Not run yet: FileLoaderTests.cs is the next file to write | Not run | - | Irwin | - |

## 7. Database

All database tests use a separate test database, never the real one.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| DB-01 | Integration | Automatic | Database | Script builds from nothing | Local CinemaBookingTest database built from cinema.sql | Run `cinema.sql` on an empty database | - | All tables created, no errors | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-02 | Integration | Automatic | Database | Save then read back a film | Local CinemaBookingTest database built from cinema.sql | Save Inception, load all | - | Same film, every field equal | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-03 | Integration | Automatic | Database | Delete a film | Local CinemaBookingTest database built from cinema.sql | Delete Inception, load all | - | Inception is gone | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-04 | Integration | Automatic | Database | Save and read back a showing | Local CinemaBookingTest database built from cinema.sql | Save a showing, load it | - | Same film, screen, time and price | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-05 | Integration | Automatic | Database | Save and read back a member | Local CinemaBookingTest database built from cinema.sql | Save a member with tier and points | - | Same tier and points | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-06 | Integration | Automatic | Database | Same seat booked twice | Local CinemaBookingTest database built from cinema.sql | Save C5 for showing 10, then save C5 for showing 10 again | - | Second save refused by the unique key `uq_booked_seats_showing_seat` (showing_id, seat_code), plain message, no crash | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-07 | Integration | Automatic | Database | Same seat, different showing | Local CinemaBookingTest database built from cinema.sql | Save C5 for showing 10 and C5 for showing 11 | - | Both saved | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-08 | Integration | Automatic | Database | Guest booking | Local CinemaBookingTest database built from cinema.sql | Booking with no member (member ID empty) | - | Saved, read back with member ID still empty | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-09 | Integration | Automatic | Database | Booking total | Local CinemaBookingTest database built from cinema.sql | Booking with total 475.50 | - | Reads back as 475.50 exactly | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-10 | Security | Automatic | Database | SQL injection text | Local CinemaBookingTest database built from cinema.sql | Title `x'); DROP TABLE films;--` | - | Stored and read back as plain text, films table still there | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-11 | Security | Automatic | Database | Apostrophe in text | Local CinemaBookingTest database built from cinema.sql | Title "Ocean's Eleven" | - | Saved and read back correctly | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-12 | Integration | Automatic | Database | Database down | Local CinemaBookingTest database built from cinema.sql | Stop SQL Server, then load | - | Clear message, no crash | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| DB-13 | Integration | Automatic | Database | Restart keeps data | Local CinemaBookingTest database built from cinema.sql | Save, close, reopen, load | - | Data is still there | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |

## 8. Users and audit log (card 337)

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| USER-01 | Integration | Automatic | Users | Save and read back a user | Local CinemaBookingTest database built from cinema.sql | User with username and role | - | Same username and role | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| USER-02 | Security | Automatic | Users | No plain password stored | Local CinemaBookingTest database built from cinema.sql | Save a user with password "abc123" | - | The password column holds a hash, not "abc123" | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| USER-03 | Integration | Automatic | Users | Update failed attempts | Local CinemaBookingTest database built from cinema.sql | Set failed attempts to 2 | - | Reads back as 2 | Not run yet: the users table has no failed_attempts or locked_until column, waits for Kushal's card 318 | Not run | - | Irwin | - |
| USER-04 | Integration | Automatic | Users | Locked-until saved | Local CinemaBookingTest database built from cinema.sql | Set a lock time | - | Reads back the same time | Not run yet: the users table has no failed_attempts or locked_until column, waits for Kushal's card 318 | Not run | - | Irwin | - |
| USER-05 | Security | Automatic | Users | Two users with the same username | Local CinemaBookingTest database built from cinema.sql | Save the same username twice | - | Second is refused by the unique key `uq_users_username` | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| AUD-01 | Integration | Automatic | Audit log | Write an audit row | Local CinemaBookingTest database built from cinema.sql | Write "admin added film 5" | - | Row saved with who, what and when | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |
| AUD-02 | Integration | Automatic | Audit log | Rows come back in order | Local CinemaBookingTest database built from cinema.sql | Write 3 rows | - | Read back oldest to newest (or newest first, TBC) | Not run yet: waits for the CinemaBookingTest database | Not run | - | Irwin | - |

## 9. Manual tests: full journeys (cards 363 and 364)

Run on the app from `main`. For each step, also try bad input: blank boxes, letters in number boxes, very long text. Log every problem as a Bug card with steps, expected and actual, and a screenshot.

**J1. Guest books seats**
1. Start the app. Click "Book without an account".
2. Pick a film, then a showing.
3. Click two free seats. Check that the price is right.
4. Try to click a taken seat. It should not be picked.
5. Leave name, email or phone empty and click Book. A clear message should appear.
6. Fill in the details and book. A booking reference should be shown.

**J2. Guest finds and cancels a booking**
1. Open "Find my booking". Type the reference and the email.
2. Try the right reference with the wrong email. It should be refused.
3. Cancel the booking and confirm Yes. The seats should be free again on the seat map.
4. Try to cancel the same booking again. It should be refused.

**J3. Guest signs up**
1. From the booking summary, click "Create account".
2. Sign up. The new member should be Standard with 0 points.
3. Try to sign up again with the same email. It should be refused.

**J4. Customer books and cancels**
1. Log in. Check the name, tier and points in the side menu.
2. Book seats. Check the discount for the tier and the points earned.
3. Use points on a booking. Check the money taken off (100 points = Rs 50).
4. Open "My tickets" and cancel. The points earned should be taken back and the points used returned.
5. Type a wrong password 3 times. The account should lock.

**J5. Staff books for a walk-in**
1. Log in as staff.
2. Book seats for a walk-in customer.
3. Register a new member, then edit them.
4. Cancel another person's booking.
5. Check that staff cannot see the Admin windows (films, showings, staff accounts).

**J6. Admin adds a showing**
1. Log in as admin.
2. Add a film with a poster. Remove the poster file, then reopen the film. A placeholder should show, with no crash.
3. Add a showing. Type letters in the price box. A clear message should appear.
4. Try two showings on the same screen at the same time. The second must be refused (decided 16 Nov; the database also has the unique key on screen_id, starts_at).
5. Load a film file with some bad lines. A message like "Loaded 8, skipped 2" should appear.

## 10. Fake database (card 432)

MSTest files: `tests/CinemaBooking.Tests/Fakes/FakeDatabase.cs` and `tests/CinemaBooking.Tests/FakeDatabaseTests.cs`. The fake implements `IDatabase` and keeps everything in memory with our own `HashTable` and `MemberTable`, so service tests run without SQL Server. It copies the return values of the real `Database` class (for example -1 for a taken seat, false for a double cancel) and has no business rules. Written test first: red commit, then green.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| FAKE-01 | White-box | Automatic | Films | Save and load a film (LoadFilms_AfterAddFilm_FilmIsLoaded) | Empty FakeDatabase | Inception, ID 1 | Film in the table | Loaded 1, Inception found | Loaded 1, found | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-02 | White-box | Automatic | Members | New member gets an ID (SaveMember_NewEmail_ReturnsNewId) | Empty FakeDatabase | ann@mail.com | 1 member | ID above 0 | ID 1 | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-03 | Boundary/negative | Automatic | Members | Same email, other case and spaces (SaveMember_SameEmailOtherCaseAndSpaces_ReturnsMinusOne) | ann@mail.com saved | " ANN@mail.com " | Still 1 member | -1 | -1 | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-04 | White-box | Automatic | Members | Saved member is listed (GetAllMembers_AfterSave_HasMemberWithId) | ann@mail.com saved | GetAllMembers | No change | 1 member with the new ID | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-05 | White-box | Automatic | Members | Points and tier saved (UpdateMemberPoints_ExistingMember_SavesPoints) | ann@mail.com saved | 120 points, Silver | Member has 120 points, Silver | True, values saved | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-06 | Boundary/negative | Automatic | Members | Update a missing member (UpdateMemberPoints_MissingMember_ReturnsFalse) | Empty FakeDatabase | Member ID 99 | No change | False | False | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-07 | White-box | Automatic | Bookings | Save on free seats (SaveBooking_FreeSeats_ReturnsIdAndReferenceExists) | Empty FakeDatabase | CB-AAAAAA, showing 1, C5 and C6 | Booking saved | ID above 0, reference exists | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-08 | Boundary/negative | Automatic | Bookings | Taken seat refused, all or nothing (SaveBooking_TakenSeat_ReturnsMinusOne) | C5 booked for showing 1 | CB-BBBBBB, C4 and C5 | No change | -1, CB-BBBBBB not saved, C4 still free | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-09 | White-box | Automatic | Bookings | Same seat, other showing (SaveBooking_SameSeatOtherShowing_IsSaved) | C5 booked for showing 1 | C5 for showing 2 | 2 bookings | ID above 0 | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-10 | White-box | Automatic | Bookings | Taken seats after a save (GetTakenSeats_AfterSave_ReturnsSeats) | C5 and C6 booked for showing 1 | GetTakenSeats(1) | No change | C5 and C6 | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-11 | White-box | Automatic | Bookings | Cancel frees the seats (CancelBooking_Confirmed_FreesSeats) | C5 booked | Cancel, then book C5 again | Booking Cancelled | True, no taken seats, C5 can be booked again | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-12 | Boundary/negative | Automatic | Bookings | No double cancel (CancelBooking_Twice_SecondReturnsFalse) | Booking already cancelled | Cancel again | No change | False | False | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-13 | Boundary/negative | Automatic | Bookings | Cancel a missing booking (CancelBooking_MissingId_ReturnsFalse) | Empty FakeDatabase | Booking ID 99 | No change | False | False | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-14 | White-box | Automatic | Bookings | Find by reference (FindBooking_Saved_HasSeatsAndStatus) | CB-AAAAAA with C5 and C6 | "CB-AAAAAA" | No change | Booking with 2 seats, Confirmed | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-15 | Boundary/negative | Automatic | Bookings | Missing reference (FindBooking_MissingReference_ReturnsNull) | Empty FakeDatabase | "CB-ZZZZZZ" | No change | Null | Null | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-16 | White-box | Automatic | Bookings | Bookings per member (GetBookingsForMember_TwoMembers_OnlyTheirOwn) | Member 1 has 2 bookings, member 2 has 1 | Member 1 | No change | 2 bookings | 2 bookings | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-17 | Boundary/negative | Automatic | Showings | Same showing ID twice (SaveShowing_SameIdTwice_SecondReturnsFalse) | Showing 1 saved | Showing 1 again | Still 1 showing | False | False | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-18 | Boundary/negative | Automatic | Showings | Same screen, same time (SaveShowing_SameScreenSameTime_SecondReturnsFalse) | Showing 1 on screen 1 at 20:30 | Showing 2 on screen 1 at 20:30 | Still 1 showing | False | False | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-19 | White-box | Automatic | Showings | Load marks booked seats (LoadShowings_WithBooking_SeatIsTakenInGrid) | Showing 1 with C5 booked | LoadShowings | Table has showing 1 | 1 added, C5 taken, C6 free | As expected | Pass | 20 Nov 2026 | Irwin | - |
| FAKE-20 | Integration | Automatic | Service | BookingService runs with the fake, no SQL Server (BookingService_WithFake_GuestBookingIsSaved) | Showing 1 loaded into the table | Guest Kevin books C5 and C6 | 2 seats taken in the fake | True, booking returned, 2 seats taken | As expected | Pass | 20 Nov 2026 | Irwin | - |

Not run yet: the same 20 tests on the real `Database` with the local CinemaBookingTest database (Integration). They wait until the test database is set up. Known differences, by design: the fake does not sort `GetTakenSeats` or `GetBookingsForMember` (the real database does), and it does not check that the film and screen of a showing exist (the real database uses foreign keys).

Summary (card 432, run on 20 Nov 2026): 20 tests, all automatic. White-box 11, Boundary/negative 8, Integration 1. Passed 20, failed 0, not run 0 (plus the real-database run still to do). Pass rate 100%. Open bugs: none. Screenshots: `432-fake-database-red.png` and `432-fake-database-green.png`.

## 11. Extra test types (card 418) and summary

### Regression: the whole suite after every merge

| Date | What changed | Command | Result |
|---|---|---|---|
| 15 Nov 2026 | Merged Kushal's generic HashTable, moved the hash table tests to it | Test Explorer, Run All | 15 tests, 15 passed |
| 17 Nov 2026 | Merged `dev` (all branches) | `dotnet test CinemaBooking.slnx` | 161 tests, 161 passed |
| 20 Nov 2026 | Merged Kushal's IDatabase (card 413), added resize and search tests (409) | Test Explorer, Run All | 187 tests, 187 passed |
| 20 Nov 2026 | Added FakeDatabase (432), red step | Test Explorer, Run All | 207 tests, 187 passed, 20 failed on purpose (red) |
| 20 Nov 2026 | FakeDatabase green step | Test Explorer, Run All | 207 tests, 207 passed |
| 20 Nov 2026 | Added seat grid, showing and member tests (361) | Test Explorer, Run All | 242 tests, 242 passed |

Retest of HASH-15 (BUG-04): failed on 12 Nov on the old `FilmHashTable`, passed on 15 Nov on the new `HashTable<int, Film>` and in every run since.

### Performance

| Test ID | What | Measured | Note |
|---|---|---|---|
| PERF-01 | Add 1000 films and find each one | 1 ms | Section 2 |
| RES-03 | Add 1000 films (many resizes), find all | Part of the 22 resize and search tests, 31 to 100 ms for the whole class | Test Explorer duration |
| SEAT-13 | Book all 96 seats one by one | Part of the seat grid class, 1 ms for all 35 runs | Test Explorer duration |
| Not run yet | Database queries and a full booking on SQL Server | - | Waits for the CinemaBookingTest database |
| Not run yet | Sort and search at 1,000, 10,000 and 100,000 films | - | Card 425, waits for Kushal's sort code (424) |

### Security

| Test ID | What | Result |
|---|---|---|
| SRCH-10 | Apostrophe in the search text ("Ocean's") | Pass |
| DB-10 | SQL injection text stored as plain text | Not run, waits for the test database |
| DB-11 | Apostrophe saved and read back from SQL Server | Not run, waits for the test database |
| USER-02 | No plain password in the users table | Not run, waits for the test database |
| USER-05 | Same username refused | Not run, waits for the test database |
| USER-03, USER-04 | Lock-out columns | Not run, the columns are missing (Kushal's card 318) |
| Windows | SQL injection text typed in every text box | Not run, part of the hand tests (card 363) once the windows exist |

### Boundary and negative

Marked Boundary/negative in the tables above: empty tables, missing IDs and emails, the same ID or email twice, negative ID, the 75% resize line (12 then 13 films), seats outside the grid (I1, A13, A0), badly written seat codes, a full grid, empty lists, null, and the taken seat and double cancel cases on the fake.

### Summary of this file (20 Nov 2026)

| Test type | Tests | Passed | Failed | Not run |
|---|---|---|---|---|
| White-box | 46 | 44 | 0 | 2 |
| Boundary/negative | 48 | 39 | 0 | 9 |
| Integration | 18 | 1 | 0 | 17 |
| Security | 5 | 1 | 0 | 4 |
| Performance | 1 | 1 | 0 | 0 |
| **Total** | **118** | **86** | **0** | **32** |

Mode: 118 automatic, 0 manual (the journeys J1 to J6 in section 9 are manual and are run in Sprint 4). Pass rate of the tests that were run: 100%. Open bugs: none (BUG-04 is closed).
