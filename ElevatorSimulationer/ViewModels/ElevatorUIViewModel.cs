using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorSimulationer.Misc;
using Microsoft.Extensions.Logging;
using Prism.Commands;

namespace ElevatorSimulationer.ViewModels
{
    internal class ElevatorUIViewModel:ViewModelBase
    {
        private readonly ILogger<ElevatorUIViewModel> _logger;
        public ObservableCollection<ElevatorFloorViewModel> ElevatorFloorModel { get; set; } = new ObservableCollection<ElevatorFloorViewModel>();

        public ElevatorUIViewModel(ILogger<ElevatorUIViewModel> logger)
        {
            _logger = logger;

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
                    floor.IsActived = true;
            });

            DoubleClickCancelCommand = new DelegateCommand<object>(data =>
            {
                _logger.LogInformation("梯内：取消楼层:" + data);
                var floor = ElevatorFloorModel.FirstOrDefault(x => x.Floor == (int)data);
                if (floor != null)
                    floor.IsActived = false;
            });
        }

        public DelegateCommand<object> FloatClickCommand { get; set; }
        public DelegateCommand<object> DoubleClickCancelCommand { get;set; }
    }
}
