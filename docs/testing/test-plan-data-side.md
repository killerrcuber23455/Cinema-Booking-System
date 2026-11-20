# Test plan: data side

Author: Irwin Chinathoo (Tester 2)
Card: 354 (AB#354), data-side section for Akilesh to merge into `test-plan.md`
Status: draft 5, 20 Nov 2026. Resize and search sections (card 409) use the new table columns and are run. File loader and database rows updated from Kushal's answers (cards 307 and 313). Hash table rows (card 356) all run and passing. Old card numbers replaced, showing-table IDs renamed to SHOWDS so they do not clash with Akilesh. Rows marked TBC wait for an answer from Kushal or Shankar.

This section covers Kushal's code: our own hash tables and linked list, the seat grid, search, the file loader and the database. It uses the same columns as the main plan. The last two columns stay empty until the tests are run.

Default sizes used below come from the AI context file (section 6 and 8): seats are rows A to H and seats 1 to 12 (96 seats), and the hash table grows at about 75% full.

## 1. Hash table (films by ID, card 356)

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| HASH-01 | Hash table | Empty table | New table | Count is 0, list all is empty | Count 0, list all empty | Pass |
| HASH-02 | Hash table | Find on an empty table | Find ID 1 | Not found, no crash | Returns null, no crash | Pass |
| HASH-03 | Hash table | Add then find | Add film ID 1, find ID 1 | Same film comes back | Same film returned | Pass |
| HASH-04 | Hash table | Count goes up | Add 3 films | Count is 3 | Count is 3 | Pass |
| HASH-05 | Hash table | Find a missing ID | Table has IDs 1 to 3, find ID 99 | Not found, no crash | Returns null, no crash | Pass |
| HASH-06 | Hash table | Two films in the same slot | Two IDs that land in the same slot (picked from Kushal's hash function) | Both can be found, count is 2 | Both found, count is 2 | Pass |
| HASH-07 | Hash table | Remove the first item in a chain | Three films in one slot, remove the first | Other two still found, count goes down by 1 | Other two found, count is 2 | Pass |
| HASH-08 | Hash table | Remove from the middle of a chain | Three films in one slot, remove the middle one | Other two still found | Other two found | Pass |
| HASH-09 | Hash table | Remove the last item in a chain | Three films in one slot, remove the last | Other two still found | Other two found | Pass |
| HASH-10 | Hash table | Remove a missing ID | Remove ID 99 | Refused or returns false, count unchanged, no crash | Returns false, count still 1 | Pass |
| HASH-11 | Hash table | Remove then find | Add ID 1, remove ID 1, find ID 1 | Not found | Removed film not found (checked in the three remove tests) | Pass |
| HASH-12 | Hash table | Add the same ID twice | Add ID 1 twice | Second is refused (Add returns false), first film kept | Second add refused, first film kept, count 1 | Pass |
| HASH-13 | Hash table | List all | Add 5 films | List all returns exactly those 5, no repeats | 5 films, each ID once | Pass |
| HASH-14 | Hash table | List all after remove | Add 5, remove 2 | List all returns the other 3 | 3 films left, 2 and 4 not in the list | Pass |
| HASH-15 | Hash table | Negative ID | Add film ID -1 | No crash, film can be found. (Refusing a negative film ID is the Film class's job: IsValidId, tested by Akilesh) | Added and found, no crash. The old FilmHashTable crashed here; Kushal's new table moves a negative slot back into range | Pass |

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

## 3. Other hash tables (showings by ID, members by email)

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| SHOWDS-01 | Showings | Add then find a showing | Add showing ID 10, find ID 10 | Same showing comes back | | |
| SHOWDS-02 | Showings | Showings for one film | Film 1 has 3 showings, film 2 has 1 | Asking for film 1 gives exactly its 3 showings | | |
| SHOWDS-03 | Showings | Film with no showings | Ask for showings of a film with none | Empty result, no crash | | |
| MEMB-01 | Members | Add then find by email | Add member "priya@example.com", find it | Same member comes back | | |
| MEMB-02 | Members | Email in a different case | Find "PRIYA@example.com" | TBC with Kushal: found (we suggest yes, email is not case sensitive) | | |
| MEMB-03 | Members | Email not in the table | Find "nobody@example.com" | Not found, no crash | | |
| MEMB-04 | Members | Two members with the same email | Add "priya@example.com" twice | Second is refused | | |
| MEMB-05 | Members | Empty email | Find "" | Not found or refused, no crash | | |

## 4. Seat grid (one per showing, card 361)

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| SEAT-01 | Seat grid | New grid is all free | New grid 8 rows by 12 seats | All 96 seats free, taken list is empty | | |
| SEAT-02 | Seat grid | Book one seat | Book C5 | C5 is taken, 95 free | | |
| SEAT-03 | Seat grid | Book several seats | Book C5 and C6 | Both taken, 94 free | | |
| SEAT-04 | Seat grid | Book a taken seat | C5 is taken, book C5 again | Refused, still 95 free | | |
| SEAT-05 | Seat grid | One taken seat in a group | C6 is taken, book C5 and C6 | Whole booking refused, C5 stays free | | |
| SEAT-06 | Seat grid | Free a seat | C5 is taken, free C5 | C5 is free again | | |
| SEAT-07 | Seat grid | Free a seat that is already free | Free D1 when it is free | Refused or no change, no crash | | |
| SEAT-08 | Seat grid | Corner seats | Book A1, A12, H1, H12 | All four taken | | |
| SEAT-09 | Seat grid | Row outside the grid | Book I1 | Refused, no crash | | |
| SEAT-10 | Seat grid | Seat number outside the grid | Book A13 and A0 | Both refused, no crash | | |
| SEAT-11 | Seat grid | Bad seat code | Book "", "5C", "ZZ" | Refused, no crash | | |
| SEAT-12 | Seat grid | Lower case code | Book "c5" | TBC with Kushal: same as C5, or refused | | |
| SEAT-13 | Seat grid | Full grid | Book all 96 seats, then book any seat | Last booking refused, 0 free | | |
| SEAT-14 | Seat grid | List taken seats | Book B2, C5, H12 | Taken list is exactly B2, C5, H12 | | |

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

## 6. File loader

Sample files are kept in the test folder. The format is the CSV from the AI context file: `id,title,genre,year,duration_minutes,age_rating,description,poster_file`.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| LOAD-01 | File loader | Good file | 10 good lines | Loaded 10, skipped 0 | | |
| LOAD-02 | File loader | Some bad lines | `data/films_sample.csv`: 10 good lines, 2 bad (letters in the year, extra commas) | Loaded 10, skipped 2 | | |
| LOAD-03 | File loader | Letters in a number column | Year "20x0" | That line skipped, others loaded | | |
| LOAD-04 | File loader | Missing columns | A line with only 3 values | That line skipped | | |
| LOAD-05 | File loader | Blank line | An empty line in the middle | Skipped or ignored, no crash | | |
| LOAD-06 | File loader | Header only | File with the header and no films | Loaded 0, skipped 0, no crash | | |
| LOAD-07 | File loader | Empty file | 0 bytes | Loaded 0, skipped 0, no crash (the window should then say no films were loaded) | | |
| LOAD-08 | File loader | File does not exist | Name of a missing file | Loader throws FileNotFoundException; the window catches it and shows a clear message, no crash | | |
| LOAD-09 | File loader | Same ID twice in the file | Two lines with ID 5 | Second one skipped and counted (confirmed: Kushal's loader refuses a duplicate ID) | | |
| LOAD-10 | File loader | Comma inside the description | Description with a comma | Line skipped and counted (decided: descriptions cannot contain commas) | | |
| LOAD-11 | File loader | Very long title | 500-character title | Skipped or cut, no crash | | |

## 7. Database

All database tests use a separate test database, never the real one.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| DB-01 | Database | Script builds from nothing | Run `cinema.sql` on an empty database | All tables created, no errors | | |
| DB-02 | Database | Save then read back a film | Save Inception, load all | Same film, every field equal | | |
| DB-03 | Database | Delete a film | Delete Inception, load all | Inception is gone | | |
| DB-04 | Database | Save and read back a showing | Save a showing, load it | Same film, screen, time and price | | |
| DB-05 | Database | Save and read back a member | Save a member with tier and points | Same tier and points | | |
| DB-06 | Database | Same seat booked twice | Save C5 for showing 10, then save C5 for showing 10 again | Second save refused by the unique key `uq_booked_seats_showing_seat` (showing_id, seat_code), plain message, no crash | | |
| DB-07 | Database | Same seat, different showing | Save C5 for showing 10 and C5 for showing 11 | Both saved | | |
| DB-08 | Database | Guest booking | Booking with no member (member ID empty) | Saved, read back with member ID still empty | | |
| DB-09 | Database | Booking total | Booking with total 475.50 | Reads back as 475.50 exactly | | |
| DB-10 | Database | SQL injection text | Title `x'); DROP TABLE films;--` | Stored and read back as plain text, films table still there | | |
| DB-11 | Database | Apostrophe in text | Title "Ocean's Eleven" | Saved and read back correctly | | |
| DB-12 | Database | Database down | Stop SQL Server, then load | Clear message, no crash | | |
| DB-13 | Database | Restart keeps data | Save, close, reopen, load | Data is still there | | |

## 8. Users and audit log (card 337)

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| USER-01 | Users | Save and read back a user | User with username and role | Same username and role | | |
| USER-02 | Users | No plain password stored | Save a user with password "abc123" | The password column holds a hash, not "abc123" | | |
| USER-03 | Users | Update failed attempts | Set failed attempts to 2 | Reads back as 2 | | |
| USER-04 | Users | Locked-until saved | Set a lock time | Reads back the same time | | |
| USER-05 | Users | Two users with the same username | Save the same username twice | Second is refused by the unique key `uq_users_username` | | |
| AUD-01 | Audit log | Write an audit row | Write "admin added film 5" | Row saved with who, what and when | | |
| AUD-02 | Audit log | Rows come back in order | Write 3 rows | Read back oldest to newest (or newest first, TBC) | | |

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
4. Try two showings on the same screen at the same time. The database refuses the exact same start time (unique key on screen_id, starts_at). Whether overlapping times are refused is TBC with Shankar (same as Akilesh's SHOW-07).
5. Load a film file with some bad lines. A message like "Loaded 8, skipped 2" should appear.

## Summary (card 409, run on 20 Nov 2026)

| Test type | Tests | Passed | Failed | Not run |
|---|---|---|---|---|
| White-box | 15 | 15 | 0 | 0 |
| Boundary/negative | 5 | 5 | 0 | 0 |
| Security | 1 | 1 | 0 | 0 |
| Performance | 1 | 1 | 0 | 0 |
| **Total** | **22** | **22** | **0** | **0** |

All 22 are automatic (MSTest). Pass rate 100%. Open bugs: none. Screenshot: `docs/testing/screenshots/409-resize-search-tests-green.png`.

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
