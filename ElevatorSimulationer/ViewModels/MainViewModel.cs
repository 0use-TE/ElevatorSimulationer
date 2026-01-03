using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Prism.Commands;
using Prism.Dialogs;

namespace ElevatorSimulationer.ViewModels
{
    internal class MainViewModel : ViewModelBase
    {
        public DelegateCommand OpenSetting { get; set; }
        private readonly IDialogService _dialogService;

        public MainViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            Title = "Elevator Eimulationer";
            OpenSetting = new DelegateCommand(() =>
            {
                dialogService.ShowDialog("SettingView");

            });
        }
    }
}
