using NetDock.WPF.Enums;
using NetDock.WPF.Helpers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using NetDock.Helpers;
using Brushes = System.Windows.Media.Brushes;
using Orientation = System.Windows.Controls.Orientation;
using Point = System.Windows.Point;
using NetDock.WPF.Components;
using System.Windows.Media;
using Color = System.Windows.Media.Color;


namespace NetDock;

public class DockSurface : Grid
{
    public static int dockIdx = 1;
    public static HashSet<DockSurface> Surfaces = new();

    public DockContext Context { get; set; }
    public LayoutDirection LayoutDirection { get; set; }
    public DockItem Item { get; set; }
    public DockSurface Top { get; set; }
    public DockSurface Bottom { get; set; }
    public DockSurface Left { get; set; }
    public DockSurface Right { get; set; }
    public DockSurface ParentSurface { get; set; }

    public bool IsEmpty => Item == null && Top == null && Left == null;
    public bool HasItem => Item != null;
    public bool IsRoot => this.Parent != null && this.ParentSurface == null;

    StackPanel Tab;
    Border Content;
    RowDefinition tabRowDefinition;

    public bool ShowTab
    {
        get => tabRowDefinition.Height.Value == 0;
        //set { tabRowDefinition.Height = new GridLength(value ? 26 : 0); }
        set { tabRowDefinition.Height = new GridLength(value ? 26 : 0); }
    }
    public int id { get; } = ++dockIdx;

    public event EventHandler<EventArgs> DockedItemsChanged;

    public DockSurface(DockContext context)
    {
        this.Context = context;
        tabRowDefinition = new RowDefinition { Height = new GridLength(0) };
        RowDefinitions.Add(tabRowDefinition);
        RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });

        Tab = new StackPanel()
        {
            Orientation = Orientation.Horizontal,
        };

        Content = new Border
        {
            //BorderThickness = new Thickness(2),
            //BorderBrush = Brushes.DarkGoldenrod,
            Padding = new Thickness(0),
            Margin = new Thickness(0),
            CornerRadius = new CornerRadius(0, 3, 3, 3),
            //ClipToBounds = true
        };

        Grid.SetRow(Tab, 0);
        Grid.SetRow(Content, 1);
        Children.Add(Tab);
        Children.Add(Content);

        Unloaded += (s, e) => { Surfaces.Remove(this); };
        Loaded += (s, e) => { Surfaces.Add(this); };
    }
    public DockSurface() : this(DockContext.Default) { }


    public void From(DockSurface other)
    {
        this.LayoutDirection = other.LayoutDirection;
        this.Top = other.Top;
        this.Bottom = other.Bottom;
        this.Left = other.Left;
        this.Right = other.Right;
        this.Item = other.Item;
        this.Content.Child = other.DetachChild();
        this.ParentSurface = other.ParentSurface;
        other.ParentSurface = null;

        this.Update();

        DockedItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Add(DockItem dockItem, DockDirection dir = DockDirection.Right, double percentage = 50)
    {
        using var disabler = Dispatcher.DisableProcessing();

        if (IsEmpty)
        {
            Item = dockItem;
            Content.Child = dockItem.Content;

            

            Update();
        }
        else if (HasItem)
        {
  

            var currentItem = DetachItem(false);
            var current = new DockSurface(this.Context);
            current.Add(currentItem);

            var next = new DockSurface(this.Context);
            next.Add(dockItem);

            Merge(this, current, next, dir, percentage);
        }
        else
        {
        


            var current = new DockSurface(this.Context);
            current.From(this);

            var next = new DockSurface(this.Context);
            next.Add(dockItem);
            Merge(this, current, next, dir, percentage);
        }

        Update();

        DockedItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Remove()
    {
        Traverse((d) =>
        {
            if (d.HasItem)
                Context.OnDockItemRemoved(d.Item);
        });

        Detach();
        Clear();
    }

    public DockDirection GetCurrentDockDirectionAtParent()
    {
        if (ParentSurface == null)
            return DockDirection.Stack;

        if (ParentSurface.Left == this) return DockDirection.Left;
        if (ParentSurface.Top == this) return DockDirection.Top;
        if (ParentSurface.Right == this) return DockDirection.Right;
        if (ParentSurface.Bottom == this) return DockDirection.Bottom;

        return DockDirection.Stack;
    }

    public double GetDockPortion()
    {
        if (this.IsEmpty)
            return 0;

        if (this.HasItem)
            return 0;

        var ratio = LayoutHelper.GetPortitionPercentage(Content.Child as Grid);

        return ratio;
        //ParentSurface.Content.
    }

    public void Detach()
    {
        if (ParentSurface == null)
            return;

        DockSurface remain = null;

        if (ParentSurface.Top == this)
            remain = ParentSurface.Bottom;
        if (ParentSurface.Bottom == this)
            remain = ParentSurface.Top;
        if (ParentSurface.Left == this)
            remain = ParentSurface.Right;
        if (ParentSurface.Right == this)
            remain = ParentSurface.Left;

        if (remain == null)
            throw new Exception("remain can not be null");

        ParentSurface.Right = remain.Right;
        ParentSurface.Left = remain.Left;
        ParentSurface.Top = remain.Top;
        ParentSurface.Bottom = remain.Bottom;
        ParentSurface.Item = remain.Item;

        ParentSurface.Content.Child = remain.DetachChild();
        ParentSurface.Update();
        ParentSurface = null;

        DockedItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public void Clear()
    {
        Content.Child = null;
        Left = null;
        Right = null;
        Bottom = null;
        Top = null;
        Item = null;
        Update();

        DockedItemsChanged?.Invoke(this, EventArgs.Empty);
    }

    public bool IsHovered(Point p, out DockSurface hovered)
    {
        var rr = this.PointFromScreen(p);

        var isHovered =
            rr.X > 0 && rr.Y > 0 && rr.X < ActualWidth &&
            rr.Y < ActualHeight; //p.X >= rr.Left && p.Y >= rr.Top && p.X <= rr.Right && p.Y <= rr.Bottom;


        hovered = null;

        if (!isHovered)
            return false;

        //var dir = GetHoverDirection(p);
        // isHovered = isHovered && DockDirections.HasFlag(dir);
        // if (Item == null &&  Direction == LayoutDirection.Columns && (dir == DockDirection.Left || dir == DockDirection.Right))
        //     isHovered = false;
        // if (Item == null && Direction == LayoutDirection.Rows && (dir == DockDirection.Top || dir == DockDirection.Bottom))
        //     isHovered = false;

        if (Top?.IsHovered(p, out hovered) == true)
            return true;
        if (Bottom?.IsHovered(p, out hovered) == true)
            return true;
        if (Left?.IsHovered(p, out hovered) == true)
            return true;
        if (Right?.IsHovered(p, out hovered) == true)
            return true;

        hovered = this;
        return true;
    }

    internal DockDirection GetHoverDirection(Point p)
    {
        var o = this.PointFromScreen(p);

        if (IsEmpty)
            return DockDirection.Stack;

        var xSize = Math.Min(ActualWidth * .3, 200);
        var ySize = Math.Min(ActualHeight * .3, 200);

        if (o.X < xSize) return DockDirection.Left;
        if (o.X > ActualWidth - xSize) return DockDirection.Right;
        if (o.Y < ySize) return DockDirection.Top;
        if (o.Y > ActualHeight - ySize) return DockDirection.Bottom;
        return DockDirection.Stack;
    }

    public void Update()
    {
        if (this.Item != null)
        {
            RenderOptions.SetEdgeMode(Content, EdgeMode.Aliased);


            this.Item.Surface = this;
            this.Item.Context = this.Context;
            Tab.Children.Clear();
            var btn = new DockTabButton()
            {
                Title = Item.Title,

            };
            //var ress = Application.Current.Resources;
            //btn.Style = Application.Current.FindResource("DockButtonStyle") as Style;
            Tab.Children.Add(btn);
            ShowTab = true;

            //Content.BorderBrush = Brushes.Goldenrod;
            Content.BorderThickness = new Thickness(2);
            byte num = 221;
            num = 243;
            //Content.BorderBrush = new SolidColorBrush(Color.FromRgb(221, 221, 221));
            Content.BorderBrush = new SolidColorBrush(Color.FromRgb(num, num, num));
            Content.Background = new SolidColorBrush(Color.FromRgb(num, num, num));

            BindTabButtonEvents(btn);


            Context.OnDockStateChanged(this.Item, DockState.Docked);

        }
        else
        {
            RenderOptions.SetEdgeMode(Content, EdgeMode.Unspecified);

            ShowTab = false;
            Content.BorderThickness = new Thickness(0);
            Content.Background = Brushes.Transparent;
        }

        if (Right != null)
            Right.ParentSurface = this;
        if (Left != null)
            Left.ParentSurface = this;
        if (Top != null)
            Top.ParentSurface = this;
        if (Bottom != null)
            Bottom.ParentSurface = this;
    }
    DockItem DetachItem(bool update)
    {
        var item = Item;
        if (item != null)
        {
            this.Content.Child = null;
            this.Item = null;
            if (update)
                Update();
        }

        DockedItemsChanged?.Invoke(this, EventArgs.Empty);

        return item;
    }
    internal static DockSurface Merge(DockSurface target, DockSurface current, DockSurface next, DockDirection dir, double percent)
    {
        var brush = Brushes.Transparent;
        var size = 4;
        UIElement content = null;
        if (dir == DockDirection.Left) content = LayoutHelper.Horizontal(next, current, size, brush, percent);
        if (dir == DockDirection.Right) content = LayoutHelper.Horizontal(current, next, size, brush, percent);
        if (dir == DockDirection.Top) content = LayoutHelper.Vertical(next, current, size, brush, percent);
        if (dir == DockDirection.Bottom) content = LayoutHelper.Vertical(current, next, size, brush, percent);

        target.Top = target.Left = target.Right = target.Bottom = null;


        if (dir == DockDirection.Left)
        {
            target.LayoutDirection = LayoutDirection.Horizotal;
            target.Left = next;
            target.Right = current;
        }

        if (dir == DockDirection.Right)
        {
            target.LayoutDirection = LayoutDirection.Horizotal;
            target.Left = current;
            target.Right = next;
        }

        if (dir == DockDirection.Top)
        {
            target.LayoutDirection = LayoutDirection.Vertical;
            target.Top = next;
            target.Bottom = current;
        }

        if (dir == DockDirection.Bottom)
        {
            target.LayoutDirection = LayoutDirection.Vertical;
            target.Top = current;
            target.Bottom = next;
        }

        next.ParentSurface = target;
        current.ParentSurface = target;

        target.Content.Child = content;

        return target;
    }
    void BindTabButtonEvents(System.Windows.Controls.Control button)
    {
        var begin = new Point();
        var offset = new Point();
        var down = false;
        button.PreviewMouseDown += (s, e) =>
        {
            if (e.MiddleButton == MouseButtonState.Pressed)
            {
                this.Remove();
            }
            if (e.LeftButton == MouseButtonState.Pressed)
            {
                down = true;
                begin = Win32Helper.GetMousePosition();
                offset = button.PointFromScreen(begin);
            }
        };
        button.PreviewMouseDoubleClick += (s, e) =>
        {
            var ds = this.ParentSurface ?? this;
            var dd = this.GetCurrentDockDirectionAtParent();
            var dp = this.ParentSurface?.GetDockPortion() ?? 0;
            var pos = this.PointToScreen(new Point(0, 0));

            e.Handled = true;
            Detach();

            var item = DetachItem(true);
            var win = Context.GetDockWindow(item);// new DockWindow(item)
            win.DockedParent = ds;
            win.DockedDirection = dd;
            win.DockedPercentage = dp;

            win.Left = pos.X;
            win.Top = pos.Y;
            win.Width = ActualWidth;
            win.Height = ActualHeight;
            win.WindowState = WindowState.Normal;
            win.Show();
            win.WindowState = WindowState.Maximized;
            win.Activate();
        };
        button.PreviewMouseUp += (s, e) => { down = false; };
        button.PreviewMouseMove += (s, e) =>
        {
            if (!down)
                return;
            var pos = Win32Helper.GetMousePosition();
            var bounds = button.PointFromScreen(pos);

            if (bounds.X < 0 || bounds.Y < 0 || bounds.X > button.ActualWidth || bounds.Y > button.ActualHeight)
            {
                var ds = this.ParentSurface ?? this;
                var dd = this.GetCurrentDockDirectionAtParent();
                var dp = this.ParentSurface?.GetDockPortion() ?? 0;

                Detach();
                var item = DetachItem(true);
                var win = Context.GetDockWindow(item);

                win.DockedParent = ds;
                win.DockedDirection = dd;
                win.DockedPercentage = dp;

                win.Left = pos.X - offset.X;
                win.Top = pos.Y - offset.Y;
                win.Width = ActualWidth;
                win.Height = ActualHeight;

                win.Show();
                win.Activate();
                win.DragMove();
            }
        };
    }
    UIElement DetachChild()
    {
        var child = this.Content.Child;
        if (child == null)
            return null;

        this.Content.Child = null;
        return child;
    }

    public void Traverse(Action<DockSurface> action)
    {
        this.Left?.Traverse(action);
        this.Right?.Traverse(action);
        this.Top?.Traverse(action);
        this.Bottom?.Traverse(action);
        action(this);
    }

    public override string ToString()
    {
        if (IsEmpty)
        {
            return $"<dock id='{id}' />";
        }
        else if (Item != null)
        {
            return $"<dock id='{id}' item='{Item.Title}'/>";
        }
        else
        {
            var dir = LayoutDirection;
            var ratio = GetDockPortion();

            return $"<dock id='{id}' ratio='{ratio:0}' dir='{dir}'>{Top}{Left}{Right}{Bottom}</dock>";
        }
    }

}