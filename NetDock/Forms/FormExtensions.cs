using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace NetDock.WPF.Forms;

public static class FormExtensions
{
    public static Dictionary<Form, (int id, int x, int y, int width, int height, double opacity, Task t, int duration)> tasks = new();


    public static void ResizeSmooth(this Form form, int width, int height, int duration = 200, bool preserve = false)
    {
        var sw = Stopwatch.StartNew();
        var p = 0.0;

        var baseW = form.Width;
        var baseH = form.Height;

        var diff_width = width - baseW;
        var diff_height = height - baseH;

        var baseX = form.Location.X;
        var baseY = form.Location.Y;
        while (p < 1.0)
        {
            p = Math.Min(sw.Elapsed.TotalMilliseconds / duration, 1.0);

            var px = Math.Sin(p * Math.PI / 2);

            var dw = (int)(baseW + px * diff_width);
            var dh = (int)(baseH + px * diff_height);

            var deltaX = preserve ? (dw - baseW) / 2 : 0;
            var deltaY = preserve ? (dh - baseH) / 2 : 0;
            form.SetBounds(baseX - deltaX, baseY - deltaY, dw, dh);
            Application.DoEvents();
        }
    }
    public static void ResizeSmooth(this Form form, int x, int y, int width, int height, int duration = 200)
    {
        form.SuspendLayout();

        var sw = Stopwatch.StartNew();
        var p = 0.0;


        var baseW = form.Width;
        var baseH = form.Height;
        var baseX = form.Location.X;
        var baseY = form.Location.Y;

        var diff_width = width - baseW;
        var diff_height = height - baseH;
        var diff_x = x - baseX;
        var diff_y = y - baseY;

        var half = false;

        var _dw = 0;
        var _dh = 0;
        var _dx = 0;
        var _dy = 0;

        while (p < 1.0)
        {
            p = Math.Min(sw.Elapsed.TotalMilliseconds / duration, 1.0);

            var px = Math.Sin(p * Math.PI / 2);

            var dw = (int)(baseW + px * diff_width);
            var dh = (int)(baseH + px * diff_height);
            var dx = (int)(baseX + px * diff_x);
            var dy = (int)(baseY + px * diff_y);

            if (_dw != dw || _dh != dh || _dx != dx || dy != _dy)
                form.SetBounds(dx, dy, dw, dh);
            else
            {
                _ = 1;
            }

            _dw = dw;
            _dh = dh;
            _dx = dx;
            _dy = dy;

            Application.DoEvents();
        }

        form.ResumeLayout();
    }
    public static Task ResizeSmoothAsync(this Form form, int x, int y, int width, int height, double opacity = 1.0, int duration = 200, bool suspend = true)
    {
        if (tasks.TryGetValue(form, out var cur))
        {
            if (cur.x == x && cur.y == y && cur.width == width && cur.height == height && opacity == cur.opacity)
                return cur.t;

            tasks[form] = (cur.id + 1, x, y, width, height, opacity, cur.t, duration);
            return cur.t;
        }
        if (!form.IsHandleCreated)
            return Task.CompletedTask;

        var p = 0.0;

        var baseW = form.Width;
        var baseH = form.Height;
        var baseX = form.Location.X;
        var baseY = form.Location.Y;
        var baseOp = form.Opacity;

        var diff_width = width - baseW;
        var diff_height = height - baseH;
        var diff_x = x - baseX;
        var diff_y = y - baseY;
        var diff_op = opacity - baseOp;

        if (diff_height == 0 && diff_width == 0 && diff_x == 0 && diff_y == 0 && diff_op == 0)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        int id = 0;
        tasks[form] = (id, x, y, width, height, opacity, tcs.Task, duration);

        // Console.WriteLine($"resize task x:{x} y:{y} w:{width} h:{height} op:{opacity}");
        var sw = Stopwatch.StartNew();

        if (suspend)
            form.SuspendLayout();

        Task.Run(() =>
        {
            while (p < 1.0)
            {
                if (tasks[form].id != id)
                {
                    // Console.WriteLine("animation changed with id " + id);
                    var c = tasks[form];
                    p = 0.0;
                    baseW = form.Width;
                    baseH = form.Height;
                    baseX = form.Location.X;
                    baseY = form.Location.Y;
                    baseOp = form.Opacity;

                    diff_width = c.width - baseW;
                    diff_height = c.height - baseH;
                    diff_x = c.x - baseX;
                    diff_y = c.y - baseY;
                    diff_op = c.opacity - baseOp;

                    id = c.id;

                    sw.Restart();
                }

                p = Math.Min(sw.Elapsed.TotalMilliseconds / duration, 1.0);

                var px = Math.Sin(p * Math.PI / 2);

                var dw = (int)(baseW + px * diff_width);
                var dh = (int)(baseH + px * diff_height);
                var dx = (int)(baseX + px * diff_x);
                var dy = (int)(baseY + px * diff_y);
                var d_o = (baseOp + px * diff_op);

                form.Invoke(() =>
                {
                    form.SetBounds(dx, dy, dw, dh);
                    form.Width = dw;
                    form.Opacity = d_o;
                });
            }

            form.Invoke(() =>
            {
                // Console.WriteLine($"finished task loc:{form.Location} width:{form.Width} height:{form.Height}");

                if (suspend)
                    form.ResumeLayout();
            });

            tcs.SetResult();
            tasks.Remove(form);
        });
        return tcs.Task;
    }
    public static void HideSmooth(this Form form, int duration = 200)
    {
        if (!form.Visible)
            return;

        var t = tasks.TryGetValue(form, out var res)
            ? form.ResizeSmoothAsync(res.x, res.y, res.width, res.height, 0)
            : form.ResizeSmoothAsync(form.Left, form.Top, form.Width, form.Height, 0);
        t?.ContinueWith(_ =>
        {
            form.Invoke(() =>
            {
                if (form.Opacity == 0)
                    form.Hide();
            });
        });

    }
    public static void StartDrag(this Form form)
    {
        const int WM_NCLBUTTONDOWN = 0xA1;
        const int HT_CAPTION = 0x2;

        ReleaseCapture();
        SendMessage(form.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
    }



    public static void MakeTransparent(this Form form)
    {
        var c = Color.FromArgb(255, 128, 127, 127);
        c = Color.FromArgb(255, 255, 254, 254);
        form.BackColor = c;
        form.TransparencyKey = c;

        int trueValue = 0x01;
        DwmSetWindowAttribute(form.Handle, DwmWindowAttribute.DWMWA_USE_IMMERSIVE_DARK_MODE, ref trueValue, Marshal.SizeOf(typeof(int)));
    }

    public static void MakeBlurred(this Form form)
    {
        var accent = new AccentPolicy { AccentState = AccentState.ACCENT_ENABLE_TRANSPARENTGRADIENT };

        var accentStructSize = Marshal.SizeOf(accent);

        var accentPtr = Marshal.AllocHGlobal(accentStructSize);
        Marshal.StructureToPtr(accent, accentPtr, false);

        var data = new WindowCompositionAttributeData
        {
            Attribute = WindowCompositionAttribute.WCA_ACCENT_POLICY,
            SizeOfData = accentStructSize,
            Data = accentPtr
        };

        SetWindowCompositionAttribute(form.Handle, ref data);
        Marshal.FreeHGlobal(accentPtr);

        var c = Color.FromArgb(255, 128, 127, 127);
        c = Color.FromArgb(255, 255, 254, 254);
    }




    [DllImport("user32.dll")]
    public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);
    [DllImport("user32.dll")]
    public static extern bool ReleaseCapture();
    [DllImport("dwmapi.dll")]
    public static extern int DwmSetWindowAttribute(IntPtr hwnd, DwmWindowAttribute dwAttribute, ref int pvAttribute, int cbAttribute);
    [Flags]
    public enum DwmWindowAttribute : uint
    {
        DWMWA_USE_IMMERSIVE_DARK_MODE = 20,
        DWMWA_MICA_EFFECT = 1029
    }


    [DllImport("user32.dll")]
    public static extern int SetWindowCompositionAttribute(IntPtr hwnd, ref WindowCompositionAttributeData data);

    [StructLayout(LayoutKind.Sequential)]
    public struct WindowCompositionAttributeData
    {
        public WindowCompositionAttribute Attribute;
        public IntPtr Data;
        public int SizeOfData;
    }

    public enum WindowCompositionAttribute
    {
        WCA_ACCENT_POLICY = 19
    }

    public enum AccentState
    {
        ACCENT_DISABLED = 0,
        ACCENT_ENABLE_GRADIENT = 1,
        ACCENT_ENABLE_TRANSPARENTGRADIENT = 2,
        ACCENT_ENABLE_BLURBEHIND = 3,
        ACCENT_ENABLE_ACRYLICBLURBEHIND = 4
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct AccentPolicy
    {
        public AccentState AccentState;
        public int AccentFlags;
        public int GradientColor;
        public int AnimationId;
    }


    [DllImport("dwmapi.dll")]
    private static extern int DwmSetWindowAttribute(IntPtr hwnd, int attr, ref int attrValue, int attrSize);

    private const int DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1 = 19;
    private const int DWMWA_USE_IMMERSIVE_DARK_MODE = 20;

    public static bool UseDarkMode(this Form form, bool enabled)
    {
        return UseImmersiveDarkMode(form.Handle, enabled);
    }

    private static bool UseImmersiveDarkMode(IntPtr handle, bool enabled)
    {
        if (IsWindows10OrGreater(17763))
        {
            var attribute = DWMWA_USE_IMMERSIVE_DARK_MODE_BEFORE_20H1;
            if (IsWindows10OrGreater(18985))
            {
                attribute = DWMWA_USE_IMMERSIVE_DARK_MODE;
            }

            int useImmersiveDarkMode = enabled ? 1 : 0;
            return DwmSetWindowAttribute(handle, (int)attribute, ref useImmersiveDarkMode, sizeof(int)) == 0;
        }

        return false;
    }

    private static bool IsWindows10OrGreater(int build = -1)
    {
        return Environment.OSVersion.Version.Major >= 10 && Environment.OSVersion.Version.Build >= build;
    }
}

