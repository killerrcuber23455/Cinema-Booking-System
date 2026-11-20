// unit tests for kushal's seat grid, the showings table and the members table (owner: irwin)
// card 361. test ids (SEAT-01, SHOWDS-01, MEMB-01...) match docs/testing/test-plan-data-side.md.

using CinemaBooking.DataStructures;
using CinemaBooking.Models;

namespace CinemaBooking.Tests;

[TestClass]
public class SeatGridTests
{
    // our screens are 8 rows (A to H) by 12 seats (decision 8 nov)
    private const int ROWS = 8;
    private const int SEATS_PER_ROW = 12;

    // makes an empty 8 by 12 grid
    private static SeatGrid MakeGrid()
    {
        return new SeatGrid(ROWS, SEATS_PER_ROW);
    }

    // makes a member with a given email
    private static Member MakeMember(int member_id, string email)
    {
        return new Member(member_id, "Priya", email, "5712 3456", Tier.Standard, 0, 0, DateTime.Now);
    }

    // ---------- seat grid ----------

    // SEAT-01: a new grid has every seat free
    [TestMethod]
    public void NewGrid_AllSeatsFree()
    {
        // arrange: empty grid
        SeatGrid grid = MakeGrid();

        // assert: no taken seats, and the first and last seats are free
        Assert.AreEqual(0, grid.ListTakenSeats().Length);
        Assert.IsTrue(grid.IsSeatFree("A1"));
        Assert.IsTrue(grid.IsSeatFree("H12"));
    }

    // SEAT-02: booking one seat makes it taken
    [TestMethod]
    public void BookSeats_OneSeat_SeatIsTaken()
    {
        SeatGrid grid = MakeGrid();

        bool is_booked = grid.BookSeats(new string[] { "C5" });

        Assert.IsTrue(is_booked);
        Assert.IsFalse(grid.IsSeatFree("C5"));
        Assert.AreEqual(1, grid.ListTakenSeats().Length);
    }

    // SEAT-03: booking two seats takes both
    [TestMethod]
    public void BookSeats_TwoSeats_BothTaken()
    {
        SeatGrid grid = MakeGrid();

        bool is_booked = grid.BookSeats(new string[] { "C5", "C6" });

        Assert.IsTrue(is_booked);
        Assert.IsFalse(grid.IsSeatFree("C5"));
        Assert.IsFalse(grid.IsSeatFree("C6"));
    }

    // SEAT-04: a taken seat cannot be booked again
    [TestMethod]
    public void BookSeats_TakenSeat_IsRefused()
    {
        // arrange: C5 is taken
        SeatGrid grid = MakeGrid();
        grid.BookSeats(new string[] { "C5" });

        // act: book C5 again
        bool is_booked = grid.BookSeats(new string[] { "C5" });

        // assert: refused, still only one taken seat
        Assert.IsFalse(is_booked);
        Assert.AreEqual(1, grid.ListTakenSeats().Length);
    }

    // SEAT-05: one taken seat in a group refuses the whole group (all or nothing)
    [TestMethod]
    public void BookSeats_OneTakenInGroup_NoneBooked()
    {
        // arrange: C6 is taken
        SeatGrid grid = MakeGrid();
        grid.BookSeats(new string[] { "C6" });

        // act: ask for C5 and C6 together
        bool is_booked = grid.BookSeats(new string[] { "C5", "C6" });

        // assert: refused, and C5 was not taken either
        Assert.IsFalse(is_booked);
        Assert.IsTrue(grid.IsSeatFree("C5"));
    }

    // SEAT-06: freeing a taken seat makes it free again
    [TestMethod]
    public void FreeSeats_TakenSeat_IsFreeAgain()
    {
        SeatGrid grid = MakeGrid();
        grid.BookSeats(new string[] { "C5" });

        bool is_freed = grid.FreeSeats(new string[] { "C5" });

        Assert.IsTrue(is_freed);
        Assert.IsTrue(grid.IsSeatFree("C5"));
    }

    // SEAT-07: freeing a seat that is already free does no harm
    [TestMethod]
    public void FreeSeats_AlreadyFree_NoChange()
    {
        SeatGrid grid = MakeGrid();

        bool is_freed = grid.FreeSeats(new string[] { "D1" });

        // the grid accepts it and the seat is simply still free
        Assert.IsTrue(is_freed);
        Assert.IsTrue(grid.IsSeatFree("D1"));
    }

    // SEAT-08: the four corner seats can all be booked (edges of the array)
    [TestMethod]
    public void BookSeats_FourCorners_AllTaken()
    {
        SeatGrid grid = MakeGrid();

        bool is_booked = grid.BookSeats(new string[] { "A1", "A12", "H1", "H12" });

        Assert.IsTrue(is_booked);
        Assert.AreEqual(4, grid.ListTakenSeats().Length);
    }

    // SEAT-09: row I is outside an 8 row grid
    [TestMethod]
    public void BookSeats_RowOutsideGrid_IsRefused()
    {
        SeatGrid grid = MakeGrid();

        Assert.IsFalse(grid.BookSeats(new string[] { "I1" }));
    }

    // SEAT-10: seat 13 and seat 0 are outside a 12 seat row
    [TestMethod]
    [DataRow("A13")]
    [DataRow("A0")]
    public void BookSeats_SeatNumberOutsideGrid_IsRefused(string seat_code)
    {
        SeatGrid grid = MakeGrid();

        Assert.IsFalse(grid.BookSeats(new string[] { seat_code }));
    }

    // SEAT-11: badly written seat codes are refused, not crashed on
    [TestMethod]
    [DataRow("")]
    [DataRow("5C")]
    [DataRow("ZZ")]
    [DataRow("C")]
    public void BookSeats_BadCode_IsRefused(string seat_code)
    {
        SeatGrid grid = MakeGrid();

        Assert.IsFalse(grid.BookSeats(new string[] { seat_code }));
    }

    // SEAT-12: lower case "c5" is refused, the grid only takes capital row letters
    [TestMethod]
    public void BookSeats_LowerCaseCode_IsRefused()
    {
        SeatGrid grid = MakeGrid();

        Assert.IsFalse(grid.BookSeats(new string[] { "c5" }));
    }

    // SEAT-13: when all 96 seats are taken, one more seat is refused
    [TestMethod]
    public void BookSeats_FullGrid_NextSeatRefused()
    {
        // arrange: book every seat, row by row
        SeatGrid grid = MakeGrid();
        for (int row = 0; row < ROWS; row++)
        {
            for (int seat = 1; seat <= SEATS_PER_ROW; seat++)
            {
                // row 0 is A, row 1 is B ... so 'A' + row gives the letter
                string seat_code = ((char)('A' + row)).ToString() + seat;
                grid.BookSeats(new string[] { seat_code });
            }
        }

        // assert: 96 taken and any seat is now refused
        Assert.AreEqual(96, grid.ListTakenSeats().Length);
        Assert.IsFalse(grid.BookSeats(new string[] { "D6" }));
    }

    // SEAT-14: the taken list has exactly the booked seats, in order A1 to H12
    [TestMethod]
    public void ListTakenSeats_ThreeBooked_ReturnsThemInOrder()
    {
        SeatGrid grid = MakeGrid();
        grid.BookSeats(new string[] { "H12", "B2", "C5" });

        string[] taken = grid.ListTakenSeats();

        Assert.AreEqual(3, taken.Length);
        Assert.AreEqual("B2", taken[0]);
        Assert.AreEqual("C5", taken[1]);
        Assert.AreEqual("H12", taken[2]);
    }

    // SEAT-15: the same seat twice in one request is refused
    [TestMethod]
    public void BookSeats_SameSeatTwiceInRequest_IsRefused()
    {
        SeatGrid grid = MakeGrid();

        bool is_booked = grid.BookSeats(new string[] { "C5", "C5" });

        Assert.IsFalse(is_booked);
        Assert.IsTrue(grid.IsSeatFree("C5"));
    }

    // SEAT-16: an empty list of seats is refused
    [TestMethod]
    public void BookSeats_EmptyList_IsRefused()
    {
        SeatGrid grid = MakeGrid();

        Assert.IsFalse(grid.BookSeats(new string[0]));
    }

    // SEAT-17: freeing a bad seat code is refused, and the other seat in the request stays taken (all or nothing)
    [TestMethod]
    public void FreeSeats_OneBadCode_NoneFreed()
    {
        // arrange: C5 is taken
        SeatGrid grid = MakeGrid();
        grid.BookSeats(new string[] { "C5" });

        // act: free C5 and a seat that does not exist
        bool is_freed = grid.FreeSeats(new string[] { "C5", "Z99" });

        // assert: refused, C5 is still taken
        Assert.IsFalse(is_freed);
        Assert.IsFalse(grid.IsSeatFree("C5"));
    }

    // SEAT-18: freeing an empty list is refused
    [TestMethod]
    public void FreeSeats_EmptyList_IsRefused()
    {
        SeatGrid grid = MakeGrid();

        Assert.IsFalse(grid.FreeSeats(new string[0]));
    }

    // SEAT-19: asking if a bad seat code is free says no, it does not crash
    [TestMethod]
    [DataRow("")]
    [DataRow("I1")]
    [DataRow("A13")]
    public void IsSeatFree_BadCode_ReturnsFalse(string seat_code)
    {
        SeatGrid grid = MakeGrid();

        Assert.IsFalse(grid.IsSeatFree(seat_code));
    }

    // ---------- showings table ----------

    // SHOWDS-01: a showing we create can be found again with its own grid
    [TestMethod]
    public void CreateShowing_ThenFind_ReturnsShowing()
    {
        // arrange: empty table
        ShowingTable table = new ShowingTable();

        // act: create showing 10 for film 1 on screen 1
        bool is_created = table.CreateShowing(10, 1, 1, new DateTime(2026, 12, 1, 20, 30, 0), 250m, ROWS, SEATS_PER_ROW);

        // assert: found, with an empty grid
        Assert.IsTrue(is_created);
        Assert.IsNotNull(table.FindShowing(10));
        Assert.AreEqual(0, table.GetGrid(10)!.ListTakenSeats().Length);
    }

    // SHOWDS-02: each showing has its own grid, booking one does not touch the other
    [TestMethod]
    public void BookSeat_InOneShowing_OtherShowingStillFree()
    {
        // arrange: two showings
        ShowingTable table = new ShowingTable();
        table.CreateShowing(10, 1, 1, new DateTime(2026, 12, 1, 18, 0, 0), 250m, ROWS, SEATS_PER_ROW);
        table.CreateShowing(11, 1, 1, new DateTime(2026, 12, 1, 21, 0, 0), 250m, ROWS, SEATS_PER_ROW);

        // act: book C5 in showing 10
        table.GetGrid(10)!.BookSeats(new string[] { "C5" });

        // assert: C5 is taken in 10 but still free in 11
        Assert.IsFalse(table.GetGrid(10)!.IsSeatFree("C5"));
        Assert.IsTrue(table.GetGrid(11)!.IsSeatFree("C5"));
    }

    // SHOWDS-03: a showing that does not exist gives null, not a crash
    [TestMethod]
    public void FindShowing_MissingId_ReturnsNull()
    {
        ShowingTable table = new ShowingTable();

        Assert.IsNull(table.FindShowing(99));
        Assert.IsNull(table.GetGrid(99));
    }

    // SHOWDS-04: the same showing id twice is refused
    [TestMethod]
    public void CreateShowing_SameIdTwice_SecondRefused()
    {
        ShowingTable table = new ShowingTable();
        table.CreateShowing(10, 1, 1, new DateTime(2026, 12, 1, 20, 30, 0), 250m, ROWS, SEATS_PER_ROW);

        bool is_created = table.CreateShowing(10, 2, 2, new DateTime(2026, 12, 2, 20, 30, 0), 300m, ROWS, SEATS_PER_ROW);

        Assert.IsFalse(is_created);
        Assert.AreEqual(1, table.Count);
    }

    // ---------- members table (by email) ----------

    // MEMB-01: a member we add can be found by email
    [TestMethod]
    public void AddMember_ThenFindByEmail_ReturnsMember()
    {
        MemberTable table = new MemberTable();
        table.AddMember(MakeMember(1, "priya@example.com"));

        Member? found = table.FindByEmail("priya@example.com");

        Assert.IsNotNull(found);
        Assert.AreEqual(1, found.Id);
    }

    // MEMB-02: capitals and spaces do not matter, the table trims and lowercases the email
    [TestMethod]
    public void FindByEmail_OtherCaseAndSpaces_FindsMember()
    {
        MemberTable table = new MemberTable();
        table.AddMember(MakeMember(1, "priya@example.com"));

        Member? found = table.FindByEmail("  PRIYA@Example.com ");

        Assert.IsNotNull(found);
    }

    // MEMB-03: an email that is not in the table gives null
    [TestMethod]
    public void FindByEmail_Missing_ReturnsNull()
    {
        MemberTable table = new MemberTable();
        table.AddMember(MakeMember(1, "priya@example.com"));

        Assert.IsNull(table.FindByEmail("nobody@example.com"));
    }

    // MEMB-04: two members with the same email are refused, even with other capitals
    [TestMethod]
    public void AddMember_SameEmailOtherCase_SecondRefused()
    {
        MemberTable table = new MemberTable();
        table.AddMember(MakeMember(1, "priya@example.com"));

        bool is_added = table.AddMember(MakeMember(2, "PRIYA@example.com"));

        Assert.IsFalse(is_added);
        Assert.AreEqual(1, table.Count);
    }

    // MEMB-05: an empty email finds nothing and does not crash
    [TestMethod]
    public void FindByEmail_EmptyText_ReturnsNull()
    {
        MemberTable table = new MemberTable();
        table.AddMember(MakeMember(1, "priya@example.com"));

        Assert.IsNull(table.FindByEmail(""));
    }

    // MEMB-06: adding nothing (null) is refused, not crashed on
    [TestMethod]
    public void AddMember_Null_IsRefused()
    {
        MemberTable table = new MemberTable();

        bool is_added = table.AddMember(null!);

        Assert.IsFalse(is_added);
        Assert.AreEqual(0, table.Count);
    }
}
