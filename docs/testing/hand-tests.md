# Hand tests: every window (card 363)

Author: Irwin Chinathoo (Tester 2)
Card: 363 (AB#363). Black-box system tests, done by hand through the screen only, without reading the code.
Build tested: `feature/divyesh`, Debug build (so the demo showings are loaded), on a local development database `CinemaBooking` built from `database/cinema.sql`. Never the real database.
Film file used: `films_sample.csv` (10 films load, 2 bad rows skipped). No film in this file has a poster image.
Status: run on 22 and 23 Nov 2026. Several pages are not built yet (accounts, saving a booking), so their tests are marked Not run and should be run again when those parts are merged.

How to read the table: each row is one thing to click or type. Expected result comes from the rules in file 27 and the decision log, not from the code. Every Fail gets a Bug card in Azure and a Bug ID here. Screenshots are saved in `docs/testing/screenshots/hand/` as `HT-xx.png`.

## Windows tested

| Window | Who sees it | Built? |
|---|---|---|
| Start-up (settings and database) | Everyone | Yes |
| Create first admin | First run only | Page yes, saving not yet |
| Log in | Everyone | Page and checks yes, logging in not yet |
| Sign up | Guest | Page and checks yes, saving not yet |
| Main window and side menu | Everyone, buttons depend on the role | Yes (guest menu only, no account can be made yet) |
| Films (browse and search) | Everyone | Yes |
| Film detail (showings) | Everyone | Yes |
| Seat map | Everyone | Yes |
| Booking summary (guest details) | Everyone | Yes, but the booking is not saved yet |
| Showings page | Staff, Admin | Not reachable yet (needs a login) |
| Admin films | Admin | Not reachable yet (needs a login) |
| My account | Everyone | Not reachable yet (needs a login) |
| Cancel booking and other placeholder pages | Depends on the role | Placeholder pages only |

## Test table

| Test ID | Test type | Mode | Area | What is tested | Preconditions | Test data | Post-conditions | Expected result | Actual result | Pass/Fail | Executed date | Tester | Bug ID | Screenshot |
|---|---|---|---|---|---|---|---|---|---|---|---|---|---|---|
| HT-01 | System | Manual | Start-up | App starts with no appsettings.json | appsettings.json renamed | Start the app | Nothing saved | Clear message that the settings file is missing, no crash | Not run this time | Not run | - | Irwin | - | - |
| HT-02 | System | Manual | Start-up | App starts with SQL Server stopped | SQL Server service stopped | Start the app, then try to log in | Nothing saved | Clear message that the database cannot be reached, no crash | Not run this time (covered by the automatic test DB-12b in the data-side plan) | Not run | - | Irwin | - | - |
| HT-03 | System | Manual | Create first admin | First run shows "create first admin" | Empty users table | Start the app, click Sign in, then Set up the first admin | - | The create-first-admin window appears, no default password anywhere | "Admin setup" page appears with email, password and password-again boxes, no default password | Pass | 23 Nov 2026 | Irwin | - | HT-03.png |
| HT-04 | Boundary/negative | Manual | Create first admin | Blank boxes | First run | Leave email and password empty, click create | No user saved | Clear message under the boxes, no crash | Red messages shown under the boxes, no crash | Pass | 23 Nov 2026 | Irwin | - | - |
| HT-05 | Boundary/negative | Manual | Create first admin | Short or wrong password | First run | Password "abc"; two passwords that do not match | No user saved | Refused, password must be at least 8 characters | Both refused with a red message ("passwords do not match" shown in the screenshot), no crash | Pass | 23 Nov 2026 | Irwin | - | HT-05.png |
| HT-06 | System | Manual | Create first admin | Valid admin | First run | admin@site.mu, password 8 or more characters | 1 admin saved | Message that the admin account was created, then the login page | Message "first admin not built yet", nothing saved | Not run (not built yet) | 23 Nov 2026 | Irwin | - | HT-06.png |
| HT-07 | System | Manual | Log in | Admin logs in | Admin from HT-06 | Right email and password | Signed in | Main window shows the admin menu (Films, Showings, Book seats, Cancel booking, Members, Admin, My account) | No account can be created yet | Not run | - | Irwin | - | - |
| HT-08 | Boundary/negative | Manual | Log in | Wrong password | Admin exists | Wrong password once | Failed attempts 1 | Clear message, not signed in, does not say whether the email exists | No account can be created yet | Not run | - | Irwin | - | - |
| HT-09 | Security | Manual | Log in | Lock-out after 3 wrong passwords | Admin exists | Wrong password 3 times, then the right one | Account locked | Locked after the 3rd try, the right password is refused until the lock ends | No account can be created yet | Not run | - | Irwin | - | - |
| HT-10 | Boundary/negative | Manual | Log in | Bad or blank boxes | Any | Email "12345", empty password | - | Clear message, no crash | "email must look like name@site.com" and "password cannot be empty" in red, no crash | Pass | 23 Nov 2026 | Irwin | - | HT-10.png |
| HT-11 | Security | Manual | Log in | SQL injection text | Any | Email `x'); DROP TABLE users;--`, any password | Users table unchanged | Refused like any wrong login, no crash, users can still log in afterwards | Not run this time (the same text is tested automatically in SEC-03) | Not run | - | Irwin | - | - |
| HT-12 | System | Manual | Sign up | New customer | Logged out | Name, new email, phone, password 8 or more | 1 customer and 1 member (Standard, 0 points) | Account created, can log in | Saving an account is not built yet | Not run (not built yet) | 22 Nov 2026 | Irwin | - | - |
| HT-13 | Boundary/negative | Manual | Sign up | Email already used | Customer from HT-12 | Same email, other capitals | No new user | Refused with a clear message | Needs HT-12 | Not run | - | Irwin | - | - |
| HT-14 | Boundary/negative | Manual | Sign up | Bad input | Logged out | Blank name; "abc" as email; letters as phone | No user saved | Each refused with a clear message, no crash | A red message shown for each bad box, no crash (300-letter name not tried) | Pass | 22 Nov 2026 | Irwin | - | - |
| HT-15 | System | Manual | Main window | Menu for a guest | Not signed in | - | - | Only Films, Cancel booking (find my booking), My account | Films, Cancel booking, Sign in and Exit. "Sign in" is shown instead of "My account", which makes sense for a guest | Pass | 23 Nov 2026 | Irwin | - | HT-15.png |
| HT-16 | System | Manual | Main window | Menu for a customer | Customer signed in | - | - | Only Films, My bookings, My account | No account can be created yet | Not run | - | Irwin | - | - |
| HT-17 | System | Manual | Main window | Menu for staff | Staff account | - | - | Films, Showings, Book seats, Cancel booking, Members, My account, no Admin | No account can be created yet | Not run | - | Irwin | - | - |
| HT-18 | System | Manual | Main window | Every menu button opens a page | Admin signed in | Click each button in turn | - | Each opens its page or a placeholder, no crash, every function in 2 clicks or fewer | No account can be created yet | Not run | - | Irwin | - | - |
| HT-19 | System | Manual | Films | Film list with posters | Films loaded | Open Films, load films_sample.csv | - | Poster cards with titles; a film with no poster shows a plain placeholder card | 10 film cards with title, genre, year, length and age rating. Every card shows the "(no poster)" placeholder (no film in this file has a poster image) | Pass | 23 Nov 2026 | Irwin | - | HT-19.png |
| HT-20 | System | Manual | Films | Search by part of a title, any case | Films loaded | "ORBIT" (there is no Inception in films_sample.csv) | - | Silent Orbit shown | Only Silent Orbit shown, "showing 1 film" | Pass | 23 Nov 2026 | Irwin | - | HT-20.png |
| HT-21 | System | Manual | Films | Search by genre | Films loaded | Pick Sci-Fi | - | Only Sci-Fi films | Only Silent Orbit (Sci-Fi) shown | Pass | 23 Nov 2026 | Irwin | - | HT-21.png |
| HT-22 | Boundary/negative | Manual | Films | Search with no match | Films loaded | "zzz" | - | Empty list with a message, no crash | "no film matches your search", no crash (500-letter text not tried) | Pass | 23 Nov 2026 | Irwin | - | HT-22.png |
| HT-23 | Security | Manual | Films | SQL injection text in the search box | Films loaded | `x'); DROP TABLE films;--` | Films unchanged | No match, no crash, all films still there after clearing the search | "no film matches your search", no crash, all 10 films back after clearing | Pass | 22 Nov 2026 | Irwin | - | - |
| HT-24 | System | Manual | Film detail | Open a film | Films loaded | Click The Last Voyage | - | Title, genre, year, age rating, description and its showings (date and time) | All shown, plus 3 showings with screen, price and seats left | Pass | 23 Nov 2026 | Irwin | - | HT-24.png |
| HT-25 | System | Manual | Film detail | Back button | Film detail open | Click "Back to films" | - | Returns to the film list | Returns to the film list | Pass | 22 Nov 2026 | Irwin | - | - |
| HT-26 | System | Manual | Seat map | Open a showing | Demo showing | Click Choose seats, pick A1, B2, C3 | - | Grid A1 to H12, free, taken and selected seats look different | 8 x 12 grid with a Free / Chosen / Taken key, chosen seats in red, "chosen: A1, B2, C3" | Pass | 23 Nov 2026 | Irwin | - | HT-26.png |
| HT-27 | System | Manual | Seat map | Select and unselect seats | Seat map open | Click two seats, then one of them again | - | First seat selected, second unselected | Clicking a chosen seat again unselects it and the "chosen" text updates | Pass | 22 Nov 2026 | Irwin | - | - |
| HT-28 | Boundary/negative | Manual | Seat map | Taken seat | 18:30 Screen 2 showing (5 seats taken) | Click a taken seat | - | Cannot be picked | Taken seats are dark and nothing happens when clicked | Pass | 22 Nov 2026 | Irwin | - | - |
| HT-29 | Boundary/negative | Manual | Seat map | Continue with no seats | Seat map open | Pick nothing | - | Clear message, stays on the seat map | "no seats chosen" and the Continue button is greyed out | Pass | 23 Nov 2026 | Irwin | - | HT-29.png |
| HT-30 | System | Manual | Booking summary | Guest books | Guest, seats picked | Name, email, phone | 2 seats taken | Price shown, booking reference shown at the end, offer to sign in or sign up | Price shown (1 x Rs 250.00) and a "Sign in to earn points" button, but the last page says "The booking is made when the booking service is connected." No booking saved, no reference | Not run (not built yet) | 23 Nov 2026 | Irwin | - | HT-30.png |
| HT-31 | Boundary/negative | Manual | Booking summary | Guest details missing or wrong | Guest, seat A1 picked | Blank name; email "abc"; phone "abcdefg" | Nothing booked | Each refused with a clear message | Three red messages: name cannot be empty, email must look like name@site.com, phone must have 7 to 15 digits | Pass | 23 Nov 2026 | Irwin | - | HT-31.png |
| HT-32 | System | Manual | Booking summary | Customer price and points | Customer signed in, seats picked | - | - | Tier discount and points to earn shown | No account can be created yet (as a guest the summary correctly says "none for guests, sign in to earn points") | Not run | - | Irwin | - | - |
| HT-33 | System | Manual | Showings page | Add a showing | Admin or staff | Film, screen, future date and time, price 250 | 1 more showing | Showing added and listed | Needs a staff or admin login | Not run | - | Irwin | - | - |
| HT-34 | Boundary/negative | Manual | Showings page | Bad showing input | Admin or staff | Price 0; price "abc"; date in the past | Nothing saved | Each refused with a message under the box | Needs a staff or admin login | Not run | - | Irwin | - | - |
| HT-35 | Boundary/negative | Manual | Showings page | Same screen, same time | A showing exists | Same screen and start time again | Nothing saved | Refused (decided 16 Nov) | Needs a staff or admin login | Not run | - | Irwin | - | - |
| HT-36 | System | Manual | Showings page | Remove a showing | A showing exists | Remove, then No, then Remove and Yes | Removed after Yes only | A Yes/No box first; No keeps it, Yes removes it | Needs a staff or admin login | Not run | - | Irwin | - | - |
| HT-37 | System | Manual | Admin films | Add a film with a poster | Admin | All 8 fields, pick an image | 1 more film | Film added, poster shown in Films | Needs an admin login | Not run | - | Irwin | - | - |
| HT-38 | Boundary/negative | Manual | Admin films | Bad film input | Admin | Blank title; year "abc"; age rating 14; duration 0 | Nothing saved | Each refused with a clear message | Needs an admin login | Not run | - | Irwin | - | - |
| HT-39 | System | Manual | Admin films | Remove a film | Admin, a film exists | Remove, Yes | Film gone | Yes/No box first, film no longer in Films | Needs an admin login | Not run | - | Irwin | - | - |
| HT-40 | Security | Manual | Admin films | Staff cannot reach admin pages | Staff signed in | Look for the Admin button | - | No Admin button | Needs a staff login | Not run | - | Irwin | - | - |
| HT-41 | System | Manual | My account | Details and sign out | Customer signed in | Open My account, click sign out | Signed out | Shows the email, role, tier and points; sign out returns to the guest menu | Needs a customer login | Not run | - | Irwin | - | - |
| HT-42 | System | Manual | Placeholder pages | Placeholder pages open | Guest | Open Cancel booking | - | Opens a "will be done here" page, no crash (not built yet) | "A booking is cancelled here with its reference." shown, no crash. The other placeholders need a login | Pass | 23 Nov 2026 | Irwin | - | HT-42.png |
| HT-43 | Performance | Manual | Films | Time to load and show the films | App open | Load films_sample.csv | - | Shows in under 2 seconds (counted) | About 1 second | Pass | 23 Nov 2026 | Irwin | - | - |
| HT-44 | System | Manual | Main window | Exit | Any | Click Exit | - | The program closes | Not run this time | Not run | - | Irwin | - | - |
| HT-45 | Security | Manual | Films | A guest cannot load the film file | Not signed in | Click "Load film file" as a guest | Films unchanged | Only an admin can load a film file, the button is hidden or refused for a guest | A guest can click "Load film file" and load 10 films | Fail | 22 Nov 2026 | Irwin | (Akilesh to number) | HT-45.png |

## Summary

| | Count |
|---|---|
| Tests in this plan | 45 |
| Passed | 20 |
| Failed | 1 (HT-45) |
| Not run, part not built yet (accounts, saving a booking) | 20 |
| Not run, skipped this time (HT-01, 02, 11, 44) | 4 |
| Pass rate of the tests that were run | 20 of 21 (95%) |

By type (run only): System 12 pass; Boundary/negative 6 pass; Security 1 pass, 1 fail; Performance 1 pass.

Bug raised: HT-45, a guest can load the film file (security, role check missing). Sent to Akilesh for the bug log and to Divyesh for the Azure Bug card.

Not a bug: on 22 Nov the "loaded 10 films, skipped 2" message looked cut off once. On 23 Nov it showed in full every time, so no bug was raised.

To do when accounts and booking are merged: run HT-06 to 09, 12, 13, 16 to 18, 30, 32 to 41 again, plus the skipped HT-01, 02, 11, 44.
