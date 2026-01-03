using System;
using System.Collections.Generic;
using System.Diagnostics;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DryIoc.Microsoft.DependencyInjection;
using ElevatorSimulationer.DispatchAlgorithm;
using ElevatorSimulationer.LogModule;
using ElevatorSimulationer.Services;
using ElevatorSimulationer.ViewModels;
using ElevatorSimulationer.Views;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Prism.Container.DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Serilog;

namespace ElevatorSimulationer
{
    public partial class App : PrismApplication
    {
        private ElevatorScheduler? _elevatorScheduler;
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            // Required when overriding Initialize
            base.Initialize();
        }

        protected override AvaloniaObject CreateShell()
        {
            // 如果是桌面端，返回 MainWindow
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime)
            {
                return Container.Resolve<MainWindow>();
            }

            // 如果是 WASM 或移动端，返回 MainView (UserControl)
            return Container.Resolve<MainView>();
        }
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            serviceCollection.AddSingleton<ElevatorUIViewModel>();
            serviceCollection.AddSingleton<OutElevatorUIViewModel>();

            serviceCollection.AddSingleton<MemoryLogService>();

            Container.GetContainer().Populate(serviceCollection);

            //注册DyIOC
            containerRegistry.RegisterDialog<SettingView, SettingViewModel>("SettingView");

            var memorySink = Container.Resolve<MemorySink>();
            //Logger
            // 配置日志
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()                     // 最低日志等级
#if BROWSER
#else
               .WriteTo.File(
                    "Logs/log-.txt",                     // 日志文件路径
                    rollingInterval: RollingInterval.Day, // 每天一个日志文件
                    retainedFileCountLimit: 7           // 保留最近7天
                )
#endif
                .WriteTo.Debug()
                .WriteTo.Sink(memorySink)
                .CreateLogger();
            // Register you Services, Views, Dialogs, etc.
        }
        protected override void OnInitialized()
        {
            //实例化调度类
            base.OnInitialized();
            _elevatorScheduler = Container.Resolve<ElevatorScheduler>();
            var _elevatorUIViewModel = Container.Resolve<ElevatorUIViewModel>();
            var _outElevatorUIViewModel = Container.Resolve<OutElevatorUIViewModel>();
            var _logger = Container.Resolve<ILogger<App>>();
            try
            {
                if (_elevatorScheduler != null)
                {
                    //设置电梯内信息
                    _elevatorScheduler.ElevatorFloorViewModels = _elevatorUIViewModel.ElevatorFloorModel;
                    Debug.WriteLine(_elevatorScheduler.ElevatorFloorViewModels.Count);
                    //设置电梯外信息
                    _elevatorScheduler.OutElevatorFloorViewModels = _outElevatorUIViewModel.OutElevatorFloorModel;
                    Debug.WriteLine(_outElevatorUIViewModel.OutElevatorFloorModel.Count);
                    _logger.LogInformation("调度算法加载成功!");
                }
                else
                    _logger.LogError("调度算法加载失败!");
            }
            catch (Exception ex)
            {
                _logger.LogError("调度算法加载失败!\n" + ex.Message);
            }
        }
    }
}
