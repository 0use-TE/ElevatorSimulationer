using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ElevatorSimulationer.Services;
using Serilog.Core;
using Serilog.Events;

namespace ElevatorSimulationer.LogModule
{
    internal class MemorySink : ILogEventSink
    {
        private readonly MemoryLogService _memoryLogService;
        public MemorySink(MemoryLogService memoryLogService)
        {
            _memoryLogService = memoryLogService;
        }

        public void Emit(LogEvent logEvent)
        {
            //加入日志到内存
            _memoryLogService.Logs[logEvent.Level].Add(logEvent.RenderMessage());
        }
    }
}
