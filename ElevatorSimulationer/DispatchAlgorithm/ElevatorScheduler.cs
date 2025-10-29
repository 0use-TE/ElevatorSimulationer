using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;
using ElevatorSimulationer.Events;
using ElevatorSimulationer.ViewModels;
using Microsoft.Extensions.Logging;
using Prism.Events;

namespace ElevatorSimulationer.DispatchAlgorithm
{
    internal class ElevatorScheduler
    {
        private readonly IEventAggregator _eventAggregator;
        private readonly ILogger<ElevatorScheduler> _logger;
        public IReadOnlyCollection<ElevatorFloorViewModel> ElevatorFloorViewModels { get; set; } = new List<ElevatorFloorViewModel>();
        public IReadOnlyCollection<OutElevatorFloorViewModel> OutElevatorFloorViewModels { get; set; } = new List<OutElevatorFloorViewModel>();
        public ElevatorScheduler(IEventAggregator eventAggregator,ILogger<ElevatorScheduler> logger)
        {
            _eventAggregator= eventAggregator;
            _logger= logger;
            _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Subscribe(Scheduler);
        }
        public void Scheduler()
        {
            _logger.LogDebug($"处理电梯调度");
            //开始计算

        }
    }
}
