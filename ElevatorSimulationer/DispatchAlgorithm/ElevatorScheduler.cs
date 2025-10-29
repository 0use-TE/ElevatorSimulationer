using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Extensions.Logging;
using Prism.Events;
using ElevatorSimulationer.Events;
using ElevatorSimulationer.ViewModels;
using System.Diagnostics;

namespace ElevatorSimulationer.DispatchAlgorithm
{
    enum ElevatorDirection
    {
        Node,//代表静止
        Up,
        Down,
    }
    internal class ElevatorScheduler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger<ElevatorScheduler> _logger;

        public IReadOnlyCollection<ElevatorFloorViewModel> ElevatorFloorViewModels { get; set; }
            = new List<ElevatorFloorViewModel>();

        public IReadOnlyCollection<OutElevatorFloorViewModel> OutElevatorFloorViewModels { get; set; }
            = new List<OutElevatorFloorViewModel>();

        private int _currentFloor = 1;
        private ElevatorDirection _currentDirection = ElevatorDirection.Node;

        private readonly SortedSet<int> _upStops = new();
        private readonly SortedSet<int> _downStops = new();

        // 最终调度结果（避免重复发布）
        private int? _nextTargetFloor = null;

        public ElevatorScheduler(IEventAggregator eventAggregator, ILogger<ElevatorScheduler> logger)
        {
            _eventAggregator = eventAggregator;
            _logger = logger;

            _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Subscribe(HandleStateChanged);
            // 可选：订阅电梯到达事件，用于清除按钮
            _eventAggregator.GetEvent<ElevatorArrivedEvent>().Subscribe(OnElevatorArrived);
        }

        private void HandleStateChanged()
        {
            _logger.LogDebug("=== 电梯状态改变，触发调度 ===");
            Scheduler();
        }

        public void Scheduler()
        {
            _upStops.Clear();
            _downStops.Clear();
            _nextTargetFloor = null;

            _logger.LogInformation($"当前楼层: {_currentFloor}，当前方向: {_currentDirection}");

            // 1. 收集请求
            CollectAllRequests();

            // 2. 无方向 + 有请求 → 选最近
            if (_currentDirection == ElevatorDirection.Node && (_upStops.Any() || _downStops.Any()))
            {
                ChooseNearestFloorAsTarget();
            }
            // 3. 有方向 → 同方向优先
            else if (TryGetNextStopInCurrentDirection(out int next))
            {
                SetTarget(next, _currentDirection);
            }
            // 4. 同方向无停靠 → 尝试反方向
            else if (TrySwitchDirectionAndGetNextStop(out next))
            {
                SetTarget(next, _currentDirection);
            }
            // 5. 无任何请求 → 静止
            else
            {
                _currentDirection = ElevatorDirection.Node;
                _nextTargetFloor = _currentFloor;
                _logger.LogInformation("无任何请求，电梯进入静止状态");
            }

            // === 统一发布目标 ===
            PublishFinalTarget();
        }

        #region 收集请求 + 日志
        private void CollectAllRequests()
        {
            var upList = new List<int>();
            var downList = new List<int>();
            foreach(var item in ElevatorFloorViewModels)
            {
                Debug.WriteLine(item.IsActived);
            }
            // 内部按钮
            foreach (var vm in ElevatorFloorViewModels.Where(vm => vm.IsActived))
            {
                int f = vm.Floor;
                if (f > _currentFloor) { _upStops.Add(f); upList.Add(f); }
                else if (f < _currentFloor) { _downStops.Add(f); downList.Add(f); }
                else _logger.LogDebug($"内部按钮 {f} 层已在当前层，待开门");
            }

            // 外部按钮
            foreach (var vm in OutElevatorFloorViewModels)
            {
                int f = vm.Floor;
                if (vm.IsUpActived) { _upStops.Add(f); upList.Add(f); }

                if (vm.IsDownActived) { _downStops.Add(f); downList.Add(f); }
            }

            _logger.LogDebug($"收集完成 → UpStops: [{string.Join(",", upList.OrderBy(x => x))}]");
            _logger.LogDebug($"           → DownStops: [{string.Join(",", downList.OrderByDescending(x => x))}]");
        }
        #endregion

        #region 选择最近目标
        private void ChooseNearestFloorAsTarget()
        {
            int? up = _upStops.Min;
            int? down = _downStops.Max;

            _logger.LogDebug($"静止选最近 → Up候选: {up}, Down候选: {down}");

            if (up.HasValue && down.HasValue)
            {
                int distUp = up.Value - _currentFloor;
                int distDown = _currentFloor - down.Value;
                if (distUp <= distDown)
                {
                    SetTarget(up.Value, ElevatorDirection.Up);
                    _upStops.Remove(up.Value);
                }
                else
                {
                    SetTarget(down.Value, ElevatorDirection.Down);
                    _downStops.Remove(down.Value);
                }
            }
            else if (up.HasValue)
            {
                SetTarget(up.Value, ElevatorDirection.Up);
                _upStops.Remove(up.Value);
            }
            else if (down.HasValue)
            {
                SetTarget(down.Value, ElevatorDirection.Down);
                _downStops.Remove(down.Value);
            }
        }
        #endregion

        #region 同方向下一个
        private bool TryGetNextStopInCurrentDirection(out int nextFloor)
        {
            nextFloor = 0;

            if (_currentDirection == ElevatorDirection.Up && _upStops.Any())
            {
                nextFloor = _upStops.FirstOrDefault(f => f >= _currentFloor);
                if (nextFloor != 0)
                {
                    _upStops.Remove(nextFloor);
                    _logger.LogDebug($"同方向(Up) → 下一个停靠: {nextFloor}");
                    return true;
                }
            }

            if (_currentDirection == ElevatorDirection.Down && _downStops.Any())
            {
                nextFloor = _downStops.LastOrDefault(f => f <= _currentFloor);
                if (nextFloor != 0)
                {
                    _downStops.Remove(nextFloor);
                    _logger.LogDebug($"同方向(Down) → 下一个停靠: {nextFloor}");
                    return true;
                }
            }

            _logger.LogDebug($"同方向无停靠 (当前方向: {_currentDirection})");
            return false;
        }
        #endregion

        #region 切换方向
        private bool TrySwitchDirectionAndGetNextStop(out int nextFloor)
        {
            nextFloor = 0;

            if (_currentDirection == ElevatorDirection.Up && _downStops.Any())
            {
                _currentDirection = ElevatorDirection.Down;
                nextFloor = _downStops.Max;
                _downStops.Remove(nextFloor);
                _logger.LogInformation($"方向切换 → Down，目标: {nextFloor}");
                return true;
            }

            if (_currentDirection == ElevatorDirection.Down && _upStops.Any())
            {
                _currentDirection = ElevatorDirection.Up;
                nextFloor = _upStops.Min;
                _upStops.Remove(nextFloor);
                _logger.LogInformation($"方向切换 → Up，目标: {nextFloor}");
                return true;
            }

            _logger.LogDebug("无反方向请求，无法切换");
            return false;
        }
        #endregion

        #region 统一设置目标
        private void SetTarget(int floor, ElevatorDirection dir)
        {
            _nextTargetFloor = floor;
            _currentDirection = dir;
            _logger.LogDebug($"设置目标 → 楼层: {floor}，方向: {dir}");
        }
        #endregion

        #region 统一发布
        private void PublishFinalTarget()
        {
            int floorToPublish = _nextTargetFloor ?? _currentFloor;
            _eventAggregator.GetEvent<ElevatorTargetFoundEvent>().Publish(floorToPublish);
            _logger.LogInformation($"===== 最终调度目标: {floorToPublish} 楼，方向: {_currentDirection} =====");
        }
        #endregion

        #region 电梯到达后清除按钮（关键！）
        private void OnElevatorArrived(int arrivedFloor)
        {
            _currentFloor = arrivedFloor;
            _logger.LogInformation($"电梯到达 {arrivedFloor} 楼，清除该层按钮");

            ClearFloorButtons(arrivedFloor);

            // 到达后重新调度（可能有新内部目标）
            Scheduler();
        }

        private void ClearFloorButtons(int floor)
        {
            // 清除内部按钮
            var internalVm = ElevatorFloorViewModels.FirstOrDefault(v => v.Floor == floor);
            if (internalVm != null && internalVm.IsActived)
            {
                internalVm.IsActived = false;
                _logger.LogDebug($"清除内部按钮 {floor} 层");
            }

            // 清除外部按钮
            var outVm = OutElevatorFloorViewModels.FirstOrDefault(v => v.Floor == floor);
            if (outVm != null)
            {
                if (outVm.IsUpActived)
                {
                    outVm.IsUpActived = false;
                    _logger.LogDebug($"清除外部上行按钮 {floor} 层");
                }

                if (outVm.IsDownActived)
                {
                    outVm.IsDownActived = false;
                    _logger.LogDebug($"清除外部下行按钮 {floor} 层");
                }
            }
        }
        #endregion
    }
}
