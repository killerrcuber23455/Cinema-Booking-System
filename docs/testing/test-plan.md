# Test plan: Cinema Film Booking and Membership System

Author: Akilesh Sookhareea (Tester 1)
Card: 354 (AB#354)
Status: draft 2, 8 Nov 2026. Rows marked TBC wait for Shankar's validation rules table or a team decision.

## 1. Testing approach

We test the system at three levels. Each level catches a different kind of mistake.

**Unit tests.** These check one class or one service method on its own. We test the rules in Film, Screen, Showing, Member and Booking, the booking service (choosing seats, cancelling, price), the loyalty service (discount, points, tiers), and the login and sign-up service (passwords, lock-out, roles). Unit tests need no window. Where possible they also need no real database, so they run fast and give the same result every time. We write them with MSTest, one idea per test, in the Arrange, Act, Assert pattern.

**Integration tests.** These check that parts work together: the services with the hash tables and seat grid, and the services with the SQL Server database (save and read back, bad data, database down, the same seat booked twice). Irwin writes the data-side part of this plan (hash tables, linked list, seat grid, search, file loader, database, users and audit log) and I merge it into this document.

**System tests.** These check the whole program through its windows. Irwin hand-tests every window and the full user journeys (guest books, guest signs up, customer books and cancels, staff books for a walk-in, admin adds a showing). He tries good input and bad input at every field and keeps screenshots. I help him and retest every fix.

**How we choose test values.** For every rule we test one normal value, one clearly wrong value, and the values on each side of the limit. For example, the seat map has rows A to H and seats 1 to 12, so we test A1 and H12 (accepted) and A0, A13 and I1 (refused). For tiers we test 99, 100, 299 and 300 lifetime points. We also test the empty case and the "does not exist" case.

**When tests are written.** We write the test cases and their expected results from the requirements and from Shankar's validation table, before the code exists or while it is being written. We never copy the program's output into the expected column. If the actual result differs from the expected result, either the code or our understanding of the rule is wrong, and we find out which.

**Bugs.** A failing test becomes a Bug card in Azure Boards with steps to repeat, expected result, actual result and severity. It is assigned to the developer who owns the code. After the fix we rerun all the unit tests, then repeat the failing steps, and only then close the card.

**What we do not test.** We do not test Visual Studio or the .NET framework itself. We do not test what the AI model says in its answers, only that our own code checks the role and runs the same services as the buttons.

## 2. Test-case table

Columns: ID, area, what is tested, input, expected result, actual result, pass/fail. The last two stay empty until the tests are run.

Limits written as [MAX_TITLE] and so on come from Shankar's validation table and are filled in once it is agreed. Money is in rupees.

### Film

Fields: Id, Title, Genre, Year, DurationMinutes, AgeRating, Description, PosterFile. Seats and price are not on Film. They belong to Showing and Screen.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| FILM-01 | Film | Valid film is accepted | Title "Inception", genre "Sci-Fi", year 2010, duration 148, age rating 12, description "A thief who steals secrets through dreams", poster "inception.jpg" | Film is accepted | | |
| FILM-02 | Film | Empty title | Title "" | Rejected with a clear message | | |
| FILM-03 | Film | Title with only spaces | Title "   " | Rejected with a clear message | | |
| FILM-04 | Film | Title at the maximum length | Title of [MAX_TITLE] characters | Accepted | | |
| FILM-05 | Film | Title one over the maximum | Title of [MAX_TITLE] + 1 characters | Rejected | | |
| FILM-06 | Film | Empty genre | Genre "" | Rejected | | |
| FILM-07 | Film | Year below the allowed range | Year [MIN_YEAR] - 1 | Rejected | | |
| FILM-08 | Film | Duration of zero | Duration 0 | Rejected | | |
| FILM-09 | Film | Negative duration | Duration -1 | Rejected | | |
| FILM-10 | Film | Age rating that is not allowed | Age rating "99" (valid ratings TBC with Shankar) | Rejected | | |
| FILM-11 | Film | Description one over the maximum | Description of [MAX_DESC] + 1 characters | Rejected | | |
| FILM-12 | Film | Poster file name left empty | PosterFile "" | TBC with Shankar (accepted with placeholder, or rejected) | | |

### Screen and Showing

A screen has Rows and SeatsPerRow (two screens of 8 rows by 12 seats to start). A showing has a film, a screen, a start time and a price.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| SHOW-01 | Showing | Valid showing is accepted | Existing film, existing screen, start time in the future, price 250 | Accepted | | |
| SHOW-02 | Showing | Film does not exist | Film ID not in the table | Rejected with a clear message | | |
| SHOW-03 | Showing | Screen does not exist | Screen ID not in the table | Rejected | | |
| SHOW-04 | Showing | Price of zero | Price 0 | TBC with Shankar (free showing allowed or not) | | |
| SHOW-05 | Showing | Negative price | Price -1 | Rejected | | |
| SHOW-06 | Showing | Start time in the past | Start time yesterday | TBC with Shankar (rejected, or allowed for old data) | | |
| SHOW-07 | Showing | Two showings overlap on the same screen | Same screen, same start time | TBC with Shankar (is there an overlap rule?) | | |
| SCREEN-01 | Screen | Valid screen | Name "Screen 1", rows 8, seats per row 12 | Accepted, 96 seats | | |
| SCREEN-02 | Screen | Zero rows | Rows 0 | Rejected | | |
| SCREEN-03 | Screen | Zero seats per row | Seats per row 0 | Rejected | | |
| SCREEN-04 | Screen | Rows over the maximum | Rows [MAX_ROWS] + 1 | Rejected | | |

### Member

Fields: Id, Name, Email, Phone, Tier, Points, LifetimePoints, JoinedAt.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| MEM-01 | Member | Valid Standard member | Name "Priya", email "priya@mail.com", phone "5712 3456", tier Standard | Accepted | | |
| MEM-02 | Member | Valid Silver member | Same details, tier Silver | Accepted | | |
| MEM-03 | Member | Valid Gold member | Same details, tier Gold | Accepted | | |
| MEM-04 | Member | Empty name | Name "" | Rejected | | |
| MEM-05 | Member | Name one over the maximum | Name of [MAX_NAME] + 1 characters | Rejected | | |
| MEM-06 | Member | Email with no @ | Email "priya.mail.com" | Rejected | | |
| MEM-07 | Member | Empty email | Email "" | Rejected | | |
| MEM-08 | Member | Duplicate email | Add a second member with email "priya@mail.com" | Rejected with a clear message | | |
| MEM-09 | Member | Phone with letters | Phone "abcd" | Rejected | | |
| MEM-10 | Member | Tier that does not exist | Tier "Platinum" | Rejected | | |
| MEM-11 | Member | Negative points | Points -1 | Rejected | | |
| MEM-12 | Member | Edit a member's details | Change phone of an existing member | Saved, other fields unchanged | | |
| MEM-13 | Member | Find a member by email | Email of an existing member | That member is returned | | |
| MEM-14 | Member | Find a member by an email that does not exist | Email "nobody@mail.com" | Nothing found, no crash | | |

### Booking with seats

Seat codes are a row letter A to H and a seat number 1 to 12, for example C5. The screens are 8 rows by 12 seats. Bookings have a reference such as CB-7K2M9Q, a status (Confirmed or Cancelled) and either a member or guest details.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| BOOK-01 | Booking | Book one free seat | Showing with all seats free, seat C5, customer | Booking made, status Confirmed, C5 is now taken | | |
| BOOK-02 | Booking | Book two seats together | Seats C5, C6 | Booking made, both seats taken | | |
| BOOK-03 | Booking | Taken seat is refused | Book C5 again on the same showing | Refused with a clear message, no change to the seats | | |
| BOOK-04 | Booking | One taken seat among several | Seats C6, C7 where C6 is taken | Refused, C7 is not taken either (all or nothing, TBC with Shankar) | | |
| BOOK-05 | Booking | Same seat in one request twice | Seats C5, C5 | Refused | | |
| BOOK-06 | Booking | No seats chosen | Empty seat list | Refused | | |
| BOOK-07 | Booking | First seat on the map | Seat A1 | Accepted | | |
| BOOK-08 | Booking | Last seat on the map | Seat H12 | Accepted | | |
| BOOK-09 | Booking | Seat number 0 | Seat A0 | Refused | | |
| BOOK-10 | Booking | Seat number one over the row | Seat A13 | Refused | | |
| BOOK-11 | Booking | Row letter one past the last | Seat I1 | Refused | | |
| BOOK-12 | Booking | Badly typed seat code | Seat "5C", "", "C" | Refused for each | | |
| BOOK-13 | Booking | Showing does not exist | Showing ID not in the table | Refused with a clear message | | |
| BOOK-14 | Booking | Member does not exist | Member ID not in the table | Refused with a clear message | | |
| BOOK-15 | Booking | Total price is seats times showing price | 2 seats at Rs 250, Standard member | Total is Rs 500 | | |
| BOOK-16 | Booking | Booking gets a reference | Make any booking | Reference is not empty and has the agreed format | | |
| BOOK-17 | Booking | Two bookings get different references | Make two bookings | The two references are different | | |
| BOOK-18 | Booking | Cancel frees the seats | Cancel the booking of C5, C6 | Status Cancelled, C5 and C6 are free again | | |
| BOOK-19 | Booking | Cancel twice is refused | Cancel the same booking a second time | Refused, seats and points unchanged | | |
| BOOK-20 | Booking | Cancel a booking that does not exist | Reference "CB-XXXXXX" | Refused with a clear message | | |
| BOOK-21 | Booking | A freed seat can be booked again | Book C5 after cancelling it | Accepted | | |
| BOOK-22 | Booking | Booking a showing that has already started | Showing start time in the past | TBC with Shankar | | |

### Guest booking

A guest books without an account and gives a name, email and phone. Guest details are only checked when there is no member on the booking.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| GUEST-01 | Guest | Valid guest booking | Name "Kevin", email "kevin@mail.com", phone "5712 3456", seat D4 | Booking made, no member on it, reference given | | |
| GUEST-02 | Guest | Guest name missing | Name "" | Refused | | |
| GUEST-03 | Guest | Guest email invalid | Email "kevin.mail.com" | Refused | | |
| GUEST-04 | Guest | Guest phone invalid | Phone "abcd" | Refused | | |
| GUEST-05 | Guest | Guest earns no points | Guest books seats worth Rs 500 | Points earned is 0 | | |
| GUEST-06 | Guest | Guest cannot use points | Guest tries to use 100 points | Refused | | |
| GUEST-07 | Guest | Find booking with reference and email | Correct reference and email | Booking found | | |
| GUEST-08 | Guest | Find booking with the wrong email | Correct reference, wrong email | Not found, no details shown | | |
| GUEST-09 | Guest | Guest cancels with reference and email | Correct reference and email | Cancelled, seats freed | | |
| GUEST-10 | Guest | Guest cancels with the wrong email | Correct reference, wrong email | Refused, booking unchanged | | |
| GUEST-11 | Guest | Member booking ignores guest fields | Member booking with empty guest name | Accepted, guest fields are not checked | | |

### Loyalty

Rules decided 8 Nov. Standard: 0% discount, 1 point per Rs 10. Silver at 100 lifetime points: 5% discount, 1.25 times the points. Gold at 300 lifetime points: 10% discount, 1.5 times the points. 100 points are worth Rs 50 off. Worked example: 2 seats at Rs 250 is Rs 500 before discount.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| LOY-01 | Loyalty | Standard pays full price | Standard customer, total Rs 500 | Discount 0, pays Rs 500 | | |
| LOY-02 | Loyalty | Silver gets 5% off | Silver customer, total Rs 500 | Pays Rs 475 | | |
| LOY-03 | Loyalty | Gold gets 10% off | Gold customer, total Rs 500 | Pays Rs 450 | | |
| LOY-04 | Loyalty | Higher tier pays less | Same booking for Standard, Silver, Gold | Gold total is lower than Silver, Silver is lower than Standard | | |
| LOY-05 | Loyalty | Standard points earned | Standard customer pays Rs 500 | Earns 50 points | | |
| LOY-06 | Loyalty | Silver points earned | Silver customer pays Rs 475 | Earns the Silver rate (1.25 times). Rounding and whether points use the price before or after discount: TBC with Shankar | | |
| LOY-07 | Loyalty | Gold points earned | Gold customer pays Rs 450 | Earns the Gold rate (1.5 times). Same TBC as above | | |
| LOY-08 | Loyalty | Stay Standard just below Silver | Lifetime points 99 | Tier is Standard | | |
| LOY-09 | Loyalty | Become Silver at 100 | Lifetime points 100 | Tier is Silver | | |
| LOY-10 | Loyalty | Stay Silver just below Gold | Lifetime points 299 | Tier is Silver | | |
| LOY-11 | Loyalty | Become Gold at 300 | Lifetime points 300 | Tier is Gold | | |
| LOY-12 | Loyalty | A booking that crosses a tier | Customer at 95 lifetime points earns 10 | Lifetime points 105, tier becomes Silver | | |
| LOY-13 | Loyalty | Use points for a discount | Standard customer with 100 points uses 100 on a Rs 500 booking | Pays Rs 450, points used is 100, balance drops by 100 | | |
| LOY-14 | Loyalty | Use more points than owned | Customer with 50 points tries to use 100 | Refused | | |
| LOY-15 | Loyalty | Use zero points | Uses 0 | Accepted, no discount from points | | |
| LOY-16 | Loyalty | Use negative points | Uses -10 | Refused | | |
| LOY-17 | Loyalty | Points amount that is not a multiple of 100 | Uses 150 | TBC with Shankar (allowed, rounded, or refused) | | |
| LOY-18 | Loyalty | Points discount larger than the total | Uses points worth more than the total | TBC with Shankar (capped at the total, or refused) | | |
| LOY-19 | Loyalty | Cancel takes back earned points | Cancel a booking that earned 50 points | Balance goes down by 50 | | |
| LOY-20 | Loyalty | Cancel returns used points | Cancel a booking that used 100 points | Balance goes up by 100 | | |
| LOY-21 | Loyalty | Points never go negative | Cancel after the earned points were already used | Balance is not below 0 (rule TBC with Shankar) | | |
| LOY-22 | Loyalty | Cancel after a tier change | Cancel a booking that moved the member to Silver | TBC with Shankar (does the tier go back down?) | | |
| LOY-23 | Loyalty | Staff and Admin do not earn points | Staff books for a walk-in | No points earned | | |

### Login and sign-up

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| LOGIN-01 | Login | Correct email and password | Valid user, correct password | Login succeeds | | |
| LOGIN-02 | Login | Wrong password once | Valid user, wrong password | Refused, failed attempts becomes 1 | | |
| LOGIN-03 | Login | Unknown email | Email that does not exist | Refused with a clear message | | |
| LOGIN-04 | Login | Three wrong tries lock the account | Valid user, wrong password 3 times | Account is locked | | |
| LOGIN-05 | Login | Correct password while locked | Locked user, correct password | Refused, account still locked | | |
| LOGIN-06 | Login | Lock ends after the lock time | Locked user, wait past [LOCK_TIME] | Login works again | | |
| LOGIN-07 | Login | Counter resets after a good login | 2 wrong tries, then correct password | Login succeeds, failed attempts back to 0 (TBC with Shankar) | | |
| LOGIN-08 | Login | Empty email | Email "" | Refused | | |
| LOGIN-09 | Login | Empty password | Password "" | Refused | | |
| LOGIN-10 | Login | Password is not stored as plain text | Create a user with password "abc123" | Stored hash is not "abc123"; two users with the same password get different hashes | | |
| SIGN-01 | Sign-up | Valid sign-up | New email, valid password and details | Account made, role Customer, a member record is made with tier Standard and 0 points | | |
| SIGN-02 | Sign-up | Sign up with an email already used | Existing email | Refused with a clear message | | |
| SIGN-03 | Sign-up | Invalid email | Email "abc" | Refused | | |
| SIGN-04 | Sign-up | Empty or too short password | Password "" and one under [MIN_PASSWORD] | Refused (minimum length TBC with Shankar) | | |
| SIGN-05 | Sign-up | Sign-up cannot make a Staff or Admin | Try to sign up with role Staff | Account is a Customer or the request is refused | | |
| FIRST-01 | First admin | No users exist | Start with an empty users table | The create-first-admin step is offered | | |
| FIRST-02 | First admin | Users already exist | Start with at least one user | The create-first-admin step is not offered | | |
| FIRST-03 | First admin | No default password | Check the code and database script | No default or hard-coded password exists | | |

### Roles

The rules come from the "who may do what" table in the team's AI context file (section 6). Each test calls the service directly, with no window.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| ROLE-01 | Roles | Everyone can browse | Guest, Customer, Staff, Admin browse films, showings, seat map | Allowed for all four | | |
| ROLE-02 | Roles | Everyone can book for themselves | Guest (with details), Customer, Staff, Admin book seats | Allowed for all four | | |
| ROLE-03 | Roles | Only Staff and Admin book for a walk-in | Each of the four books for a walk-in | Staff and Admin allowed; Customer and Guest refused | | |
| ROLE-04 | Roles | Cancel own booking | Each role cancels its own booking (guest uses reference and email) | Allowed for all four | | |
| ROLE-05 | Roles | Only Staff and Admin cancel any booking | Customer tries to cancel another customer's booking | Refused; Staff and Admin allowed | | |
| ROLE-06 | Roles | Only customers earn and use points | Each role books and tries to use points | Customer allowed; Guest, Staff and Admin refused | | |
| ROLE-07 | Roles | Register and edit members | Each role registers a member | Staff and Admin allowed; Guest refused | | |
| ROLE-08 | Roles | Customer edits own details only | Customer edits own details, then another member's | Own allowed; other member's refused | | |
| ROLE-09 | Roles | Only Admin manages films, posters, showings, screens | Staff, Customer, Guest try to add a film | All three refused; Admin allowed | | |
| ROLE-10 | Roles | Only Admin loads a file or syncs the database | Staff tries to load a file | Refused; Admin allowed | | |
| ROLE-11 | Roles | Only Admin manages staff accounts and tier rules | Staff tries to create a staff account | Refused; Admin allowed | | |
| ROLE-12 | Roles | Only Admin sees the audit log | Staff tries to open the audit log | Refused; Admin allowed | | |
| ROLE-13 | Roles | The check is in the service, not only the button | Guest calls the add-film service directly | Refused even though no button is involved | | |
| ROLE-14 | Roles | Refused actions are not changed | Staff tries to remove a film, then list films | The film is still there | | |
| ROLE-15 | Roles | AI assistant follows the role | Guest asks the assistant to remove a film | Refused with the same rule as the button | | |

### Windows (hand tests, Irwin refines)

These are starter rows. Irwin owns the detailed steps and the full journeys.

| ID | Area | What is tested | Input | Expected result | Actual | Pass/Fail |
|---|---|---|---|---|---|---|
| WIN-01 | Windows | Landing page options | Start the program | Log in, Sign up and Continue as guest are shown | | |
| WIN-02 | Windows | Login with both fields blank | Click Login with nothing typed | Clear message, no crash | | |
| WIN-03 | Windows | Side menu shows only allowed items | Log in as Customer, then as Admin | Customer sees no admin items; Admin sees all | | |
| WIN-04 | Windows | Film browse and search | "inc", then "INCEPTION", then genre "Sci-Fi" | Finds Inception both times; genre lists Sci-Fi films | | |
| WIN-05 | Windows | Search with no match | "zzzz" | Empty result with a message, no crash | | |
| WIN-06 | Windows | Missing poster image | Film whose poster file is not on disk | Plain placeholder card with the title, no crash | | |
| WIN-07 | Windows | Seat map colours | Open a showing with some taken seats | Free, taken and selected seats look different | | |
| WIN-08 | Windows | Taken seat cannot be picked | Click a taken seat | Nothing is selected | | |
| WIN-09 | Windows | Guest booking offers sign in or sign up | Continue as guest, pick seats, go to the summary | Name, email and phone are asked; sign in and sign up are offered | | |
| WIN-10 | Windows | Booking reference is shown | Finish a booking | Reference shown on the last screen | | |
| WIN-11 | Windows | Cancel asks first | Click Cancel booking | Yes / No box appears | | |
| WIN-12 | Windows | Letters in a number field | "abc" in a price or points field | Clear message, no crash | | |
| WIN-13 | Windows | Very long text | 500 characters in a title or name | Rejected or cut with a message, no crash | | |
| WIN-14 | Windows | Load a file with some bad lines | CSV with 2 bad lines out of 50 | Message such as "Loaded 48, skipped 2" | | |
| WIN-15 | Windows | Database is down | Stop SQL Server and start the program | Program says so and still works from a loaded file | | |

### Data-side tests (Irwin's section, merged here later)

These are ideas for Irwin's section. They are not final rows.

- Hash tables for films, showings and members by email: add then find, find a missing key, duplicate key, two keys in one slot, remove first, middle and last in a chain, count after add and remove, empty table, many items to force a resize and check every item is still found.
- Seat grid: create for a screen size, a seat is free, book seats, list taken seats, first and last seat.
- Booking history linked list per member.
- Search by title (partial, any case) and by genre.
- File loader: good file, bad lines, empty file, missing columns, missing file.
- Database: save and read back, the same seat booked twice is refused, bad data, database down, SQL text stored as plain text.
- Users and audit log tables.

### AI agent tests (card 43, mine)

- The right tool is chosen for "find action films tonight" and "book 2 seats for Inception at 20:30".
- The role check runs before any service, so a Guest asking to remove a film is refused.
- The memory remembers an earlier turn and forgets the oldest after the limit.
- A bad or empty request is handled without a crash.
- The AI model never changes data directly. Only our services do.
