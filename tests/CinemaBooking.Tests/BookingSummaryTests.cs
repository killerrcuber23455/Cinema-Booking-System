// tests for the numbers and lines shown on the booking summary page (owner: divyesh)
// written before the code for card 403, so they fail first (red) and pass after (green)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class BookingSummaryTests
{
    // a showing with a ticket price of Rs 250
    private static Showing MakeShowing()
    {
        return new Showing(1, 10, 2, new DateTime(2026, 11, 20, 18, 30, 0), 250m, new SeatGrid(8, 12));
    }

    private static Member MakeMember(Tier tier)
    {
        return new Member(5, "Anna Smith", "anna@example.com", "57123456", tier, 80, 80, new DateTime(2026, 1, 1));
    }

    private static readonly string[] TWO_SEATS = new string[] { "A1", "A2" };

    // numbers

    [TestMethod]
    public void Guest_TwoSeats_PaysFullPriceAndEarnsNoPoints()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), TWO_SEATS, null);

        Assert.AreEqual(2, summary.SeatCount);
        Assert.AreEqual(500m, summary.TicketTotal);
        Assert.AreEqual(0m, summary.DiscountAmount);
        Assert.AreEqual(500m, summary.AmountToPay);
        Assert.AreEqual(0, summary.PointsToEarn);
    }

    [TestMethod]
    public void StandardMember_TwoSeats_PaysFullPriceAndEarnsPoints()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), TWO_SEATS, MakeMember(Tier.Standard));

        Assert.AreEqual(500m, summary.AmountToPay);
        Assert.AreEqual(50, summary.PointsToEarn);
    }

    [TestMethod]
    public void SilverMember_TwoSeats_GetsFivePercentOffAndMorePoints()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), TWO_SEATS, MakeMember(Tier.Silver));

        Assert.AreEqual(25m, summary.DiscountAmount);
        Assert.AreEqual(475m, summary.AmountToPay);
        Assert.AreEqual(59, summary.PointsToEarn);
    }

    [TestMethod]
    public void GoldMember_TwoSeats_GetsTenPercentOffAndMorePoints()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), TWO_SEATS, MakeMember(Tier.Gold));

        Assert.AreEqual(50m, summary.DiscountAmount);
        Assert.AreEqual(450m, summary.AmountToPay);
        Assert.AreEqual(67, summary.PointsToEarn);
    }

    // lines shown on the page

    [TestMethod]
    public void DescribeLines_Guest_ShowsSeatsTicketsPriceAndTheSignInHint()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), TWO_SEATS, null);

        string[] lines = summary.DescribeLines();

        Assert.AreEqual(4, lines.Length);
        Assert.AreEqual("Seats: A1, A2", lines[0]);
        Assert.AreEqual("Tickets: 2 x Rs 250.00 = Rs 500.00", lines[1]);
        Assert.AreEqual("To pay: Rs 500.00", lines[2]);
        Assert.AreEqual("Points to earn: none for guests, sign in to earn points", lines[3]);
    }

    [TestMethod]
    public void DescribeLines_StandardMember_HasNoDiscountLine()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), TWO_SEATS, MakeMember(Tier.Standard));

        string[] lines = summary.DescribeLines();

        Assert.AreEqual(4, lines.Length);
        Assert.AreEqual("To pay: Rs 500.00", lines[2]);
        Assert.AreEqual("Points to earn: 50", lines[3]);
    }

    [TestMethod]
    public void DescribeLines_SilverMember_ShowsTheDiscountLine()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), TWO_SEATS, MakeMember(Tier.Silver));

        string[] lines = summary.DescribeLines();

        Assert.AreEqual(5, lines.Length);
        Assert.AreEqual("Silver discount (5%): -Rs 25.00", lines[2]);
        Assert.AreEqual("To pay: Rs 475.00", lines[3]);
        Assert.AreEqual("Points to earn: 59", lines[4]);
    }

    [TestMethod]
    public void DescribeLines_GoldMemberOneSeat_ShowsTheGoldDiscount()
    {
        BookingSummary summary = new BookingSummary(MakeShowing(), new string[] { "C5" }, MakeMember(Tier.Gold));

        string[] lines = summary.DescribeLines();

        Assert.AreEqual("Seats: C5", lines[0]);
        Assert.AreEqual("Tickets: 1 x Rs 250.00 = Rs 250.00", lines[1]);
        Assert.AreEqual("Gold discount (10%): -Rs 25.00", lines[2]);
        Assert.AreEqual("To pay: Rs 225.00", lines[3]);
    }
}
