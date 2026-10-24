# Test plan: Cinema Film Booking and Membership System

Author: Akilesh Sookhareea (Tester 1)
Card: 18 (AB#18)
Status: draft 1, 24 Oct 2026. Rows marked TBC wait for Shankar's validation rules table.

## 1. Testing approach

We test the system at three levels. Each level checks a different kind of mistake.

**Unit tests.** These check one class or one service method on its own. We test the rules in Film, Member and Booking, the booking service (booking, cancelling, discounts), and the login service (passwords, lock-out, roles). Unit tests need no window. Where possible they also need no real database, so they run fast and give the same result every time. We write them with MSTest, one idea per test, in the Arrange, Act, Assert pattern.

**Integration tests.** These check that parts work together: the services with the hash table, and the services with the SQL Server database (save a film, read it back, bad data, database down). Irwin and I agree which of us owns which of these before Sprint 2.

**System tests.** These check the whole program through its windows. We click every button with good input and with bad input (blank, letters where a number is expected, very long text, unknown IDs). We keep screenshots of the results.

**How we choose test values.** For every rule we test one normal value, one clearly wrong value, and the values on each side of the limit. For example, for seats we test 0, 1, the maximum and the maximum plus 1. We also test the empty case and the "does not exist" case.

**When tests are written.** We write the test cases and their expected results from the requirements and from Shankar's validation table, before the code exists or while it is being written. We never copy the program's output into the expected column. If the actual result differs from the expected result, either the code or our understanding of the rule is wrong, and we find out which.

**Bugs.** A failing test becomes a Bug card in Azure Boards with steps to repeat, expected result, actual result and severity. It is assigned to the developer who owns the code. After the fix we rerun all the unit tests, then repeat the failing steps, and only then close the card.

**What we do not test.** We do not test Visual Studio or the .NET framework itself. We do not test what the AI model says in its answers, only that our own code checks the role and runs the same services as the buttons. We do not test performance under heavy load beyond the resize case for the hash table.

## 2. Test-case table

Columns: ID, area, what is tested, input, expected result, actual result, pass/fail. The last two stay empty until the tests are run.

Limits written as [MAX_TITLE] and so on come from Shankar's validation table and are filled in once it is agreed.

### Film

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| FILM-01 | Film | Valid film is accepted | Title "Inception", genre "Sci-Fi", year 2010, duration 148, age rating 12, seats total 120, seats free 120, price 250 | Film is accepted | | |
| FILM-02 | Film | Empty title | Title "" | Rejected with a clear message | | |
| FILM-03 | Film | Title with only spaces | Title "   " | Rejected with a clear message | | |
| FILM-04 | Film | Title at the maximum length | Title of [MAX_TITLE] characters | Accepted | | |
| FILM-05 | Film | Title one over the maximum | Title of [MAX_TITLE] + 1 characters | Rejected | | |
| FILM-06 | Film | Empty genre | Genre "" | Rejected | | |
| FILM-07 | Film | Year below the allowed range | Year [MIN_YEAR] - 1 | Rejected | | |
| FILM-08 | Film | Duration of zero | Duration 0 | Rejected | | |
| FILM-09 | Film | Negative duration | Duration -1 | Rejected | | |
| FILM-10 | Film | Seats total of zero | Seats total 0 | Rejected | | |
| FILM-11 | Film | Seats total of one | Seats total 1, seats free 1 | Accepted | | |
| FILM-12 | Film | Seats total at the maximum | Seats total [MAX_SEATS] | Accepted | | |
| FILM-13 | Film | Seats total one over the maximum | Seats total [MAX_SEATS] + 1 | Rejected | | |
| FILM-14 | Film | Free seats more than total seats | Seats total 120, seats free 121 | Rejected | | |
| FILM-15 | Film | Negative free seats | Seats free -1 | Rejected | | |
| FILM-16 | Film | Negative price | Price -1 | Rejected | | |
| FILM-17 | Film | Price of zero | Price 0 | TBC with Shankar (accepted or rejected) | | |

### Member

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| MEM-01 | Member | Valid Standard member | Name "Priya", tier Standard | Accepted | | |
| MEM-02 | Member | Valid Silver member | Name "Kevin", tier Silver | Accepted | | |
| MEM-03 | Member | Valid Gold member | Name "Aisha", tier Gold | Accepted | | |
| MEM-04 | Member | Empty name | Name "" | Rejected | | |
| MEM-05 | Member | Name one over the maximum | Name of [MAX_NAME] + 1 characters | Rejected | | |
| MEM-06 | Member | Tier that does not exist | Tier "Platinum" | Rejected | | |
| MEM-07 | Member | Empty tier | Tier "" | Rejected | | |

### Booking

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| BOOK-01 | Booking | Book some of the free seats | Film with 10 free seats, book 2 | Booking made, film now has 8 free seats | | |
| BOOK-02 | Booking | Book exactly all the free seats | Film with 10 free seats, book 10 | Booking made, film has 0 free seats | | |
| BOOK-03 | Booking | Overbooking is refused | Film with 10 free seats, book 11 | Refused, free seats stay 10 | | |
| BOOK-04 | Booking | Book zero seats | Book 0 | Refused | | |
| BOOK-05 | Booking | Book a negative number of seats | Book -1 | Refused | | |
| BOOK-06 | Booking | Film does not exist | Film ID that is not in the table | Refused with a clear message | | |
| BOOK-07 | Booking | Member does not exist | Member ID that is not in the table | Refused with a clear message | | |
| BOOK-08 | Booking | Cancel returns the seats | Cancel the booking of 2 seats (film had 8 free) | Booking cancelled, film has 10 free seats | | |
| BOOK-09 | Booking | Cancel twice is refused | Cancel the same booking a second time | Second cancel refused, free seats unchanged | | |
| BOOK-10 | Booking | Cancel a booking that does not exist | Booking ID that is not in the table | Refused | | |
| BOOK-11 | Booking | Standard price has no discount | Standard member books 2 seats at price 250 | Total is 500 | | |
| BOOK-12 | Booking | Silver discount applied | Silver member books 2 seats at price 250 | Total is 500 minus the Silver discount [SILVER_PCT] | | |
| BOOK-13 | Booking | Gold discount applied | Gold member books 2 seats at price 250 | Total is 500 minus the Gold discount [GOLD_PCT] | | |
| BOOK-14 | Booking | Higher tier pays less | Same booking for Standard, Silver, Gold | Gold total is lower than Silver, Silver is lower than Standard | | |

### Login

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| LOGIN-01 | Login | Correct username and password | Valid user, correct password | Login succeeds | | |
| LOGIN-02 | Login | Wrong password once | Valid user, wrong password | Refused, failed attempts becomes 1 | | |
| LOGIN-03 | Login | Unknown username | Username that does not exist | Refused with a clear message | | |
| LOGIN-04 | Login | Three wrong tries lock the account | Valid user, wrong password 3 times | Account is locked | | |
| LOGIN-05 | Login | Correct password while locked | Locked user, correct password | Refused, account still locked | | |
| LOGIN-06 | Login | Lock ends after the lock time | Locked user, wait past [LOCK_TIME] | Login works again | | |
| LOGIN-07 | Login | Counter resets after a good login | 2 wrong tries, then correct password | Login succeeds, failed attempts back to 0 (TBC with Shankar) | | |
| LOGIN-08 | Login | Empty username | Username "" | Refused | | |
| LOGIN-09 | Login | Empty password | Password "" | Refused | | |
| LOGIN-10 | Login | Password is not stored as plain text | Create a user with password "abc123" | Stored hash is not "abc123"; two users with the same password get different hashes | | |

### Roles

The rules come from the "who may do what" table in the team's AI context file, section 6. Each test calls the service directly, with no window.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| ROLE-01 | Roles | Viewer can view and search films | Viewer searches films | Allowed | | |
| ROLE-02 | Roles | Viewer cannot book | Viewer books seats | Refused | | |
| ROLE-03 | Roles | Viewer cannot cancel | Viewer cancels a booking | Refused | | |
| ROLE-04 | Roles | Viewer cannot register a member | Viewer registers a member | Refused | | |
| ROLE-05 | Roles | Staff can book and cancel | Staff books, then cancels | Allowed | | |
| ROLE-06 | Roles | Staff can register and edit members | Staff registers, then edits a member | Allowed | | |
| ROLE-07 | Roles | Staff cannot add a film | Staff adds a film | Refused | | |
| ROLE-08 | Roles | Staff cannot remove a film | Staff removes a film | Refused | | |
| ROLE-09 | Roles | Staff cannot load a file or sync the database | Staff loads a file | Refused | | |
| ROLE-10 | Roles | Staff cannot manage users | Staff changes a user's role | Refused | | |
| ROLE-11 | Roles | Staff cannot see the audit log | Staff opens the audit log | Refused | | |
| ROLE-12 | Roles | Admin can do every action above | Admin runs each action | All allowed | | |
| ROLE-13 | Roles | The check is in the service, not only the button | Viewer calls the booking service directly | Refused even though no button is involved | | |

### Windows (hand tests)

Irwin adds the detailed steps. Each row is repeated for every window that has the control.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| WIN-01 | Windows | Login with both fields blank | Click Login with nothing typed | Clear message, no crash | | |
| WIN-02 | Windows | First run with no users | Start the program on an empty database | "Create first admin" window appears | | |
| WIN-03 | Windows | Add a film with a field left blank | Leave each field blank in turn | Clear message for each field, no crash | | |
| WIN-04 | Windows | Letters in a number field | "abc" in year, seats and price | Clear message, no crash | | |
| WIN-05 | Windows | Very long text | 500 characters in title | Rejected or cut with a message, no crash | | |
| WIN-06 | Windows | Search by title, partial and any case | "inc" and "INCEPTION" | Finds Inception both times | | |
| WIN-07 | Windows | Search by genre | "Sci-Fi" | Lists the Sci-Fi films | | |
| WIN-08 | Windows | Search with no match | "zzzz" | Empty result with a message, no crash | | |
| WIN-09 | Windows | Delete and cancel ask first | Click Delete, then Cancel booking | Yes / No box appears each time | | |
| WIN-10 | Windows | Load a file with some bad lines | CSV with 2 bad lines out of 50 | Message such as "Loaded 48, skipped 2" | | |
| WIN-11 | Windows | Database is down | Stop SQL Server and start the program | Program says so and still works from a loaded file | | |
| WIN-12 | Windows | Buttons for a role the user does not have | Log in as Viewer | Booking and delete buttons are hidden or disabled | | |

### Still to add (owner to be agreed with Irwin)

- Hash table: add then find, find a missing key, duplicate key, two keys in one slot, remove first, middle and last in a chain, count after add and remove, empty table, many films to force a resize and check every film is still found.
- Search by title and genre in the hash table.
- File loader: good file, bad lines, empty file, missing columns, missing file.
- Database: save and read back, bad data, database down, SQL text stored as plain text.
- Users and audit log tables.
- AI agent (card 43): right tool chosen, role check before anything runs, memory of earlier turns, bad request handled.
