using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Markup.Xaml;
using Avalonia.Media;
using ElevatorSimulationer.Misc;

namespace ElevatorSimulationer.Views
{
    public partial class ElevatorSportView : UserControl
    {
        private const double FloorHeight = 70;

        public ElevatorSportView()
        {
            InitializeComponent();
        }
        private void Canvas_Loaded(object? sender, Avalonia.Interactivity.RoutedEventArgs e)
        {
            var canvas = this.FindControl<Canvas>("FloorLinesCanvas");
            if (canvas == null) return;

            canvas.Children.Clear();

            for (int floor = 0; floor <= Settings.FloorCount; floor++)
            {
                double y = FloorHeight * (Settings.FloorCount- floor);
                var line = new Line
                {
                    StartPoint = new Point(0, y),
                    EndPoint = new Point(100, y),
                    Stroke = Brushes.Gray,
                    StrokeThickness = 2
                };
                var text = new TextBlock
                {
                    Text = floor.ToString(),
                };
                Canvas.SetLeft(text, 100);
                Canvas.SetTop(text, 400 - 70 * floor);
                canvas.Children.Add(line);
                canvas.Children.Add(text);

            }
        }
    }
}
