// tests for the text of one tier rule line, card 408 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class TierRuleLineTests
{
    [TestMethod]
    public void Text_Standard_SaysNoDiscountAndStartsAtZeroPoints()
    {
        string text = TierRuleLine.Text(new TierRuleRow(Tier.Standard, 0, 0m, 1m));

        StringAssert.Contains(text, "Standard");
        StringAssert.Contains(text, "from 0 lifetime points");
        StringAssert.Contains(text, "no discount");
        StringAssert.Contains(text, "earns 1x points");
    }

    [TestMethod]
    public void Text_Silver_ShowsPointsDiscountAndMultiplier()
    {
        string text = TierRuleLine.Text(new TierRuleRow(Tier.Silver, 100, 5m, 1.25m));

        StringAssert.Contains(text, "Silver");
        StringAssert.Contains(text, "from 100 lifetime points");
        StringAssert.Contains(text, "5% off tickets");
        StringAssert.Contains(text, "earns 1.25x points");
    }

    [TestMethod]
    public void Text_Gold_ShowsPointsDiscountAndMultiplier()
    {
        string text = TierRuleLine.Text(new TierRuleRow(Tier.Gold, 300, 10m, 1.5m));

        StringAssert.Contains(text, "Gold");
        StringAssert.Contains(text, "from 300 lifetime points");
        StringAssert.Contains(text, "10% off tickets");
        StringAssert.Contains(text, "earns 1.5x points");
    }
}
