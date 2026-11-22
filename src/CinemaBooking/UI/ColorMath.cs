// mixes two colours and finds the main colour of a picture, used for glows and the poster tint (owner: divyesh)
using System.Drawing;

namespace CinemaBooking.UI
{
    public static class ColorMath
    {
        // a big poster is not read pixel by pixel, about 24 x 24 points are enough to know its colour
        private const int SAMPLES_PER_SIDE = 24;

        // amount 0 gives the first colour, 1 gives the second, 0.5 the middle. the result is never see-through
        public static Color Blend(Color from, Color to, double amount)
        {
            double share = Math.Max(0, Math.Min(1, amount));

            return Color.FromArgb(
                MixChannel(from.R, to.R, share),
                MixChannel(from.G, to.G, share),
                MixChannel(from.B, to.B, share));
        }

        // the average colour of the picture, read on a grid of points
        public static Color Average(Bitmap picture)
        {
            int step_x = Math.Max(1, picture.Width / SAMPLES_PER_SIDE);
            int step_y = Math.Max(1, picture.Height / SAMPLES_PER_SIDE);

            double red_total = 0;
            double green_total = 0;
            double blue_total = 0;
            int points = 0;

            for (int y = 0; y < picture.Height; y += step_y)
            {
                for (int x = 0; x < picture.Width; x += step_x)
                {
                    Color point = picture.GetPixel(x, y);
                    red_total += point.R;
                    green_total += point.G;
                    blue_total += point.B;
                    points++;
                }
            }

            if (points == 0)
            {
                return Color.Black;
            }

            return Color.FromArgb(RoundChannel(red_total / points), RoundChannel(green_total / points),
                RoundChannel(blue_total / points));
        }

        private static int MixChannel(int from, int to, double share)
        {
            return RoundChannel(from + ((to - from) * share));
        }

        // 127.5 becomes 128, and the value always stays a real colour channel
        private static int RoundChannel(double value)
        {
            int rounded = (int)Math.Round(value, MidpointRounding.AwayFromZero);
            return Math.Max(0, Math.Min(255, rounded));
        }
    }
}
