using System.Windows;

namespace NetDock;

public class DockItem
{
    public string Title { get; set; }
    public FrameworkElement Content { get; set; }
    public DockSurface Surface { get; set; }
    public DockWindow Window { get; set; }
    public DockContext Context { get; set; }

    public void Update()
    {
        Surface?.Update();
        Window?.Update();
    }
}
