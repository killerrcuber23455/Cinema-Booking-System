// tests the arm table of the AI agent: arm name to allowed roles and handler, an unknown arm name is refused (owner: kushal)
using CinemaBooking.DataStructures;
using CinemaBooking.Models;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;

namespace CinemaBooking.Tests
{
    [TestClass]
    public class KeywordTableTests
    {
        private static readonly Role[] ALL_ROLES = { Role.Guest, Role.Customer, Role.Staff, Role.Admin };
        private static readonly Role[] STAFF_AND_ADMIN = { Role.Staff, Role.Admin };

        // ---------- Add ----------

        [TestMethod]
        public void NewTable_IsEmpty()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            Assert.AreEqual(0, table.Count);
            Assert.IsEmpty(table.ListNames());
        }

        [TestMethod]
        public void Add_NewArm_ReturnsTrue()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            Assert.IsTrue(table.Add("search_films", ALL_ROLES, "handler"));
        }

        [TestMethod]
        public void Add_NewArm_CountIsOne()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            table.Add("search_films", ALL_ROLES, "handler");

            Assert.AreEqual(1, table.Count);
        }

        [TestMethod]
        public void Add_EmptyName_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            Assert.IsFalse(table.Add("", ALL_ROLES, "handler"));
            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void Add_WhitespaceName_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            Assert.IsFalse(table.Add("   ", ALL_ROLES, "handler"));
            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void Add_NullRoles_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            Assert.IsFalse(table.Add("search_films", null!, "handler"));
            Assert.AreEqual(0, table.Count);
        }

        [TestMethod]
        public void Add_DuplicateName_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            Assert.IsTrue(table.Add("search_films", ALL_ROLES, "first"));

            Assert.IsFalse(table.Add("search_films", ALL_ROLES, "second"));
            Assert.AreEqual(1, table.Count);
        }

        [TestMethod]
        public void Add_DuplicateNameOtherCase_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            Assert.IsTrue(table.Add("search_films", ALL_ROLES, "first"));

            Assert.IsFalse(table.Add("SEARCH_Films", ALL_ROLES, "second"));
        }

        [TestMethod]
        public void Add_DuplicateName_KeepsTheFirstHandler()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "first");

            table.Add("search_films", ALL_ROLES, "second");

            KeywordEntry<string>? entry = table.Find("search_films");
            Assert.IsNotNull(entry);
            Assert.AreEqual("first", entry.Handler);
        }

        [TestMethod]
        public void Add_EmptyRoles_NobodyIsAllowed()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            Assert.IsTrue(table.Add("locked_arm", new Role[0], "handler"));

            foreach (Role role in ALL_ROLES)
            {
                Assert.IsFalse(table.IsAllowed("locked_arm", role));
            }
        }

        [TestMethod]
        public void Add_CopiesTheRoles()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            Role[] roles = { Role.Admin };
            table.Add("admin_arm", roles, "handler");

            // changing the caller's array later must not change the table
            roles[0] = Role.Guest;

            Assert.IsTrue(table.IsAllowed("admin_arm", Role.Admin));
            Assert.IsFalse(table.IsAllowed("admin_arm", Role.Guest));
        }

        [TestMethod]
        public void Add_ManyArms_AllFound()
        {
            KeywordTable<string> table = new KeywordTable<string>();

            for (int number = 0; number < 40; number++)
            {
                Assert.IsTrue(table.Add("arm_" + number, ALL_ROLES, "handler " + number));
            }

            Assert.AreEqual(40, table.Count);
            for (int number = 0; number < 40; number++)
            {
                Assert.IsTrue(table.Contains("arm_" + number));
            }
        }

        // ---------- Contains ----------

        [TestMethod]
        public void Contains_ExistingArm_ReturnsTrue()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            Assert.IsTrue(table.Contains("search_films"));
        }

        [TestMethod]
        public void Contains_UnknownArm_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            Assert.IsFalse(table.Contains("cancel_booking"));
        }

        [TestMethod]
        public void Contains_OtherCase_ReturnsTrue()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            Assert.IsTrue(table.Contains("Search_Films"));
        }

        [TestMethod]
        public void Contains_WithSpaces_ReturnsTrue()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            Assert.IsTrue(table.Contains("  search_films "));
        }

        [TestMethod]
        public void Contains_EmptyName_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            Assert.IsFalse(table.Contains(""));
        }

        // ---------- Find ----------

        [TestMethod]
        public void Find_ExistingArm_ReturnsTheEntry()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("check_free_seats", STAFF_AND_ADMIN, "seat handler");

            KeywordEntry<string>? entry = table.Find("check_free_seats");

            Assert.IsNotNull(entry);
            Assert.AreEqual("check_free_seats", entry.Name);
            Assert.AreEqual("seat handler", entry.Handler);
            Assert.HasCount(2, entry.AllowedRoles);
        }

        [TestMethod]
        public void Find_OtherCase_FindsIt()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("check_free_seats", ALL_ROLES, "seat handler");

            Assert.IsNotNull(table.Find("CHECK_FREE_SEATS"));
        }

        [TestMethod]
        public void Find_UnknownArm_ReturnsNull()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("check_free_seats", ALL_ROLES, "seat handler");

            Assert.IsNull(table.Find("cancel_booking"));
        }

        [TestMethod]
        public void Find_HandlerCanBeAFunction()
        {
            KeywordTable<Func<string, string>> table = new KeywordTable<Func<string, string>>();
            table.Add("shout", ALL_ROLES, text => text.ToUpperInvariant());

            KeywordEntry<Func<string, string>>? entry = table.Find("shout");

            Assert.IsNotNull(entry);
            Assert.AreEqual("HELLO", entry.Handler("hello"));
        }

        // ---------- IsAllowed ----------

        [TestMethod]
        public void IsAllowed_RoleInList_ReturnsTrue()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("remove_showing", STAFF_AND_ADMIN, "handler");

            Assert.IsTrue(table.IsAllowed("remove_showing", Role.Staff));
            Assert.IsTrue(table.IsAllowed("remove_showing", Role.Admin));
        }

        [TestMethod]
        public void IsAllowed_RoleNotInList_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("remove_showing", STAFF_AND_ADMIN, "handler");
            Assert.IsTrue(table.IsAllowed("remove_showing", Role.Admin));

            Assert.IsFalse(table.IsAllowed("remove_showing", Role.Guest));
            Assert.IsFalse(table.IsAllowed("remove_showing", Role.Customer));
        }

        [TestMethod]
        public void IsAllowed_AllFourRoles_AllAllowed()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            foreach (Role role in ALL_ROLES)
            {
                Assert.IsTrue(table.IsAllowed("search_films", role));
            }
        }

        [TestMethod]
        public void IsAllowed_OtherCase_ReturnsTrue()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            Assert.IsTrue(table.IsAllowed(" Search_Films ", Role.Guest));
        }

        [TestMethod]
        public void IsAllowed_UnknownArm_ReturnsFalse()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "handler");

            // an arm name that is not in the table is refused for every role
            foreach (Role role in ALL_ROLES)
            {
                Assert.IsFalse(table.IsAllowed("delete_everything", role));
            }
        }

        // ---------- ListNames ----------

        [TestMethod]
        public void ListNames_ReturnsAllNames()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add("search_films", ALL_ROLES, "a");
            table.Add("list_showings", ALL_ROLES, "b");
            table.Add("find_booking", ALL_ROLES, "c");

            string[] names = table.ListNames();

            Assert.HasCount(3, names);
            Assert.Contains("search_films", names);
            Assert.Contains("list_showings", names);
            Assert.Contains("find_booking", names);
        }

        [TestMethod]
        public void ListNames_NamesAreLowercase()
        {
            KeywordTable<string> table = new KeywordTable<string>();
            table.Add(" Search_Films ", ALL_ROLES, "a");

            string[] names = table.ListNames();

            Assert.HasCount(1, names);
            Assert.AreEqual("search_films", names[0]);
        }
    }
}