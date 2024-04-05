using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media.Animation;
using NetDock.Helpers;
using NetDock.WPF.Enums;
using NetDock.WPF.Extensions;
using static System.Windows.Forms.VisualStyles.VisualStyleElement.StartPanel;
using Point = System.Windows.Point;

namespace NetDock;

public class DockWindow : Window
{
    public static bool InTransition { get; set; } = false;

    public DockItem DockItem { get; set; }
    public DockContext Context { get; set; }

    double w = 0;
    double h = 0;
    double x = 0;
    double y = 0;
    bool resized = false;
    bool moved = false;

    internal DockDirection DockedDirection { get; set; }
    internal DockSurface DockedParent { get; set; }
    internal double DockedPercentage { get; set; }

    public DockWindow(DockItem dockItem, DockContext ctx = null)
    {
        DockItem = dockItem;
        this.AddChild(dockItem.Content);

        Context = ctx ?? DockContext.Default;
        dockItem.Window = this;
        dockItem.Surface = null;
        dockItem.Context = ctx ?? dockItem.Context;

        //DockedDirection = dir;
        //DockedParent = parent;
        //Ratio = ratio;

        Icon = null;

        Update();

        IsVisibleChanged += (s, e) =>
        {
            if (Visibility != Visibility.Visible) return;
            x = Left;
            y = Top;
        };
    }

    public void Update()
    {
        if (DockItem != null && DockItem.Title != null)
        {
            this.Title = DockItem.Title;
        }
    }

    protected override void OnSourceInitialized(EventArgs e)
    {
        base.OnSourceInitialized(e);
        HwndSource? source = PresentationSource.FromVisual(this) as HwndSource;
        source?.AddHook(WndProc);
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        const int WM_EXITSIZEMOVE = 0x0232;
        const int WM_NCLBUTTONDBLCLK = 0x00A3;
        if (msg == WM_EXITSIZEMOVE)
        {
            if (moved)
            {
                HandleMovement(Win32Helper.GetMousePosition(), true);
                moved = false;
            }
            else
            {
                HandlePreviewLost();
            }

            w = Width;
            h = Height;
            x = Left;
            y = Top;
        }
        else
        if (msg == WM_NCLBUTTONDBLCLK)
        {
            if (DockItem != null && DockedParent != null)
            {
                if (!DockSurface.Surfaces.Contains(DockedParent))
                {
                    return IntPtr.Zero;
                }

                var pos = DockedParent.PointToScreen(new Point(0, 0));

                var x = pos.X + (DockedDirection == DockDirection.Right ? DockedParent.ActualWidth / 2 : 0);
                var y = pos.Y + (DockedDirection == DockDirection.Bottom ? DockedParent.ActualHeight / 2 : 0);
                var w = (DockedDirection == DockDirection.Right || DockedDirection == DockDirection.Left)
                    ? DockedParent.ActualWidth / 2 : DockedParent.ActualWidth;
                var h = (DockedDirection == DockDirection.Top || DockedDirection == DockDirection.Bottom)
                    ? DockedParent.ActualHeight / 2 : DockedParent.ActualHeight;

                if (this.WindowState == WindowState.Maximized)
                    this.WindowState = WindowState.Normal;

                Transition(x, y, w, h).ContinueWith(_ =>
                {
                    Dispatcher.Invoke(() =>
                    {
                        this.RemoveChild(this.DockItem.Content);
                        DockedParent.Add(this.DockItem, DockedDirection, DockedPercentage);
                        DockItem = null;
                        this.Close();
                    });
                });
            }
        }

        return IntPtr.Zero;
    }

    protected override void OnLocationChanged(EventArgs e)
    {
        base.OnLocationChanged(e);
        resized = w != Width || h != Height;
        w = Width;
        h = Height;
        moved = Math.Abs(x - Left) > 1 || Math.Abs(y - Top) > 1;

        //Title = $"x:{Left} y:{Top} w:{Width} h:{Height} {m} {(resized ? "Resized" : "")} {(moved ? "Moved" : "")}";

        moved = moved && !resized;

        if (moved)
        {
            var m = Win32Helper.GetMousePosition();
            HandleMovement(m, false);
        }

        resized = false;
    }

    protected override void OnClosed(EventArgs e)
    {
        base.OnClosed(e);
        if (DockItem != null)
            DockItem.Context?.OnDockItemRemoved(DockItem);
    }

    public void HandlePreviewLost()
    {
        NetDock.WPF.Forms.DockPreview.Hide();
    }
    public void HandleMovement(Point pos, bool released)
    {
        var roots = DockSurface.Surfaces.Where(s => s.IsRoot && s.IsVisible).ToArray();

        DockSurface hovered = null;
        DockSurface r = null;
        foreach (var root in roots)
            if (root.IsHovered(pos, out hovered))
            {
                r = root;
                break;
            }



        if (hovered == null)
        {
            HandlePreviewLost();
            return;
        }


        var dir = hovered.GetHoverDirection(pos);
        var loc = hovered.PointToScreen(new Point(0, 0));

        if (!hovered.IsEmpty && dir == DockDirection.Stack)
        {
            HandlePreviewLost();
            return;
        }

        if (released)
        {

            var x = loc.X;
            var y = loc.Y;
            var w = hovered.ActualWidth;
            var h = hovered.ActualHeight;

            if (dir == DockDirection.Left)
                w = w / 2;

            if (dir == DockDirection.Right)
            {
                x += w / 2;
                w = w / 2;
            }

            if (dir == DockDirection.Top)
                h = h / 2;

            if (dir == DockDirection.Bottom)
            {
                y = y + h / 2;
                h = h / 2;
            }

            var item = DockItem;
            this.DockItem = null;

            InTransition = true;
            //Transition(x, y, w, h).ContinueWith(_ =>
            //{
               Dispatcher.Invoke(() =>
                {
                    //this.Width = w; this.Height = h;
                    //Left = x;
                    //Top = y;    



                    item.Content.Detach();
                    hovered.Add(item, dir);
                    Close();
                    HandlePreviewLost();
                    InTransition = false;
                });
           //});

            //this.ResizeSmoothAsync(x, y, w, h).ContinueWith(_ =>
            //{
            //    Dispatcher.Invoke(() =>
            //    {
            //        DockItem.Content.Detach();
            //        hovered.Add(DockItem, dir);
            //        Close();
            //        HandlePreviewLost();
            //        //r.HideHover();
            //    });
            //});

            return;
        }

        //r.ShowHover(this, loc.X, loc.Y, hovered.ActualWidth, hovered.ActualHeight, dir);

        if (!InTransition)
        {
            var mp = Win32Helper.GetMousePosition();
            var preview = NetDock.WPF.Forms.DockPreview.Get(false);
            var scale = DPIUtil.ScaleFactor(preview, new System.Drawing.Point((int)mp.X, (int)mp.Y)) / 100;
            ShowPreviewX(this, dir, loc.X, loc.Y, hovered.ActualWidth, hovered.ActualHeight, scale);
        }
    }


    Task Transition(double x, double y, double w, double h)
    {
        if (Double.IsNaN(this.Top) || Double.IsNaN(this.Left))
            return Task.CompletedTask;

        var tsc = new TaskCompletionSource();
        var ease = new CubicEase();

        ease.EasingMode = EasingMode.EaseOut;
        var animDuration = 300;

        //ScaleTransform scale = new ScaleTransform(1.0, 1.0);
        //this.RenderTransformOrigin = new Point(0.0, 0.5);
        //rectLeft.RenderTransform = scale;

        var storyboard = new Storyboard();
        storyboard.Completed += (s, e) =>
        {
            tsc.TrySetResult();
        };
        var growAnimationW = new DoubleAnimation();
        growAnimationW.Duration = TimeSpan.FromMilliseconds(animDuration);
        growAnimationW.To = w;
        growAnimationW.From = this.Width;
        growAnimationW.EasingFunction = ease;
        storyboard.Children.Add(growAnimationW);
        Storyboard.SetTargetProperty(growAnimationW, new PropertyPath(WindowWidthAnimationProperty));
        Storyboard.SetTarget(growAnimationW, this);

        var growAnimationH = new DoubleAnimation();
        growAnimationH.Duration = TimeSpan.FromMilliseconds(animDuration);
        growAnimationH.To = h;
        growAnimationH.From = this.Height;
        growAnimationH.EasingFunction = ease;
        storyboard.Children.Add(growAnimationH);
        Storyboard.SetTargetProperty(growAnimationH, new PropertyPath(WindowHeightAnimationProperty));
        Storyboard.SetTarget(growAnimationH, this);

        var growAnimationT = new DoubleAnimation();
        growAnimationT.Duration = TimeSpan.FromMilliseconds(animDuration);
        growAnimationT.To = y;
        growAnimationT.From = this.Top;
        growAnimationT.EasingFunction = ease;
        storyboard.Children.Add(growAnimationT);
        Storyboard.SetTargetProperty(growAnimationT, new PropertyPath(TopProperty));
        Storyboard.SetTarget(growAnimationT, this);

        var growAnimationL = new DoubleAnimation();
        growAnimationL.Duration = TimeSpan.FromMilliseconds(animDuration);
        growAnimationL.To = x;
        growAnimationL.From = this.Left;
        growAnimationL.EasingFunction = ease;
        storyboard.Children.Add(growAnimationL);
        Storyboard.SetTargetProperty(growAnimationL, new PropertyPath(LeftProperty));
        Storyboard.SetTarget(growAnimationL, this);

        storyboard.Begin();
        tsc.Task.ContinueWith((t) =>
        {
            //todo: fix the crash if no current is given
            Dispatcher.Invoke(() =>
            {
                storyboard.Stop();

            });
        });
        return tsc.Task;
    }


    private static void ShowPreviewX(Window win, DockDirection dir, double x, double y, double w, double h)
    {
        var opacity = 0.7;

        double x0 = 0;
        double y0 = 0;
        double w0 = 0;
        double h0 = 0;

        double x1 = 0;
        double y1 = 0;
        double w1 = 0;
        double h1 = 0;

        var preview = NetDock.WPF.Forms.DockPreview.Get(false);
        var scale = DPIUtil.ScaleFactor(preview, new System.Drawing.Point((int)x0, (int)y0)) / 100;

        w *= scale;
        h *= scale;

        switch (dir)
        {
            case DockDirection.Left:
                x0 = x;
                y0 = y;
                w0 = 0;
                h0 = h;

                x1 = x;
                y1 = y;
                w1 = w / 2;
                h1 = h;
                break;
            case DockDirection.Right:
                x0 = x + w;
                y0 = y;
                w0 = 0;
                h0 = h;

                x1 = x + w / 2;
                y1 = y;
                w1 = w / 2;
                h1 = h;
                break;
            case DockDirection.Top:
                x0 = x;
                y0 = y;
                w0 = w;
                h0 = 0;

                x1 = x;
                y1 = y;
                w1 = w;
                h1 = h / 2;
                break;
            case DockDirection.Bottom:
                x0 = x;
                y0 = y + h;
                w0 = w;
                h0 = 0;

                x1 = x;
                y1 = y + h / 2;
                w1 = w;
                h1 = h / 2;
                break;
            case DockDirection.Stack:
                x0 = x + w / 2;
                y0 = y + h / 2;
                w0 = 0;
                h0 = 0;

                x1 = x;
                y1 = y;
                w1 = w;
                h1 = h;
                break;
        }

        NetDock.WPF.Forms.DockPreview.Show(() =>
        {
            win.Activate();

        }, (int)x0, (int)y0, (int)w0, (int)h0, (int)x1, (int)y1, (int)w1, (int)h1, opacity, false);

    }

    private static void ShowPreviewX(Window win, DockDirection dir, double x, double y, double w, double h, double scale)
    {
        var opacity = 0.7;

        double x0 = 0;
        double y0 = 0;
        double w0 = 0;
        double h0 = 0;

        double x1 = 0;
        double y1 = 0;
        double w1 = 0;
        double h1 = 0;

        w *= scale;
        h *= scale;

        switch (dir)
        {
            case DockDirection.Left:
                x0 = x;
                y0 = y;
                w0 = 0;
                h0 = h;

                x1 = x;
                y1 = y;
                w1 = w / 2;
                h1 = h;
                break;
            case DockDirection.Right:
                x0 = x + w;
                y0 = y;
                w0 = 0;
                h0 = h;

                x1 = x + w / 2;
                y1 = y;
                w1 = w / 2;
                h1 = h;
                break;
            case DockDirection.Top:
                x0 = x;
                y0 = y;
                w0 = w;
                h0 = 0;

                x1 = x;
                y1 = y;
                w1 = w;
                h1 = h / 2;
                break;
            case DockDirection.Bottom:
                x0 = x;
                y0 = y + h;
                w0 = w;
                h0 = 0;

                x1 = x;
                y1 = y + h / 2;
                w1 = w;
                h1 = h / 2;
                break;
            case DockDirection.Stack:
                x0 = x + w / 2;
                y0 = y + h / 2;
                w0 = 0;
                h0 = 0;

                x1 = x;
                y1 = y;
                w1 = w;
                h1 = h;
                break;
        }

        NetDock.WPF.Forms.DockPreview.Show(() =>
        {
            win.Activate();

        }, (int)x0, (int)y0, (int)w0, (int)h0, (int)x1, (int)y1, (int)w1, (int)h1, opacity, false);

    }



    [StructLayout(LayoutKind.Sequential)]
    public struct RECT
    {
        public int X;
        public int Y;
        public int Width;
        public int Height;
    }

    public enum SpecialWindowHandles
    {
        HWND_TOP = 0,
        HWND_BOTTOM = 1,
        HWND_TOPMOST = -1,
        HWND_NOTOPMOST = -2
    }

    [DllImport("user32.dll", SetLastError = true)]
    static extern bool GetWindowRect(IntPtr hWnd, ref RECT Rect);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int X, int Y, int cx, int cy, uint uFlags);

    public static readonly DependencyProperty WindowHeightAnimationProperty = DependencyProperty.Register(nameof(WindowHeightAnimation), typeof(double),
                                                                                                typeof(DockWindow), new PropertyMetadata(OnWindowHeightAnimationChanged));
    private static void OnWindowHeightAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = d as Window;

        if (window != null)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            var rect = new RECT();
            if (GetWindowRect(handle, ref rect))
            {
                rect.X = (int)window.Left;
                rect.Y = (int)window.Top;

                rect.Width = (int)window.ActualWidth;
                rect.Height = (int)(double)e.NewValue;  // double casting from object to double to int

                SetWindowPos(handle, new IntPtr((int)SpecialWindowHandles.HWND_TOP), rect.X, rect.Y, rect.Width, rect.Height, (uint)SWP.SHOWWINDOW);
            }
        }
    }

    public double WindowHeightAnimation
    {
        get { return (double)GetValue(WindowHeightAnimationProperty); }
        set { SetValue(WindowHeightAnimationProperty, value); }
    }

    public static readonly DependencyProperty WindowWidthAnimationProperty = DependencyProperty.Register(nameof(WindowWidthAnimation), typeof(double),
                                                                                                typeof(DockWindow), new PropertyMetadata(OnWindowWidthAnimationChanged));

    private static void OnWindowWidthAnimationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        var window = d as Window;

        if (window != null)
        {
            IntPtr handle = new WindowInteropHelper(window).Handle;
            var rect = new RECT();
            if (GetWindowRect(handle, ref rect))
            {
                rect.X = (int)window.Left;
                rect.Y = (int)window.Top;
                var width = (int)(double)e.NewValue;
                rect.Width = width;
                rect.Height = (int)window.ActualHeight;

                SetWindowPos(handle, new IntPtr((int)SpecialWindowHandles.HWND_TOP), rect.X, rect.Y, rect.Width, rect.Height, (uint)SWP.SHOWWINDOW);
            }
        }
    }

    public double WindowWidthAnimation
    {
        get { return (double)GetValue(WindowWidthAnimationProperty); }
        set { SetValue(WindowWidthAnimationProperty, value); }
    }

    /// <summary>
    /// SetWindowPos Flags
    /// </summary>
    public static class SWP
    {
        public static readonly int
        NOSIZE = 0x0001,
        NOMOVE = 0x0002,
        NOZORDER = 0x0004,
        NOREDRAW = 0x0008,
        NOACTIVATE = 0x0010,
        DRAWFRAME = 0x0020,
        FRAMECHANGED = 0x0020,
        SHOWWINDOW = 0x0040,
        HIDEWINDOW = 0x0080,
        NOCOPYBITS = 0x0100,
        NOOWNERZORDER = 0x0200,
        NOREPOSITION = 0x0200,
        NOSENDCHANGING = 0x0400,
        DEFERERASE = 0x2000,
        ASYNCWINDOWPOS = 0x4000;
    }
}