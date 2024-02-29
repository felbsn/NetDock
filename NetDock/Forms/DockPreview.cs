using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Interop;

namespace NetDock.WPF.Forms;



public class DockPreview : Form
{
    static DockPreview instance;
    private DockPreview()
    {
        HandleCreated += (s, e) =>
        {
            try
            {
                var attribute = DWMWINDOWATTRIBUTE.DWMWA_WINDOW_CORNER_PREFERENCE;
                var preference = DWM_WINDOW_CORNER_PREFERENCE.DWMWCP_ROUNDSMALL;
                DwmSetWindowAttribute(this.Handle, attribute, ref preference, sizeof(uint));
            }
            catch (Exception ex)
            {
                _ = ex;
            }
        
        };
    }


    static bool darkMode = false;
    static DockPreview Get(bool dark)
    {
        darkMode = dark;
        return Get();

    }
    static DockPreview Get()
    {
        if (instance == null)
        {
            instance = new DockPreview()
            {
                Margin = Padding.Empty,
                Padding = Padding.Empty,
                ShowIcon = false,
                //BackColor = Color.White,
                //BackColor = Color.FromArgb(43, 43, 43),
                BackColor = Color.FromArgb(170, 170, 180),
                MaximizeBox = false,
                ControlBox = false,
                Opacity = 0.4,
                ShowInTaskbar = false,
                MinimumSize = new Size(1, 1),
                MinimizeBox = false,
                FormBorderStyle = FormBorderStyle.None,
                //FormBorderStyle = FormBorderStyle.SizableToolWindow,
                AutoSize = false,
                DoubleBuffered = true,
                StartPosition = FormStartPosition.Manual,
                //ForeColor = Color.White,
                //TransparencyKey = Color.White,
                //AllowTransparency = true,
            };
            instance.HandleCreated += (s, e) =>
            {
                //instance.BackColor = darkMode ? Color.FromArgb(43, 43, 43) : Color.White;
                //instance.UseDarkMode(darkMode);
            };
        }
        else
        {
            //instance.BackColor = darkMode ? Color.FromArgb(43, 43, 43) : Color.White;
            //if (instance.IsHandleCreated)
            //    instance.UseDarkMode(darkMode);
        }

        return instance;
    }

    public static void Show(Form? grab, Rectangle initial, Rectangle target, double opacity, bool dark = false)
    {
        var preview = Get(dark);


        if (!preview.Visible)
        {

            preview.SetBounds(initial.X, initial.Y, initial.Width, initial.Height);
            preview.Show();
            grab?.Activate();
        }

        preview.ResizeSmoothAsync(target.X, target.Y, target.Width, target.Height, opacity, 200);
    }

    public static void Show(Action? grab, int x0, int y0, int w0, int w1, int x, int y, int w, int h, double opacity, bool dark = false)
    {
        var preview = Get(dark);


        if (!preview.Visible)
        {

            preview.SetBounds(x0, y0, w0, w1);
            preview.Show();
            grab?.Invoke();
        }

        preview.ResizeSmoothAsync(x, y, w, h, opacity, 200);
    }

    public static new void Hide()
    {
        var win = Get();
        win.HideSmooth();
    }



    public enum DWMWINDOWATTRIBUTE
    {
        DWMWA_WINDOW_CORNER_PREFERENCE = 33
    }

    // The DWM_WINDOW_CORNER_PREFERENCE enum for DwmSetWindowAttribute's third parameter, which tells the function
    // what value of the enum to set.
    // Copied from dwmapi.h
    public enum DWM_WINDOW_CORNER_PREFERENCE
    {
        DWMWCP_DEFAULT = 0,
        DWMWCP_DONOTROUND = 1,
        DWMWCP_ROUND = 2,
        DWMWCP_ROUNDSMALL = 3
    }

    // Import dwmapi.dll and define DwmSetWindowAttribute in C# corresponding to the native function.
    [DllImport("dwmapi.dll", CharSet = CharSet.Unicode, PreserveSig = false)]
    internal static extern void DwmSetWindowAttribute(IntPtr hwnd,
                                                     DWMWINDOWATTRIBUTE attribute,
                                                     ref DWM_WINDOW_CORNER_PREFERENCE pvAttribute,
                                                     uint cbAttribute);

}

