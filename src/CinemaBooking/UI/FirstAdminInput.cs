// empty stub so the tests compile and fail first (red), the real code comes in the green commit (owner: divyesh)
namespace CinemaBooking.UI
{
    public class FirstAdminInput
    {
        public string Email { get; } = "";
        public string Password { get; } = "";

        // the place of each box in the list that CheckBoxes gives back, in page order
        public const int BOX_EMAIL = 0;
        public const int BOX_PASSWORD = 1;
        public const int BOX_CONFIRM_PASSWORD = 2;
        public const int BOX_COUNT = 3;

        // empty stub so the tests compile and fail first (red), the real code comes in the green commit
        public static string[] CheckBoxes(string email, string password, string confirm_password)
        {
            return new string[BOX_COUNT];
        }

        public static bool TryCreate(string email, string password, string confirm_password, out FirstAdminInput? input, out string error_message)
        {
            input = null;
            error_message = "not done yet";
            return false;
        }
    }
}
