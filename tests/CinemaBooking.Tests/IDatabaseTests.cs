// tests that the services depend on IDatabase, not on the Database class (owner: kushal)
using CinemaBooking.Data;
using CinemaBooking.Services;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Linq;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class IDatabaseTests
    {
        [TestMethod]
        public void Database_Implements_IDatabase()
        {
            Assert.IsNotNull(typeof(Database).GetInterface("IDatabase"));
        }

        [TestMethod]
        public void IDatabase_Has_The_Methods_The_Services_Use()
        {
            Type? idb = typeof(Database).GetInterface("IDatabase");
            Assert.IsNotNull(idb);

            string[] names = { "LoadFilms", "SaveMember", "GetAllMembers", "UpdateMemberPoints",
                "SaveBooking", "ReferenceExists", "GetBookingsForMember", "FindBooking",
                "CancelBooking", "GetTakenSeats", "SaveShowing", "LoadShowings" };

            foreach (string name in names)
            {
                Assert.IsNotNull(idb.GetMethod(name), "missing: " + name);
            }
        }

        [TestMethod]
        public void BookingService_Takes_IDatabase()
        {
            AssertConstructorTakesIDatabase(typeof(BookingService));
        }

        [TestMethod]
        public void MemberService_Takes_IDatabase()
        {
            AssertConstructorTakesIDatabase(typeof(MemberService));
        }

        private static void AssertConstructorTakesIDatabase(Type service)
        {
            bool found = service.GetConstructors()
                .Any(c => c.GetParameters().Any(p => p.ParameterType.Name == "IDatabase"));
            Assert.IsTrue(found, service.Name + " must take IDatabase");
        }
    }
}