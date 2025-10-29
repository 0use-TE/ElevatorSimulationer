using System.Collections.ObjectModel;
using System.Linq;
using ElevatorSimulationer.Events;
using ElevatorSimulationer.Misc;
using ElevatorSimulationer.ViewModels;
using Microsoft.Extensions.Logging;
using Prism.Commands;
using Prism.Events;
namespace ElevatorSimulationer.ViewModels;

internal class OutElevatorUIViewModel : ViewModelBase
{
    private readonly ILogger<OutElevatorUIViewModel> _logger;
    private readonly IEventAggregator _eventAggregator;

    public ObservableCollection<OutElevatorFloorViewModel> OutElevatorFloorModel { get; private set; }
        = new ObservableCollection<OutElevatorFloorViewModel>();

    public DelegateCommand<object> UpClickCommand { get; }
    public DelegateCommand<object> UpDoubleClickCancelCommand { get; }
    public DelegateCommand<object> DownClickCommand { get; }
    public DelegateCommand<object> DownDoubleClickCancelCommand { get; }

    public OutElevatorUIViewModel(ILogger<OutElevatorUIViewModel> logger, IEventAggregator eventAggregator)
    {
        _logger = logger;
        _eventAggregator = eventAggregator;

        foreach (var item in Enumerable.Range(1, Settings.FloorCount))
        {
            OutElevatorFloorModel.Add(new OutElevatorFloorViewModel
            {
                Floor = item,
            });
        }

        // 上行点击
        UpClickCommand = new DelegateCommand<object>(data =>
        {
            var floorNum = (int)data;
            var floor = OutElevatorFloorModel.FirstOrDefault(x => x.Floor == floorNum);
            if (floor != null) floor.IsUpActived = true;
            _logger.LogInformation($"上行 {floorNum} 楼按钮点击");
            _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Publish();
        });

        UpDoubleClickCancelCommand = new DelegateCommand<object>(data =>
        {
            var floorNum = (int)data;
            var floor = OutElevatorFloorModel.FirstOrDefault(x => x.Floor == floorNum);
            if (floor != null) floor.IsUpActived = false;
            _logger.LogInformation($"上行 {floorNum} 楼按钮取消");
            _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Publish();
        });

        DownClickCommand = new DelegateCommand<object>(data =>
        {
            var floorNum = (int)data;
            var floor = OutElevatorFloorModel.FirstOrDefault(x => x.Floor == floorNum);
            if (floor != null) floor.IsDownActived = true;
            _logger.LogInformation($"下行 {floorNum} 楼按钮点击");
            _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Publish();
        });

        DownDoubleClickCancelCommand = new DelegateCommand<object>(data =>
        {
            var floorNum = (int)data;
            var floor = OutElevatorFloorModel.FirstOrDefault(x => x.Floor == floorNum);
            if (floor != null) floor.IsDownActived = false;
            _logger.LogInformation($"下行 {floorNum} 楼按钮取消");
            _eventAggregator.GetEvent<ElevatorStateChangedEvent>().Publish();
        });
    }
}

