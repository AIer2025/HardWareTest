using LabTestPlatform.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Windows.Input;

namespace LabTestPlatform.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _currentPage;

        public ViewModelBase CurrentPage
        {
            get => _currentPage;
            set => this.RaiseAndSetIfChanged(ref _currentPage, value);
        }

        public SystemManagementViewModel SystemManagement { get; }
        public DataImportViewModel DataImport { get; }
        public WeibullAnalysisViewModel WeibullAnalysis { get; }
        public ReportExportViewModel ReportExport { get; }

        // Commands for navigation
        public ICommand ShowSystemManagementCommand { get; }
        public ICommand ShowDataImportCommand { get; }
        public ICommand ShowWeibullAnalysisCommand { get; }
        public ICommand ShowReportExportCommand { get; }

        // 修正：修正了构造函数，为其他VM传递 'services'
        public MainWindowViewModel(IServiceProvider services)
        {
            // 1. 只解析 SystemManagementViewModel 需要的服务
            var systemService = services.GetRequiredService<ISystemService>();

            // 2. 将特定服务传递给我们修改过的 VM
            SystemManagement = new SystemManagementViewModel(systemService);

            // 3. 将原始的 'services' 传递给所有其他 VM (这修复了 CS1503 和 CS1729)
            DataImport = new DataImportViewModel(services);
            WeibullAnalysis = new WeibullAnalysisViewModel(services);
            ReportExport = new ReportExportViewModel(services);

            // Initialize commands
            ShowSystemManagementCommand = ReactiveCommand.Create(() => Navigate("SystemManagement"));
            ShowDataImportCommand = ReactiveCommand.Create(() => Navigate("DataImport"));
            ShowWeibullAnalysisCommand = ReactiveCommand.Create(() => Navigate("WeibullAnalysis"));
            ShowReportExportCommand = ReactiveCommand.Create(() => Navigate("ReportExport"));

            // 设置默认视图
            _currentPage = SystemManagement;
        }

        public void Navigate(string viewName)
        {
            switch (viewName)
            {
                case "SystemManagement":
                    CurrentPage = SystemManagement;
                    break;
                case "DataImport":
                    CurrentPage = DataImport;
                    break;
                case "WeibullAnalysis":
                    CurrentPage = WeibullAnalysis;
                    break;
                case "ReportExport":
                    CurrentPage = ReportExport;
                    break;
            }
        }
    }
}