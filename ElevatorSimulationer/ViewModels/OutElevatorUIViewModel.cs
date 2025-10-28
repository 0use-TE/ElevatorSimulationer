using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace ElevatorSimulationer.ViewModels
{
    internal class OutElevatorUIViewModel:ViewModelBase
    {
        private readonly ILogger<OutElevatorUIViewModel> _logger;
        public OutElevatorUIViewModel(ILogger<OutElevatorUIViewModel> logger)
        {
            _logger = logger;
        }
    }
}
