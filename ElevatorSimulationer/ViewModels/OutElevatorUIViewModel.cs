using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorSimulationer.Misc;
using Microsoft.Extensions.Logging;

namespace ElevatorSimulationer.ViewModels
{
    internal class OutElevatorUIViewModel:ViewModelBase
    {
        private readonly ILogger<OutElevatorUIViewModel> _logger;
        public ObservableCollection<OutElevatorFloorViewModel> OutElevatorFloorModel { get;private set; } = new ObservableCollection<OutElevatorFloorViewModel>();

        public OutElevatorUIViewModel(ILogger<OutElevatorUIViewModel> logger)
        {
            _logger = logger;
            foreach(var item in Enumerable.Range(1,Settings.FloorCount))
            {
                OutElevatorFloorModel.Add(new OutElevatorFloorViewModel
                {
                    Floor = item,
                });
            }
        }
    }
}
