// tests for the tier rules list of the read-only tier rules window, card 408 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.Services;

namespace CinemaBooking.Tests;

[TestClass]
public class TierRuleListTests
{
    [TestMethod]
    public void Rows_AreStandardSilverGoldInThatOrder()
    {
        TierRuleRow[] rows = TierRuleList.Rows();

        Assert.AreEqual(3, rows.Length);
        Assert.AreEqual(Tier.Standard, rows[0].Tier);
        Assert.AreEqual(Tier.Silver, rows[1].Tier);
        Assert.AreEqual(Tier.Gold, rows[2].Tier);
    }

    [TestMethod]
    public void Rows_PointsNeeded_AreZeroHundredAndThreeHundred()
    {
        TierRuleRow[] rows = TierRuleList.Rows();

        Assert.AreEqual(0, rows[0].FromPoints);
        Assert.AreEqual(100, rows[1].FromPoints);
        Assert.AreEqual(300, rows[2].FromPoints);
    }

    [TestMethod]
    public void Rows_Discounts_AreZeroFiveAndTenPercent()
    {
        TierRuleRow[] rows = TierRuleList.Rows();

        Assert.AreEqual(0m, rows[0].DiscountPercent);
        Assert.AreEqual(5m, rows[1].DiscountPercent);
        Assert.AreEqual(10m, rows[2].DiscountPercent);
    }

    [TestMethod]
    public void Rows_PointsMultipliers_AreOneOnePointTwoFiveAndOnePointFive()
    {
        TierRuleRow[] rows = TierRuleList.Rows();

        Assert.AreEqual(1m, rows[0].PointsMultiplier);
        Assert.AreEqual(1.25m, rows[1].PointsMultiplier);
        Assert.AreEqual(1.5m, rows[2].PointsMultiplier);
    }

    [TestMethod]
    public void PointsNeededFor_IsExactlyWhereTheTierStarts()
    {
        foreach (Tier tier in new Tier[] { Tier.Standard, Tier.Silver, Tier.Gold })
        {
            int needed = LoyaltyService.PointsNeededFor(tier);

            Assert.AreEqual(tier, LoyaltyService.TierForLifetimePoints(needed), tier.ToString());
        }

        Assert.AreEqual(Tier.Standard, LoyaltyService.TierForLifetimePoints(LoyaltyService.PointsNeededFor(Tier.Silver) - 1));
        Assert.AreEqual(Tier.Silver, LoyaltyService.TierForLifetimePoints(LoyaltyService.PointsNeededFor(Tier.Gold) - 1));
    }

    [TestMethod]
    public void RupeesPerPoint_IsTen()
    {
        Assert.AreEqual(10m, LoyaltyService.RupeesPerPoint);
    }

    [TestMethod]
    public void PointsRateText_SaysOnePointForEveryTenRupees()
    {
        StringAssert.Contains(TierRuleList.PointsRateText(), "1 point for every Rs 10");
    }
}
