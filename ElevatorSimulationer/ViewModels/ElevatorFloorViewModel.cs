using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSimulationer.ViewModels
{
    internal class ElevatorFloorViewModel:ViewModelBase
    {
        private int _floor;
        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor { get => _floor; set => SetProperty(ref _floor, value); }

        private bool _isSumbited;
        /// <summary>
        /// 代表被点击了
        /// </summary>
        public bool IsSumbited { get=>_isSumbited; set=>SetProperty(ref _isSumbited,value); }
    }
}
