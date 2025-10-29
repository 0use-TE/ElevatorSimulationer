using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Media;

namespace ElevatorSimulationer.Misc
{
    internal static class Settings
    {
        public static int FloorCount { get; set; } = 5;
        public static IBrush ClickDefaultFill { get; set; } = Brushes.OrangeRed;
        public static IBrush DefaultFill { get; set; } = Brushes.White;
    }
}
