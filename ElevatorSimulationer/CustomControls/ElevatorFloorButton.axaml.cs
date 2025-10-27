using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Metadata;
using Avalonia.Input;
using Avalonia.Markup.Xaml;
using Avalonia.Media;

namespace ElevatorSimulationer.CustomControls;

public partial class ElevatorFloorButton : UserControl
{
    public ElevatorFloorButton()
    {
        InitializeComponent();
    }
    protected override void OnPointerPressed(PointerPressedEventArgs e)
    {
        FillEllipse.Fill = Brushes.Red;
    }
}
