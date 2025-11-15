using System;
using Avalonia;
using Avalonia.ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using LabTestPlatform.Core.Services;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Repositories;
using LabTestPlatform.Analysis;

namespace LabTestPlatform.UI;

class Program
{
    // Initialization code. Don't use any Avalonia, third-party APIs or any
    // SynchronizationContext-reliant code before AppMain is called: things aren't initialized
    // yet and stuff might break.
    [STAThread]
    public static void Main(string[] args) => BuildAvaloniaApp()
        .StartWithClassicDesktopLifetime(args);

    // Avalonia configuration, don't remove; also used by visual designer.
    public static AppBuilder BuildAvaloniaApp()
        => AppBuilder.Configure<App>()
            .UsePlatformDetect()
            .WithInterFont()
            .LogToTrace()
            .UseReactiveUI();

    public static IServiceProvider ConfigureServices()
    {
        var services = new ServiceCollection();

        // Configuration
        var configuration = new ConfigurationBuilder()
            .SetBasePath(AppDomain.CurrentDomain.BaseDirectory)
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        services.AddSingleton<IConfiguration>(configuration);

        // Database
        var connectionString = configuration.GetConnectionString("DefaultConnection") 
            ?? throw new InvalidOperationException("Connection string not found");
        services.AddSingleton<IDbConnectionFactory>(sp => new DbConnectionFactory(connectionString));

        // Repositories
        services.AddScoped<ISystemRepository, SystemRepository>();
        services.AddScoped<IPlatformRepository, PlatformRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<ITestDataRepository, TestDataRepository>();
        services.AddScoped<IWeibullAnalysisRepository, WeibullAnalysisRepository>();

        // Services
        services.AddScoped<ISystemService, SystemService>();
        services.AddScoped<IDataImportService, DataImportService>();
        services.AddScoped<IWeibullAnalysisService, WeibullAnalysisService>();
        services.AddScoped<IReportService, ReportService>();

        // Analysis Engine
        services.AddSingleton<IWeibullEngine, WeibullEngine>();

        return services.BuildServiceProvider();
    }
}
