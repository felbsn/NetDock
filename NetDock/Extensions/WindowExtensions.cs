using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Threading;
using NetDock.Helpers;

namespace NetDock.WPF.Extensions;

public static class WindowExtensions
{
    public static
        Dictionary<Window, (int id, double x, double y, double width, double height, double opacity, Task t, int
            duration)> tasks = new();

    public static Task ResizeSmoothAsync(this Window form, double xBegin, double yBegin, double wBegin, double hBegin,
        double x, double y, double width, double height,
        double opacity = 1.0, int duration = 200)
    {
        //Win32Helper.SetBounds(form, (int)xBegin, (int)yBegin, (int)wBegin, (int)hBegin);
        form.Width = wBegin;
        form.Height = hBegin;
        form.Left = xBegin;
        form.Top = yBegin;

        return form.ResizeSmoothAsync(x, y, width, height, opacity, duration);
    }

    public static Task ResizeSmoothAsync(this Window form, double x, double y, double width, double height,
        double opacity = 1.0, int duration = 200)
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

        if ((int)diff_height == 0 && (int)diff_width == 0 && (int)diff_x == 0 && (int)diff_y == 0 && diff_op == 0)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        int id = 0;
        tasks[form] = (id, x, y, width, height, opacity, tcs.Task, duration);

        var sw = Stopwatch.StartNew();

        int loop = 0;
        var t = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(1),
        };
        t.Tick += (s, e) =>
        {
            if (p < 1.0)
            {
                loop++;
                // await Task.Delay(10);

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

                var dw = (int)(baseW + px * diff_width);
                var dh = (int)(baseH + px * diff_height);
                var dx = (int)(baseX + px * diff_x);
                var dy = (int)(baseY + px * diff_y);
                var d_o = (baseOp + px * diff_op);

                // if (ldy != dy || ldx != dx || ldw != dw || ldh != dh)
                //Win32Helper.SetBounds(form, (int)dx, (int)dy, (int)dw, (int)dh);
                form.Width = dw;
                form.Height = dh;
                form.Top = dy;
                form.Left = dx;
                form.Opacity = d_o;
            }
            else
            {
                Console.WriteLine($"finised with {loop} count");
                t.Stop();
                tcs.SetResult();
                tasks.Remove(form);
            }
        };
        t.Start();

        return tcs.Task;
    }


    public static Task ResizeSmoothAsyncY(this Window form, double x, double y, double width, double height,
        double opacity = 1.0, int duration = 200)
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

        if ((int)diff_height == 0 && (int)diff_width == 0 && (int)diff_x == 0 && (int)diff_y == 0 && diff_op == 0)
        {
            return Task.CompletedTask;
        }

        var tcs = new TaskCompletionSource();
        int id = 0;
        tasks[form] = (id, x, y, width, height, opacity, tcs.Task, duration);

        var sw = Stopwatch.StartNew();

        int loop = 0;
        var t = new DispatcherTimer
        {
            Interval = TimeSpan.FromMilliseconds(5)
        };
        t.Tick += (s, e) =>
        {
            if (p < 1.0)
            {
                loop++;
                // await Task.Delay(10);

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

                var dw = (int)(baseW + px * diff_width);
                var dh = (int)(baseH + px * diff_height);
                var dx = (int)(baseX + px * diff_x);
                var dy = (int)(baseY + px * diff_y);
                var d_o = (baseOp + px * diff_op);

                // if (ldy != dy || ldx != dx || ldw != dw || ldh != dh)
                Win32Helper.SetBounds(form, (int)dx, (int)dy, (int)dw, (int)dh);
                // form.Width = dw;
                // form.Height = dh;
                // form.Top = dy;
                // form.Left = dx;
                form.Opacity = d_o;
            }
            else
            {
                Console.WriteLine($"finised with {loop} count");
                t.Stop();
                tcs.SetResult();
                tasks.Remove(form);
            }
        };
        t.Start();

        return tcs.Task;
    }


    public static Task ResizeSmoothAsyncX2(this Window form, double x, double y, double width, double height,
        double opacity = 1.0, int duration = 200)
    {
        if (tasks.TryGetValue(form, out var cur))
        {
            if (cur.x == x && cur.y == y && cur.width == width && cur.height == height && opacity == cur.opacity)
                return cur.t;

            Console.WriteLine("reparameters");
            tasks[form] = (cur.id + 1, x, y, width, height, opacity, cur.t, duration);
            return cur.t;
        }

        if (!form.IsInitialized)
            return Task.CompletedTask;

        var p = 0.0;

        if (double.IsNaN(form.Width))
            return Task.CompletedTask;

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


        if ((int)diff_height == 0 && (int)diff_width == 0 && (int)diff_x == 0 && (int)diff_y == 0 && diff_op == 0)
        {
            return Task.CompletedTask;
        }


        var tcs = new TaskCompletionSource();
        int id = 0;
        tasks[form] = (id, x, y, width, height, opacity, tcs.Task, duration);

        Console.WriteLine($"resize task x:{x} y:{y} w:{width} h:{height} op:{opacity}");
        var sw = Stopwatch.StartNew();

        Task.Run(async () =>
        {
            form.Dispatcher.Invoke(() =>
            {
                int loop = 0;
                while (p < 1.0)
                {
                    loop++;
                    // await Task.Delay(10);

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

                    var dw = (int)(baseW + px * diff_width);
                    var dh = (int)(baseH + px * diff_height);
                    var dx = (int)(baseX + px * diff_x);
                    var dy = (int)(baseY + px * diff_y);
                    var d_o = (baseOp + px * diff_op);


                    // form.set
                    // form.SetBounds(dx, dy, dw, dh);
                    // Win32Helper.SetBounds(form, (int)dx,(int)dy,(int)dw,(int)dh);
                    //
                    form.Left = dx;
                    form.Top = dy;
                    form.Width = dw;
                    form.Height = dh;
                    form.Opacity = d_o;
                }
                Console.WriteLine($"finished task loop:{loop}");
                tcs.SetResult();
                tasks.Remove(form);
            });
        });
        return tcs.Task;
    }


    public static Task ResizeSmoothAsyncOrg(this Window form, double x, double y, double width, double height,
        double opacity = 1.0, int duration = 200)
    {
        if (tasks.TryGetValue(form, out var cur))
        {
            if (cur.x == x && cur.y == y && cur.width == width && cur.height == height && opacity == cur.opacity)
                return cur.t;

            Console.WriteLine("reparameters");
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


        if ((int)diff_height == 0 && (int)diff_width == 0 && (int)diff_x == 0 && (int)diff_y == 0 && diff_op == 0)
        {
            return Task.CompletedTask;
        }


        var tcs = new TaskCompletionSource();
        int id = 0;
        tasks[form] = (id, x, y, width, height, opacity, tcs.Task, duration);

        Console.WriteLine($"resize task x:{x} y:{y} w:{width} h:{height} op:{opacity}");
        var sw = Stopwatch.StartNew();

        var ldw = 0;
        var ldh = 0;
        var ldx = 0;
        var ldy = 0;

        Task.Run(async () =>
        {
            int loop = 0;
            while (p < 1.0)
            {
                loop++;
                // await Task.Delay(10);

                if (tasks[form].id != id)
                {
                    Console.WriteLine("animation changed with id " + id);
                    var c = tasks[form];
                    p = 0.0;

                    form.Dispatcher.Invoke(() =>
                    {
                        baseW = form.Width;
                        baseH = form.Height;
                        baseX = form.Left;
                        baseY = form.Top;
                        baseOp = form.Opacity;
                    });


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

                if (ldy != dy || ldx != dx || ldw != dw || ldh != dh)
                    form.Dispatcher.Invoke(() =>
                    {
                        // form.set
                        // form.SetBounds(dx, dy, dw, dh);
                        // Win32Helper.SetBounds(form, (int)dx,(int)dy,(int)dw,(int)dh);
                        //
                        form.Left = dx;
                        form.Top = dy;
                        form.Width = dw;
                        form.Height = dh;
                        form.Opacity = d_o;

                        ldx = dx;
                        ldy = dy;
                        ldw = dw;
                        ldh = dh;
                    });
                else
                    await Task.Delay(10);
            }

            form.Dispatcher.Invoke(() =>
            {
                Console.WriteLine(
                    $"finished task loop:{loop} loc:{form.Left},{form.Top} width:{form.Width} height:{form.Height}");
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
            ? form.ResizeSmoothAsync(res.x, res.y, res.width, res.height, 0, duration)
            : form.ResizeSmoothAsync(form.Left, form.Top, form.Width, form.Height, 0, duration);
        t.ContinueWith(_ =>
        {
            form.Dispatcher.Invoke(() =>
            {
                if (form.Opacity == 0)
                    form.Hide();
            });
        });
    }

    public static void DoEvents()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
            new DispatcherOperationCallback(
                delegate (object f)
                {
                    ((DispatcherFrame)f).Continue = false;
                    return null;
                }), frame);
        Dispatcher.PushFrame(frame);
    }
}