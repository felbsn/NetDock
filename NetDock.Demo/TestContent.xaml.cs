using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace NetDock.Demo
{
    /// <summary>
    /// Interaction logic for TestContent.xaml
    /// </summary>
    public partial class TestContent : UserControl
    {
        public TestContent()
        {
            InitializeComponent();

            var c = Color.FromRgb((byte)Random.Shared.Next(1, 255),
                   (byte)Random.Shared.Next(1, 255), (byte)Random.Shared.Next(1, 233));

            var c1 = Color.FromRgb((byte)Random.Shared.Next(1, 255),
                   (byte)Random.Shared.Next(1, 255), (byte)Random.Shared.Next(1, 233));

            var myLinearGradientBrush =
           new LinearGradientBrush();
            myLinearGradientBrush.StartPoint = new Point(0, 0);
            myLinearGradientBrush.EndPoint = new Point(1, 1);
            myLinearGradientBrush.GradientStops.Add(
                new GradientStop(c, 0.0));
            myLinearGradientBrush.GradientStops.Add(
            //    new GradientStop(Colors.Red, 0.25));
            //myLinearGradientBrush.GradientStops.Add(
            //    new GradientStop(Colors.Blue, 0.75));
            //myLinearGradientBrush.GradientStops.Add(
                new GradientStop(c1, 1.0));

            grid.Background = myLinearGradientBrush;
        }
    }
}
