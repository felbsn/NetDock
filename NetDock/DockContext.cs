using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace NetDock;

public enum DockState
{
    None,
    Docked,
    Windowed,
}

public class DockStateChangedEvent : EventArgs
{
    public DockItem Item { get; init; }
    public DockState DockState { get; init; }
}



public class DockContext
{
    public static DockContext Default { get; } = new();

    public event EventHandler<DockStateChangedEvent> DockStateChanged;
    public event EventHandler<DockWindow> DockWindowOpened;
    public event EventHandler<DockItem> DockItemRemoved;
    public event EventHandler<DockItem> DockItemRestored;

    public void OnDockItemRestored(DockItem item)
    {
        DockItemRestored?.Invoke(this, item);
    }
    public void OnDockItemRemoved(DockItem item)
    {
        DockItemRemoved?.Invoke(this, item);
    }
    public void OnDockStateChanged(DockItem item, DockState dockState)
    {
        DockStateChanged?.Invoke(this, new DockStateChangedEvent
        {
            DockState = dockState,
            Item = item,
        });
    }

    public void OnDockWindowOpened(DockWindow dockWindow)
    {
        DockWindowOpened?.Invoke(this, dockWindow);
    }

    public virtual DockWindow GetDockWindow(DockItem item) => new DockWindow(item, this);
    public virtual byte[] Serialize(DockItem item)
    {
        var serialized = JsonSerializer.SerializeToUtf8Bytes(new DataContract
        {
            Title = item.Title,
            Class = item.Content.GetType().AssemblyQualifiedName,
        });
        return serialized;
    }
    public virtual NetDock.DockItem Deserialize(byte[] bytes)
    {
        var contract = JsonSerializer.Deserialize<DataContract>(bytes);

        var type = Type.GetType(contract.Class);
        var control = Activator.CreateInstance(type) as FrameworkElement;

        return new NetDock.DockItem()
        {
            Content = control,
            Title = contract.Title,
        };
    }

    class DataContract
    {
        public string Title { get; set; }
        public string Class { get; set; }
    }

}


