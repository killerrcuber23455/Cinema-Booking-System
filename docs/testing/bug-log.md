# Bug log: Cinema Film Booking and Membership System

Card: 417 (AB#417)
Owner: Akilesh Sookhareea. Irwin uses the same file.

Every failed test gets a Bug ID in the test table (docs/testing/test-plan.md) and a row here. Bug IDs run BUG-01, BUG-02 and so on. Severity is High (program crashes or data is wrong), Medium (a rule is broken but the program works) or Low (wording or looks). Status is Open, Fixed, Retested or Closed. A bug is Closed only after the whole suite has been run again and the failing steps repeated.

| Bug ID | Found in test ID | Date found | Found by | Area | Description | Steps to reproduce | Severity | Assigned to | Status | Fix commit or card | Date fixed | Retest result |
|---|---|---|---|---|---|---|---|---|---|---|---|---|
| BUG-01 | CANCEL-05, CANCEL-06 | 2026-11-18 | Akilesh | Cancel booking | The guest email is not checked when cancelling. A guest with a wrong email, or with an empty email, can cancel someone else's guest booking | Make a guest booking with email a@test.com. Call CancelBooking as Guest with the right reference and the email b@test.com (then with an empty email) | High | Shankar | Open | card 399 | | |
| BUG-02 | CANCEL-13, CANCEL-14, CANCEL-15 | 2026-11-18 | Akilesh | Cancel booking | The member's points after a cancel are wrong. Expected points - earned + used, never below 0 | Member with 80 points, booking earned 10 and used 0, cancel it: points are 10, expected 70. Booking used 20: 30, expected 90. Member with 5 points, booking earned 10: 5, expected 0 | High | Shankar | Open | card 399 | | |
| BUG-03 | CANCEL-17 | 2026-11-18 | Akilesh | Cancel booking | When the points cannot be saved the booking is cancelled but error_message is empty instead of the points-not-saved message | Member 100 points and 100 lifetime points, booking earned 10 and used 50, cancel it as Admin: the message is empty (expected "the booking was cancelled but the points could not be updated, please tell staff") | Medium | Shankar | Open | card 399 | | |

## Design note (from 318)

AuthService turns every -1 from SaveUser into "this email is already used". Since 318, SaveUser also returns -1 when the database refuses other data (bad role, text too long, unknown member id), so those cases would show the wrong message. Windows cannot reach them today, so it is not urgent. Fix idea: SaveUser could return a separate code or an out error_message. Owner of AuthService: Shankar.

