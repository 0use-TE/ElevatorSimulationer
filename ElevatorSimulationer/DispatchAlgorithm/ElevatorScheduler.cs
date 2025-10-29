using System;
using System.Collections.Generic;
using System.Linq;
using ElevatorSimulationer.Events;
using ElevatorSimulationer.ViewModels;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace ElevatorSimulationer.DispatchAlgorithm
{
    /// <summary>
    /// 电梯运行方向
    /// </summary>
    internal enum ElevatorDirection
    {
        Node, // 静止
        Up,
        Down,
    }

    /// <summary>
    /// 电梯调度器（SCAN 算法实现）
    /// </summary>
    internal class ElevatorScheduler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger<ElevatorScheduler> _logger;

        // ---------- 配置 ----------
        private const int MinFloor = 1;   // 最低楼层（可自行修改）
        private const int MaxFloor = 20;  // 最高楼层（可自行修改）
        // -------------------------

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
            _eventAggregator.GetEvent<ElevatorFloorCompletedEvent>().Subscribe(OnElevatorArrived);

            _eventAggregator.GetEvent<ElevatorPassingFloorEvent>().Subscribe(x =>
            {
                _currentFloor = x;
            });
        }

        #region 入口：状态改变时触发调度

        private void HandleStateChanged()
        {
            _logger.LogDebug("=== 电梯状态改变，触发调度 ===");
            Scheduler();
        }

        /// <summary>
        /// 主调度入口
        /// </summary>
        public void Scheduler()
        {
            _upStops.Clear();
            _downStops.Clear();
            _nextTargetFloor = null;

            _logger.LogInformation($"当前楼层: {_currentFloor}，当前方向: {_currentDirection}");

            // 1. 收集所有请求
            CollectAllRequests();

            bool hasAnyRequest = _upStops.Any() || _downStops.Any();

            // 2. 静止 + 有请求 → 选最近
            if (_currentDirection == ElevatorDirection.Node && hasAnyRequest)
            {
                ChooseNearestFloorAsTarget();
            }
            // 3. 有方向 + 同方向有停靠 → 继续
            else if (TryGetNextStopInCurrentDirection(out int next))
            {
                SetTarget(next, _currentDirection);
            }
            // 4. 同方向无停靠 → 尝试反方向
            else if (TrySwitchDirectionAndGetNextStop(out next))
            {
                SetTarget(next, _currentDirection);
            }
            // 5. 完全无请求 → 选择“最近楼层”作为临时目标
            else
            {
                ChooseNearestFloorAsEmergencyStop();
            }

            // 统一发布目标
            PublishFinalTarget();
        }

        #endregion

        #region 1. 收集请求（内部 + 外部按钮）
        private void CollectAllRequests()
        {
            var upList = new List<int>();
            var downList = new List<int>();

            // ---- 内部按钮（无方向，按目标楼层）----
            foreach (var vm in ElevatorFloorViewModels.Where(vm => vm.IsActived))
            {
                int f = vm.Floor;
                if (!IsValidFloor(f)) continue;

                if (f > _currentFloor) { _upStops.Add(f); upList.Add(f); }
                else if (f < _currentFloor) { _downStops.Add(f); downList.Add(f); }
                else _logger.LogDebug($"内部按钮 {f} 层已在当前层，待开门");
            }

            // ---- 外部按钮（关键：按按钮方向分类！）----
            foreach (var vm in OutElevatorFloorViewModels)
            {
                int f = vm.Floor;
                if (!IsValidFloor(f)) continue;

                // 上行按钮 → 只能加入上行队列（无论楼层高低）
                if (vm.IsUpActived)
                {
                    _upStops.Add(f);
                    upList.Add(f);
                    _logger.LogDebug($"外部上行按钮 → {f} 楼加入上行队列");
                }

                // 下行按钮 → 只能加入下行队列（无论楼层高低）
                if (vm.IsDownActived)
                {
                    _downStops.Add(f);
                    downList.Add(f);
                    _logger.LogDebug($"外部下行按钮 → {f} 楼加入下行队列");
                }

                // 当前楼层按钮 → 直接开门，不加入队列
                if (f == _currentFloor && (vm.IsUpActived || vm.IsDownActived))
                {
                    _logger.LogDebug($"外部按钮 {f} 层在当前层，待开门");
                }
            }

            _logger.LogDebug($"收集完成:");
            _logger.LogDebug($"→ UpStops: [{string.Join(",", upList.OrderBy(x => x))}]");
            _logger.LogDebug($"→ DownStops: [{string.Join(",", downList.OrderByDescending(x => x))}]");
        }
        #endregion

        #region 2. 静止时选择最近楼层

        private void ChooseNearestFloorAsTarget()
        {
            int? up = _upStops.Any() ? _upStops.Min : (int?)null;
            int? down = _downStops.Any() ? _downStops.Max : (int?)null;

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

        #region 3. 同方向下一个停靠点

        private bool TryGetNextStopInCurrentDirection(out int nextFloor)
        {
            nextFloor = 0;

            if (_currentDirection == ElevatorDirection.Up && _upStops.Any())
            {
                // 取 >= 当前楼层 的最小值
                nextFloor = _upStops.FirstOrDefault(f => f >= _currentFloor);
                if (nextFloor != 0 && IsValidFloor(nextFloor))
                {
                    _upStops.Remove(nextFloor);
                    _logger.LogDebug($"同方向(Up) → 下一个停靠: {nextFloor}");
                    return true;
                }
            }

            if (_currentDirection == ElevatorDirection.Down && _downStops.Any())
            {
                // 取 <= 当前楼层 的最大值
                nextFloor = _downStops.LastOrDefault(f => f <= _currentFloor);
                if (nextFloor != 0 && IsValidFloor(nextFloor))
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

        #region 4. 切换方向并取反方向最近点

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

        #region 统一发布目标楼层

        private void PublishFinalTarget()
        {
            int floorToPublish = _nextTargetFloor ?? _currentFloor;
            _eventAggregator.GetEvent<ElevatorTargetFoundEvent>().Publish(floorToPublish);
            _logger.LogInformation($"===== 最终调度目标: {floorToPublish} 楼，方向: {_currentDirection} =====");
        }

        #endregion

        #region 电梯到达后处理（清除按钮 + 重新调度）

        private void OnElevatorArrived(int arrivedFloor)
        {
            if (!IsValidFloor(arrivedFloor))
            {
                _logger.LogWarning($"到达楼层 {arrivedFloor} 非法，已忽略");
                return;
            }

            _currentFloor = arrivedFloor;
            _logger.LogInformation($"电梯到达 {arrivedFloor} 楼，清除该层按钮");

            ClearFloorButtons(arrivedFloor);

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

        #region 辅助：楼层合法性校验

        private bool IsValidFloor(int floor)
        {
            if (floor < MinFloor || floor > MaxFloor)
            {
                _logger.LogWarning($"楼层 {floor} 超出合法范围 [{MinFloor}-{MaxFloor}]，已忽略");
                return false;
            }

            return true;
        }

        #endregion

        /// <summary>
        /// 无任何请求时：选择最近的楼层作为紧急停靠点（安全停靠）
        /// </summary>
        private void ChooseNearestFloorAsEmergencyStop()
        {
            var allFloors = Enumerable.Range(MinFloor, MaxFloor - MinFloor + 1);

            // 找出距离当前楼层最近的楼层
            int nearestFloor = allFloors
                .OrderBy(f => Math.Abs(f - _currentFloor))
                .First();

            // 确定方向（如果不在当前楼层）
            ElevatorDirection targetDir = nearestFloor > _currentFloor ? ElevatorDirection.Up :
                                          nearestFloor < _currentFloor ? ElevatorDirection.Down :
                                          ElevatorDirection.Node;

            _currentDirection = targetDir;
            _nextTargetFloor = nearestFloor;

            _logger.LogInformation($"【紧急停靠】无请求，电梯将停靠最近楼层: {nearestFloor} 楼");
        }
    }
}
