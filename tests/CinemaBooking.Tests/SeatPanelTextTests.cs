// tests for the words of the summary panel next to the seat map (owner: divyesh)
// written before the code of UI upgrade step 3, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class SeatPanelTextTests
{
    [TestMethod]
    public void TicketsText_NoSeats_AsksToChooseSeats()
    {
        Assert.AreEqual("No seats chosen yet", SeatPanelText.TicketsText(0));
    }

    [TestMethod]
    public void TicketsText_OneSeat_IsSingular()
    {
        Assert.AreEqual("1 ticket", SeatPanelText.TicketsText(1));
    }

    [TestMethod]
    public void TicketsText_ThreeSeats_IsPlural()
    {
        Assert.AreEqual("3 tickets", SeatPanelText.TicketsText(3));
    }

    [TestMethod]
    public void PriceEachText_ShowsTwoDecimals()
    {
        Assert.AreEqual("Rs 300.00 each", SeatPanelText.PriceEachText(300m));
    }

    [TestMethod]
    public void TotalText_TwoSeats_IsTwiceThePrice()
    {
        Assert.AreEqual("Rs 600.00", SeatPanelText.TotalText(2, 300m));
    }

    // the text reads the same on every computer: no thousands comma, a dot for the cents
    [TestMethod]
    public void TotalText_BigAmount_HasNoThousandsComma()
    {
        Assert.AreEqual("Rs 1234.50", SeatPanelText.TotalText(1, 1234.5m));
    }
}
