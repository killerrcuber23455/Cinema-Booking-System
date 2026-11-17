# Test plan: Cinema Film Booking and Membership System

Author: Akilesh Sookhareea (Tester 1)
Card: 354 (AB#354)
Status: draft 3, 16 Nov 2026. Limits and decisions come from Shankar's validation rules table and Divyesh's messages. Rows still marked TBC wait for an answer.

## 1. Testing approach

We use four kinds of test, because each one catches a different kind of mistake. The Test type column in the table below says which kind a row is.

**White-box (unit) tests.** I write these while looking at the code, so every branch of a rule gets at least one test. Each test checks one class or method on its own: Film, Screen, Showing, Member, Booking, FilmService, and the booking, loyalty and login services. They are MSTest tests that follow Arrange, Act, Assert, with one idea per test. They don't open a window or touch the real database, so they run quickly and give the same answer every time.

**Black-box tests.** These are written from the rules only, without reading the code. For each rule we try a valid value, an invalid value and the values at the edges. We write the cases this way first, using the validation rules table. After that I read the code and add a white-box test for any branch we missed.

**Integration tests.** These check that parts work together, for example a service talking to the real Database class. They run on a separate test SQL Server database and never on the real one. Irwin writes the data-side rows (hash table, linked list, seat grid, search, file loader, database, users and audit log) and I merge them into this file.

**System tests.** These run the whole program through its windows by hand. Irwin goes through the full journeys (a guest books, a guest signs up, a customer books and cancels, staff book for a walk-in, an admin adds a showing) and I help. We keep screenshots as proof.

**More test types.** Regression means we run the whole suite again after every merge and every bug fix and write down the date and the result. Performance means we time things with about 1000 films (resize and search) and a booking, in ms. Security means SQL injection text in every text box, role checks, lock-out after 3 wrong passwords and no plain text passwords. Boundary/negative means the edge values and the wrong input for each rule. Every test is either Automatic (an MSTest test) or Manual (by hand, with a screenshot).

**Test-driven development.** From Sprint 3, for code that isn't written yet, we work in three steps. First red: write a test that fails and commit it. Then green: write just enough code to make it pass. Then refactor: tidy up and check the tests still pass. We save a Test Explorer screenshot of the red run and the green run in docs/testing/screenshots. The failing tests are pushed before the developer starts the code. The older code (Film, Member, Booking and the hash table) was tested after it was written, and the report says so.

**Choosing test values.** For each rule we test a normal value, a clearly wrong value, and the values just inside and just outside the limit. For example, the seats run from A1 to H12, so A1 and H12 must be accepted while A0, A13 and I1 must be refused. For the tiers we test 99, 100, 299 and 300 lifetime points. We also test empty input and things that don't exist.

**When the tests are written.** The test cases and their expected results come from the requirements and the validation rules table, written before or while the code is built. We never copy what the program prints into the expected column. If the actual result is different, either the code or our reading of the rule is wrong, and we work out which.

**Results.** The Pass/Fail column only holds Pass, Fail or Not run.

**Bugs.** Every failing test becomes a Bug card in Azure Boards with the steps to repeat it, the expected result, the actual result and how serious it is. The card goes to the developer who owns that code. After the fix we run all the unit tests again, repeat the steps that failed, and only then close the card.

**What we don't test.** We don't test Visual Studio or .NET themselves. For the AI agent we don't judge what the model says, only that our own code checks the user's role and uses the same services as the buttons.

## 2. Test-case table

Columns: Test ID, Test type, Mode, Area, What is tested, Preconditions, Test data, Post-conditions, Expected result, Actual result, Pass/Fail, Executed date, Tester, Bug ID. Test type is White-box, Black-box, Integration, System, Regression, Performance, Security or Boundary/negative. Mode is Automatic (an MSTest test) or Manual (by hand, with a screenshot). Pass/Fail only holds Pass, Fail or Not run. The preconditions, test data, post-conditions and expected result are written before the test is run. The actual result and the executed date stay empty until the test is run. Every Fail gets a Bug ID here and a row in docs/testing/bug-log.md.

Limits still written in square brackets, such as [MAX_ROWS], are not decided yet. Film test objects use an Id of 1 or higher, because Id 0 fails validation. Money is in rupees.

### Film

Fields: Id, Title, Genre, Year, DurationMinutes, AgeRating, Description, PosterFile. Seats and price are not on Film. They belong to Showing and Screen.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| FILM-01 | White-box | Automatic | Film | Valid film is accepted | A valid Film object (Id 1 or higher) with every field correct except the one tested | Id 1, title "Inception", genre "Sci-Fi", year 2010, duration 148, age rating 12, description "A thief who steals secrets through dreams", poster "inception.jpg" | No data stored or changed (validation only) | Film is accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-02 | White-box | Automatic | Film | Empty title | A valid Film object (Id 1 or higher) with every field correct except the one tested | Title "" | Nothing is stored or changed | Rejected with a clear message | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-03 | White-box | Automatic | Film | Title with only spaces | A valid Film object (Id 1 or higher) with every field correct except the one tested | Title "   " | Nothing is stored or changed | Rejected with a clear message | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-04 | Boundary/negative | Automatic | Film | Title at the maximum length | A valid Film object (Id 1 or higher) with every field correct except the one tested | Title of 100 characters | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-05 | Boundary/negative | Automatic | Film | Title one over the maximum | A valid Film object (Id 1 or higher) with every field correct except the one tested | Title of 101 characters | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-06 | White-box | Automatic | Film | Empty genre | A valid Film object (Id 1 or higher) with every field correct except the one tested | Genre "" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-07 | Boundary/negative | Automatic | Film | Year below the allowed range (1900 to next year) | A valid Film object (Id 1 or higher) with every field correct except the one tested | Year 1899 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-08 | Boundary/negative | Automatic | Film | Duration of zero | A valid Film object (Id 1 or higher) with every field correct except the one tested | Duration 0 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-09 | Boundary/negative | Automatic | Film | Negative duration | A valid Film object (Id 1 or higher) with every field correct except the one tested | Duration -1 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-10 | White-box | Automatic | Film | Age rating that is not allowed | A valid Film object (Id 1 or higher) with every field correct except the one tested | Age rating 10 (allowed ratings are 0, 12, 15, 18) | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-11 | Boundary/negative | Automatic | Film | Description one over the maximum | A valid Film object (Id 1 or higher) with every field correct except the one tested | Description of 501 characters | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-12 | White-box | Automatic | Film | Empty poster is accepted | A valid Film object (Id 1 or higher) with every field correct except the one tested | PosterFile "" | No data stored or changed (validation only) | Accepted, a placeholder card is shown when the film is displayed | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-13 | Boundary/negative | Automatic | Film | Year at the lowest allowed | A valid Film object (Id 1 or higher) with every field correct except the one tested | Year 1900 | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-14 | Boundary/negative | Automatic | Film | Year at the highest allowed | A valid Film object (Id 1 or higher) with every field correct except the one tested | Year = next year | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-15 | Boundary/negative | Automatic | Film | Year one past the highest allowed | A valid Film object (Id 1 or higher) with every field correct except the one tested | Year = next year + 1 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-16 | Boundary/negative | Automatic | Film | Duration at the lowest allowed | A valid Film object (Id 1 or higher) with every field correct except the one tested | Duration 1 | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-17 | Boundary/negative | Automatic | Film | Duration at the highest allowed | A valid Film object (Id 1 or higher) with every field correct except the one tested | Duration 300 | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-18 | Boundary/negative | Automatic | Film | Duration one over the highest allowed | A valid Film object (Id 1 or higher) with every field correct except the one tested | Duration 301 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-19 | Boundary/negative | Automatic | Film | Genre one over the maximum | A valid Film object (Id 1 or higher) with every field correct except the one tested | Genre of 51 characters | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-20 | Boundary/negative | Automatic | Film | Genre at the maximum | A valid Film object (Id 1 or higher) with every field correct except the one tested | Genre of 50 characters | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-21 | White-box | Automatic | Film | Every allowed age rating | A valid Film object (Id 1 or higher) with every field correct except the one tested | Age rating 0, then 12, then 15, then 18 | No data stored or changed (validation only) | Accepted each time | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-22 | Boundary/negative | Automatic | Film | Description at the maximum | A valid Film object (Id 1 or higher) with every field correct except the one tested | Description of 500 characters | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-23 | Boundary/negative | Automatic | Film | Film Id of zero | A valid Film object (Id 1 or higher) with every field correct except the one tested | Id 0 | Nothing is stored or changed | Rejected (Id must be 1 or higher) | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-24 | White-box | Automatic | Film | Poster with a folder path | A valid Film object (Id 1 or higher) with every field correct except the one tested | PosterFile "images/a.jpg" | Nothing is stored or changed | Rejected (file name only) | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-25 | Boundary/negative | Automatic | Film | Poster name one over the maximum | A valid Film object (Id 1 or higher) with every field correct except the one tested | PosterFile of 101 characters | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-26 | White-box | Automatic | Film | Poster with only spaces | A valid Film object (Id 1 or higher) with every field correct except the one tested | PosterFile "   " | No data stored or changed (validation only) | Accepted (placeholder shown) | as expected | Pass | 2026-11-16 | Akilesh | - |
| FILM-27 | White-box | Automatic | Film | Several bad fields at once | A valid Film object (Id 1 or higher) with every field correct except the one tested | Empty title and duration 0 | No data stored or changed (validation only) | IsValid returns false with one message line per bad field | as expected | Pass | 2026-11-16 | Akilesh | - |

### FilmService

Shankar's FilmService adds and removes films. Only an Admin may do either.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| FS-01 | White-box | Automatic | FilmService | Admin adds a valid film | A new FilmService with an empty HashTable<int, Film> | Admin, valid film | The film table holds only what the expected result says | Returns true, the film is stored | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-02 | White-box | Automatic | FilmService | Staff cannot add a film | A new FilmService with an empty HashTable<int, Film> | Staff, valid film | Nothing is stored or changed | Refused | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-03 | White-box | Automatic | FilmService | Customer cannot add a film | A new FilmService with an empty HashTable<int, Film> | Customer, valid film | Nothing is stored or changed | Refused | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-04 | White-box | Automatic | FilmService | Guest cannot add a film | A new FilmService with an empty HashTable<int, Film> | Guest, valid film | Nothing is stored or changed | Refused | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-05 | White-box | Automatic | FilmService | Null film is refused | A new FilmService with an empty HashTable<int, Film> | Admin, null film | Nothing is stored or changed | Refused, no crash | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-06 | White-box | Automatic | FilmService | Invalid film gives the validation message | A new FilmService with an empty HashTable<int, Film> | Admin, film with an empty title | Nothing is stored or changed | Refused with the message from the validation rules | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-07 | White-box | Automatic | FilmService | Duplicate Id is refused | A new FilmService with an empty HashTable<int, Film> | Admin adds a second film with Id 1 | Nothing is stored or changed | Refused, the first film is unchanged | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-08 | White-box | Automatic | FilmService | Admin removes an existing film | A new FilmService with an empty HashTable<int, Film> | Admin, Id of a stored film | The film table holds only what the expected result says | Removed, the film can no longer be found | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-09 | White-box | Automatic | FilmService | Unknown Id is refused on remove | A new FilmService with an empty HashTable<int, Film> | Admin, Id that is not stored | Nothing is stored or changed | Refused, no crash | as expected | Pass | 2026-11-16 | Akilesh | - |
| FS-10 | White-box | Automatic | FilmService | Non-admin cannot remove a film | A new FilmService with an empty HashTable<int, Film> | Staff, Customer and Guest remove a stored film | The film table holds only what the expected result says | All three refused, the film is still there |  | Not run |  | Akilesh |  |

### Screen and Showing

A screen has Rows and SeatsPerRow (two screens of 8 rows by 12 seats to start). A showing has a film, a screen, a start time and a price.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| SHOWING-01 | White-box | Automatic | Showing | Valid showing is accepted | A valid screen and film exist; the showing table is empty | Existing film, existing screen, start time in the future, price 250 | Data matches the expected result | Accepted |  | Not run |  | Akilesh |  |
| SHOWING-02 | White-box | Automatic | Showing | Film does not exist | A valid screen and film exist; the showing table is empty | Film ID not in the table | Nothing is stored or changed | Rejected with a clear message |  | Not run |  | Akilesh |  |
| SHOWING-03 | White-box | Automatic | Showing | Screen does not exist | A valid screen and film exist; the showing table is empty | Screen ID not in the table | Nothing is stored or changed | Rejected |  | Not run |  | Akilesh |  |
| SHOWING-04 | Boundary/negative | Automatic | Showing | Price of zero is refused | A valid screen and film exist; the showing table is empty | Price 0 | Nothing is stored or changed | Rejected (the price must be above 0) |  | Not run |  | Akilesh |  |
| SHOWING-05 | Boundary/negative | Automatic | Showing | Negative price | A valid screen and film exist; the showing table is empty | Price -1 | Nothing is stored or changed | Rejected |  | Not run |  | Akilesh |  |
| SHOWING-06 | White-box | Automatic | Showing | Start time in the past is refused | A valid screen and film exist; the showing table is empty | Start time yesterday | Nothing is stored or changed | Rejected |  | Not run |  | Akilesh |  |
| SHOWING-07 | White-box | Automatic | Showing | Two showings overlap on the same screen | A valid screen and film exist; the showing table is empty | Same screen, a second showing starting before the first one ends | Nothing is stored or changed | Rejected |  | Not run |  | Akilesh |  |
| SCREEN-01 | White-box | Automatic | Screen | Valid screen | A valid screen and film exist; the showing table is empty | Name "Screen 1", rows 8, seats per row 12 | Data matches the expected result | Accepted, 96 seats |  | Not run |  | Akilesh |  |
| SCREEN-02 | White-box | Automatic | Screen | Zero rows | A valid screen and film exist; the showing table is empty | Rows 0 | Nothing is stored or changed | Rejected |  | Not run |  | Akilesh |  |
| SCREEN-03 | White-box | Automatic | Screen | Zero seats per row | A valid screen and film exist; the showing table is empty | Seats per row 0 | Nothing is stored or changed | Rejected |  | Not run |  | Akilesh |  |
| SCREEN-04 | White-box | Automatic | Screen | Rows over the maximum | A valid screen and film exist; the showing table is empty | Rows [MAX_ROWS] + 1 | Nothing is stored or changed | Rejected |  | Not run |  | Akilesh |  |

### Member

Fields: Id, Name, Email, Phone, Tier, Points, LifetimePoints, JoinedAt.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| MEM-01 | Black-box | Automatic | Member | Valid Standard member | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Name "Priya", email "priya@mail.com", phone "5712 3456", tier Standard | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-02 | Black-box | Automatic | Member | Valid Silver member | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Same details, tier Silver | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-03 | Black-box | Automatic | Member | Valid Gold member | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Same details, tier Gold | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-04 | Black-box | Automatic | Member | Empty name | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Name "" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-05 | Boundary/negative | Automatic | Member | Name one over the maximum | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Name of 101 characters | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-06 | Black-box | Automatic | Member | Email with no @ | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Email "priya.mail.com" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-07 | Black-box | Automatic | Member | Empty email | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Email "" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-08 | Black-box | Automatic | Member | Duplicate email | TEST database with at least one member (priya@mail.com) | Add a second member with email "priya@mail.com" | Nothing is stored or changed | Rejected with a clear message |  | Not run |  | Akilesh |  |
| MEM-09 | Black-box | Automatic | Member | Phone with letters is refused | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Phone "abcd" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-10 | Black-box | Automatic | Member | Tier that does not exist | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Tier "Platinum" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-11 | Boundary/negative | Automatic | Member | Negative points | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Points -1 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-12 | Black-box | Automatic | Member | Edit a member's details | TEST database with at least one member (priya@mail.com) | Change phone of an existing member | No data stored or changed (validation only) | Saved, other fields unchanged |  | Not run |  | Akilesh |  |
| MEM-13 | Black-box | Automatic | Member | Find a member by email | TEST database with at least one member (priya@mail.com) | Email of an existing member | No data stored or changed (validation only) | That member is returned |  | Not run |  | Akilesh |  |
| MEM-14 | Black-box | Automatic | Member | Find a member by an email that does not exist | TEST database with at least one member (priya@mail.com) | Email "nobody@mail.com" | Nothing is stored or changed | Nothing found, no crash |  | Not run |  | Akilesh |  |
| MEM-15 | Boundary/negative | Automatic | Member | Phone one digit too short | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Phone with 6 digits | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-16 | Boundary/negative | Automatic | Member | Phone at the shortest allowed | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Phone with 7 digits | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-17 | Boundary/negative | Automatic | Member | Phone at the longest allowed | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Phone with 15 digits | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-18 | Boundary/negative | Automatic | Member | Phone one digit too long | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Phone with 16 digits | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-19 | Black-box | Automatic | Member | Phone with spaces, plus and dash | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Phone "+230 5712-3456" | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-20 | Boundary/negative | Automatic | Member | Name at the maximum length | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Name of 100 characters | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-21 | Black-box | Automatic | Member | Points above lifetime points | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Points 50, lifetime points 20 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-22 | Boundary/negative | Automatic | Member | Points equal to lifetime points | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Points 20, lifetime points 20 | No data stored or changed (validation only) | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-23 | Black-box | Automatic | Member | Join date in the future | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | JoinedAt = tomorrow | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-24 | Black-box | Automatic | Member | Email with a space | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Email "pri ya@mail.com" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-25 | Black-box | Automatic | Member | Email with no dot after the @ | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Email "priya@mail" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| MEM-26 | Black-box | Automatic | Member | Email starting with @ | A valid Member object (Id 1, Standard, 0 points) with every field correct except the one tested | Email "@mail.com" | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |

### Booking with seats

Seat codes are a row letter A to H and a seat number 1 to 12, for example C5. The screens are 8 rows by 12 seats. Bookings have a reference such as CB-7K2M9Q, a status (Confirmed or Cancelled) and either a member or guest details.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| BOOK-01 | Integration | Automatic | Booking | Book one free seat | TEST database with a showing whose seats are free and a member who exists | Showing with all seats free, seat C5, customer | Seats and the booking table match the expected result | Booking made, status Confirmed, C5 is now taken |  | Not run |  | Akilesh |  |
| BOOK-02 | Integration | Automatic | Booking | Book two seats together | TEST database with a showing whose seats are free and a member who exists | Seats C5, C6 | Seats and the booking table match the expected result | Booking made, both seats taken |  | Not run |  | Akilesh |  |
| BOOK-03 | Integration | Automatic | Booking | Taken seat is refused | TEST database with a showing whose seats are free and a member who exists | Book C5 again on the same showing | Nothing is stored or changed | Refused with a clear message, no change to the seats |  | Not run |  | Akilesh |  |
| BOOK-04 | Integration | Automatic | Booking | One taken seat among several | TEST database with a showing whose seats are free and a member who exists | Seats C6, C7 where C6 is taken | Nothing is stored or changed | Refused, C7 is not taken either (a booking is all or nothing) |  | Not run |  | Akilesh |  |
| BOOK-05 | Integration | Automatic | Booking | Same seat in one request twice | TEST database with a showing whose seats are free and a member who exists | Seats C5, C5 | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| BOOK-06 | Integration | Automatic | Booking | No seats chosen | TEST database with a showing whose seats are free and a member who exists | Empty seat list | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| BOOK-07 | Integration | Automatic | Booking | First seat on the map | TEST database with a showing whose seats are free and a member who exists | Seat A1 | Seats and the booking table match the expected result | Accepted |  | Not run |  | Akilesh |  |
| BOOK-08 | Integration | Automatic | Booking | Last seat on the map | TEST database with a showing whose seats are free and a member who exists | Seat H12 | Seats and the booking table match the expected result | Accepted |  | Not run |  | Akilesh |  |
| BOOK-09 | Integration | Automatic | Seat grid | Seat grid refuses seat number 0 | TEST database with a showing whose seats are free and a member who exists | Seat A0 on the seat grid | Nothing is stored or changed | Refused by the seat grid |  | Not run |  | Akilesh |  |
| BOOK-10 | Integration | Automatic | Seat grid | Seat grid refuses a seat number past the row end | TEST database with a showing whose seats are free and a member who exists | Seat A13 on the seat grid | Nothing is stored or changed | Refused by the seat grid (Booking.IsValid accepts the format) |  | Not run |  | Akilesh |  |
| BOOK-11 | Integration | Automatic | Seat grid | Seat grid refuses a row letter past the last row | TEST database with a showing whose seats are free and a member who exists | Seat I1 on the seat grid | Nothing is stored or changed | Refused by the seat grid (Booking.IsValid accepts the format) |  | Not run |  | Akilesh |  |
| BOOK-12 | Black-box | Automatic | Booking | Booking format check refuses badly typed codes | A valid Booking object (member booking, seat C5) with every field correct except the one tested | Seat "5C", "", "C" | Nothing is stored or changed | Refused by Booking.IsValid for each | as expected | Pass | 2026-11-16 | Akilesh | - |
| BOOK-13 | Integration | Automatic | Booking | Showing does not exist | TEST database with a showing whose seats are free and a member who exists | Showing ID not in the table | Nothing is stored or changed | Refused with a clear message |  | Not run |  | Akilesh |  |
| BOOK-14 | Integration | Automatic | Booking | Member does not exist | TEST database with a showing whose seats are free and a member who exists | Member ID not in the table | Nothing is stored or changed | Refused with a clear message |  | Not run |  | Akilesh |  |
| BOOK-15 | Black-box | Automatic | Booking | Total price is seats times showing price | A valid Booking object (member booking, seat C5) with every field correct except the one tested | 2 seats at Rs 250, Standard member | Seats and the booking table match the expected result | Total is Rs 500 |  | Not run |  | Akilesh |  |
| BOOK-16 | Integration | Automatic | Booking | Booking gets a reference | TEST database with a showing whose seats are free and a member who exists | Make any booking | Seats and the booking table match the expected result | Reference is not empty and has the agreed format |  | Not run |  | Akilesh |  |
| BOOK-17 | Integration | Automatic | Booking | Two bookings get different references | TEST database with a showing whose seats are free and a member who exists | Make two bookings | Seats and the booking table match the expected result | The two references are different |  | Not run |  | Akilesh |  |
| BOOK-18 | Integration | Automatic | Booking | Cancel frees the seats | TEST database with a showing whose seats are free and a member who exists | Cancel the booking of C5, C6 | Seats and the booking table match the expected result | Status Cancelled, C5 and C6 are free again |  | Not run |  | Akilesh |  |
| BOOK-19 | Integration | Automatic | Booking | Cancel twice is refused | TEST database with a showing whose seats are free and a member who exists | Cancel the same booking a second time | Nothing is stored or changed | Refused, seats and points unchanged |  | Not run |  | Akilesh |  |
| BOOK-20 | Integration | Automatic | Booking | Cancel a booking that does not exist | TEST database with a showing whose seats are free and a member who exists | Reference "CB-XXXXXX" | Nothing is stored or changed | Refused with a clear message |  | Not run |  | Akilesh |  |
| BOOK-21 | Integration | Automatic | Booking | A freed seat can be booked again | TEST database with a showing whose seats are free and a member who exists | Book C5 after cancelling it | Seats and the booking table match the expected result | Accepted |  | Not run |  | Akilesh |  |
| BOOK-22 | Integration | Automatic | Booking | Booking a showing that has already started | TEST database with a showing whose seats are free and a member who exists | Showing start time in the past | Nothing is stored or changed | Refused (a start time in the past is not allowed) |  | Not run |  | Akilesh |  |
| BOOK-23 | Black-box | Automatic | Booking | Booking only checks the seat code format | A valid Booking object (member booking, seat C5) with every field correct except the one tested | Seat A13 in Booking.IsValid | Seats and the booking table match the expected result | Format accepted (the seat grid refuses it later, see BOOK-10) | as expected | Pass | 2026-11-16 | Akilesh | - |
| BOOK-24 | Black-box | Automatic | Booking | Seat code with a leading zero | A valid Booking object (member booking, seat C5) with every field correct except the one tested | Seat "C05" and "C0" | Nothing is stored or changed | Refused by Booking.IsValid | as expected | Pass | 2026-11-16 | Akilesh | - |
| BOOK-25 | Black-box | Automatic | Booking | Lowercase seat code | A valid Booking object (member booking, seat C5) with every field correct except the one tested | Seat "c5" | Nothing is stored or changed | Refused by Booking.IsValid | as expected | Pass | 2026-11-16 | Akilesh | - |
| BOOK-26 | Boundary/negative | Automatic | Booking | Negative total | A valid Booking object (member booking, seat C5) with every field correct except the one tested | Total -1 | Nothing is stored or changed | Rejected | as expected | Pass | 2026-11-16 | Akilesh | - |
| BOOK-27 | Boundary/negative | Automatic | Booking | Total of zero | A valid Booking object (member booking, seat C5) with every field correct except the one tested | Total 0 (points pay for it) | Seats and the booking table match the expected result | Accepted | as expected | Pass | 2026-11-16 | Akilesh | - |
| BOOK-28 | Black-box | Automatic | Booking | Guest booking with points | A valid Booking object (member booking, seat C5) with every field correct except the one tested | MemberId empty, points used 100 | Nothing is stored or changed | Rejected (a guest booking cannot earn or use points) | as expected | Pass | 2026-11-16 | Akilesh | - |

### Guest booking

A guest books without an account and gives a name, email and phone. Guest details are only checked when there is no member on the booking.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| GUEST-01 | Integration | Automatic | Guest | Valid guest booking | TEST database with a showing whose seats are free; the person is not logged in | Name "Kevin", email "kevin@mail.com", phone "5712 3456", seat D4 | Seats and the booking table match the expected result | Booking made, no member on it, reference given |  | Not run |  | Akilesh |  |
| GUEST-02 | Black-box | Automatic | Guest | Guest name missing | A guest booking object (no member id) with every field correct except the one tested | Name "" | Nothing is stored or changed | Refused | as expected | Pass | 2026-11-16 | Akilesh | - |
| GUEST-03 | Black-box | Automatic | Guest | Guest email invalid | A guest booking object (no member id) with every field correct except the one tested | Email "kevin.mail.com" | Nothing is stored or changed | Refused | as expected | Pass | 2026-11-16 | Akilesh | - |
| GUEST-04 | Black-box | Automatic | Guest | Guest phone invalid | A guest booking object (no member id) with every field correct except the one tested | Phone "abcd" | Nothing is stored or changed | Refused | as expected | Pass | 2026-11-16 | Akilesh | - |
| GUEST-05 | Integration | Automatic | Guest | Guest earns no points | TEST database with a showing whose seats are free; the person is not logged in | Guest books seats worth Rs 500 | Seats and the booking table match the expected result | Points earned is 0 |  | Not run |  | Akilesh |  |
| GUEST-06 | Integration | Automatic | Guest | Guest cannot use points | TEST database with a showing whose seats are free; the person is not logged in | Guest tries to use 100 points | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| GUEST-07 | Integration | Automatic | Guest | Find booking with reference and email | TEST database with a showing whose seats are free; the person is not logged in | Correct reference and email | Seats and the booking table match the expected result | Booking found |  | Not run |  | Akilesh |  |
| GUEST-08 | Integration | Automatic | Guest | Find booking with the wrong email | TEST database with a showing whose seats are free; the person is not logged in | Correct reference, wrong email | Nothing is stored or changed | Not found, no details shown |  | Not run |  | Akilesh |  |
| GUEST-09 | Integration | Automatic | Guest | Guest cancels with reference and email | TEST database with a showing whose seats are free; the person is not logged in | Correct reference and email | Seats and the booking table match the expected result | Cancelled, seats freed |  | Not run |  | Akilesh |  |
| GUEST-10 | Integration | Automatic | Guest | Guest cancels with the wrong email | TEST database with a showing whose seats are free; the person is not logged in | Correct reference, wrong email | Nothing is stored or changed | Refused, booking unchanged |  | Not run |  | Akilesh |  |
| GUEST-11 | Integration | Automatic | Guest | Member booking ignores guest fields | TEST database with a showing whose seats are free; the person is not logged in | Member booking with empty guest name | Seats and the booking table match the expected result | Accepted, guest fields are not checked | as expected | Pass | 2026-11-16 | Akilesh | - |

### Loyalty

Rules decided 8 Nov. Standard: 0% discount, 1 point per Rs 10. Silver at 100 lifetime points: 5% discount, 1.25 times the points. Gold at 300 lifetime points: 10% discount, 1.5 times the points. 100 points are worth Rs 50 off. Worked example: 2 seats at Rs 250 is Rs 500 before discount.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| LOY-01 | Black-box | Automatic | Loyalty | Standard pays full price | No database needed; the loyalty rules are called directly with the stated tier and amount | Standard customer, total Rs 500 | No data changed | Discount 0, pays Rs 500 | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-02 | Black-box | Automatic | Loyalty | Silver gets 5% off | No database needed; the loyalty rules are called directly with the stated tier and amount | Silver customer, ticket total Rs 200 | No data changed | Pays Rs 190 | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-03 | Black-box | Automatic | Loyalty | Gold gets 10% off | No database needed; the loyalty rules are called directly with the stated tier and amount | Gold customer, ticket total Rs 200 | No data changed | Pays Rs 180 | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-04 | Black-box | Automatic | Loyalty | Higher tier pays less | No database needed; the loyalty rules are called directly with the stated tier and amount | Same Rs 200 ticket for Standard, Silver, Gold | No data changed | Gold pays Rs 180, Silver pays Rs 190, Standard pays Rs 200 | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-05 | Black-box | Automatic | Loyalty | Standard points earned, rounded down | No database needed; the loyalty rules are called directly with the stated tier and amount | Standard customer, amount paid Rs 295 | No data changed | Earns 29 points (29.5 rounded down) | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-06 | Black-box | Automatic | Loyalty | Silver points earned (1.25 times) | No database needed; the loyalty rules are called directly with the stated tier and amount | Silver customer, amount paid Rs 200 | No data changed | Earns 25 points (check against the test table in loyalty-rules.md) | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-07 | Black-box | Automatic | Loyalty | Gold points earned (1.5 times) | No database needed; the loyalty rules are called directly with the stated tier and amount | Gold customer, amount paid Rs 200 | No data changed | Earns 30 points (check against the test table in loyalty-rules.md) | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-08 | Boundary/negative | Automatic | Loyalty | Stay Standard just below Silver | No database needed; the loyalty rules are called directly with the stated tier and amount | Lifetime points 99 | No data changed | Tier is Standard | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-09 | Black-box | Automatic | Loyalty | Become Silver at 100 | No database needed; the loyalty rules are called directly with the stated tier and amount | Lifetime points 100 | No data changed | Tier is Silver | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-10 | Boundary/negative | Automatic | Loyalty | Stay Silver just below Gold | No database needed; the loyalty rules are called directly with the stated tier and amount | Lifetime points 299 | No data changed | Tier is Silver | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-11 | Black-box | Automatic | Loyalty | Become Gold at 300 | No database needed; the loyalty rules are called directly with the stated tier and amount | Lifetime points 300 | No data changed | Tier is Gold | as expected | Pass | 2026-11-16 | Akilesh | - |
| LOY-12 | Integration | Automatic | Loyalty | A booking that crosses a tier | TEST database with a member at the stated tier and points, and a confirmed booking | Customer at 95 lifetime points earns 10 | No data changed | Lifetime points 105, tier becomes Silver |  | Not run |  | Akilesh |  |
| LOY-13 | Black-box | Automatic | Loyalty | Use points for a discount | No database needed; the loyalty rules are called directly with the stated tier and amount | Standard customer with 100 points uses 100 on a Rs 500 booking | No data changed | Pays Rs 450, points used is 100, balance drops by 100 |  | Not run |  | Akilesh |  |
| LOY-14 | Black-box | Automatic | Loyalty | Use more points than owned | No database needed; the loyalty rules are called directly with the stated tier and amount | Customer with 50 points tries to use 100 | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| LOY-15 | Black-box | Automatic | Loyalty | Use zero points | No database needed; the loyalty rules are called directly with the stated tier and amount | Uses 0 | No data changed | Accepted, no discount from points |  | Not run |  | Akilesh |  |
| LOY-16 | Black-box | Automatic | Loyalty | Use negative points | No database needed; the loyalty rules are called directly with the stated tier and amount | Uses -10 | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| LOY-17 | Black-box | Automatic | Loyalty | Points used only in multiples of 100 | No database needed; the loyalty rules are called directly with the stated tier and amount | Customer with 300 points tries to use 150 | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| LOY-18 | Black-box | Automatic | Loyalty | Points discount never takes the price below Rs 0 | No database needed; the loyalty rules are called directly with the stated tier and amount | Customer uses points worth more than the amount to pay | No data changed | Amount to pay is Rs 0, never below |  | Not run |  | Akilesh |  |
| LOY-19 | Integration | Automatic | Loyalty | Cancel takes back earned points | TEST database with a member at the stated tier and points, and a confirmed booking | Cancel a booking that earned 50 points | No data changed | Balance goes down by 50 |  | Not run |  | Akilesh |  |
| LOY-20 | Integration | Automatic | Loyalty | Cancel returns used points | TEST database with a member at the stated tier and points, and a confirmed booking | Cancel a booking that used 100 points | No data changed | Balance goes up by 100 |  | Not run |  | Akilesh |  |
| LOY-21 | Integration | Automatic | Loyalty | Cancel never takes the balance below 0 | TEST database with a member at the stated tier and points, and a confirmed booking | Cancel after the earned points were already used | No data changed | Balance is 0 at the lowest, never negative |  | Not run |  | Akilesh |  |
| LOY-22 | Integration | Automatic | Loyalty | Cancel can lower the tier | TEST database with a member at the stated tier and points, and a confirmed booking | Cancel a booking that moved the member to Silver | No data changed | Tier goes back down, because the cancel takes back the earned points (confirm with Divyesh) |  | Not run |  | Akilesh |  |
| LOY-23 | Integration | Automatic | Loyalty | Staff and Admin do not earn points | TEST database with a member at the stated tier and points, and a confirmed booking | Staff books for a walk-in | No data changed | No points earned |  | Not run |  | Akilesh |  |
| LOY-24 | Black-box | Automatic | Loyalty | Order of steps: tier discount, then points discount, then points earned on what is paid | No database needed; the loyalty rules are called directly with the stated tier and amount | Standard customer, ticket Rs 500, uses 100 points | No data changed | Pays Rs 450 and earns 45 points (not 50) |  | Not run |  | Akilesh |  |
| LOY-25 | Black-box | Automatic | Loyalty | Same order for a Silver customer | No database needed; the loyalty rules are called directly with the stated tier and amount | Silver customer, ticket Rs 200, uses 100 points | No data changed | Tier discount makes it Rs 190, points make it Rs 140, earns 17 points (14 x 1.25 = 17.5, rounded down) |  | Not run |  | Akilesh |  |

### Login and sign-up

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| LOGIN-01 | White-box | Automatic | Login | Correct email and password | TEST database with the users table; the program is at the login or sign-up step | Valid user, correct password | User state matches the expected result; no password stored in plain text | Login succeeds |  | Not run |  | Akilesh |  |
| LOGIN-02 | White-box | Automatic | Login | Wrong password once | TEST database with the users table; the program is at the login or sign-up step | Valid user, wrong password | Nothing is stored or changed | Refused, failed attempts becomes 1 |  | Not run |  | Akilesh |  |
| LOGIN-03 | White-box | Automatic | Login | Unknown email | TEST database with the users table; the program is at the login or sign-up step | Email that does not exist | Nothing is stored or changed | Refused with a clear message |  | Not run |  | Akilesh |  |
| LOGIN-04 | White-box | Automatic | Login | Three wrong tries lock the account | TEST database with the users table; the program is at the login or sign-up step | Valid user, wrong password 3 times | User state matches the expected result; no password stored in plain text | Account is locked |  | Not run |  | Akilesh |  |
| LOGIN-05 | White-box | Automatic | Login | Correct password while locked | TEST database with the users table; the program is at the login or sign-up step | Locked user, correct password | Nothing is stored or changed | Refused, account still locked |  | Not run |  | Akilesh |  |
| LOGIN-06 | White-box | Automatic | Login | Lock ends after the lock time | TEST database with the users table; the program is at the login or sign-up step | Locked user, wait past [LOCK_TIME] | User state matches the expected result; no password stored in plain text | Login works again |  | Not run |  | Akilesh |  |
| LOGIN-07 | White-box | Automatic | Login | Counter resets after a good login | TEST database with the users table; the program is at the login or sign-up step | 2 wrong tries, then correct password | User state matches the expected result; no password stored in plain text | Login succeeds, failed attempts back to 0 (TBC with Shankar) |  | Not run |  | Akilesh |  |
| LOGIN-08 | White-box | Automatic | Login | Empty email | TEST database with the users table; the program is at the login or sign-up step | Email "" | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| LOGIN-09 | White-box | Automatic | Login | Empty password | TEST database with the users table; the program is at the login or sign-up step | Password "" | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| LOGIN-10 | White-box | Automatic | Login | Password is not stored as plain text | TEST database with the users table; the program is at the login or sign-up step | Create a user with password "abc123" | User state matches the expected result; no password stored in plain text | Stored hash is not "abc123"; two users with the same password get different hashes |  | Not run |  | Akilesh |  |
| SIGN-01 | White-box | Automatic | Sign-up | Valid sign-up | TEST database with the users table; the program is at the login or sign-up step | New email, valid password and details | User state matches the expected result; no password stored in plain text | Account made, role Customer, a member record is made with tier Standard and 0 points |  | Not run |  | Akilesh |  |
| SIGN-02 | White-box | Automatic | Sign-up | Sign up with an email already used | TEST database with the users table; the program is at the login or sign-up step | Existing email | Nothing is stored or changed | Refused with a clear message |  | Not run |  | Akilesh |  |
| SIGN-03 | White-box | Automatic | Sign-up | Invalid email | TEST database with the users table; the program is at the login or sign-up step | Email "abc" | Nothing is stored or changed | Refused |  | Not run |  | Akilesh |  |
| SIGN-04 | White-box | Automatic | Sign-up | Empty or too short password is refused | TEST database with the users table; the program is at the login or sign-up step | Password "" and a password of 7 characters | User state matches the expected result; no password stored in plain text | Both refused (minimum is 8 characters) |  | Not run |  | Akilesh |  |
| SIGN-05 | White-box | Automatic | Sign-up | Sign-up cannot make a Staff or Admin | TEST database with the users table; the program is at the login or sign-up step | Try to sign up with role Staff | User state matches the expected result; no password stored in plain text | Account is a Customer or the request is refused |  | Not run |  | Akilesh |  |
| SIGN-06 | White-box | Automatic | Sign-up | Password at the minimum length | TEST database with the users table; the program is at the login or sign-up step | Password of 8 characters | User state matches the expected result; no password stored in plain text | Accepted |  | Not run |  | Akilesh |  |
| FIRST-01 | White-box | Automatic | First admin | No users exist | TEST database with the users table; the program is at the login or sign-up step | Start with an empty users table | User state matches the expected result; no password stored in plain text | The create-first-admin step is offered |  | Not run |  | Akilesh |  |
| FIRST-02 | White-box | Automatic | First admin | Users already exist | TEST database with the users table; the program is at the login or sign-up step | Start with at least one user | User state matches the expected result; no password stored in plain text | The create-first-admin step is not offered |  | Not run |  | Akilesh |  |
| FIRST-03 | White-box | Automatic | First admin | No default password | TEST database with the users table; the program is at the login or sign-up step | Check the code and database script | User state matches the expected result; no password stored in plain text | No default or hard-coded password exists |  | Not run |  | Akilesh |  |

### Roles

The rules come from the "who may do what" table in the team's AI context file (section 6). Each test calls the service directly, with no window.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| ROLE-01 | White-box | Automatic | Roles | Everyone can browse | A service that checks the role, called as the role in the test data | Guest, Customer, Staff, Admin browse films, showings, seat map | User state matches the expected result; no password stored in plain text | Allowed for all four |  | Not run |  | Akilesh |  |
| ROLE-02 | White-box | Automatic | Roles | Everyone can book for themselves | A service that checks the role, called as the role in the test data | Guest (with details), Customer, Staff, Admin book seats | User state matches the expected result; no password stored in plain text | Allowed for all four |  | Not run |  | Akilesh |  |
| ROLE-03 | White-box | Automatic | Roles | Only Staff and Admin book for a walk-in | A service that checks the role, called as the role in the test data | Each of the four books for a walk-in | User state matches the expected result; no password stored in plain text | Staff and Admin allowed; Customer and Guest refused |  | Not run |  | Akilesh |  |
| ROLE-04 | White-box | Automatic | Roles | Cancel own booking | A service that checks the role, called as the role in the test data | Each role cancels its own booking (guest uses reference and email) | User state matches the expected result; no password stored in plain text | Allowed for all four |  | Not run |  | Akilesh |  |
| ROLE-05 | White-box | Automatic | Roles | Only Staff and Admin cancel any booking | A service that checks the role, called as the role in the test data | Customer tries to cancel another customer's booking | Nothing is stored or changed | Refused; Staff and Admin allowed |  | Not run |  | Akilesh |  |
| ROLE-06 | White-box | Automatic | Roles | Only customers earn and use points | A service that checks the role, called as the role in the test data | Each role books and tries to use points | User state matches the expected result; no password stored in plain text | Customer allowed; Guest, Staff and Admin refused |  | Not run |  | Akilesh |  |
| ROLE-07 | White-box | Automatic | Roles | Register and edit members | A service that checks the role, called as the role in the test data | Each role registers a member | User state matches the expected result; no password stored in plain text | Staff and Admin allowed; Guest refused |  | Not run |  | Akilesh |  |
| ROLE-08 | White-box | Automatic | Roles | Customer edits own details only | A service that checks the role, called as the role in the test data | Customer edits own details, then another member's | User state matches the expected result; no password stored in plain text | Own allowed; other member's refused |  | Not run |  | Akilesh |  |
| ROLE-09 | White-box | Automatic | Roles | Only Admin manages films, posters, showings, screens | A service that checks the role, called as the role in the test data | Staff, Customer, Guest try to add a film | User state matches the expected result; no password stored in plain text | All three refused; Admin allowed |  | Not run |  | Akilesh |  |
| ROLE-10 | White-box | Automatic | Roles | Only Admin loads a file or syncs the database | A service that checks the role, called as the role in the test data | Staff tries to load a file | Nothing is stored or changed | Refused; Admin allowed |  | Not run |  | Akilesh |  |
| ROLE-11 | White-box | Automatic | Roles | Only Admin manages staff accounts and tier rules | A service that checks the role, called as the role in the test data | Staff tries to create a staff account | Nothing is stored or changed | Refused; Admin allowed |  | Not run |  | Akilesh |  |
| ROLE-12 | White-box | Automatic | Roles | Only Admin sees the audit log | A service that checks the role, called as the role in the test data | Staff tries to open the audit log | Nothing is stored or changed | Refused; Admin allowed |  | Not run |  | Akilesh |  |
| ROLE-13 | White-box | Automatic | Roles | The check is in the service, not only the button | A service that checks the role, called as the role in the test data | Guest calls the add-film service directly | Nothing is stored or changed | Refused even though no button is involved |  | Not run |  | Akilesh |  |
| ROLE-14 | White-box | Automatic | Roles | Refused actions are not changed | A service that checks the role, called as the role in the test data | Staff tries to remove a film, then list films | User state matches the expected result; no password stored in plain text | The film is still there |  | Not run |  | Akilesh |  |
| ROLE-15 | White-box | Automatic | Roles | AI assistant follows the role | A service that checks the role, called as the role in the test data | Guest asks the assistant to remove a film | Nothing is stored or changed | Refused with the same rule as the button |  | Not run |  | Akilesh |  |

### Windows (hand tests, Irwin refines)

These are starter rows. Irwin owns the detailed steps and the full journeys.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| WIN-01 | System | Manual | Windows | Landing page options | Program running on the TEST database, window open as the stated user | Start the program | The window shows the expected result; nothing else changed | Log in, Sign up and Continue as guest are shown |  | Not run |  | Irwin |  |
| WIN-02 | System | Manual | Windows | Login with both fields blank | Program running on the TEST database, window open as the stated user | Click Login with nothing typed | The window shows the expected result; nothing else changed | Clear message, no crash |  | Not run |  | Irwin |  |
| WIN-03 | System | Manual | Windows | Side menu shows only allowed items | Program running on the TEST database, window open as the stated user | Log in as Customer, then as Admin | The window shows the expected result; nothing else changed | Customer sees no admin items; Admin sees all |  | Not run |  | Irwin |  |
| WIN-04 | System | Manual | Windows | Film browse and search | Program running on the TEST database, window open as the stated user | "inc", then "INCEPTION", then genre "Sci-Fi" | The window shows the expected result; nothing else changed | Finds Inception both times; genre lists Sci-Fi films |  | Not run |  | Irwin |  |
| WIN-05 | System | Manual | Windows | Search with no match | Program running on the TEST database, window open as the stated user | "zzzz" | The window shows the expected result; nothing else changed | Empty result with a message, no crash |  | Not run |  | Irwin |  |
| WIN-06 | System | Manual | Windows | Missing poster image | Program running on the TEST database, window open as the stated user | Film whose poster file is not on disk | The window shows the expected result; nothing else changed | Plain placeholder card with the title, no crash |  | Not run |  | Irwin |  |
| WIN-07 | System | Manual | Windows | Seat map colours | Program running on the TEST database, window open as the stated user | Open a showing with some taken seats | The window shows the expected result; nothing else changed | Free, taken and selected seats look different |  | Not run |  | Irwin |  |
| WIN-08 | System | Manual | Windows | Taken seat cannot be picked | Program running on the TEST database, window open as the stated user | Click a taken seat | The window shows the expected result; nothing else changed | Nothing is selected |  | Not run |  | Irwin |  |
| WIN-09 | System | Manual | Windows | Guest booking offers sign in or sign up | Program running on the TEST database, window open as the stated user | Continue as guest, pick seats, go to the summary | The window shows the expected result; nothing else changed | Name, email and phone are asked; sign in and sign up are offered |  | Not run |  | Irwin |  |
| WIN-10 | System | Manual | Windows | Booking reference is shown | Program running on the TEST database, window open as the stated user | Finish a booking | The window shows the expected result; nothing else changed | Reference shown on the last screen |  | Not run |  | Irwin |  |
| WIN-11 | System | Manual | Windows | Cancel asks first | Program running on the TEST database, window open as the stated user | Click Cancel booking | The window shows the expected result; nothing else changed | Yes / No box appears |  | Not run |  | Irwin |  |
| WIN-12 | System | Manual | Windows | Letters in a number field | Program running on the TEST database, window open as the stated user | "abc" in a price or points field | The window shows the expected result; nothing else changed | Clear message, no crash |  | Not run |  | Irwin |  |
| WIN-13 | System | Manual | Windows | Very long text | Program running on the TEST database, window open as the stated user | 500 characters in a title or name | Nothing is stored or changed | Rejected or cut with a message, no crash |  | Not run |  | Irwin |  |
| WIN-14 | System | Manual | Windows | Load a file with some bad lines | Program running on the TEST database, window open as the stated user | CSV with 2 bad lines out of 50 | The window shows the expected result; nothing else changed | Message such as "Loaded 48, skipped 2" |  | Not run |  | Irwin |  |
| WIN-15 | System | Manual | Windows | Database is down | Program running on the TEST database, window open as the stated user | Stop SQL Server and start the program | The window shows the expected result; nothing else changed | Program says so and still works from a loaded file |  | Not run |  | Irwin |  |

### Black-box by hand and integration

Black-box rows are run by hand from the rules only. Integration rows use the separate TEST database and wait until the lecturer confirms the test database.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| BB-01 | Black-box | Manual | File loader | Load the sample file | TEST database loaded with the sample films and a member | Run the console program with F5, type the path of data/films_sample.csv | Data matches the expected result | Loaded and skipped counts match the number of good and bad lines counted by hand in the file |  | Not run |  | Akilesh |  |
| BB-02 | Black-box | Manual | File loader | File path that does not exist | TEST database loaded with the sample films and a member | Type a path to a file that is not there | Data matches the expected result | Clear message, no crash |  | Not run |  | Akilesh |  |
| INT-01 | Integration | Automatic | FilmService and Database | Add a film and read it back | TEST database loaded with the sample films and a member | Admin adds a film, then load all films from the TEST database | Data matches the expected result | The same film is returned |  | Not run |  | Irwin |  |
| INT-02 | Integration | Automatic | FilmService and Database | A refused action writes nothing | TEST database loaded with the sample films and a member | Staff tries to add a film, then load all films from the TEST database | Data matches the expected result | Film is not there |  | Not run |  | Irwin |  |

### Regression, security and performance

Added after Divyesh's rules of 16 Nov. The times in ms are written in the Actual result column when the test is run.

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| REG-01 | Regression | Automatic | All unit tests | Run the whole suite after a merge or a bug fix | Latest main merged into the branch | Test Explorer, Run All | Nothing changes | Every test that passed before still passes |  | Not run |  | Akilesh |  |
| REG-02 | Regression | Automatic | All unit tests | Run the whole suite after the last bug fix before the deadline | All open bugs Fixed | Test Explorer, Run All | Nothing changes | 0 failed; the date and the result are written in the summary |  | Not run |  | Akilesh |  |
| SEC-01 | Security | Manual | Windows | SQL injection text in the film search box | Program running on the TEST database | Text ' OR 1=1; DROP TABLE Films;-- | Films table unchanged | No error, no extra films, table still there |  | Not run |  | Akilesh |  |
| SEC-02 | Security | Manual | Login | SQL injection text in the email box at login | Program running on the TEST database | Email ' OR '1'='1 and any password | No session started | Login refused with the normal message |  | Not run |  | Akilesh |  |
| SEC-03 | Security | Manual | Sign-up | SQL injection text in the name box at sign-up | Program running on the TEST database | Name x'); DELETE FROM Members;-- | Members table unchanged | Refused or saved as plain text, no member lost |  | Not run |  | Akilesh |  |
| SEC-04 | Security | Manual | Login | Lock-out after 3 wrong passwords | One user exists | Wrong password 3 times, then the right one | User locked | Locked message shown; the right password is refused until the lock ends |  | Not run |  | Akilesh |  |
| SEC-05 | Security | Manual | Users table | No plain text passwords | One user exists | Open the Users table in the TEST database | No data changed | Password column holds a hash, never the typed password |  | Not run |  | Akilesh |  |
| SEC-06 | Security | Automatic | Services | A Guest calls an admin only method | Services created with the TEST database | Role Guest on AddFilm and RemoveFilm | No film added or removed | Refused for every admin only method (see FS-02 to FS-04) |  | Not run |  | Akilesh |  |
| PERF-01 | Performance | Automatic | Hash table | Add and find with about 1000 films | Empty HashTable<int, Film> | 1000 valid films, then Find on 10 of them | Table holds 1000 films | Time in ms written in Actual result; every film found |  | Not run |  | Akilesh |  |
| PERF-02 | Performance | Automatic | FilmService | Search by title with about 1000 films | FilmService holding 1000 films | SearchByTitle("a") | No data changed | Time in ms written in Actual result |  | Not run |  | Akilesh |  |
| PERF-03 | Performance | Automatic | BookingService | Make one booking | TEST database with a showing and a member | Seats C5, C6 | Booking saved | Time in ms written in Actual result |  | Not run |  | Akilesh |  |

### Data-side tests (Irwin's file test-plan-data-side.md, merged here when he is ready)

These are ideas for Irwin's section. They are not final rows.

- Hash tables for films, showings and members by email: add then find, find a missing key, duplicate key, two keys in one slot, remove first, middle and last in a chain, count after add and remove, empty table, many items to force a resize and check every item is still found.
- Seat grid: create for a screen size, a seat is free, book seats, list taken seats, first and last seat.
- Booking history linked list per member.
- Search by title (partial, any case) and by genre.
- File loader: good file, bad lines, empty file, missing columns, missing file.
- Database: save and read back, the same seat booked twice is refused, bad data, database down, SQL text stored as plain text.
- Users and audit log tables.

### AI agent tests (card 375, mine)

- The right tool is chosen for "find action films tonight" and "book 2 seats for Inception at 20:30".
- The role check runs before any service, so a Guest asking to remove a film is refused.
- The memory remembers an earlier turn and forgets the oldest after the limit.
- A bad or empty request is handled without a crash.
- The AI model never changes data directly. Only our services do.

## 3. Summary

Worked out on 16 Nov 2026. Update it after every run.

| Item | Count |
|---|---|
| Tests of type White-box | 63 |
| Tests of type Black-box | 46 |
| Tests of type Integration | 36 |
| Tests of type System | 15 |
| Tests of type Regression | 2 |
| Tests of type Performance | 3 |
| Tests of type Security | 6 |
| Tests of type Boundary/negative | 31 |
| Automatic tests | 180 |
| Manual tests | 22 |
| Total tests | 202 |
| Passed | 80 |
| Failed | 0 |
| Not run | 122 |
| Pass rate (passed out of tests run) | 100% |
| Open bugs | 0 |
