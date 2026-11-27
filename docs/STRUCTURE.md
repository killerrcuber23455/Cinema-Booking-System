# Project structure: Cinema Booking System

This file says where every file in the repo goes, who owns each part, and what each folder must contain. If you are not sure where to put something, look here first. If it is still unclear, ask Divyesh before you create a new folder.

## 1. The final layout of the repo

```
Cinema-Booking-System/
├── README.md                     how to compile, run and use the program (Kushal)
├── .gitignore                    keeps bin/, obj/, .vs/ and the real config file out of Git
├── CinemaBooking.sln             the Visual Studio solution
│
├── src/CinemaBooking/            the program
│   ├── Models/                   the classes that hold data and check it
│   ├── DataStructures/           our own hash table, linked list, seat grid
│   ├── Services/                 the rules and the role checks
│   ├── Data/                     files and SQL only
│   ├── UI/                       the Windows Forms windows
│   ├── Agent/                    the AI assistant (+10)
│   ├── appsettings.example.json  example config with FAKE values (committed)
│   └── appsettings.json          real config (NEVER committed, in .gitignore)
│
├── tests/CinemaBooking.Tests/    MSTest unit tests
│
├── database/
│   ├── cinema.sql                one script that builds every table from nothing
│   └── sample_data.sql           sample films, screens and showings
│
├── data/
│   └── films_sample.csv          sample file for the file loader
│
├── media/
│   ├── posters/                  film poster images
│   └── video/                    the demo video (MP4, 6 minutes or less)
│
└── docs/
    ├── STRUCTURE.md              this file
    ├── TEAM_RULES.md             how we code, commit and review
    ├── decisions/                the decision log and lecturer replies
    ├── design/                   pseudocode, complexity, data structure choice, UI design
    ├── testing/                  test plan, test results, hand tests
    ├── management/               Gantt, burn-down, board screenshots, per sprint
    ├── minutes/                  sprint planning and review minutes
    ├── standups/                 daily standup notes, per sprint
    └── report/                   report sections while writing, and the final PDF
```

Git does not keep empty folders. Every folder gets a short `README.md` that says what goes in it.

## 2. What each section must contain

### src/CinemaBooking/Models/
One class per file. Each class holds its data and checks it (validation).

| File | What it holds | Owner |
| --- | --- | --- |
| `Film.cs` | Id, Title, Genre, Year, DurationMinutes, AgeRating, Description, PosterFile | Shankar |
| `Screen.cs` | Id, Name, Rows, SeatsPerRow | Shankar |
| `Showing.cs` | Id, FilmId, ScreenId, StartsAt, Price | Shankar |
| `Member.cs` | Id, Name, Email, Phone, Tier, Points, LifetimePoints, JoinedAt | Shankar |
| `Booking.cs` | Id, Reference, ShowingId, MemberId (int?, null for a guest), GuestName, GuestEmail, GuestPhone, Seats (string[]), Total (decimal), PointsEarned, PointsUsed, Status, CreatedAt | Shankar |
| `BookingStatus.cs` | enum: Confirmed, Cancelled | Shankar |
| `User.cs`, `Role.cs`, `Tier.cs` | login user, the four roles, the three tiers | Shankar |

Rule: no SQL, no forms, no `MessageBox` here.

### src/CinemaBooking/DataStructures/
Our own data structures. No `List<>`, `Dictionary<>`, `HashSet<>`, `Queue<>` or `LinkedList<>`.

| File | What it does | Owner |
| --- | --- | --- |
| `Node.cs`, `LinkedList.cs` | the chain used inside the hash table, and booking history | Kushal |
| `HashTable.cs` | one table that works for any item with a key: add, find, remove, count, list all; resize at about 75% full. Used for films (by Id), showings (by Id) and members (by email) | Kushal |
| `SeatGrid.cs` | seats for one showing: is free, book, free, list taken | Kushal |

Rule: these files know nothing about films, forms or SQL.

### src/CinemaBooking/Services/
The rules. Every method that changes data checks the role first.

| File | What it does | Owner |
| --- | --- | --- |
| `FilmService.cs` | add, remove, search by title and genre | Shankar |
| `ShowingService.cs` | showings for a film, seat grid per showing | Shankar |
| `MemberService.cs` | register members, view tier | Shankar |
| `BookingService.cs` | book seats, make the reference, cancel | Shankar |
| `LoyaltyService.cs` | points, tiers, discount, using points | Shankar (rules written with Divyesh) |
| `AuthService.cs`, `PasswordHasher.cs` | login, lock-out, salted hash, role checks | Shankar |

Rule: services never touch a form, `MessageBox` or `Console`.

### src/CinemaBooking/Data/
Files and SQL only. Parameterised SQL only.

| File | What it does | Owner |
| --- | --- | --- |
| `Database.cs` | connect with ADO.NET, read and save films, showings, members, bookings | Kushal |
| `UserRepository.cs` | users and audit log | Kushal |
| `FileLoader.cs` | load films from a CSV the user picks, count bad lines | Kushal |
| `PosterStore.cs` | save and find poster images, placeholder when missing | Kushal |

### src/CinemaBooking/UI/
The windows. They only read input and show results. No rules, no SQL, no hash table code in a form.

| File | What it shows | Owner |
| --- | --- | --- |
| `UiTheme.cs`, `RoundedPanel.cs`, `InputCheck.cs` | colours, fonts, panels, shared input checks | Divyesh |
| `MainForm.cs`, `LandingForm.cs`, `LoginForm.cs`, `HomeForm.cs` | main window, landing, login, home with side menu | Divyesh |
| `FilmBrowseForm.cs`, `FilmDetailForm.cs` | poster cards and search, film detail with showings | Divyesh |
| `SeatMapForm.cs`, `BookingSummaryForm.cs` | seat map, booking summary with guest details | Divyesh |
| `MyBookingsForm.cs`, `MyBookingLine.cs`, `FindBookingForm.cs` | customer bookings, guest find and cancel | Divyesh |
| `MembersForm.cs`, `MemberLine.cs`, `MemberInput.cs` | Members page for staff and admin: list, find by email, register (editing a member is not built, card 342) | Divyesh |
| `AdminFilmsForm.cs`, `AdminShowingsForm.cs`, `AdminTiersForm.cs`, `FirstAdminForm.cs` | admin windows: films and posters, showings, tier rules, first admin (all in `UI/`, there is no `Admin/` folder) | Divyesh |

### src/CinemaBooking/Agent/
| File | What it does | Owner |
| --- | --- | --- |
| `IAiClient.cs` | the interface the agent talks to, so tests can use a fake client (card 372) | Shankar |
| `AiReply.cs`, `AiMessage.cs` | the reply and the message the AI client returns and sends | Shankar |
| `AiClient.cs` | calls the AI model's web API with HttpClient; key read from config | Shankar |
| `AgentLoop.cs` | reads the request, decides which service to call | Shankar |
| `Tools.cs` | the arms: call our search, member and booking services | Shankar |
| `MemoryQueue.cs`, `KeywordTable.cs` | conversation memory (our own queue), keyword matching | Kushal |

### tests/CinemaBooking.Tests/
One test file per thing tested. Test names say what they check: `BookSeats_MoreThanFree_IsRefused`.

| File | Owner |
| --- | --- |
| `FilmTests.cs`, `MemberBookingTests.cs`, `BookingLoyaltyTests.cs`, `AuthTests.cs`, `AgentTests.cs` | Akilesh |
| `HashTableTests.cs`, `ResizeSearchTests.cs`, `SeatGridTests.cs`, `DatabaseUserTests.cs` | Irwin |

### database/
| File | Owner |
| --- | --- |
| `cinema.sql`: tables films, screens, showings, members, bookings, booked_seats, users, audit_log | Kushal |
| `sample_data.sql`: 2 screens of 8 x 12, some films and showings | Kushal |

### docs/
| Folder | Files and names | Owner |
| --- | --- | --- |
| `decisions/` | `decision-log.md`, `lecturer-replies.md` | Krishna |
| `design/` | `data-structure-choice.md`, `pseudocode-films.md`, `complexity.md`, `seat-grid.md`, `ai-agent.md`, `validation-rules.md`, `ui-design.md` | Kushal, Shankar, Divyesh |
| `testing/` | `test-plan.md`, `test-plan-data-side.md`, `test-results.md`, `hand-tests.md`, `rules-check.md` | Akilesh, Irwin |
| `management/` | `Gantt_and_Burndown_Charts.xlsx`, `sprint-2/board.png`, `sprint-2/burndown.png` (one folder per sprint) | Krishna, Divyesh |
| `minutes/` | `sprint-2-planning.md`, `sprint-2-review.md` (one pair per sprint) | Krishna |
| `standups/` | `sprint-2/2026-11-09.md` (one file per standup day) | Divyesh |
| `report/` | `introduction.md`, `design.md`, `testing.md`, `conclusion.md`, `references.md`, final `report.pdf` | Krishna, Kushal, Akilesh, Divyesh, Irwin |

### File names
- Docs: lowercase with hyphens, `.md` (for example `sprint-2-review.md`).
- Dates in file names: `YYYY-MM-DD` (for example `2026-11-09.md`).
- C# files: PascalCase, one class per file, file name = class name.
- Images: lowercase with hyphens (`sprint-2-burndown.png`).

## 3. Each person's part of the repo

| Person | Folders you work in | Main files |
| --- | --- | --- |
| **Divyesh** | `src/CinemaBooking/UI/` (all windows), `docs/standups/`, `docs/management/`, `media/video/` | every form, UiTheme, InputCheck, Admin windows, standups, video |
| **Shankar** | `src/CinemaBooking/Models/`, `Services/`, `Agent/` (AiClient, AgentLoop, Tools) | classes, validation, services, role checks, loyalty, login logic, AI agent |
| **Kushal** | `src/CinemaBooking/DataStructures/`, `Data/`, `Agent/` (memory, keywords), `database/`, `data/`, `README.md` | hash table, seat grid, SQL script, database code, file loader, posters |
| **Krishna** | `docs/decisions/`, `docs/minutes/`, `docs/management/`, `docs/report/` (introduction, references) | decision log, minutes, Gantt and burn-down, user stories, research |
| **Akilesh** | `tests/CinemaBooking.Tests/` (his files), `docs/testing/` | test plan, Film, Member, Booking, loyalty, login and agent tests |
| **Irwin** | `tests/CinemaBooking.Tests/` (his files), `docs/testing/` | hash table, resize, seat grid, database tests, hand tests, report layout check |

If you need to change a file that someone else owns, tell them first, and say which file and why.
