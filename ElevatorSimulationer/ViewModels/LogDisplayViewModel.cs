using System.Collections.ObjectModel;
using ElevatorSimulationer.Services;
using Serilog.Events;

namespace ElevatorSimulationer.ViewModels
{
    internal class LogDisplayViewModel
    {
        private readonly MemoryLogService _memoryLogService;

        public LogDisplayViewModel(MemoryLogService memoryLogService)
        {
            _memoryLogService = memoryLogService;
            // 直接绑定到 MemoryLogService 的集合，实现共享
            Info = _memoryLogService.Logs[LogEventLevel.Information];
            Debug = _memoryLogService.Logs[LogEventLevel.Debug];
            Warning = _memoryLogService.Logs[LogEventLevel.Warning];
            Error = _memoryLogService.Logs[LogEventLevel.Error];
        }

        public ObservableCollection<string> Info { get; }
        public ObservableCollection<string> Debug { get; }
        public ObservableCollection<string> Warning { get; }
        public ObservableCollection<string> Error { get; }
    }
}
