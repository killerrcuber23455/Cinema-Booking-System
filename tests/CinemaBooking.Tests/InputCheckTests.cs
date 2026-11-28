// tests for the input checks the forms use before they call a service (owner: divyesh)
// written before the code for card 343, so they fail first (red) and pass after (green)
using CinemaBooking.UI;

namespace CinemaBooking.Tests;

[TestClass]
public class InputCheckTests
{
    // whole numbers

    [TestMethod]
    public void TryReadWholeNumber_PlainNumber_IsRead()
    {
        bool is_read = InputCheck.TryReadWholeNumber("12", "showing id", out int value, out string message);

        Assert.IsTrue(is_read, message);
        Assert.AreEqual(12, value);
    }

    [TestMethod]
    public void TryReadWholeNumber_SpacesAroundNumber_AreIgnored()
    {
        bool is_read = InputCheck.TryReadWholeNumber("  7 ", "showing id", out int value, out string message);

        Assert.IsTrue(is_read, message);
        Assert.AreEqual(7, value);
    }

    [TestMethod]
    public void TryReadWholeNumber_Empty_SaysTheFieldCannotBeEmpty()
    {
        bool is_read = InputCheck.TryReadWholeNumber("", "showing id", out int value, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("showing id cannot be empty", message);
    }

    [TestMethod]
    [DataRow("abc")]
    [DataRow("1.5")]
    [DataRow("12a")]
    [DataRow("99999999999")]
    public void TryReadWholeNumber_NotAWholeNumber_SaysSo(string text)
    {
        bool is_read = InputCheck.TryReadWholeNumber(text, "showing id", out int value, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("showing id must be a whole number", message);
    }

    // money

    [TestMethod]
    public void TryReadMoney_TwoDecimals_IsRead()
    {
        bool is_read = InputCheck.TryReadMoney("12.50", "price", out decimal value, out string message);

        Assert.IsTrue(is_read, message);
        Assert.AreEqual(12.50m, value);
    }

    [TestMethod]
    public void TryReadMoney_WholeAmount_IsRead()
    {
        bool is_read = InputCheck.TryReadMoney("12", "price", out decimal value, out string message);

        Assert.IsTrue(is_read, message);
        Assert.AreEqual(12m, value);
    }

    [TestMethod]
    public void TryReadMoney_Empty_SaysThePriceCannotBeEmpty()
    {
        bool is_read = InputCheck.TryReadMoney("  ", "price", out decimal value, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("price cannot be empty", message);
    }

    [TestMethod]
    [DataRow("abc")]
    [DataRow("12,50")]
    [DataRow("1,250.00")]
    public void TryReadMoney_NotAnAmount_SaysSo(string text)
    {
        bool is_read = InputCheck.TryReadMoney(text, "price", out decimal value, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("price must be an amount like 12.50", message);
    }

    [TestMethod]
    public void TryReadMoney_ThreeDecimals_IsRefused()
    {
        bool is_read = InputCheck.TryReadMoney("12.505", "price", out decimal value, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("price can have two decimals at most", message);
    }

    // seat list typed by the user

    [TestMethod]
    public void TryReadSeatList_CommasAndSmallLetters_GivesCapitalCodes()
    {
        bool is_read = InputCheck.TryReadSeatList("c5, a12", out string[] seats, out string message);

        Assert.IsTrue(is_read, message);
        Assert.AreEqual(2, seats.Length);
        Assert.AreEqual("C5", seats[0]);
        Assert.AreEqual("A12", seats[1]);
    }

    [TestMethod]
    public void TryReadSeatList_SpacesBetweenSeats_IsRead()
    {
        bool is_read = InputCheck.TryReadSeatList("C5 C6  C7", out string[] seats, out string message);

        Assert.IsTrue(is_read, message);
        Assert.AreEqual(3, seats.Length);
    }

    [TestMethod]
    public void TryReadSeatList_Empty_NeedsAtLeastOneSeat()
    {
        bool is_read = InputCheck.TryReadSeatList("   ", out string[] seats, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("a booking needs at least 1 seat", message);
    }

    [TestMethod]
    public void TryReadSeatList_BadCode_GivesTheSeatCodeMessage()
    {
        bool is_read = InputCheck.TryReadSeatList("C05", out string[] seats, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("seat code must be a letter then a number, for example C5", message);
    }

    [TestMethod]
    public void TryReadSeatList_SameSeatTwiceInDifferentCase_IsRefused()
    {
        bool is_read = InputCheck.TryReadSeatList("C5, c5", out string[] seats, out string message);

        Assert.IsFalse(is_read);
        Assert.AreEqual("seat C5 is listed more than once", message);
    }

    // password

    [TestMethod]
    public void IsValidPassword_EightCharacters_IsAccepted()
    {
        bool is_valid = InputCheck.IsValidPassword("abcd1234", out string message);

        Assert.IsTrue(is_valid, message);
    }

    [TestMethod]
    public void IsValidPassword_SevenCharacters_IsRefused()
    {
        bool is_valid = InputCheck.IsValidPassword("abcd123", out string message);

        Assert.IsFalse(is_valid);
        Assert.AreEqual("password must be at least 8 characters", message);
    }

    [TestMethod]
    public void IsValidPassword_OnlySpaces_IsRefused()
    {
        bool is_valid = InputCheck.IsValidPassword("        ", out string message);

        Assert.IsFalse(is_valid);
        Assert.AreEqual("password must be at least 8 characters", message);
    }

    [TestMethod]
    public void IsValidPassword_Null_IsRefused()
    {
        bool is_valid = InputCheck.IsValidPassword(null!, out string message);

        Assert.IsFalse(is_valid);
        Assert.AreEqual("password must be at least 8 characters", message);
    }
}
