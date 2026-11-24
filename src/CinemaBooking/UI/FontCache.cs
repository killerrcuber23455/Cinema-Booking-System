// the Inter fonts at the sizes the pages ask for, made once and shared, so a resize does not make new fonts each time (owner: divyesh)
// only the window thread uses it, the fonts live as long as the program
using System.Collections.Generic;
using System.Drawing;
using System.Globalization;

namespace CinemaBooking.UI
{
    public static class FontCache
    {
        public const string REGULAR = "regular";
        public const string MEDIUM = "medium";
        public const string SEMIBOLD = "semibold";
        public const string BOLD = "bold";
        public const string DISPLAY = "display";

        private static readonly Dictionary<string, Font> _fonts = new Dictionary<string, Font>();

        // the font of this kind at this size, made the first time it is asked for
        public static Font Get(string kind, float size)
        {
            string key = kind + "|" + size.ToString(CultureInfo.InvariantCulture);

            if (_fonts.TryGetValue(key, out Font? found))
            {
                return found;
            }

            Font made;
            switch (kind)
            {
                case DISPLAY:
                    made = AppFonts.Display(size);
                    break;
                case BOLD:
                    made = AppFonts.Bold(size);
                    break;
                case SEMIBOLD:
                    made = AppFonts.SemiBold(size);
                    break;
                case MEDIUM:
                    made = AppFonts.Medium(size);
                    break;
                default:
                    made = AppFonts.Regular(size);
                    break;
            }

            _fonts[key] = made;
            return made;
        }

        // the font of this kind at a theme size made bigger by the window step
        public static Font Scaled(string kind, float base_size, float step)
        {
            return Get(kind, UiScale.Size(base_size, step));
        }
    }
}
