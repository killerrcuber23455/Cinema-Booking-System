// how much bigger the text and spaces get when the window is made wider or maximised (owner: divyesh)
// three steps only, so the pages are drawn for three sizes and never for every pixel of width
using System;

namespace CinemaBooking.UI
{
    public static class UiScale
    {
        // the window width the pages were first drawn for
        public const int BASE_WIDTH = 1100;

        // from these widths the text and spaces go up one step
        public const int MEDIUM_FROM_WIDTH = 1400;
        public const int LARGE_FROM_WIDTH = 1800;

        public const float SMALL_STEP = 1.0f;
        public const float MEDIUM_STEP = 1.15f;
        public const float LARGE_STEP = 1.3f;

        // the step for a window this wide, a minimised window (width 0) keeps the small step
        public static float StepFor(int window_width)
        {
            if (window_width >= LARGE_FROM_WIDTH)
            {
                return LARGE_STEP;
            }

            if (window_width >= MEDIUM_FROM_WIDTH)
            {
                return MEDIUM_STEP;
            }

            return SMALL_STEP;
        }

        // a font size made bigger by the step, rounded to half a point so the text stays sharp
        public static float Size(float base_points, float step)
        {
            double grown = (double)base_points * step;
            return (float)(Math.Round(grown * 2.0, MidpointRounding.AwayFromZero) / 2.0);
        }

        // a space or a height in pixels made bigger by the step
        public static int Pixels(int base_pixels, float step)
        {
            return (int)Math.Round((double)base_pixels * step, MidpointRounding.AwayFromZero);
        }
    }
}
