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

        private bool _isUpActived;
        /// <summary>
        /// 代表被点击了
        /// </summary>
        public bool IsUpActived { get => _isUpActived; set => SetProperty(ref _isUpActived, value); }
        private bool _isDownActived;
        /// <summary>
        /// 代表被点击了
        /// </summary>
        public bool IsDownActived { get => _isDownActived; set => SetProperty(ref _isDownActived, value); }
    }
}
