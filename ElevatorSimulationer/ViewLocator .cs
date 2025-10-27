using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using ElevatorSimulationer.ViewModels;
using StaticViewLocator;
namespace ElevatorSimulationer
{
    [StaticViewLocator]
    public partial class ViewLocator : IDataTemplate
    {
        public Control? Build(object? data)
        {
            if (data is null)
            {
                return null;
            }

            var type = data.GetType();

            if (s_views.TryGetValue(type, out var func))
            {
                return func.Invoke();
            }

            throw new Exception($"Unable to create view for type: {type}");
        }

        public bool Match(object? data)
        {
            return data is ViewModelBase;
        }
    }
}
