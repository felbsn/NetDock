using System.Drawing;
using System.Windows.Forms;

namespace NetDock.WPF.Forms;



public class DockPreview : Form
{
    static DockPreview instance;
    private DockPreview()
    {
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

}

