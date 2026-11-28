// tests for listing and adding screens from the admin screens window, card 406 (owner: divyesh)
// written before the code, so they fail first (red) and pass after (green)
using CinemaBooking.Models;
using CinemaBooking.Services;
using CinemaBooking.Tests.Fakes;

namespace CinemaBooking.Tests;

[TestClass]
public class ScreenServiceTests
{
    // the fake database starts with Screen 1 and Screen 2, like cinema.sql
    private static ScreenService MakeService(FakeDatabase? database = null)
    {
        return new ScreenService(database ?? new FakeDatabase());
    }

    // list

    [TestMethod]
    public void ListScreens_Admin_GivesTheTwoStartingScreensLowestIdFirst()
    {
        CinemaScreen[] screens = MakeService().ListScreens(Role.Admin, out string error);

        Assert.AreEqual("", error);
        Assert.AreEqual(2, screens.Length);
        Assert.AreEqual(1, screens[0].Id);
        Assert.AreEqual("Screen 1", screens[0].Name);
        Assert.AreEqual(2, screens[1].Id);
    }

    [TestMethod]
    public void ListScreens_Staff_IsRefused()
    {
        CinemaScreen[] screens = MakeService().ListScreens(Role.Staff, out string error);

        Assert.AreEqual(0, screens.Length);
        Assert.AreEqual(ScreenService.ADMIN_ONLY_TEXT, error);
    }

    [TestMethod]
    public void ListScreens_GuestAndCustomer_AreRefused()
    {
        Assert.AreEqual(0, MakeService().ListScreens(Role.Guest, out string guest_error).Length);
        Assert.AreEqual(ScreenService.ADMIN_ONLY_TEXT, guest_error);

        Assert.AreEqual(0, MakeService().ListScreens(Role.Customer, out string customer_error).Length);
        Assert.AreEqual(ScreenService.ADMIN_ONLY_TEXT, customer_error);
    }

    // add

    [TestMethod]
    public void AddScreen_Admin_GetsTheNextFreeId()
    {
        ScreenService service = MakeService();

        int new_id = service.AddScreen("Screen 3", 10, 14, Role.Admin, out string error);

        Assert.AreEqual(3, new_id);
        Assert.AreEqual("", error);
    }

    [TestMethod]
    public void AddScreen_Admin_TheScreenIsInTheListAfterwards()
    {
        ScreenService service = MakeService();
        service.AddScreen("Screen 3", 10, 14, Role.Admin, out string unused);

        CinemaScreen[] screens = service.ListScreens(Role.Admin, out string error);

        Assert.AreEqual(3, screens.Length);
        Assert.AreEqual("Screen 3", screens[2].Name);
        Assert.AreEqual(10, screens[2].RowCount);
        Assert.AreEqual(14, screens[2].SeatsPerRow);
    }

    [TestMethod]
    public void AddScreen_TwoScreens_GetIdsThreeAndFour()
    {
        ScreenService service = MakeService();

        Assert.AreEqual(3, service.AddScreen("Screen 3", 8, 12, Role.Admin, out string unused_one));
        Assert.AreEqual(4, service.AddScreen("IMAX", 20, 30, Role.Admin, out string unused_two));
    }

    [TestMethod]
    public void AddScreen_NameWithSpaces_IsTrimmed()
    {
        ScreenService service = MakeService();
        service.AddScreen("  Screen 3  ", 8, 12, Role.Admin, out string unused);

        Assert.AreEqual("Screen 3", service.ListScreens(Role.Admin, out string error)[2].Name);
    }

    [TestMethod]
    public void AddScreen_Staff_IsRefusedAndNothingIsSaved()
    {
        FakeDatabase database = new FakeDatabase();
        ScreenService service = MakeService(database);

        int new_id = service.AddScreen("Screen 3", 8, 12, Role.Staff, out string error);

        Assert.AreEqual(-1, new_id);
        Assert.AreEqual(ScreenService.ADMIN_ONLY_TEXT, error);
        Assert.AreEqual(2, service.ListScreens(Role.Admin, out string unused).Length);
    }

    [TestMethod]
    public void AddScreen_NameAlreadyUsed_IsRefusedEvenWithOtherCapitals()
    {
        ScreenService service = MakeService();

        int new_id = service.AddScreen("screen 1", 8, 12, Role.Admin, out string error);

        Assert.AreEqual(-1, new_id);
        StringAssert.Contains(error, "already");
        Assert.AreEqual(2, service.ListScreens(Role.Admin, out string unused).Length);
    }

    [TestMethod]
    public void AddScreen_EmptyName_IsRefusedWithTheRuleMessage()
    {
        int new_id = MakeService().AddScreen("   ", 8, 12, Role.Admin, out string error);

        Assert.AreEqual(-1, new_id);
        StringAssert.Contains(error, "name cannot be empty");
    }

    [TestMethod]
    public void AddScreen_TooManyRows_IsRefused()
    {
        int new_id = MakeService().AddScreen("Big", 27, 12, Role.Admin, out string error);

        Assert.AreEqual(-1, new_id);
        StringAssert.Contains(error, "rows must be between 1 and 26");
    }

    [TestMethod]
    public void AddScreen_TooManySeatsPerRow_IsRefused()
    {
        int new_id = MakeService().AddScreen("Wide", 8, 31, Role.Admin, out string error);

        Assert.AreEqual(-1, new_id);
        StringAssert.Contains(error, "seats per row must be between 1 and 30");
    }

    [TestMethod]
    public void AddScreen_LargestAllowedScreen_IsAccepted()
    {
        Assert.AreEqual(3, MakeService().AddScreen("Giant", 26, 30, Role.Admin, out string error));
    }

    [TestMethod]
    public void AddScreen_SmallestAllowedScreen_IsAccepted()
    {
        Assert.AreEqual(3, MakeService().AddScreen("Tiny", 1, 1, Role.Admin, out string error));
    }
}
