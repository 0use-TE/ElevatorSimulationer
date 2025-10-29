using System;
using System.Diagnostics;
using System.Windows.Input;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Media;
using ElevatorSimulationer.Misc;

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

        public static readonly StyledProperty<IBrush> ButtonFillProperty =
            AvaloniaProperty.Register<ElevatorFloorButton, IBrush>(nameof(ButtonFill),Settings.DefaultFill);

        public IBrush ButtonFill
        {
            get => GetValue(ButtonFillProperty);
            set => SetValue(ButtonFillProperty, value);
        }

        public static readonly StyledProperty<bool> IsActiveProperty =
            AvaloniaProperty.Register<ElevatorFloorButton, bool>(nameof(IsActive));

        public bool IsActive
        {
            get => GetValue(IsActiveProperty);
            set => SetValue(IsActiveProperty, value);
        }

        private const double DoubleClickTimeoutMs = 300;
        private bool _isChecked;
        private DateTime _lastClickTime;

        public ElevatorFloorButton()
        {
            InitializeComponent();
        }

        protected override void OnPropertyChanged(AvaloniaPropertyChangedEventArgs change)
        {
            base.OnPropertyChanged(change);

            // 当外部或内部修改 IsActive 时，同步视觉和内部状态
            if (change.Property == IsActiveProperty)
            {
                _isChecked = change.GetNewValue<bool>(); // 内部同步
                ButtonFill = _isChecked ? Settings.ClickDefaultFill : Settings.DefaultFill;
            }
        }

        protected override void OnPointerPressed(PointerPressedEventArgs e)
        {
            base.OnPointerPressed(e);

            var now = DateTime.Now;
            var interval = (now - _lastClickTime).TotalMilliseconds;
            _lastClickTime = now;

            // 检查是否是双击（400ms 内第二次点击）
            if (_isChecked && interval < DoubleClickTimeoutMs)
            {
                Debug.WriteLine($"触发梯内 {Floor} 楼双击取消事件");
                _isChecked = false;
                IsActive = false; // 反向同步到属性
                DoubleClickCancelCommand?.Execute(Floor);
                return;
            }

            // 首次点击
            if (!_isChecked)
            {
                Debug.WriteLine($"触发梯内 {Floor} 楼点击事件");
                _isChecked = true;
                IsActive = true; // 激活
                ClickCommand?.Execute(Floor);
            }
        }
    }
}
