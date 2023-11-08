using Microsoft.Web.WebView2.Wpf;
using NetDock.Demo;
using NetDock.WPF.Helpers;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace NetDock.WPF.Demo
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        int count = 0;
        private void Button_Click(object sender, RoutedEventArgs e)
        {
            var c = count++;
            ds.Add(new DockItem()
            {
                Title = "test -> " + c,
                Content = new TestContent(),

                //new Label()
                //{
                //    Content = "x:" + c,
                //    FontSize = 44,
                //    HorizontalAlignment = HorizontalAlignment.Stretch,
                //    VerticalAlignment = VerticalAlignment.Stretch,
                //    Background = Brushes.Lime,
                //}
            });
        }

        void add_win(object sender, RoutedEventArgs e)
        {
            var win = new DockWindow(new DockItem() { Title = "test", Content = new Label() { Content = "dockable..." }, Context = new DockContext() });

            win.Left = 200;
            win.Top = 200;
            win.Width = 600;
            win.Height = 300;
            win.Show();
        }

        private void add_bottom(object sender, RoutedEventArgs e)
        {
            var c = count++;
            // ds.Add(new DockItem()
            // {
            //     Name = "i:"+c,
            //     Content = new Label()
            //     {
            //         Content = "x:" + c,
            //         FontSize = 44,
            //         HorizontalAlignment = HorizontalAlignment.Center,
            //         VerticalAlignment = VerticalAlignment.Center
            //     }
            // },DockDirection.Bottom);
            ds.Add(new DockItem()
            {
                Title = "b:" + c,
                Content = new WebView2()
                {
                    Source = new Uri("http://randomcolour.com/")
                }
            }, Enums.DockDirection.Bottom);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            int x = Random.Shared.Next(20, 1500);
            int y = Random.Shared.Next(20, 600);
            int w = Random.Shared.Next(100, 800);
            int h = Random.Shared.Next(100, 800);

            this.ResizeSmoothAsync(x, y, w, h);
        }




        // Can execute
        private void CommandBinding_CanExecute(object sender, CanExecuteRoutedEventArgs e)
        {
            e.CanExecute = true;
        }

        // Minimize
        private void CommandBinding_Executed_Minimize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MinimizeWindow(this);
        }

        // Maximize
        private void CommandBinding_Executed_Maximize(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.MaximizeWindow(this);
        }

        // Restore
        private void CommandBinding_Executed_Restore(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.RestoreWindow(this);
        }

        // Close
        private void CommandBinding_Executed_Close(object sender, ExecutedRoutedEventArgs e)
        {
            SystemCommands.CloseWindow(this);
        }


        public void dock_test(object sender, EventArgs e)
        {
            var str = ds.ToString();
            MessageBox.Show(str);
        }
        string xml = "";
        public void dock_save(object sender, EventArgs e)
        {
            xml = ds.ToXml();

            MessageBox.Show(xml);

        }
        public void dock_load(object sender, EventArgs e)
        {
            ds.FromXml(xml);
        }
    }


}


public static class WpfExtensions
{
    public static Dictionary<Window, (int id, double x, double y, double width, double height, double opacity, Task t, int duration)> tasks = new();

    public static Task ResizeSmoothAsync(this Window form, double x, double y, double width, double height, double opacity = 1.0, int duration = 200, bool suspend = true)
    {
        if (tasks.TryGetValue(form, out var cur))
        {
            if (cur.x == x && cur.y == y && cur.width == width && cur.height == height && opacity == cur.opacity)
                return cur.t;

            tasks[form] = (cur.id + 1, x, y, width, height, opacity, cur.t, duration);
            return cur.t;
        }
        if (!form.IsInitialized)
            return Task.CompletedTask;

        var p = 0.0;

        var baseW = form.Width;
        var baseH = form.Height;
        var baseX = form.Left;
        var baseY = form.Top;
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

        Console.WriteLine($"resize task x:{x} y:{y} w:{width} h:{height} op:{opacity}");
        var sw = Stopwatch.StartNew();

        //if (suspend)
        //    form.SuspendLayout();

        Task.Run(() =>
        {
            while (p < 1.0)
            {
                if (tasks[form].id != id)
                {
                    Console.WriteLine("animation changed with id " + id);
                    var c = tasks[form];
                    p = 0.0;
                    baseW = form.Width;
                    baseH = form.Height;
                    baseX = form.Left;
                    baseY = form.Top;
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

                var dw = (baseW + px * diff_width);
                var dh = (baseH + px * diff_height);
                var dx = (baseX + px * diff_x);
                var dy = (baseY + px * diff_y);
                var d_o = (baseOp + px * diff_op);

                form.Dispatcher.Invoke(() =>
                {
                    // form.set
                    // form.SetBounds(dx, dy, dw, dh);
                    form.Left = dx;
                    form.Top = dy;
                    form.Width = dw;
                    form.Height = dh;
                    form.Opacity = d_o;
                });
            }

            form.Dispatcher.Invoke(() =>
            {
                Console.WriteLine($"finished task loc:{form.Left},{form.Top} width:{form.Width} height:{form.Height}");

                //if (suspend)
                //    form.ResumeLayout();
            });

            tcs.SetResult();
            tasks.Remove(form);
        });
        return tcs.Task;
    }
    public static void HideSmooth(this Window form, int duration = 200)
    {
        if (form.Visibility != Visibility.Visible)
            return;

        var t = tasks.TryGetValue(form, out var res)
            ? form.ResizeSmoothAsync(res.x, res.y, res.width, res.height, 0)
            : form.ResizeSmoothAsync(form.Left, form.Top, form.Width, form.Height, 0);
        t.ContinueWith(_ =>
        {
            form.Dispatcher.Invoke(() =>
            {
                Console.WriteLine("make it hidden -> " + form.Opacity);
                if (form.Opacity == 0)
                    form.Hide();
            });
        });









    }



}