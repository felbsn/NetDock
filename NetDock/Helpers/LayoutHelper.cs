using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Media3D;
using Brush = System.Windows.Media.Brush;
using HorizontalAlignment = System.Windows.HorizontalAlignment;

namespace NetDock.WPF.Helpers;

public static class LayoutHelper
{

    public static double GetPortitionPercentage(Grid grid)
    {
        double a;
        double b;
        if (grid.RowDefinitions.Count == 3)
        {
            a = grid.RowDefinitions[0].Height.Value;
            b = grid.RowDefinitions[2].Height.Value;
        }
        else
        {
            a = grid.ColumnDefinitions[0].Width.Value;
            b = grid.ColumnDefinitions[2].Width.Value;
        }

        if (a == 0)
            return 0;
        if (b == 0)
            return 0;

        return (a / (a+b)) * 100;
    }


    public static UIElement Vertical(DockSurface top, DockSurface bottom, double SplitterSize, Brush SplitBrush, double ratio = 50)
    {
        var grid = new Grid();

        var row0 = new RowDefinition();
        row0.Height = new GridLength(ratio / 100, GridUnitType.Star);

        var row1 = new RowDefinition();
        row1.Height = new GridLength(SplitterSize, GridUnitType.Pixel); ;

        var row2 = new RowDefinition();
        row2.Height = new GridLength((100 - ratio) / 100, GridUnitType.Star);

        grid.RowDefinitions.Add(row0);
        grid.RowDefinitions.Add(row1);
        grid.RowDefinitions.Add(row2);

        var gridSplitter = new GridSplitter();
        gridSplitter.Height = SplitterSize;
        gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
        gridSplitter.Background = SplitBrush;

        Grid.SetRow(top, 0);
        grid.Children.Add(top);

        Grid.SetRow(gridSplitter, 1);
        grid.Children.Add(gridSplitter);

        Grid.SetRow(bottom, 2);
        grid.Children.Add(bottom);

        return grid;
    }

    public static UIElement Horizontal(DockSurface left, DockSurface right, double SplitterSize, Brush SplitBrush, double ratio = 50)
    {
        var grid = new Grid();

        // grid.Background = Brushes.Lime;

        var col0 = new ColumnDefinition();
        col0.Width = new GridLength(ratio / 100, GridUnitType.Star);

        var col1 = new ColumnDefinition();
        col1.Width = new GridLength(SplitterSize, GridUnitType.Pixel); ;

        var col2 = new ColumnDefinition();
        col2.Width = new GridLength((100 - ratio) / 100, GridUnitType.Star);


        grid.ColumnDefinitions.Add(col0);
        grid.ColumnDefinitions.Add(col1);
        grid.ColumnDefinitions.Add(col2);

        var gridSplitter = new GridSplitter();
        gridSplitter.Width = SplitterSize;
        gridSplitter.HorizontalAlignment = HorizontalAlignment.Stretch;
        gridSplitter.Background = SplitBrush;


        Grid.SetColumn(left, 0);
        grid.Children.Add(left);

        Grid.SetColumn(gridSplitter, 1);
        grid.Children.Add(gridSplitter);

        Grid.SetColumn(right, 2);
        grid.Children.Add(right);

        return grid;
    }


}

