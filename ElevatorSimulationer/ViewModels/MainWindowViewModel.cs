using Prism.Commands;
using Prism.Dialogs;

namespace ElevatorSimulationer.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private readonly IDialogService _dialogService;
        public MainWindowViewModel(IDialogService dialogService)
        {
            _dialogService = dialogService;
            Title = "Elevator Eimulationer";
            OpenSetting=new DelegateCommand(()=>{
                dialogService.ShowDialog("SettingView");
                
            });
        }
        public DelegateCommand OpenSetting { get; set; }

    }
}
