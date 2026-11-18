// loyalty rule tests and booking form tests that need no database (owner: akilesh)
// loyalty examples come from the table at the end of docs/design/loyalty-rules.md, card 359
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class BookingLoyaltyTests
{
    // points earned, 1 point per Rs 10 then the tier multiplier, rounded down

    [TestMethod]
    public void PointsEarned_StandardRs295_Gives29()
    {
        Assert.AreEqual(29, LoyaltyService.PointsEarned(295m, Tier.Standard));
    }

    [TestMethod]
    public void PointsEarned_StandardRs9_GivesZero()
    {
        Assert.AreEqual(0, LoyaltyService.PointsEarned(9m, Tier.Standard));
    }

    [TestMethod]
    public void PointsEarned_StandardRs10_GivesOne()
    {
        Assert.AreEqual(1, LoyaltyService.PointsEarned(10m, Tier.Standard));
    }

    [TestMethod]
    public void PointsEarned_SilverRs200_Gives25()
    {
        Assert.AreEqual(25, LoyaltyService.PointsEarned(200m, Tier.Silver));
    }

    [TestMethod]
    public void PointsEarned_GoldRs200_Gives30()
    {
        Assert.AreEqual(30, LoyaltyService.PointsEarned(200m, Tier.Gold));
    }

    [TestMethod]
    public void PointsEarned_GoldRs295_RoundsDown44Point25To44()
    {
        Assert.AreEqual(44, LoyaltyService.PointsEarned(295m, Tier.Gold));
    }

    [TestMethod]
    public void PointsEarned_ZeroPaid_GivesZero()
    {
        Assert.AreEqual(0, LoyaltyService.PointsEarned(0m, Tier.Gold));
    }

    // tier discount

    [TestMethod]
    public void ApplyTierDiscount_StandardRs200_PaysRs200()
    {
        Assert.AreEqual(200m, LoyaltyService.ApplyTierDiscount(200m, Tier.Standard));
    }

    [TestMethod]
    public void ApplyTierDiscount_SilverRs200_PaysRs190()
    {
        Assert.AreEqual(190m, LoyaltyService.ApplyTierDiscount(200m, Tier.Silver));
    }

    [TestMethod]
    public void ApplyTierDiscount_GoldRs200_PaysRs180()
    {
        Assert.AreEqual(180m, LoyaltyService.ApplyTierDiscount(200m, Tier.Gold));
    }

    [TestMethod]
    public void ApplyTierDiscount_ZeroTotal_StaysZero()
    {
        Assert.AreEqual(0m, LoyaltyService.ApplyTierDiscount(0m, Tier.Gold));
    }

    [TestMethod]
    public void TierDiscountPercent_EachTier_MatchesTheRules()
    {
        Assert.AreEqual(0m, LoyaltyService.TierDiscountPercent(Tier.Standard));
        Assert.AreEqual(5m, LoyaltyService.TierDiscountPercent(Tier.Silver));
        Assert.AreEqual(10m, LoyaltyService.TierDiscountPercent(Tier.Gold));
    }

    [TestMethod]
    public void PointsMultiplier_EachTier_MatchesTheRules()
    {
        Assert.AreEqual(1m, LoyaltyService.PointsMultiplier(Tier.Standard));
        Assert.AreEqual(1.25m, LoyaltyService.PointsMultiplier(Tier.Silver));
        Assert.AreEqual(1.5m, LoyaltyService.PointsMultiplier(Tier.Gold));
    }

    // tier from lifetime points, the values on each side of 100 and 300

    [TestMethod]
    [DataRow(0, Tier.Standard)]
    [DataRow(99, Tier.Standard)]
    [DataRow(100, Tier.Silver)]
    [DataRow(299, Tier.Silver)]
    [DataRow(300, Tier.Gold)]
    [DataRow(1000, Tier.Gold)]
    public void TierForLifetimePoints_Boundaries_GiveTheRightTier(int lifetime_points, Tier expected)
    {
        Assert.AreEqual(expected, LoyaltyService.TierForLifetimePoints(lifetime_points));
    }

    // the order from the rules: tier discount first, then points are worked out on what is paid

    [TestMethod]
    public void DiscountThenPoints_GoldTwoSeatsAtRs250_Gives450PaidAnd67Points()
    {
        decimal paid = LoyaltyService.ApplyTierDiscount(2 * 250m, Tier.Gold);
        int points = LoyaltyService.PointsEarned(paid, Tier.Gold);

        Assert.AreEqual(450m, paid);
        Assert.AreEqual(67, points);
    }

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
