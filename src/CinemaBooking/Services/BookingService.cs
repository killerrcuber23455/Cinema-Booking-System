//booking rules: book seats, make reference and work out the total(owner shankar)
using CinemaBooking.Models;
using CinemaBooking.Data;
using CinemaBooking.DataStructures;
using System;

namespace CinemaBooking.Services
{
    public class BookingService
    {
        //screen size from row A to H and seats 1 to 12
        private const char FIRST_ROW_LETTER= 'A';
        private const char LAST_ROW_LETTER = 'H';
        private const int SEATS_PER_ROW = 12;

        //reference doesnt use 0 o 1 I because they look similar
        private const string REFERENCE_PREFIX = "CB-";
        private const int REFERENCE_LENGTH = 6;
        private const string REFERENCE_CHARS = "ABCDEFGHJKLMNPQRSTUVWXYZ23456789";
        private const int MAX_REFERENCE_TRIES = 20;

        //random generator for whole seervice
        private static readonly Random _random = new Random();

        //the database and the showings table are given to us, we do not create them here
        private readonly Database _database;
        private readonly ShowingTable _showing_table;

        public BookingService(Database database, ShowingTable showing_table)
        {
            _database = database;
            _showing_table = showing_table;
        }

        //book seats
        //true with the booking, false with a message
        public bool BookSeats(Role role, int? member_id, string guest_name, string guest_email,
            string guest_phone, int showing_id, string[] seat_codes, int points_to_use,
            out Booking booking, out string error_message)
        {
            booking = null;
            error_message = "";

            //role check
            if (!CanBook(role))
            {
                error_message = "your role isnt allowed to book seats";
                return false;
            }

            //check what the form sent
            if (!AreBookingDetailsValid(member_id, guest_name, guest_email, guest_phone,
                showing_id, seat_codes, points_to_use, out error_message))
            {
                return false;
            }

            //the showing must exist, it holds the price and the seat grid
            Showing? showing = _showing_table.FindShowing(showing_id);
            if (showing == null)
            {
                error_message = "this showing does not exist";
                return false;
            }
            SeatGrid grid = showing.SeatGrid;

            //check every seat before booking any, so a taken seat refuses the whole booking
            for (int index = 0; index < seat_codes.Length; index++)
            {
                if (!grid.IsSeatFree(seat_codes[index]))
                {
                    error_message = "seat " + seat_codes[index] + " is already taken";
                    return false;
                }
            }

            //a guest has no tier and no points, a member brings their own tier and points
            bool is_guest = !member_id.HasValue;
            Tier tier = Tier.Standard;
            int current_points = 0;
            int current_lifetime_points = 0;
            if (member_id.HasValue)
            {
                Member? member = FindMember(member_id.Value);
                if (member == null)
                {
                    error_message = "member not found";
                    return false;
                }
                tier = member.Tier;
                current_points = member.Points;
                current_lifetime_points = member.LifetimePoints;
            }

            //money and points
            decimal amount_paid = AmountPaid(showing.Price, seat_codes.Length, tier);
            int points_earned = PointsForBooking(is_guest, amount_paid, tier);

            //the reference
            string reference;
            if (!TryMakeUniqueReference(out reference, out error_message))
            {
                return false;
            }

            //build the booking, the id is 0 until the database gives us the real one
            Booking new_booking = new Booking(0, reference, showing_id, member_id, guest_name,
                guest_email, guest_phone, seat_codes, amount_paid, points_earned, 0,
                BookingStatus.Confirmed, DateTime.Now);

            //book the seats in the grid, all or nothing
            if (!grid.BookSeats(seat_codes))
            {
                error_message = "one of the seats was just taken, please choose again";
                return false;
            }

            //save, the database is the last guard against a double booking, -1 means a seat is taken
            int new_id;
            try
            {
                new_id = _database.SaveBooking(new_booking);
            }
            catch
            {
                //the database failed, give the seats back and pass the error on
                grid.FreeSeats(seat_codes);
                throw;
            }

            if (new_id <= 0)
            {
                grid.FreeSeats(seat_codes);
                error_message = "one of the seats was just taken, please choose again";
                return false;
            }
            new_booking.Id = new_id;

            //a member gets the points and may reach a new tier
            if (member_id.HasValue)
            {
                int new_points = current_points + points_earned;
                int new_lifetime_points = current_lifetime_points + points_earned;
                Tier new_tier = LoyaltyService.TierForLifetimePoints(new_lifetime_points);
                _database.UpdateMemberPoints(member_id.Value, new_points, new_lifetime_points, new_tier);
            }

            booking = new_booking;
            error_message = "";
            return true;
        }

        //looks for one member by id, null if there is none
        private Member? FindMember(int member_id)
        {
            Member[] members = _database.GetAllMembers();
            for (int index = 0; index < members.Length; index++)
            {
                if (members[index].Id == member_id)
                {
                    return members[index];
                }
            }
            return null;
        }



        //all roles can book
        private static bool CanBook(Role role)
        {
            return role == Role.Guest || role == Role.Customer || role == Role.Staff || role == Role.Admin;
        }

        //checks one seat is on our screens, rows A to H and seats 1 to 12
        //Booking.IsValidSeats already checked the format, this checks the real screen size
        private static bool IsSeatOnScreen(string seat_code, out string error_message)
        {
            if (string.IsNullOrEmpty(seat_code) || seat_code.Length < 2)
            {
                error_message = "seat code must be a letter then a number, for example C5";
                return false;
            }
            if (seat_code[0] < FIRST_ROW_LETTER || seat_code[0] > LAST_ROW_LETTER)
            {
                error_message = "seat " + seat_code + ": row must be " + FIRST_ROW_LETTER + " to " + LAST_ROW_LETTER;
                return false;
            }
            int seat_number;
            if (!int.TryParse(seat_code.Substring(1), out seat_number) || seat_number < 1 || seat_number > SEATS_PER_ROW)
            {
                error_message = "seat " + seat_code + ": seat number must be 1 to " + SEATS_PER_ROW;
                return false;
            }
            error_message = "";
            return true;
        }

        //check what the form sent before touching data
        private static bool AreBookingDetailsValid(int? member_id, string guest_name, string guest_email, string guest_phone,
            int showing_id, string[] seat_codes, int points_to_use,out string error_message)
        {
            if (!Booking.IsValidShowingId(showing_id, out error_message)) return false;
            if (!Booking.IsValidMemberId(member_id, out error_message)) return false;
            if (!Booking.IsValidGuestName(member_id, guest_name, out error_message)) return false;
            if (!Booking.IsValidGuestEmail(member_id, guest_email, out error_message)) return false;
            if (!Booking.IsValidGuestPhone(member_id, guest_phone, out error_message)) return false;
            if (!Booking.IsValidSeats(seat_codes, out error_message)) return false;
            for (int index = 0; index < seat_codes.Length; index++)
            {
                if (!IsSeatOnScreen(seat_codes[index], out error_message)) return false;
            }

            //-----using points to pay is card 330, so for now only 0 is accepted
            if (points_to_use != 0 )
            {
                error_message = "Using points to pay is not available yet";
                return false;
            }
            error_message = "";
            return true;
        }

        //builds a reference from random characters
        private static string GenerateReference()
        {
            string random_part = "";
            for (int count  = 0;count < REFERENCE_LENGTH; count++)
            {
                int char_index = _random.Next(REFERENCE_CHARS.Length);
                random_part += REFERENCE_CHARS[char_index];
            }
            return REFERENCE_PREFIX + random_part;
        }

        private bool TryMakeUniqueReference(out string reference, out string error_message)
        {
            for (int attempt = 0; attempt < MAX_REFERENCE_TRIES; attempt++)
            {
                string new_reference = GenerateReference();
                string check_message;
                if (Booking.IsValidReference(new_reference, out check_message) && !_database.ReferenceExists(new_reference))
                {
                    reference = new_reference;
                    error_message = "";
                    return true;
                }
            }
            reference = "";
            error_message = "Could not make a unique reference, please try again!";
            return false;
        }

        //what customer pays, price for every seat followed by tier discount
        //guest has no tier so Tier.Standard for a guest
        private static decimal AmountPaid(decimal showing_price, int seat_count, Tier tier)
        {
            decimal ticket_total = showing_price * seat_count;
            return LoyaltyService.ApplyTierDiscount(ticket_total, tier);
        }

        //points earned on amount paid, guest doesnt earn points
        private static int PointsForBooking(bool is_guest, decimal amount_paid, Tier tier)
        {
            if (is_guest) return 0;
            return LoyaltyService.PointsEarned(amount_paid, tier);
        }

    }
}
