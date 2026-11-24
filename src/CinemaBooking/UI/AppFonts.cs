// loads the Inter font that ships with the app, so the windows look the same on every PC (owner: divyesh)
// Inter is free (SIL Open Font License, see fonts/Inter-LICENSE.txt). If a file is missing, Segoe UI is used instead
using System;
using System.Drawing;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;

namespace CinemaBooking.UI
{
    public static class AppFonts
    {
        public const string FALLBACK_FAMILY = "Segoe UI";

        // the names Windows gives each file of the font
        private const string INTER = "Inter";
        private const string INTER_MEDIUM = "Inter Medium";
        private const string INTER_SEMIBOLD = "Inter SemiBold";
        private const string INTER_DISPLAY = "Inter Display";

        private static readonly string[] FONT_FILES =
        {
            "Inter-Regular.ttf",
            "Inter-Medium.ttf",
            "Inter-SemiBold.ttf",
            "Inter-Bold.ttf",
            "InterDisplay-Bold.ttf"
        };

        // the font is only for this app, it is not installed on the PC
        private const uint FR_PRIVATE = 0x10;

        // labels and buttons draw text with GDI, which only sees a font file that is added this way
        [DllImport("gdi32.dll", CharSet = CharSet.Unicode)]
        private static extern int AddFontResourceEx(string file_name, uint flags, IntPtr reserved);

        // custom painted controls draw text with GDI+, which needs the font in this collection
        private static readonly PrivateFontCollection _fonts = new PrivateFontCollection();

        static AppFonts()
        {
            string folder = Path.Combine(AppContext.BaseDirectory, "fonts");

            foreach (string file_name in FONT_FILES)
            {
                string path = Path.Combine(folder, file_name);

                if (!File.Exists(path))
                {
                    continue;
                }

                try
                {
                    _fonts.AddFontFile(path);
                    AddFontResourceEx(path, FR_PRIVATE, IntPtr.Zero);
                }
                catch (ArgumentException)
                {
                    // a broken file is skipped, the fallback font is used for it
                }
                catch (IOException)
                {
                    // a file that cannot be read is skipped too
                }
            }
        }

        // true when Inter was found next to the app
        public static bool IsInterLoaded
        {
            get { return FindFamily(INTER) != null; }
        }

        public static Font Regular(float size)
        {
            return Make(INTER, size, FontStyle.Regular, FontStyle.Regular);
        }

        public static Font Medium(float size)
        {
            return Make(INTER_MEDIUM, size, FontStyle.Regular, FontStyle.Regular);
        }

        public static Font SemiBold(float size)
        {
            return Make(INTER_SEMIBOLD, size, FontStyle.Regular, FontStyle.Bold);
        }

        public static Font Bold(float size)
        {
            return Make(INTER, size, FontStyle.Bold, FontStyle.Bold);
        }

        // the tighter cut of Inter for big titles, it only ships in bold
        public static Font Display(float size)
        {
            return Make(INTER_DISPLAY, size, FontStyle.Bold, FontStyle.Bold);
        }

        private static Font Make(string family_name, float size, FontStyle style, FontStyle fallback_style)
        {
            FontFamily? family = FindFamily(family_name);

            if (family != null && family.IsStyleAvailable(style))
            {
                return new Font(family, size, style);
            }

            return new Font(FALLBACK_FAMILY, size, fallback_style);
        }

        private static FontFamily? FindFamily(string family_name)
        {
            foreach (FontFamily family in _fonts.Families)
            {
                if (family.Name == family_name)
                {
                    return family;
                }
            }

            return null;
        }
    }
}
