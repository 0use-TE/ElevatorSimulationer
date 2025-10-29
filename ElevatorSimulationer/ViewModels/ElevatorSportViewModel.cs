using Avalonia.Threading;
using ElevatorSimulationer.Events;
using Microsoft.Extensions.Logging;
using Prism.Events;
using System;

namespace ElevatorSimulationer.ViewModels
{
    public class ElevatorSportViewModel : ViewModelBase
    {
        private const int MinFloor = 1;
        private const int MaxFloor = 5;
        private const double FloorHeight = 70;
        private const double SecPerFloor = 0.8;
        // ElevatorSportViewModel.cs（在原有代码后追加）

        private const double DoorClosedWidth = 40;    // 门关闭宽度
        private const double DoorOpenWidth = 0;   // 门完全打开宽度
        private const double DoorAnimSec = 0.6;  // 开关门动画时长
        private const double AutoCloseSec = 2.0;  // 自动关门等待时间

        private double _leftDoorWidth;
        public double LeftDoorWidth
        {
            get => _leftDoorWidth;
            set => SetProperty(ref _leftDoorWidth, value);
        }

        private double _rightDoorWidth;
        public double RightDoorWidth
        {
            get => _rightDoorWidth;
            set => SetProperty(ref _rightDoorWidth, value);
        }

        private bool _isDoorAnimating;               // 防止重复触发
        private DispatcherTimer? _doorTimer;
        private DateTime _doorStartTime;
        private double _doorStartLeft, _doorStartRight;
        private double _doorTargetLeft, _doorTargetRight;

        private double _elevatorY = 0;
        public double ElevatorY
        {
            get => _elevatorY;
            set => this.SetProperty(ref _elevatorY, value);
        }

        private int _currentFloor = 1;
        public int CurrentFloor
        {
            get => _currentFloor;
            private set => this.SetProperty(ref _currentFloor, value);
        }

        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger<ElevatorSportViewModel> _logger;
        private DispatcherTimer? _timer;
        private double _targetY;
        private double _startY;
        private DateTime _moveStartTime;

        public ElevatorSportViewModel(IEventAggregator eventAggregator, ILogger<ElevatorSportViewModel> logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;

            // 初始位置：1 楼在底部
            ElevatorY = FloorHeight * (MaxFloor - MinFloor);
            CurrentFloor = MinFloor;

            LeftDoorWidth = DoorClosedWidth;
            RightDoorWidth = DoorClosedWidth;

            _eventAggregator.GetEvent<ElevatorTargetFoundEvent>()
                .Subscribe(MoveToFloor);
        }

        private void MoveToFloor(int targetFloor)
        {
            if (targetFloor < MinFloor || targetFloor > MaxFloor) return;
            if (targetFloor == CurrentFloor) return;

            _targetY = FloorHeight * (MaxFloor - targetFloor);
            _startY = ElevatorY;
            _moveStartTime = DateTime.Now;

            double duration = Math.Abs(targetFloor - CurrentFloor) * SecPerFloor;

            _logger.LogInformation("电梯开始移动 → 从 {From} 楼 到 {To} 楼", CurrentFloor, targetFloor);

            _timer?.Stop();
            _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };

            _timer.Tick += (s, e) =>
            {
                double elapsed = (DateTime.Now - _moveStartTime).TotalSeconds;
                double t = Math.Min(elapsed / duration, 1.0);

                // EaseOutQuad
                double eased = 1 - Math.Pow(1 - t, 2);
                ElevatorY = _startY + (_targetY - _startY) * eased;

                if (t >= 1.0)
                {
                    _timer.Stop();
                    CurrentFloor = targetFloor;
                    ElevatorY = _targetY;
                    _logger.LogInformation("电梯到达目标楼层: {Floor}", targetFloor);

                    // === 关键：到达后自动开门流程 ===
                    StartDoorSequence(targetFloor);
                }
            };
            _timer.Start();
        }

        private void StartDoorSequence(int completedFloor)
        {
            if (_isDoorAnimating) return;
            _isDoorAnimating = true;

            OpenDoor(() =>
            {
                // 开门完成 → 2s 后自动关门
                _doorTimer?.Stop();
                _doorTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(AutoCloseSec) };
                _doorTimer.Tick += (s, e) =>
                {
                    _doorTimer.Stop();
                    CloseDoor(() =>
                    {
                        // 关门完成 → 发布事件
                        _eventAggregator.GetEvent<ElevatorFloorCompletedEvent>()
                            .Publish(completedFloor);

                        _isDoorAnimating = false;
                        _logger.LogInformation("楼层 {Floor} 处理完成，状态已重置", completedFloor);
                    });
                };
                _doorTimer.Start();
            });
        }
        private void OpenDoor(Action? onCompleted = null)
        {
            _doorStartLeft = LeftDoorWidth;
            _doorStartRight = RightDoorWidth;
            _doorTargetLeft = DoorOpenWidth;     // 0
            _doorTargetRight = DoorOpenWidth;     // 0
            _doorStartTime = DateTime.Now;

            StartDoorAnimation(DoorAnimSec, onCompleted);
        }

        private void CloseDoor(Action? onCompleted = null)
        {
            _doorStartLeft = LeftDoorWidth;
            _doorStartRight = RightDoorWidth;
            _doorTargetLeft = DoorClosedWidth;   // 40
            _doorTargetRight = DoorClosedWidth;   // 40
            _doorStartTime = DateTime.Now;

            StartDoorAnimation(DoorAnimSec, onCompleted);
        }

        private void StartDoorAnimation(double durationSec, Action? onCompleted)
        {
            _doorTimer?.Stop();
            _doorTimer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(16) };

            _doorTimer.Tick += (s, e) =>
            {
                double elapsed = (DateTime.Now - _doorStartTime).TotalSeconds;
                double t = Math.Min(elapsed / durationSec, 1.0);

                // EaseOutQuad 缓动
                double eased = 1 - Math.Pow(1 - t, 2);

                LeftDoorWidth = _doorStartLeft + (_doorTargetLeft - _doorStartLeft) * eased;
                RightDoorWidth = _doorStartRight + (_doorTargetRight - _doorStartRight) * eased;

                if (t >= 1.0)
                {
                    _doorTimer.Stop();
                    LeftDoorWidth = _doorTargetLeft;
                    RightDoorWidth = _doorTargetRight;
                    onCompleted?.Invoke();
                }
            };

            _doorTimer.Start();
        }
    }
}
