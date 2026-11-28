<!-- validation rules table for film, member and booking (owner: shankar) -->

# Validation rules: Film, Member and Booking

Card 394. This table lists every field of the three model classes, the rule it must pass, and the exact message the code gives when the rule fails. The tests are written from this table.

## How the code works

- Every field has a static method `IsValidX(value, out string error_message)`. It returns `true` with an empty message, or `false` with the message below.
- `IsValid(out errors)` on each object checks every field and joins all the messages, each ending with a new line. A valid object returns `true` and `errors` is empty.
- Messages are lowercase, as written in the code. The year message uses next year's number, worked out when the check runs (shown here as 2027).
- Namespace: `CinemaBooking.Models`. Enums: `Tier` (Standard, Silver, Gold) and `BookingStatus` (Confirmed, Cancelled).

## Film

| Field | Rule | Message shown when wrong |
| --- | --- | --- |
| Id | positive number (greater than 0) | film id must be a positive number |
| Title | not empty or spaces only, max 100 characters | title cannot be empty and must be 100 characters or fewer |
| Genre | not empty or spaces only, max 50 characters | genre cannot be empty and must be maximum 50 in length |
| Year | 1900 to next year | year must be between 1900 and 2027 |
| DurationMinutes | 1 to 300 | duration must be between 1 and 300 minutes |
| AgeRating | 0, 12, 15 or 18 | age rating must be 0, 12, 15 or 18 |
| Description | optional (null or empty is fine), max 500 characters | description must be 500 characters or fewer |
| PosterFile | optional (null, empty or spaces is fine; a placeholder is shown); if given, a file name only (no folder path), max 100 characters | poster must be a file name only, up to 100 characters |

## Member

| Field | Rule | Message shown when wrong |
| --- | --- | --- |
| Id | positive number (greater than 0) | member id should be a positive number |
| Name | not empty or spaces only, max 100 characters | name cannot be empty and must be 100 characters or fewer |
| Email (empty or too long) | not empty or spaces only, max 100 characters | email cant be empty and must be 100 characters or fewer |
| Email (shape) | exactly one @ that is not the first character; a dot after the @ with at least one character between them; at least one character after the last dot; no spaces | email must look like name@site.com |
| Phone | required; 7 to 15 digits; only digits, spaces, + or - allowed | phone must have 7 to 15 digits and only digits, spaces, + or - |
| Tier | one of Standard, Silver, Gold | tier must be Standard, Silver or Gold |
| Points and LifetimePoints | both 0 or more; Points not above LifetimePoints (equal is allowed) | points must be 0 or more and cannot be above lifetime points |
| JoinedAt | a real date (not the default date), today or earlier | join date must be a real date, not a future day |

## Booking

| Field | Rule | Message shown when wrong |
| --- | --- | --- |
| Id | positive number (greater than 0) | booking id should be a positive number |
| Reference | not empty or spaces only (no format check; BookingService makes it) | booking reference cannot be empty |
| ShowingId | positive number (greater than 0) | showing id should be a positive number |
| MemberId | null (guest booking) or a positive number | member id should be a positive number or empty(null) for a guest |
| GuestName | only checked when MemberId is null; then same rule as Member Name | guest name cannot be empty and must be 100 characters or fewer |
| GuestEmail | only checked when MemberId is null; then same rule as Member Email | guest email cant be empty and must be 100 characters or fewer, or: guest email must look like name@site.com |
| GuestPhone | only checked when MemberId is null; then same rule as Member Phone | guest phone must have 7 to 15 digits and only digits, spaces, + or - |
| Seats (count) | at least 1 seat | a booking needs at least 1 seat |
| Seats (code) | each code is a capital letter followed by digits, no leading zero (C5 and A12 pass; c5, C, 5C, C05 and C0 fail) | seat code must be a letter then a number, for example C5 |
| Seats (grid range) | not checked here. A13 and I1 pass `IsValidSeats` because only the format is checked; the seat grid (rows A to H, seats 1 to 12) refuses them | (no message from Booking; the seat grid refuses it) |
| Seats (repeat) | the same seat cannot appear twice | seat C5 is listed more than once (shows the repeated code) |
| Total | 0 or more (0 is allowed when points pay for the ticket) | total cannot be negative |
| PointsEarned and PointsUsed | both 0 or more | points earned and points used cannot be negative |
| PointsEarned and PointsUsed (guest) | when MemberId is null, both must be 0 | a guest booking cannot earn or use points |
| Status | Confirmed or Cancelled | status must be Confirmed or Cancelled |
| CreatedAt | a real date (not the default date), today or earlier | booking date must be a real date, not a future day |

## Decisions (confirmed)

- Showing price 0: not allowed, price must be above 0.
- Showing start time in the past: not allowed.
- Overlapping showings in the same screen: not allowed.
- Seat booking is all or nothing: if one seat fails, none are booked.
- Points earned are rounded down to a whole number.
- Points can only be used in multiples of 100.
- Cancelling a booking never takes points below 0.
- Password minimum length: 8 characters.

## Notes for the testers

- A new film or member has no Id until it is given one. The service gives the Id before `IsValid` is called (see AddFilm in card 304), so `IsValid` always sees Id 1 or higher.
- Test objects should use Id 1 or higher, because Id 0 fails the Id rule.
- For a member booking (MemberId has a number) the three guest fields are not checked at all, so any value passes.
- `IsValid` with several bad fields should give several lines, one per bad field.
- Boundary values worth testing: title 100 and 101 characters, genre 50 and 51, description 500 and 501, duration 0, 1, 300 and 301, year 1899, 1900, next year and the year after, phone 6, 7, 15 and 16 digits, points equal to and above lifetime points.
