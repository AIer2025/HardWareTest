using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using LabTestPlatform.ViewModels;
using LabTestPlatform.Views;
using LabTestPlatform.Data;
using LabTestPlatform.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using System;
using System.IO;

namespace LabTestPlatform
{
    public partial class App : Application
    {
        public IServiceProvider? ServiceProvider { get; private set; }

        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            // 配置服务
            var services = new ServiceCollection();
            ConfigureServices(services);
            ServiceProvider = services.BuildServiceProvider();

            // 配置日志
            ConfigureLogging();

            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                // 创建主窗口
                var mainWindow = new MainWindow
                {
                    DataContext = ServiceProvider.GetRequiredService<MainWindowViewModel>()
                };

                desktop.MainWindow = mainWindow;
            }

            base.OnFrameworkInitializationCompleted();
        }

        private void ConfigureServices(IServiceCollection services)
        {
            // 配置
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
                .Build();

            services.AddSingleton<IConfiguration>(configuration);

            // 数据访问层
            services.AddSingleton<DatabaseConnection>();
            services.AddSingleton<TestDataRepository>();

            // 业务服务层
            services.AddSingleton<WeibullAnalysisEngine>();
            services.AddSingleton<ExcelImportService>();
            services.AddSingleton<ReportGenerationService>();

            // ViewModels
            services.AddTransient<MainWindowViewModel>();
        }

        private void ConfigureLogging()
        {
            var configuration = ServiceProvider?.GetRequiredService<IConfiguration>();
            
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.Console()
                .WriteTo.File(
                    configuration?["Logging:FilePath"] ?? "Logs/app-.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
                .CreateLogger();

            Log.Information("应用程序启动");
        }
    }
}
