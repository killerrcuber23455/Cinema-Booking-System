\# Test database setup (owner: kushal)



Integration tests only run on a separate test database, never on the real one.

The tests refuse any connection string that does not contain "Test".



\## Steps

1\. Open SQL Server Management Studio and connect to your server (for example localhost\\SQLEXPRESS).

2\. Click New Query and run: CREATE DATABASE CinemaBookingTestDb;

3\. Open database/cinema.sql, set the database dropdown to CinemaBookingTestDb, and press F5. There must be no red errors.

4\. In PowerShell run (change the server name if yours is different):

&#x20;  setx CINEMA\_TEST\_DB "Server=localhost\\SQLEXPRESS;Database=CinemaBookingTestDb;Trusted\_Connection=True;TrustServerCertificate=True;"

5\. Close Visual Studio and PowerShell completely, then reopen them. setx only affects new windows.

6\. Check it: echo $env:CINEMA\_TEST\_DB

7\. Run: dotnet test CinemaBooking.slnx



\## Notes

\- If tests show Skipped or Inconclusive, the variable is not visible. Repeat step 5.

\- The tests delete rows from booked\_seats, bookings, showings and films. Never point CINEMA\_TEST\_DB at a database with data you want to keep.

\- Database tests run one at a time (\[DoNotParallelize]) because they share the same tables.

\- To reset the test database, run database/cinema.sql again on it.

