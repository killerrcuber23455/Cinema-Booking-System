// booking service tests that need no database (owner: akilesh)
// BookSeats checks the form before it uses the database, so none is given here, card 359
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class BookingServiceTests
{
    private static string[] _good_seats = new string[] { "C5" };

    // runs BookSeats as a customer and gives back the message, the booking must never come back
    private static bool Try(int? member_id, string guest_name, string guest_email, string guest_phone,
        int showing_id, string[] seats, int points_to_use, out string error_message)
    {
        var service = new BookingService(null!, null!);

        bool result = service.BookSeats(Role.Customer, member_id, guest_name, guest_email, guest_phone,
            showing_id, seats, points_to_use, out Booking? booking, out error_message);

        Assert.IsNull(booking);
        return result;
    }

    [TestMethod]
    public void BookSeats_ShowingIdZero_IsRefused()
    {
        bool result = Try(1, "", "", "", 0, _good_seats, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("showing id should be a positive number", error_message);
    }

    [TestMethod]
    public void BookSeats_MemberIdZero_IsRefused()
    {
        bool result = Try(0, "", "", "", 1, _good_seats, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("member id should be a positive number or empty(null) for a guest", error_message);
    }

    [TestMethod]
    public void BookSeats_GuestWithoutName_IsRefused()
    {
        bool result = Try(null, "", "kevin@mail.com", "5712 3456", 1, _good_seats, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("guest name cannot be empty and must be 100 characters or fewer", error_message);
    }

    [TestMethod]
    public void BookSeats_GuestWithBadEmail_IsRefused()
    {
        bool result = Try(null, "Kevin", "kevin.mail.com", "5712 3456", 1, _good_seats, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("guest email must look like name@site.com", error_message);
    }

    [TestMethod]
    public void BookSeats_GuestWithBadPhone_IsRefused()
    {
        bool result = Try(null, "Kevin", "kevin@mail.com", "abcd", 1, _good_seats, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("guest phone must have 7 to 15 digits and only digits, spaces, + or -", error_message);
    }

    [TestMethod]
    public void BookSeats_NoSeats_IsRefused()
    {
        bool result = Try(1, "", "", "", 1, new string[] { }, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("a booking needs at least 1 seat", error_message);
    }

    [TestMethod]
    [DataRow("5C")]
    [DataRow("c5")]
    [DataRow("C")]
    [DataRow("A0")]
    public void BookSeats_BadSeatCode_IsRefused(string seat)
    {
        bool result = Try(1, "", "", "", 1, new string[] { seat }, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("seat code must be a letter then a number, for example C5", error_message);
    }

    [TestMethod]
    public void BookSeats_SeatNumberPastTheRow_IsRefused()
    {
        bool result = Try(1, "", "", "", 1, new string[] { "A13" }, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("seat A13: seat number must be 1 to 12", error_message);
    }

    [TestMethod]
    public void BookSeats_RowPastTheLastRow_IsRefused()
    {
        bool result = Try(1, "", "", "", 1, new string[] { "I1" }, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("seat I1: row must be A to H", error_message);
    }

    [TestMethod]
    public void BookSeats_SameSeatTwice_IsRefused()
    {
        bool result = Try(1, "", "", "", 1, new string[] { "C5", "C5" }, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("seat C5 is listed more than once", error_message);
    }

    [TestMethod]
    public void BookSeats_UsingPoints_IsRefusedForNow()
    {
        // card 330 is in Sprint 4, until then only 0 points can be used
        bool result = Try(1, "", "", "", 1, _good_seats, 100, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("Using points to pay is not available yet", error_message);
    }
}
