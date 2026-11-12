# Test plan: data side

Author: Irwin Chinathoo (Tester 2)
Card: 18 (AB#18), data-side section for Akilesh to merge into `test-plan.md`
Status: draft 1, 30 Oct 2026. Rows marked TBC wait for an answer from Kushal.

This section covers Kushal's code: our own hash tables and linked list, the seat grid, search, the file loader and the database. It uses the same columns as the main plan. The last two columns stay empty until the tests are run.

Default sizes used below come from the AI context file (section 6 and 8): seats are rows A to H and seats 1 to 12 (96 seats), and the hash table grows at about 75% full.

## 1. Hash table (films by ID)

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| HASH-01 | Hash table | Empty table | New table | Count is 0, list all is empty | | |
| HASH-02 | Hash table | Find on an empty table | Find ID 1 | Not found, no crash | | |
| HASH-03 | Hash table | Add then find | Add film ID 1, find ID 1 | Same film comes back | | |
| HASH-04 | Hash table | Count goes up | Add 3 films | Count is 3 | | |
| HASH-05 | Hash table | Find a missing ID | Table has IDs 1 to 3, find ID 99 | Not found, no crash | | |
| HASH-06 | Hash table | Two films in the same slot | Two IDs that land in the same slot (picked from Kushal's hash function) | Both can be found, count is 2 | | |
| HASH-07 | Hash table | Remove the first item in a chain | Three films in one slot, remove the first | Other two still found, count goes down by 1 | | |
| HASH-08 | Hash table | Remove from the middle of a chain | Three films in one slot, remove the middle one | Other two still found | | |
| HASH-09 | Hash table | Remove the last item in a chain | Three films in one slot, remove the last | Other two still found | | |
| HASH-10 | Hash table | Remove a missing ID | Remove ID 99 | Refused or returns false, count unchanged, no crash | | |
| HASH-11 | Hash table | Remove then find | Add ID 1, remove ID 1, find ID 1 | Not found | | |
| HASH-12 | Hash table | Add the same ID twice | Add ID 1 twice | TBC with Kushal: refused, or the second replaces the first. Count must stay 1 | | |
| HASH-13 | Hash table | List all | Add 5 films | List all returns exactly those 5, no repeats | | |
| HASH-14 | Hash table | List all after remove | Add 5, remove 2 | List all returns the other 3 | | |

## 2. Resize

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| RES-01 | Resize | Table grows at about 75% full | Add films until the count passes 75% of the slots | Number of slots goes up | | |
| RES-02 | Resize | Nothing lost after one resize | Add enough films to force one resize, then find each | Every film is found | | |
| RES-03 | Resize | Large load | Add 1000 films, then find each one | All 1000 found, count is 1000 | | |
| RES-04 | Resize | No repeats after resize | Add 1000 films, list all | 1000 items, each ID once | | |
| RES-05 | Resize | Remove still works after resize | Add 1000, remove 500, find all | Removed ones not found, other 500 found | | |

## 3. Other hash tables (showings by ID, members by email)

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| SHOW-01 | Showings | Add then find a showing | Add showing ID 10, find ID 10 | Same showing comes back | | |
| SHOW-02 | Showings | Showings for one film | Film 1 has 3 showings, film 2 has 1 | Asking for film 1 gives exactly its 3 showings | | |
| SHOW-03 | Showings | Film with no showings | Ask for showings of a film with none | Empty result, no crash | | |
| MEMB-01 | Members | Add then find by email | Add member "priya@example.com", find it | Same member comes back | | |
| MEMB-02 | Members | Email in a different case | Find "PRIYA@example.com" | TBC with Kushal: found (we suggest yes, email is not case sensitive) | | |
| MEMB-03 | Members | Email not in the table | Find "nobody@example.com" | Not found, no crash | | |
| MEMB-04 | Members | Two members with the same email | Add "priya@example.com" twice | Second is refused | | |
| MEMB-05 | Members | Empty email | Find "" | Not found or refused, no crash | | |

## 4. Seat grid (one per showing)

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

## 5. Search

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| SRCH-01 | Search | Full title | "Inception" | Inception only | | |
| SRCH-02 | Search | Part of a title | "incep" | Inception | | |
| SRCH-03 | Search | Different case | "INCEPTION" | Inception | | |
| SRCH-04 | Search | Text inside the title | "part" | Dune: Part Two | | |
| SRCH-05 | Search | Several matches | "the" | Every film with "the" in the title, no repeats | | |
| SRCH-06 | Search | No match | "zzzz" | Empty result, no crash | | |
| SRCH-07 | Search | Empty search text | "" | TBC with Kushal: all films, or refused | | |
| SRCH-08 | Search | By genre | "Sci-Fi" | Exactly the Sci-Fi films | | |
| SRCH-09 | Search | Genre with no films | "Western" | Empty result, no crash | | |
| SRCH-10 | Search | Title with an apostrophe | "Ocean's" | Ocean's Eleven | | |

## 6. File loader

Sample files are kept in the test folder. The format is the CSV from the AI context file: `id,title,genre,year,duration_minutes,age_rating,description,poster_file`.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| LOAD-01 | File loader | Good file | 10 good lines | Loaded 10, skipped 0 | | |
| LOAD-02 | File loader | Some bad lines | 8 good lines, 2 bad | Loaded 8, skipped 2 | | |
| LOAD-03 | File loader | Letters in a number column | Year "20x0" | That line skipped, others loaded | | |
| LOAD-04 | File loader | Missing columns | A line with only 3 values | That line skipped | | |
| LOAD-05 | File loader | Blank line | An empty line in the middle | Skipped or ignored, no crash | | |
| LOAD-06 | File loader | Header only | File with the header and no films | Loaded 0, skipped 0, no crash | | |
| LOAD-07 | File loader | Empty file | 0 bytes | Clear message, no crash | | |
| LOAD-08 | File loader | File does not exist | Name of a missing file | Clear message, no crash | | |
| LOAD-09 | File loader | Same ID twice in the file | Two lines with ID 5 | Second one skipped and counted | | |
| LOAD-10 | File loader | Comma inside the description | Description with a comma | TBC with Kushal: how commas in text are handled | | |
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
| DB-06 | Database | Same seat booked twice | Save C5 for showing 10, then save C5 for showing 10 again | Second save refused by the unique key, plain message, no crash | | |
| DB-07 | Database | Same seat, different showing | Save C5 for showing 10 and C5 for showing 11 | Both saved | | |
| DB-08 | Database | Guest booking | Booking with no member (member ID empty) | Saved, read back with member ID still empty | | |
| DB-09 | Database | Booking total | Booking with total 475.50 | Reads back as 475.50 exactly | | |
| DB-10 | Database | SQL injection text | Title `x'); DROP TABLE films;--` | Stored and read back as plain text, films table still there | | |
| DB-11 | Database | Apostrophe in text | Title "Ocean's Eleven" | Saved and read back correctly | | |
| DB-12 | Database | Database down | Stop SQL Server, then load | Clear message, no crash | | |
| DB-13 | Database | Restart keeps data | Save, close, reopen, load | Data is still there | | |

## 8. Users and audit log (card 131)

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| USER-01 | Users | Save and read back a user | User with email and role | Same email and role | | |
| USER-02 | Users | No plain password stored | Save a user with password "abc123" | The password column holds a hash, not "abc123" | | |
| USER-03 | Users | Update failed attempts | Set failed attempts to 2 | Reads back as 2 | | |
| USER-04 | Users | Locked-until saved | Set a lock time | Reads back the same time | | |
| USER-05 | Users | Two users with the same email | Save the same email twice | Second is refused | | |
| AUD-01 | Audit log | Write an audit row | Write "admin added film 5" | Row saved with who, what and when | | |
| AUD-02 | Audit log | Rows come back in order | Write 3 rows | Read back oldest to newest (or newest first, TBC) | | |

## 9. Manual tests: full journeys (cards 147 and 38)

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
4. Try two showings on the same screen at the same time. The second should be refused.
5. Load a film file with some bad lines. A message like "Loaded 8, skipped 2" should appear.

## 10. Questions for Kushal

1. What are the method names for the hash table and the seat grid? (I will write the tests against them.)
2. Adding the same key twice: refused, or replaced? (HASH-12)
3. Is member email looked up without caring about case? (MEMB-02)
4. Is "c5" the same seat as "C5"? (SEAT-12)
5. What does an empty search return? (SRCH-07)
6. How does the loader handle a comma inside the description? (LOAD-10)
7. Which hash function do you use? I need two IDs that land in the same slot for HASH-06.
