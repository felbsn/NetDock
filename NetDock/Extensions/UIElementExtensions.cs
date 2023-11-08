using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows;

namespace NetDock.WPF.Extensions;

internal static class UIElementExtensions
{
    public static void Detach(this UIElement child)
    {
        if (child is FrameworkElement frameworkElement)
            frameworkElement.Detach();
    }
    public static void Detach(this FrameworkElement child)
    {
        var parent = child.Parent;
        switch (parent)
        {
            case System.Windows.Controls.Panel panel:
                panel.Children.Remove(child);
                break;
            case Decorator decorator:
                decorator.Child = null;
                break;
            case ContentPresenter contentPresenter:
                contentPresenter.Content = null;
                break;
            case ContentControl contentControl:
                contentControl.Content = null;
                break;
        }
    }
    public static void RemoveChild(this DependencyObject parent, UIElement child)
    {
        var panel = parent as System.Windows.Controls.Panel;
        if (panel != null)
        {
            panel.Children.Remove(child);
            return;
        }

        var decorator = parent as Decorator;
        if (decorator != null)
        {
            if (decorator.Child == child)
            {
                decorator.Child = null;
            }
            return;
        }

        var contentPresenter = parent as ContentPresenter;
        if (contentPresenter != null)
        {
            if (contentPresenter.Content == child)
            {
                contentPresenter.Content = null;
            }
            return;
        }

        var contentControl = parent as ContentControl;
        if (contentControl != null)
        {
            if (contentControl.Content == child)
            {
                contentControl.Content = null;
            }
            return;
        }

        throw new Exception("this thing cant be detected...");
        // maybe more
    }
}
