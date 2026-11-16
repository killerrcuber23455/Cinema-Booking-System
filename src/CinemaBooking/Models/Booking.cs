using System;

//a booking for a movie/showing a member or guest makes it(owner: shankar)
namespace CinemaBooking.Models
{
    public class Booking
    {
        //constants for validation rules
        private const int MIN_SEATS_PER_BOOKING = 1;

        //properties
        public int Id { get; set; }
        public string Reference { get; set; } //made by BookingService
        public int ShowingId { get; set; }
        public int? MemberId { get; set; } //null means a guest booking
        public string GuestName { get; set; } //only used when MemberId is null
        public string GuestEmail { get; set; }
        public string GuestPhone { get; set; }
        public string[] Seats { get; set; } //seat codes like C5, C6
        public decimal Total { get; set; } //final price paid, after tier discount and points used
        public int PointsEarned { get; set; }
        public int PointsUsed { get; set; }
        public BookingStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }

        //result true when user is a guest used to decide about points
        public bool IsGuest
        {
            get { return MemberId == null; }
        }

        //constructor
        public Booking(int id, string reference, int showing_id, int? member_id,
            string guest_name, string guest_email, string guest_phone, string[] seats, 
            decimal total, int points_earned, int points_used, BookingStatus status, DateTime created_at)
        {
            Id = id;
            Reference  = reference;
            ShowingId = showing_id;
            MemberId = member_id;
            GuestName = guest_name;
            GuestEmail = guest_email;
            GuestPhone = guest_phone;
            Seats = seats;
            Total = total;
            PointsEarned = points_earned;
            PointsUsed = points_used;
            Status = status;
            CreatedAt = created_at;
        }

        //validation which returns false if it fails with a reason
        public static bool IsValidId(int id, out string error_message)
        {
            if (id <= 0)
            {
                error_message = "booking id should be a positive number";
                return false;
            }
            error_message = "";
            return true;
        }

        //validation for reference does a presence check
        public static bool IsValidReference(string reference, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(reference))
            {
                error_message = "booking reference cannot be empty";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidShowingId(int showing_id, out string error_message)
        {
            if (showing_id <= 0)
            {
                error_message = "showing id should be a positive number";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidMemberId(int? member_id, out string error_message)
        {
            //null means a guest booking
            if (member_id.HasValue && member_id.Value <= 0)
            {
                error_message = "member id should be a positive number or empty(null) for a guest";
                return false;
            }
            error_message = "";
            return true;
        }

        //guest details are only checked for a guest booking, the member record holds them otherwise
        public static bool IsValidGuestName(int? member_id, string guest_name, out string error_message)
        {
            if (member_id.HasValue)
            {
                error_message = "";
                return true;
            }
            string member_message;
            if (!Member.IsValidName(guest_name, out member_message))
            {
                error_message = "guest " + member_message;
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidGuestEmail(int? member_id, string guest_email, out string error_message)
        {
            if (member_id.HasValue)
            {
                error_message = "";
                return true;
            }
            string member_message;
            if (!Member.IsValidEmail(guest_email, out member_message))
            {
                error_message = "guest " + member_message;
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidGuestPhone(int? member_id, string guest_phone, out string error_message) 
        {
            if (member_id.HasValue)
            {
                error_message = "";
                return true;
            }
            string member_message;
            if (!Member.IsValidPhone(guest_phone, out member_message))
            {
                error_message = "guest " + member_message;
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidSeats(string[] seats, out string error_message) 
        {
            if (seats == null || seats.Length < MIN_SEATS_PER_BOOKING)
            {
                error_message = "a booking needs at least " + MIN_SEATS_PER_BOOKING + " seat";
                return false;
            }
            for (int index = 0; index < seats.Length; index++)
            {
                if (!IsValidSeatCode(seats[index]))
                {
                    error_message = "seat code must be a letter then a number, for example C5";
                    return false;
                }
                //the same seat cannot appear twice in one booking
                for (int other_index = index + 1; other_index < seats.Length; other_index++)
                {
                    if (seats[index] == seats[other_index])
                    {
                        error_message = "seat " + seats[index] + " is listed more than once";
                        return false;
                    }
                }
            }
            error_message = "";
            return true;
        }

        //one seat code a capital letter for the row and seat number with no leading zero
        private static bool IsValidSeatCode(string seat_code)
        {
            if (string.IsNullOrEmpty(seat_code) || seat_code.Length < 2)
            {
                return false;
            }
            if (seat_code[0] < 'A' || seat_code[0] > 'Z')
            {
                return false;
            }
            if (seat_code[1] == '0')
            {
                return false;
            }
            for (int index = 1; index < seat_code.Length; index++)
            {
                if (!char.IsAsciiDigit(seat_code[index]))
                {
                    return false;
                }
            }
            return true;
        }

        public static bool IsValidTotal(decimal total, out string error_message)
        {
            //we allowed 0 if the user has enough points to pay his ticket
            if (total < 0)
            {
                error_message = "total cannot be negative";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidPoints(int? member_id, int points_earned, int points_used, out string error_message)
        {
            if (points_earned < 0 || points_used < 0)
            {
                error_message = "points earned and points used cannot be negative";
                return false;
            }
            //guests have no account, so they never earn or use points
            if (!member_id.HasValue && (points_earned != 0 || points_used != 0))
            {
                error_message = "a guest booking cannot earn or use points";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidStatus(BookingStatus status, out string error_message)
        {
            //status can be only one of the enum
            if (!Enum.IsDefined(status))
            {
                error_message = "status must be Confirmed or Cancelled";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidCreatedAt(DateTime created_at, out string error_message)
        {
            if (created_at == default(DateTime) || created_at > DateTime.Now)
            {
                error_message = "booking date must be a real date, not a future day";
                return false;
            }
            error_message = "";
            return true;
        }

        public bool IsValid(out string errors)
        {
            string all_errors = "";
            string error_message;
            if (!IsValidId(Id, out error_message)) all_errors += error_message + "\n";
            if (!IsValidReference(Reference, out error_message)) all_errors += error_message + "\n";
            if (!IsValidShowingId(ShowingId, out error_message)) all_errors += error_message + "\n";
            if (!IsValidMemberId(MemberId, out error_message)) all_errors += error_message + "\n";
            if (!IsValidGuestName(MemberId, GuestName, out error_message)) all_errors += error_message + "\n";
            if (!IsValidGuestEmail(MemberId, GuestEmail, out error_message)) all_errors += error_message + "\n";
            if (!IsValidGuestPhone(MemberId, GuestPhone, out error_message)) all_errors += error_message + "\n";
            if (!IsValidSeats(Seats, out error_message)) all_errors += error_message + "\n";
            if (!IsValidTotal(Total, out error_message)) all_errors += error_message + "\n";
            if (!IsValidPoints(MemberId, PointsEarned, PointsUsed, out error_message)) all_errors += error_message + "\n";
            if (!IsValidStatus(Status, out error_message)) all_errors += error_message + "\n";
            if (!IsValidCreatedAt(CreatedAt, out error_message)) all_errors += error_message + "\n";

            errors = all_errors;
            return all_errors == "";
        }

        public override string ToString()
        {
            string who = MemberId.HasValue ? "member " + MemberId.Value : "guest " + GuestName;
            string seat_list = Seats == null ? "no seats" : string.Join(", ", Seats);
            return $"{Id}: {Reference} - showing {ShowingId}, {who}, seats {seat_list}, total {Total:0.00}, {Status}";
        }

    }
}
