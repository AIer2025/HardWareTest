using LabTestPlatform.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;

namespace LabTestPlatform.UI.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private ViewModelBase _content;

        public ViewModelBase Content
        {
            get => _content;
            set => this.RaiseAndSetIfChanged(ref _content, value);
        }

        public SystemManagementViewModel SystemManagement { get; }
        public DataImportViewModel DataImport { get; }
        public WeibullAnalysisViewModel WeibullAnalysis { get; }
        public ReportExportViewModel ReportExport { get; }

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

            // 设置默认视图
            _content = SystemManagement;
        }

        public void Navigate(string viewName)
        {
            switch (viewName)
            {
                case "SystemManagement":
                    Content = SystemManagement;
                    break;
                case "DataImport":
                    Content = DataImport;
                    break;
                case "WeibullAnalysis":
                    Content = WeibullAnalysis;
                    break;
                case "ReportExport":
                    Content = ReportExport;
                    break;
            }
        }
    }
}