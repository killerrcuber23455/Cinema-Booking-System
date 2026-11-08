using System;

//cinema member (owner: shankar)
namespace CinemaBooking.Models
{
    public class Member
    {
        //validation rules constant values
        private const int MAX_NAME_LENGTH = 100;
        private const int MAX_EMAIL_LENGTH = 100;
        private const int MIN_PHONE_DIGITS = 7;
        private const int MAX_PHONE_DIGITS = 15;

        //member properties
        public int Id { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string Phone { get; set; }
        public Tier Tier { get; set; }
        public int Points { get; set; } //points the member has
        public int LifetimePoints { get; set; } //all points member ever earned, this is used to calculate his tier
        public DateTime JoinedAt { get; set; }

        //constructor
        public Member(int id, string name, string email, string phone, Tier tier, int points,
            int lifetime_points, DateTime joined_at)
        {
            Id = id;
            Name = name;
            Email = email;
            Phone = phone;
            Tier = tier;
            Points = points;
            LifetimePoints = lifetime_points;
            JoinedAt = joined_at;
        }

        //validation method it returns false with a reason
        public static bool IsValidId(int id, out string error_message)
        {
            if (id <= 0)
            {
                error_message = "member id should be a positive number";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidName(string name, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(name) || name.Length > MAX_NAME_LENGTH)
            {
                error_message = "name cannot be empty and must be " + MAX_NAME_LENGTH + " characters or fewer";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidEmail(string email, out string error_message)
        {
            if (string.IsNullOrWhiteSpace(email) || email.Length > MAX_EMAIL_LENGTH)
            {
                error_message = "email cant be empty and must be " + MAX_EMAIL_LENGTH + " characters or fewer";
                return false;
            }
            //finding index of @
            int at_index = email.IndexOf('@');
            //last occurence of .
            int dot_index = email.LastIndexOf('.');
            //making sure @ is not at index 0 and there is only one @
            bool has_one_at = at_index > 0 && at_index == email.LastIndexOf('@');
            //dot comes after the @ and the dot is not at the end
            bool has_dot_after_at = dot_index > at_index + 1 && dot_index < email.Length - 1;
            if (!has_one_at || !has_dot_after_at || email.Contains(' '))
            {
                error_message = "email must look like name@site.com";
                return false;
            }
            error_message = "";
            return true;
        }


        public static bool IsValidPhone(string phone, out string error_message)
        {
            //digits, space, +- are allowed
            int digit_count = 0;
            if (!string.IsNullOrWhiteSpace(phone))
            {
                foreach (char character in phone)
                {
                    if (char.IsDigit(character))
                    {
                        digit_count++;
                    }
                    else if (character != ' ' && character != '+' && character != '-')
                    {
                        digit_count = -1;
                        break;
                    }
                }
            }
            if (digit_count < MIN_PHONE_DIGITS || digit_count > MAX_PHONE_DIGITS)
            {
                error_message = "phone must have " + MIN_PHONE_DIGITS + " to " + MAX_PHONE_DIGITS
                    + " digits and only digits, spaces, + or -";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidTier(Tier tier, out string error_message)
        {
            if (!Enum.IsDefined(tier))
            {
                error_message = "tier must be Standard, Silver or Gold";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidPoints(int points, int lifetime_points, out string error_message)
        {
            //spendable points cant be more than what he ever earned
            if (points < 0 || lifetime_points < 0 || points > lifetime_points)
            {
                error_message = "points must be 0 or more and cannot be above lifetime points";
                return false;
            }
            error_message = "";
            return true;
        }

        public static bool IsValidJoinedAt(DateTime joined_at, out string error_message)
        {
            if (joined_at == default(DateTime) || joined_at > DateTime.Now)
            {
                error_message = "join date must be a real date, not a future day";
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
            if (!IsValidName(Name, out error_message)) all_errors += error_message + "\n";
            if (!IsValidEmail(Email, out error_message)) all_errors += error_message + "\n";
            if (!IsValidPhone(Phone, out error_message)) all_errors += error_message + "\n";
            if (!IsValidTier(Tier, out error_message)) all_errors += error_message + "\n";
            if (!IsValidPoints(Points, LifetimePoints, out error_message)) all_errors += error_message + "\n";
            if (!IsValidJoinedAt(JoinedAt, out error_message)) all_errors += error_message + "\n";

            errors = all_errors;
            return all_errors == "";
        }

        //one readable line per member
        public override string ToString()
        {
            return $"{Id}: {Name} ({Email}) - {Tier}, {Points} points, joined {JoinedAt:yyyy-MM-dd}";
        }
    }
}
