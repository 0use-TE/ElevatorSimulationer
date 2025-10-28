using System;
using System.Timers;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
using Avalonia.Threading;

namespace ElevatorSimulationer.CustomControls
{
    public partial class ElevatorFloorButton : UserControl
    {
        public static readonly StyledProperty<ICommand> ClickCommandProperty =
            AvaloniaProperty.Register<ElevatorFloorButton, ICommand>(nameof(ClickCommand));

        public ICommand ClickCommand
        {
            get => GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand> DoubleClickCancelCommandProperty =
            AvaloniaProperty.Register<ElevatorFloorButton, ICommand>(nameof(DoubleClickCancelCommand));

        public ICommand DoubleClickCancelCommand
        {
            get => GetValue(DoubleClickCancelCommandProperty);
            set => SetValue(DoubleClickCancelCommandProperty, value);
        }

        public static readonly StyledProperty<int> FloorProperty =
            AvaloniaProperty.Register<ElevatorFloorButton, int>(nameof(Floor), 0);

        public int Floor
        {
            get => GetValue(FloorProperty);
            set => SetValue(FloorProperty, value);
        }

        private const double DoubleClickTimeoutMs = 400; 

        public ElevatorFloorButton()
        {
            InitializeComponent();
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {

        }

    }
}
