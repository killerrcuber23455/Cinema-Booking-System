# Bug log: Cinema Film Booking and Membership System

Card: 417 (AB#417)
Owner: Akilesh Sookhareea. Irwin uses the same file.

Every failed test gets a Bug ID in the test table (docs/testing/test-plan.md) and a row here. Bug IDs run BUG-01, BUG-02 and so on. Severity is High (program crashes or data is wrong), Medium (a rule is broken but the program works) or Low (wording or looks). Status is Open, Fixed, Retested or Closed. A bug is Closed only after the whole suite has been run again and the failing steps repeated.

|Bug ID|Found in test ID|Date found|Found by|Area|Description|Steps to reproduce|Severity|Assigned to|Status|Fix commit or card|Date fixed|Retest result|
|-|-|-|-|-|-|-|-|-|-|-|-|-|



\## Design note (from 318)

AuthService turns every -1 from SaveUser into "this email is already used". Since 318, SaveUser also returns -1 when the database refuses other data (bad role, text too long, unknown member id), so those cases would show the wrong message. Windows cannot reach them today, so it is not urgent. Fix idea: SaveUser could return a separate code or an out error\_message. Owner of AuthService: Shankar.

