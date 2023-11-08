using NetDock.WPF.Demo;
using System.Runtime.InteropServices;
using System.Windows;


public class Program
{
    [STAThread]
    public static void Main()
    {
        var app = new System.Windows.Application();


        app.Startup += (s, e) =>
        {
            if (Environment.OSVersion.Version >= new Version(6, 3, 0)) // win 8.1 added support for per monitor dpi
            {
                if (Environment.OSVersion.Version >= new Version(10, 0, 15063)) // win 10 creators update added support for per monitor v2
                {
                    NativeMethods.SetProcessDpiAwarenessContext((int)NativeMethods.DPI_AWARENESS_CONTEXT.DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2);
                }
                else NativeMethods.SetProcessDpiAwareness(NativeMethods.PROCESS_DPI_AWARENESS.Process_Per_Monitor_DPI_Aware);
            }
            else NativeMethods.SetProcessDPIAware();


            new MainWindow().Show();
        };

        app.Run();

    }


}


//Thread.CurrentThread.SetApartmentState(ApartmentState.Unknown);
//Thread.CurrentThread.SetApartmentState(ApartmentState.STA);











internal static class NativeMethods
{
    [DllImport("user32.dll", SetLastError = true)]
    internal static extern bool SetProcessDpiAwarenessContext(int dpiFlag);

    [DllImport("SHCore.dll", SetLastError = true)]
    internal static extern bool SetProcessDpiAwareness(PROCESS_DPI_AWARENESS awareness);

    [DllImport("user32.dll")]
    internal static extern bool SetProcessDPIAware();

    internal enum PROCESS_DPI_AWARENESS
    {
        Process_DPI_Unaware = 0,
        Process_System_DPI_Aware = 1,
        Process_Per_Monitor_DPI_Aware = 2
    }

    internal enum DPI_AWARENESS_CONTEXT
    {
        DPI_AWARENESS_CONTEXT_UNAWARE = 16,
        DPI_AWARENESS_CONTEXT_SYSTEM_AWARE = 17,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE = 18,
        DPI_AWARENESS_CONTEXT_PER_MONITOR_AWARE_V2 = 34
    }
}