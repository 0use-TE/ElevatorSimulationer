using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Data.Core;
using Avalonia.Data.Core.Plugins;
using Avalonia.Markup.Xaml;
using DryIoc.Microsoft.DependencyInjection;
using ElevatorSimulationer.ViewModels;
using ElevatorSimulationer.Views;
using Microsoft.Extensions.DependencyInjection;
using Prism.Container.DryIoc;
using Prism.DryIoc;
using Prism.Ioc;
using Serilog;

namespace ElevatorSimulationer
{
    public partial class App : PrismApplication
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);

            // Required when overriding Initialize
            base.Initialize();
        }

        protected override AvaloniaObject CreateShell()
        {
            return Container.Resolve<MainWindow>();
        }

        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            IServiceCollection serviceCollection = new ServiceCollection();

            //Logger
            // 配置日志
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()                     // 最低日志等级
                .WriteTo.File(
                    "Logs/log-.txt",                     // 日志文件路径
                    rollingInterval: RollingInterval.Day, // 每天一个日志文件
                    retainedFileCountLimit: 7           // 保留最近7天
                )
                .WriteTo.Debug()
                .CreateLogger();

            serviceCollection.AddLogging(builder =>
            {
                builder.AddSerilog();
            });

            Container.GetContainer().Populate(serviceCollection);
            // Register you Services, Views, Dialogs, etc.
        }
    }
}
