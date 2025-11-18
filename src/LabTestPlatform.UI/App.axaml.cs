using LabTestPlatform.UI.Utilities;
using System;
using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LabTestPlatform.UI.ViewModels;
using LabTestPlatform.UI.Views;
using Microsoft.Extensions.DependencyInjection;

namespace LabTestPlatform.UI;

public partial class App : Application
{
    public static IServiceProvider? ServiceProvider { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
    }

    public override void OnFrameworkInitializationCompleted()
    {
        //添加代码
        // ✅ 添加这行 - 初始化日志系统
        SimpleLogger.Initialize();
        
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            // 配置依赖注入
            ServiceProvider = Program.ConfigureServices();

            desktop.MainWindow = new MainWindow
            {
                DataContext = new MainWindowViewModel(ServiceProvider)
            };
        }

        base.OnFrameworkInitializationCompleted();
    }
}
