//booking rules: book seats, make reference and work out the total(owner shankar)
using CinemaBooking.Models;
using System;

namespace CinemaBooking.Services
{
    public static class BookingService
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

        //--- temporary stubs for other people's code, replace with the real calls after the merge ---

        //todo: replace with SeatGrid.IsSeatFree (card 310)
        private static bool StubIsSeatFree(int showing_id, string seat_code)
        {
            return true;
        }

        //todo: replace with SeatGrid.BookSeats, all or nothing (card 310)
        private static bool StubBookSeatsInGrid(int showing_id, string[] seat_codes)
        {
            return true;
        }

        //todo: replace with SeatGrid.FreeSeats (card 310)
        private static bool StubFreeSeatsInGrid(int showing_id, string[] seat_codes)
        {
            return true;
        }

        //todo: replace with Database.SaveBooking (card 397), the real one returns -1 if a seat is taken
        private static int StubSaveBooking(Booking booking)
        {
            return 1;
        }

        //todo: replace with Database.ReferenceExists (card 397)
        private static bool StubReferenceExists(string reference)
        {
            return false;
        }

        //todo: replace with the real showing lookup, false means the showing does not exist
        private static bool StubGetShowingPrice(int showing_id, out decimal price)
        {
            price = 100m;
            return true;
        }

        //todo: replace with a member lookup using GetAllMembers, false means no such member
        private static bool StubGetMemberLoyalty(int member_id, out Tier tier, out int points, out int lifetime_points)
        {
            tier = Tier.Standard;
            points = 0;
            lifetime_points = 0;
            return true;
        }

        //todo: replace with Database.UpdateMemberPoints (card 397)
        private static bool StubUpdateMemberPoints(int member_id, int points, int lifetime_points, Tier tier)
        {
            return true;
        }

        //book seats
        //true with the booking, false with a message
        public static bool BookSeats(Role role, int? member_id, string guest_name, string guest_email,
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

            //the showing must exist, we need its price
            decimal showing_price;
            if (!StubGetShowingPrice(showing_id, out showing_price))
            {
                error_message = "this showing does not exist";
                return false;
            }

            //check every seat before booking any, so a taken seat refuses the whole booking
            for (int index = 0; index < seat_codes.Length; index++)
            {
                if (!StubIsSeatFree(showing_id, seat_codes[index]))
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
            if (!is_guest)
            {
                if (!StubGetMemberLoyalty(member_id.Value, out tier, out current_points, out current_lifetime_points))
                {
                    error_message = "member not found";
                    return false;
                }
            }

            //money and points
            decimal amount_paid = AmountPaid(showing_price, seat_codes.Length, tier);
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
            if (!StubBookSeatsInGrid(showing_id, seat_codes))
            {
                error_message = "one of the seats was just taken, please choose again";
                return false;
            }

            //save, the database is the last guard against a double booking, -1 or less means it refused
            int new_id = StubSaveBooking(new_booking);
            if (new_id <= 0)
            {
                //give the seats back so they are not blocked for nothing
                StubFreeSeatsInGrid(showing_id, seat_codes);
                error_message = "one of the seats was just taken, please choose again";
                return false;
            }
            new_booking.Id = new_id;

            //a member gets the points and may reach a new tier
            if (!is_guest)
            {
                int new_points = current_points + points_earned;
                int new_lifetime_points = current_lifetime_points + points_earned;
                Tier new_tier = LoyaltyService.TierForLifetimePoints(new_lifetime_points);
                StubUpdateMemberPoints(member_id.Value, new_points, new_lifetime_points, new_tier);
            }

            booking = new_booking;
            error_message = "";
            return true;

        }
        //----------------------------------------------------------------------


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

        private static bool TryMakeUniqueReference(out string reference, out string error_message)
        {
            for (int attempt = 0; attempt < MAX_REFERENCE_TRIES; attempt++)
            {
                string new_reference = GenerateReference();
                string check_message;
                if (Booking.IsValidReference(new_reference, out check_message) && !StubReferenceExists(new_reference)) //---stud here
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
