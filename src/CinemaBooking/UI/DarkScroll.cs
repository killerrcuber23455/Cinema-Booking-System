// gives a scrolling panel the dark Windows scroll bar instead of the bright white one (owner: divyesh)
// it uses the dark theme that Windows 10 (1809 and later) and Windows 11 have, older Windows just keep the normal bar
using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace CinemaBooking.UI
{
    public static class DarkScroll
    {
        [DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
        private static extern int SetWindowTheme(IntPtr window, string? app_name, string? id_list);

        public static void Apply(Control control)
        {
            if (control.IsHandleCreated)
            {
                SetDark(control);
            }

            // windows makes a new handle in some cases, the dark bar must be set again then
            control.HandleCreated += (sender, e) => SetDark(control);
        }

        private static void SetDark(Control control)
        {
            try
            {
                SetWindowTheme(control.Handle, "DarkMode_Explorer", null);
            }
            catch (DllNotFoundException)
            {
                // no theme library, the normal bar stays
            }
            catch (EntryPointNotFoundException)
            {
                // a very old Windows, the normal bar stays
            }
        }
    }
}
