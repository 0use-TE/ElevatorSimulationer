using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Serilog.Events;

namespace ElevatorSimulationer.Services
{
    internal class MemoryLogService
    {
        public MemoryLogService()
        {
            foreach (LogEventLevel level in Enum.GetValues(typeof(LogEventLevel)))
            {
                Logs[level] = new ObservableCollection<string>();
            }
        }
        public Dictionary<LogEventLevel, ObservableCollection<string>> Logs { get; set; } = new Dictionary<LogEventLevel, ObservableCollection<string>>();
        public void AddLog(LogEventLevel logEventLevel,string msg)
        {
            Logs[logEventLevel].Add(msg);
        }
    }
}
