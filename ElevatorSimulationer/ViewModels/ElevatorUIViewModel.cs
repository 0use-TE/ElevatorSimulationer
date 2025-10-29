using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorSimulationer.Events;
using ElevatorSimulationer.Misc;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;

namespace ElevatorSimulationer.ViewModels
{
    internal class ElevatorUIViewModel:ViewModelBase
    {
        private readonly ILogger<ElevatorUIViewModel> _logger;
        private readonly IEventAggregator _eventAggregator;

        private int _currentFloor=1;
        public int CurrentFloor
        {
            get => _currentFloor;
            set => SetProperty(ref _currentFloor, value);
        }
        public ObservableCollection<ElevatorFloorViewModel> ElevatorFloorModel { get;private set; } = new ObservableCollection<ElevatorFloorViewModel>();

        public ElevatorUIViewModel(ILogger<ElevatorUIViewModel> logger,IEventAggregator eventAggregator)
        {
            _logger = logger;
            _eventAggregator = eventAggregator;

            _eventAggregator.GetEvent<ElevatorPassingFloorEvent>().Subscribe(x =>
            {
                CurrentFloor = x;
            });

            foreach (var item in Enumerable.Range(0, Settings.FloorCount))
               {
                ElevatorFloorModel.Add(new ElevatorFloorViewModel
                {
                    Floor = item+1
                });
            }
            //data代表是几楼
            FloatClickCommand = new DelegateCommand<object>(data =>
            {
                _logger.LogInformation("梯内:前往楼层:" + data);
                var floor = ElevatorFloorModel.FirstOrDefault(x => x.Floor == (int)data);
                if (floor != null)
                {
                    floor.IsActived = true;
                    _logger.LogDebug($"发布事件{typeof(ElevatorStateChangedEvent)},通知点击楼层{data}");
                    _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Publish();
                }
            });

            DoubleClickCancelCommand = new DelegateCommand<object>(data =>
            {
                _logger.LogInformation("梯内：取消楼层:" + data);
                var floor = ElevatorFloorModel.FirstOrDefault(x => x.Floor == (int)data);
                if (floor != null)
                {
                    floor.IsActived = false;
                    _logger.LogDebug($"发布事件{typeof(ElevatorStateChangedEvent)},通知取消楼层{data}");
                    _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Publish();
                }
            });
        }

        public DelegateCommand<object> FloatClickCommand { get; set; }
        public DelegateCommand<object> DoubleClickCancelCommand { get;set; }
    }
}
