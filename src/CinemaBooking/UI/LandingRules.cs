// decides when the landing page is shown: log in, sign up or continue as guest (owner: divyesh)
// the main window only asks this class, so the rule is in one place and can be tested
using CinemaBooking.Models;

namespace CinemaBooking.UI
{
    public static class LandingRules
    {
        // red stub: always says yes, on purpose. the real code comes in the green step
        public static bool ShouldShow(Role role, bool has_chosen)
        {
            return true;
        }
    }
}
