// member and booking validation tests (owner: akilesh)
// written from docs/design/validation-rules.md, card 358
using CinemaBooking.Models;

namespace CinemaBooking.Tests;

[TestClass]
public class MemberBookingTests
{
    private static Member MakeMember(string name = "Priya", string email = "priya@mail.com",
        string phone = "5712 3456", Tier tier = Tier.Standard, int points = 0, int lifetime_points = 0,
        DateTime? joined_at = null)
    {
        return new Member(1, name, email, phone, tier, points, lifetime_points,
            joined_at ?? new DateTime(2024, 1, 1));
    }

    private static Booking MakeBooking(int? member_id = 1, string guest_name = "", string guest_email = "",
        string guest_phone = "", string[]? seats = null, decimal total = 500m,
        int points_earned = 0, int points_used = 0)
    {
        return new Booking(1, "CB-ABC123", 1, member_id, guest_name, guest_email, guest_phone,
            seats ?? new string[] { "C5" }, total, points_earned, points_used,
            BookingStatus.Confirmed, new DateTime(2026, 11, 1));
    }

    // member, whole object

    [TestMethod]
    [DataRow(Tier.Standard)]
    [DataRow(Tier.Silver)]
    [DataRow(Tier.Gold)]
    public void MemberIsValid_EachTier_IsAccepted(Tier tier)
    {
        var member = MakeMember(tier: tier);

        Assert.IsTrue(member.IsValid(out string errors));
        Assert.AreEqual("", errors);
    }

    [TestMethod]
    public void MemberIsValidTier_NotInTheEnum_IsRefused()
    {
        // Platinum does not exist, so 3 stands in for it
        bool result = Member.IsValidTier((Tier)3, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("tier must be Standard, Silver or Gold", error_message);
    }

    // member name

    [TestMethod]
    public void MemberIsValidName_Empty_IsRefused()
    {
        bool result = Member.IsValidName("", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("name cannot be empty and must be 100 characters or fewer", error_message);
    }

    [TestMethod]
    public void MemberIsValidName_100Characters_IsAccepted()
    {
        Assert.IsTrue(Member.IsValidName(new string('n', 100), out _));
    }

    [TestMethod]
    public void MemberIsValidName_101Characters_IsRefused()
    {
        Assert.IsFalse(Member.IsValidName(new string('n', 101), out _));
    }

    // member email

    [TestMethod]
    public void MemberIsValidEmail_Empty_IsRefused()
    {
        bool result = Member.IsValidEmail("", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("email cant be empty and must be 100 characters or fewer", error_message);
    }

    [TestMethod]
    [DataRow("priya.mail.com")]
    [DataRow("pri ya@mail.com")]
    [DataRow("priya@mail")]
    [DataRow("@mail.com")]
    [DataRow("priya@mail.")]
    [DataRow("a@@mail.com")]
    public void MemberIsValidEmail_BadShape_IsRefused(string email)
    {
        bool result = Member.IsValidEmail(email, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("email must look like name@site.com", error_message);
    }

    // member phone, 6 and 7 digits, 15 and 16 digits

    [TestMethod]
    [DataRow("123456")]
    [DataRow("1234567890123456")]
    [DataRow("abcd")]
    [DataRow("")]
    public void MemberIsValidPhone_WrongLengthOrLetters_IsRefused(string phone)
    {
        bool result = Member.IsValidPhone(phone, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("phone must have 7 to 15 digits and only digits, spaces, + or -", error_message);
    }

    [TestMethod]
    [DataRow("1234567")]
    [DataRow("123456789012345")]
    [DataRow("+230 5712-3456")]
    public void MemberIsValidPhone_AllowedPhone_IsAccepted(string phone)
    {
        Assert.IsTrue(Member.IsValidPhone(phone, out _));
    }

    // member points

    [TestMethod]
    public void MemberIsValidPoints_Negative_IsRefused()
    {
        bool result = Member.IsValidPoints(-1, 10, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("points must be 0 or more and cannot be above lifetime points", error_message);
    }

    [TestMethod]
    public void MemberIsValidPoints_AboveLifetimePoints_IsRefused()
    {
        Assert.IsFalse(Member.IsValidPoints(50, 20, out _));
    }

    [TestMethod]
    public void MemberIsValidPoints_EqualToLifetimePoints_IsAccepted()
    {
        Assert.IsTrue(Member.IsValidPoints(20, 20, out _));
    }

    // member join date

    [TestMethod]
    public void MemberIsValidJoinedAt_Tomorrow_IsRefused()
    {
        bool result = Member.IsValidJoinedAt(DateTime.Now.AddDays(1), out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("join date must be a real date, not a future day", error_message);
    }

    [TestMethod]
    public void MemberIsValidJoinedAt_DefaultDate_IsRefused()
    {
        Assert.IsFalse(Member.IsValidJoinedAt(default(DateTime), out _));
    }

    [TestMethod]
    public void MemberIsValidId_Zero_IsRefused()
    {
        bool result = Member.IsValidId(0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("member id should be a positive number", error_message);
    }

    // booking, whole object

    [TestMethod]
    public void BookingIsValid_ValidMemberBooking_IsAccepted()
    {
        var booking = MakeBooking();

        Assert.IsTrue(booking.IsValid(out string errors));
        Assert.AreEqual("", errors);
    }

    [TestMethod]
    public void BookingIsValid_ValidGuestBooking_IsAccepted()
    {
        var booking = MakeBooking(member_id: null, guest_name: "Kevin",
            guest_email: "kevin@mail.com", guest_phone: "5712 3456");

        Assert.IsTrue(booking.IsValid(out _));
    }

    [TestMethod]
    public void BookingIsValid_MemberBookingIgnoresGuestFields()
    {
        // guest fields are empty here and must not matter
        var booking = MakeBooking(member_id: 1, guest_name: "", guest_email: "", guest_phone: "");

        Assert.IsTrue(booking.IsValid(out _));
    }

    // guest fields

    [TestMethod]
    public void BookingIsValidGuestName_EmptyForGuest_IsRefused()
    {
        bool result = Booking.IsValidGuestName(null, "", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("guest name cannot be empty and must be 100 characters or fewer", error_message);
    }

    [TestMethod]
    public void BookingIsValidGuestEmail_BadShapeForGuest_IsRefused()
    {
        bool result = Booking.IsValidGuestEmail(null, "kevin.mail.com", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("guest email must look like name@site.com", error_message);
    }

    [TestMethod]
    public void BookingIsValidGuestPhone_LettersForGuest_IsRefused()
    {
        bool result = Booking.IsValidGuestPhone(null, "abcd", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("guest phone must have 7 to 15 digits and only digits, spaces, + or -", error_message);
    }

    // seats, only the format is checked here, the seat grid checks the range

    [TestMethod]
    public void BookingIsValidSeats_NullOrEmptyList_IsRefused()
    {
        bool result = Booking.IsValidSeats(new string[] { }, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("a booking needs at least 1 seat", error_message);
        Assert.IsFalse(Booking.IsValidSeats(null, out _));
    }

    [TestMethod]
    [DataRow("C5")]
    [DataRow("A1")]
    [DataRow("H12")]
    [DataRow("A12")]
    public void BookingIsValidSeats_GoodCode_IsAccepted(string seat)
    {
        Assert.IsTrue(Booking.IsValidSeats(new string[] { seat }, out _));
    }

    [TestMethod]
    [DataRow("c5")]
    [DataRow("C")]
    [DataRow("5C")]
    [DataRow("C05")]
    [DataRow("C0")]
    [DataRow("")]
    public void BookingIsValidSeats_BadCode_IsRefused(string seat)
    {
        bool result = Booking.IsValidSeats(new string[] { seat }, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("seat code must be a letter then a number, for example C5", error_message);
    }

    [TestMethod]
    [DataRow("A13")]
    [DataRow("I1")]
    public void BookingIsValidSeats_OutsideGrid_StillPassesFormatCheck(string seat)
    {
        // the seat grid refuses these later, Booking only looks at the format
        Assert.IsTrue(Booking.IsValidSeats(new string[] { seat }, out _));
    }

    [TestMethod]
    public void BookingIsValidSeats_SameSeatTwice_IsRefused()
    {
        bool result = Booking.IsValidSeats(new string[] { "C5", "C5" }, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("seat C5 is listed more than once", error_message);
    }

    // total and points

    [TestMethod]
    public void BookingIsValidTotal_Negative_IsRefused()
    {
        bool result = Booking.IsValidTotal(-1m, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("total cannot be negative", error_message);
    }

    [TestMethod]
    public void BookingIsValidTotal_Zero_IsAccepted()
    {
        Assert.IsTrue(Booking.IsValidTotal(0m, out _));
    }

    [TestMethod]
    public void BookingIsValidPoints_NegativePoints_IsRefused()
    {
        bool result = Booking.IsValidPoints(1, -5, 0, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("points earned and points used cannot be negative", error_message);
    }

    [TestMethod]
    public void BookingIsValidPoints_GuestUsingPoints_IsRefused()
    {
        bool result = Booking.IsValidPoints(null, 0, 100, out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("a guest booking cannot earn or use points", error_message);
    }

    [TestMethod]
    public void BookingIsValidPoints_GuestWithZeroPoints_IsAccepted()
    {
        Assert.IsTrue(Booking.IsValidPoints(null, 0, 0, out _));
    }

    // other fields

    [TestMethod]
    public void BookingIsValidReference_Empty_IsRefused()
    {
        bool result = Booking.IsValidReference("  ", out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("booking reference cannot be empty", error_message);
    }

    [TestMethod]
    public void BookingIsValidCreatedAt_Tomorrow_IsRefused()
    {
        bool result = Booking.IsValidCreatedAt(DateTime.Now.AddDays(1), out string error_message);

        Assert.IsFalse(result);
        Assert.AreEqual("booking date must be a real date, not a future day", error_message);
    }

    [TestMethod]
    public void BookingIsValidStatus_NotInTheEnum_IsRefused()
    {
        Assert.IsFalse(Booking.IsValidStatus((BookingStatus)5, out _));
    }

    [TestMethod]
    public void BookingIsValid_SeveralBadFields_GivesOneLineEach()
    {
        var booking = MakeBooking(seats: new string[] { }, total: -1m);

        bool result = booking.IsValid(out string errors);

        Assert.IsFalse(result);
        Assert.HasCount(2, errors.Split('\n', StringSplitOptions.RemoveEmptyEntries));
    }
}
