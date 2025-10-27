using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSimulationer.ViewModels
{
    internal class ElevatorUIViewModel:ViewModelBase
    {
        public ObservableCollection<ElevatorFloorViewModel> ElevatorFloorModel { get; set; } = new ObservableCollection<ElevatorFloorViewModel>();
        public ElevatorUIViewModel()
        {
            foreach (var item in Enumerable.Range(0, 5))
               {
                ElevatorFloorModel.Add(new ElevatorFloorViewModel
                {
                    Floor = item
                });
            }

        }

    }
}
