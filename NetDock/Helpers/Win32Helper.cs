using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;
using Point = System.Windows.Point;

namespace NetDock.Helpers;

internal class Win32Helper
{
    [DllImport("user32.dll")]
    public static extern IntPtr WindowFromPoint(Win32Point Point);

    [DllImport("user32.dll")]
    [return: MarshalAs(UnmanagedType.Bool)]
    internal static extern bool GetCursorPos(ref Win32Point pt);

    [StructLayout(LayoutKind.Sequential)]
    internal struct Win32Point
    {
        public Int32 X;
        public Int32 Y;
    };
    public static Point GetMousePosition()
    {
        var w32Mouse = new Win32Point();
        GetCursorPos(ref w32Mouse);
        return new Point(w32Mouse.X, w32Mouse.Y);
    }


    private static IntPtr _handle;
    public static void SetBounds(Window win, int left, int top, int width, int height)
    {
        _handle = new WindowInteropHelper(win).Handle;

        SetWindowPos(_handle, IntPtr.Zero, left, top, width, height, 0);
    }

    [DllImport("user32")]
    static extern bool SetWindowPos(
        IntPtr hWnd,
        IntPtr hWndInsertAfter,
        int x,
        int y,
        int cx,
        int cy,
        uint uFlags);

}
