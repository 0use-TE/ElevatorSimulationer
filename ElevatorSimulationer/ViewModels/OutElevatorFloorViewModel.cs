using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ElevatorSimulationer.ViewModels
{
    internal class OutElevatorFloorViewModel:ViewModelBase
    {
        private int _floor;
        /// <summary>
        /// 楼层
        /// </summary>
        public int Floor { get => _floor; set => SetProperty(ref _floor, value); }

        private bool _isUpSumbited;
        /// <summary>
        /// 代表被点击了
        /// </summary>
        public bool IsUpSumbited { get => _isUpSumbited; set => SetProperty(ref _isUpSumbited, value); }
        private bool _isDownSumbited;
        /// <summary>
        /// 代表被点击了
        /// </summary>
        public bool IsDownSumbited { get => _isDownSumbited; set => SetProperty(ref _isDownSumbited, value); }
    }
}
