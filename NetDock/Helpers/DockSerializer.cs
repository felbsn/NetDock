using NetDock.WPF.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml;

namespace NetDock.WPF.Helpers
{
    public static class DockSerializer
    {
        public static string ToXml(this DockSurface surface, int depth = 0)
        {
            var pad = new string('\t', depth);
            if (surface.IsEmpty)
            {
                return $"{pad}<dock id='{surface.id}' />";
            }
            else if (surface.HasItem)
            {
                var serialized = surface.Context.Serialize(surface.Item);
                var encoded = Convert.ToBase64String(serialized);
                return $"{pad}<dock id='{surface.id}' item='{surface.Item?.Title}' >{encoded}</dock>";
            }
            else
            {
                var percent = surface.GetDockPortion(); //LayoutHelper.GetPercentage(this.Content.Controls[0]);

                var first = surface.Left ?? surface.Top;
                var second = surface.Right ?? surface.Bottom;
                return
                    $"{pad}<dock id='{surface.id}' dir='{surface.LayoutDirection}' percent='{percent:.0}' >\n{first.ToXml(depth + 1)}\n{second.ToXml(depth + 1)}\n{pad}</dock>";
            }
        }

        public static void FromXml(this DockSurface surface, string xml)
        {
            try
            {
                surface.Traverse((d) =>
            {
                if (d.HasItem)
                    surface.Context.OnDockItemRemoved(d.Item);
            });
                surface.Clear();



                var doc = new XmlDocument();
                doc.LoadXml(xml);
                var dock = Inner(surface.Context, surface, doc.ChildNodes[0]);


                surface.From(dock);
                _ = dock;
            }
            catch (Exception ex)
            {

                _ = ex;
            }
        }

        static DockSurface Inner(DockContext ctx, DockSurface c, XmlNode node)
        {
            if (node.ChildNodes.Count > 1)
            {
                var dir = Enum.Parse<LayoutDirection>(node.Attributes["dir"].Value);
                var percent = double.Parse(node.Attributes["percent"].Value);
                var dock = dir == LayoutDirection.Vertical ? DockDirection.Bottom : DockDirection.Right;
                var current = Inner(ctx, c, node.ChildNodes[0]);
                var next = Inner(ctx, c, node.ChildNodes[1]);
                var merged = DockSurface.Merge(new DockSurface(c.Context), current, next, dock, percent);
                return merged;
            }
            else if (node.ChildNodes.Count == 1)
            {
                try
                {
                    var text = node.ChildNodes[0].InnerText;
                    var bytes = Convert.FromBase64String(text);
                    var item = ctx.Deserialize(bytes);
                    var surface = new DockSurface(ctx);

                    ctx.OnDockItemRestored(item);

                    surface.Add(item);
                    return surface;
                }
                catch (Exception ex)
                {
                    _ = ex;
                }

                return new DockSurface(ctx);
            }
            else
            {
                return new DockSurface(ctx);
            }
        }

    }
}
