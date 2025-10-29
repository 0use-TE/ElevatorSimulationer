using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace ElevatorSimulationer.CustomControls;

public partial class OutElevatorFloorButton : UserControl
{

    public static readonly StyledProperty<ICommand> LeftClickCommandProperty =
     AvaloniaProperty.Register<ElevatorFloorButton, ICommand>(nameof(LeftClickCommand));

    public ICommand LeftClickCommand
    {
        get => GetValue(LeftClickCommandProperty);
        set => SetValue(LeftClickCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> LeftDoubleClickCancelCommandProperty =
        AvaloniaProperty.Register<ElevatorFloorButton, ICommand>(nameof(LeftDoubleClickCancelCommand));

    public ICommand LeftDoubleClickCancelCommand
    {
        get => GetValue(LeftDoubleClickCancelCommandProperty);
        set => SetValue(LeftDoubleClickCancelCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> RIghtClickCommandProperty =
 AvaloniaProperty.Register<ElevatorFloorButton, ICommand>(nameof(RightClickCommand));

    public ICommand RightClickCommand
    {
        get => GetValue(RIghtClickCommandProperty);
        set => SetValue(RIghtClickCommandProperty, value);
    }

    public static readonly StyledProperty<ICommand> RightDoubleClickCancelCommandProperty =
        AvaloniaProperty.Register<ElevatorFloorButton, ICommand>(nameof(RightDoubleClickCancelCommand));

    public ICommand RightDoubleClickCancelCommand
    {
        get => GetValue(RightDoubleClickCancelCommandProperty);
        set => SetValue(RightDoubleClickCancelCommandProperty, value);
    }

    public static readonly StyledProperty<int> FloorProperty =
    AvaloniaProperty.Register<ElevatorFloorButton, int>(nameof(Floor), 0);

    public int Floor
    {
        get => GetValue(FloorProperty);
        set => SetValue(FloorProperty, value);
    }
    public OutElevatorFloorButton()
    {
        InitializeComponent();
    }
}
