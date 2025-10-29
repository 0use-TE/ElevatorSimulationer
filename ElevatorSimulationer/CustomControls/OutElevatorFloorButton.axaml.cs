using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Shapes;
using Avalonia.Input;
using Avalonia.Media;
using ElevatorSimulationer.Misc;

namespace ElevatorSimulationer.CustomControls
{
    public partial class OutElevatorFloorButton : UserControl
    {
        private const double DoubleClickTimeoutMs = 300;

        private bool _upChecked;
        private bool _downChecked;

        private DateTime _lastUpClickTime;
        private DateTime _lastDownClickTime;

        public static readonly StyledProperty<int> FloorProperty =
            AvaloniaProperty.Register<OutElevatorFloorButton, int>(nameof(Floor), 0);

        public int Floor
        {
            get => GetValue(FloorProperty);
            set => SetValue(FloorProperty, value);
        }

        public static readonly StyledProperty<ICommand> UpClickCommandProperty =
            AvaloniaProperty.Register<OutElevatorFloorButton, ICommand>(nameof(UpClickCommand));

        public ICommand UpClickCommand
        {
            get => GetValue(UpClickCommandProperty);
            set => SetValue(UpClickCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand> UpDoubleClickCancelCommandProperty =
            AvaloniaProperty.Register<OutElevatorFloorButton, ICommand>(nameof(UpDoubleClickCancelCommand));

        public ICommand UpDoubleClickCancelCommand
        {
            get => GetValue(UpDoubleClickCancelCommandProperty);
            set => SetValue(UpDoubleClickCancelCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand> DownClickCommandProperty =
            AvaloniaProperty.Register<OutElevatorFloorButton, ICommand>(nameof(DownClickCommand));

        public ICommand DownClickCommand
        {
            get => GetValue(DownClickCommandProperty);
            set => SetValue(DownClickCommandProperty, value);
        }

        public static readonly StyledProperty<ICommand> DownDoubleClickCancelCommandProperty =
            AvaloniaProperty.Register<OutElevatorFloorButton, ICommand>(nameof(DownDoubleClickCancelCommand));

        public ICommand DownDoubleClickCancelCommand
        {
            get => GetValue(DownDoubleClickCancelCommandProperty);
            set => SetValue(DownDoubleClickCancelCommandProperty, value);
        }

        public static readonly StyledProperty<bool> UpActiveProperty =
            AvaloniaProperty.Register<OutElevatorFloorButton, bool>(nameof(UpActive));

        public bool UpActive
        {
            get => GetValue(UpActiveProperty);
            set => SetValue(UpActiveProperty, value);
        }

        public static readonly StyledProperty<bool> DownActiveProperty =
            AvaloniaProperty.Register<OutElevatorFloorButton, bool>(nameof(DownActive));

        public bool DownActive
        {
            get => GetValue(DownActiveProperty);
            set => SetValue(DownActiveProperty, value);
        }

        public OutElevatorFloorButton()
        {
            InitializeComponent();

            UpEllipse.PointerPressed += OnUpPressed;
            DownEllipse.PointerPressed += OnDownPressed;
        }
        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            // 上行按钮状态同步
            if (change.Property == UpActiveProperty)
            {
                _upChecked = change.GetNewValue<bool>();
                // 这里可以改颜色，比如 Fill
                var brush = _upChecked ? Settings.ClickDefaultFill : Settings.DefaultFill;
                (UpEllipse.Children[0] as Ellipse)!.Fill = brush;
            }

            // 下行按钮状态同步
            if (change.Property == DownActiveProperty)
            {
                _downChecked = change.GetNewValue<bool>();
                var brush = _downChecked ? Settings.ClickDefaultFill : Settings.DefaultFill;
                (DownEllipse.Children[0] as Ellipse)!.Fill = brush;
            }
        }

        private void OnUpPressed(object? sender, PointerPressedEventArgs e)
        {
            var now = DateTime.Now;
            var interval = (now - _lastUpClickTime).TotalMilliseconds;
            _lastUpClickTime = now;

            if (_upChecked && interval < DoubleClickTimeoutMs)
            {
                Debug.WriteLine($"触发 {Floor} 楼上行双击取消事件");
                _upChecked = false;
                UpActive = false;
                UpDoubleClickCancelCommand?.Execute(Floor);
                return;
            }

            if (!_upChecked)
            {
                Debug.WriteLine($"触发 {Floor} 楼上行点击事件");
                _upChecked = true;
                UpActive = true;
                UpClickCommand?.Execute(Floor);
            }
        }

        private void OnDownPressed(object? sender, PointerPressedEventArgs e)
        {
            var now = DateTime.Now;
            var interval = (now - _lastDownClickTime).TotalMilliseconds;
            _lastDownClickTime = now;

            if (_downChecked && interval < DoubleClickTimeoutMs)
            {
                Debug.WriteLine($"触发 {Floor} 楼下行双击取消事件");
                _downChecked = false;
                DownActive = false;
                DownDoubleClickCancelCommand?.Execute(Floor);
                return;
            }

            if (!_downChecked)
            {
                Debug.WriteLine($"触发 {Floor} 楼下行点击事件");
                _downChecked = true;
                DownActive = true;
                DownClickCommand?.Execute(Floor);
            }
        }
    }
}
