// tests for the one line of text of a booking in the My bookings list, card 349 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class MyBookingLineTests
{
    private static Booking Make(BookingStatus status)
    {
        return new Booking(5, "ABC123", 1, 1, "", "", "", new string[] { "C5", "C6" }, 300m, 30, 0,
            status, new DateTime(2026, 11, 1));
    }

    [TestMethod]
    public void Text_ConfirmedBooking_ShowsShowingSeatsTotalStatusAndReference()
    {
        string text = MyBookingLine.Text(Make(BookingStatus.Confirmed), "Heat · Tue 1 Dec 2026 · 18:30");

        StringAssert.Contains(text, "Heat · Tue 1 Dec 2026 · 18:30");
        StringAssert.Contains(text, "seats C5, C6");
        StringAssert.Contains(text, "Rs 300.00");
        StringAssert.Contains(text, "Confirmed");
        StringAssert.Contains(text, "ABC123");
    }

    [TestMethod]
    public void Text_CancelledBooking_ShowsCancelledAndNotConfirmed()
    {
        string text = MyBookingLine.Text(Make(BookingStatus.Cancelled), "Heat");

        StringAssert.Contains(text, "Cancelled");
        Assert.DoesNotContain("Confirmed", text);
    }

    [TestMethod]
    public void Text_CancelledBookingWithNoSeats_SaysSeatsFreed()
    {
        Booking booking = Make(BookingStatus.Cancelled);
        booking.Seats = new string[0];

        string text = MyBookingLine.Text(booking, "Heat");

        StringAssert.Contains(text, "seats freed");
        Assert.DoesNotContain("seats  -", text);
    }

    [TestMethod]
    public void CanCancel_ConfirmedBooking_IsTrue()
    {
        Assert.IsTrue(MyBookingLine.CanCancel(Make(BookingStatus.Confirmed)));
    }

    [TestMethod]
    public void CanCancel_CancelledBooking_IsFalse()
    {
        Assert.IsFalse(MyBookingLine.CanCancel(Make(BookingStatus.Cancelled)));
    }

    [TestMethod]
    public void Text_TotalAlwaysHasTwoDecimals()
    {
        Booking booking = Make(BookingStatus.Confirmed);
        booking.Total = 150.5m;

        string text = MyBookingLine.Text(booking, "Heat");

        StringAssert.Contains(text, "Rs 150.50");
    }

    // the seats as text (card 349 follow-up: the Find booking page and My bookings must say the same)

    [TestMethod]
    public void SeatList_ConfirmedBooking_ListsTheSeats()
    {
        Assert.AreEqual("C5, C6", MyBookingLine.SeatList(Make(BookingStatus.Confirmed)));
    }

    [TestMethod]
    public void SeatList_BookingWithNoSeats_SaysFreed()
    {
        Booking cancelled = new Booking(5, "ABC123", 1, 1, "", "", "", new string[0], 300m, 30, 0,
            BookingStatus.Cancelled, new DateTime(2026, 11, 1));

        Assert.AreEqual("freed", MyBookingLine.SeatList(cancelled));
    }

    [TestMethod]
    public void Text_UsesTheSameSeatWordsAsSeatList()
    {
        Booking cancelled = new Booking(5, "ABC123", 1, 1, "", "", "", new string[0], 300m, 30, 0,
            BookingStatus.Cancelled, new DateTime(2026, 11, 1));

        StringAssert.Contains(MyBookingLine.Text(cancelled, "Heat"), "seats " + MyBookingLine.SeatList(cancelled));
    }

    // the note under the list

    [TestMethod]
    public void SelectionNote_NothingChosen_IsEmpty()
    {
        Assert.AreEqual("", MyBookingLine.SelectionNote(null));
    }

    [TestMethod]
    public void SelectionNote_ConfirmedBooking_IsEmpty()
    {
        Assert.AreEqual("", MyBookingLine.SelectionNote(Make(BookingStatus.Confirmed)));
    }

    [TestMethod]
    public void SelectionNote_CancelledBooking_SaysAlreadyCancelled()
    {
        Assert.AreEqual("This booking is already cancelled", MyBookingLine.SelectionNote(Make(BookingStatus.Cancelled)));
    }
}
