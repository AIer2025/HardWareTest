using LabTestPlatform.Core.Models;
using LabTestPlatform.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using ReactiveUI;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using Avalonia.Controls;
using Avalonia.Platform.Storage;
using System.Threading.Tasks;
using System.Reactive.Linq;
using System.Diagnostics;

namespace LabTestPlatform.UI.ViewModels
{
    // Helper class for export history items
    public class ExportHistoryItem
    {
        public string FileName { get; set; } = string.Empty;
        public DateTime ExportTime { get; set; }
        public string FilePath { get; set; } = string.Empty;
    }

    public class ReportExportViewModel : ViewModelBase
    {
        private readonly IReportService _reportService;
        private readonly ISystemService _systemService;

        // 系统、平台、模组数据
        private ObservableCollection<SystemModel> _systems;
        public ObservableCollection<SystemModel> Systems
        {
            get => _systems;
            set => this.RaiseAndSetIfChanged(ref _systems, value);
        }

        private SystemModel? _selectedSystem;
        public SystemModel? SelectedSystem
        {
            get => _selectedSystem;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedSystem, value);
                this.RaisePropertyChanged(nameof(HasSelectedSystem));
            }
        }

        private ObservableCollection<PlatformModel> _platforms;
        public ObservableCollection<PlatformModel> Platforms
        {
            get => _platforms;
            set => this.RaiseAndSetIfChanged(ref _platforms, value);
        }

        private PlatformModel? _selectedPlatform;
        public PlatformModel? SelectedPlatform
        {
            get => _selectedPlatform;
            set
            {
                this.RaiseAndSetIfChanged(ref _selectedPlatform, value);
                this.RaisePropertyChanged(nameof(HasSelectedPlatform));
            }
        }

        private ObservableCollection<ModuleModel> _modules;
        public ObservableCollection<ModuleModel> Modules
        {
            get => _modules;
            set => this.RaiseAndSetIfChanged(ref _modules, value);
        }

        private ModuleModel? _selectedModule;
        public ModuleModel? SelectedModule
        {
            get => _selectedModule;
            set => this.RaiseAndSetIfChanged(ref _selectedModule, value);
        }

        // 报表类型
        private bool _isWeibullReport = true;
        public bool IsWeibullReport
        {
            get => _isWeibullReport;
            set => this.RaiseAndSetIfChanged(ref _isWeibullReport, value);
        }

        private bool _isSystemReport;
        public bool IsSystemReport
        {
            get => _isSystemReport;
            set => this.RaiseAndSetIfChanged(ref _isSystemReport, value);
        }

        private bool _isDataSummaryReport;
        public bool IsDataSummaryReport
        {
            get => _isDataSummaryReport;
            set => this.RaiseAndSetIfChanged(ref _isDataSummaryReport, value);
        }

        private bool _isCustomReport;
        public bool IsCustomReport
        {
            get => _isCustomReport;
            set => this.RaiseAndSetIfChanged(ref _isCustomReport, value);
        }

        // 数据范围
        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set => this.RaiseAndSetIfChanged(ref _startDate, value);
        }

        private DateTime? _endDate;
        public DateTime? EndDate
        {
            get => _endDate;
            set => this.RaiseAndSetIfChanged(ref _endDate, value);
        }

        // 报表选项
        private bool _includeDetailData = true;
        public bool IncludeDetailData
        {
            get => _includeDetailData;
            set => this.RaiseAndSetIfChanged(ref _includeDetailData, value);
        }

        private bool _includeCharts = true;
        public bool IncludeCharts
        {
            get => _includeCharts;
            set => this.RaiseAndSetIfChanged(ref _includeCharts, value);
        }

        private bool _includeStatistics = true;
        public bool IncludeStatistics
        {
            get => _includeStatistics;
            set => this.RaiseAndSetIfChanged(ref _includeStatistics, value);
        }

        private bool _includeConfidenceInterval = true;
        public bool IncludeConfidenceInterval
        {
            get => _includeConfidenceInterval;
            set => this.RaiseAndSetIfChanged(ref _includeConfidenceInterval, value);
        }

        private bool _addCoverPage = true;
        public bool AddCoverPage
        {
            get => _addCoverPage;
            set => this.RaiseAndSetIfChanged(ref _addCoverPage, value);
        }

        private bool _addTableOfContents = true;
        public bool AddTableOfContents
        {
            get => _addTableOfContents;
            set => this.RaiseAndSetIfChanged(ref _addTableOfContents, value);
        }

        // 输出格式
        private bool _isExcelFormat = true;
        public bool IsExcelFormat
        {
            get => _isExcelFormat;
            set => this.RaiseAndSetIfChanged(ref _isExcelFormat, value);
        }

        private bool _isPdfFormat;
        public bool IsPdfFormat
        {
            get => _isPdfFormat;
            set => this.RaiseAndSetIfChanged(ref _isPdfFormat, value);
        }

        private bool _isWordFormat;
        public bool IsWordFormat
        {
            get => _isWordFormat;
            set => this.RaiseAndSetIfChanged(ref _isWordFormat, value);
        }

        // 保存路径
        private string _savePath = string.Empty;
        public string SavePath
        {
            get => _savePath;
            set => this.RaiseAndSetIfChanged(ref _savePath, value);
        }

        private string _fileName = string.Empty;
        public string FileName
        {
            get => _fileName;
            set => this.RaiseAndSetIfChanged(ref _fileName, value);
        }

        // 导出进度
        private bool _isExporting;
        public bool IsExporting
        {
            get => _isExporting;
            set => this.RaiseAndSetIfChanged(ref _isExporting, value);
        }

        private double _exportProgress;
        public double ExportProgress
        {
            get => _exportProgress;
            set => this.RaiseAndSetIfChanged(ref _exportProgress, value);
        }

        private string _progressMessage = string.Empty;
        public string ProgressMessage
        {
            get => _progressMessage;
            set => this.RaiseAndSetIfChanged(ref _progressMessage, value);
        }

        // 导出历史
        private ObservableCollection<ExportHistoryItem> _exportHistory;
        public ObservableCollection<ExportHistoryItem> ExportHistory
        {
            get => _exportHistory;
            set => this.RaiseAndSetIfChanged(ref _exportHistory, value);
        }

        // 计算属性
        public bool HasSelectedSystem => SelectedSystem != null;
        public bool HasSelectedPlatform => SelectedPlatform != null;
        public bool CanPreview => !string.IsNullOrEmpty(FileName);
        public bool CanExport => !string.IsNullOrEmpty(SavePath) && !string.IsNullOrEmpty(FileName) && !IsExporting;

        // 交互
        public Interaction<Unit, IStorageFolder?> ShowFolderDialog { get; }

        // 命令
        public ReactiveCommand<Unit, Unit> BrowsePathCommand { get; }
        public ReactiveCommand<Unit, Unit> PreviewCommand { get; }
        public ReactiveCommand<Unit, Unit> ExportCommand { get; }
        public ReactiveCommand<Unit, Unit> ResetCommand { get; }
        public ReactiveCommand<string, Unit> OpenFileCommand { get; }

        public ReportExportViewModel(IServiceProvider services)
        {
            _reportService = services.GetRequiredService<IReportService>();
            _systemService = services.GetRequiredService<ISystemService>();
            
            _systems = new ObservableCollection<SystemModel>();
            _platforms = new ObservableCollection<PlatformModel>();
            _modules = new ObservableCollection<ModuleModel>();
            _exportHistory = new ObservableCollection<ExportHistoryItem>();

            ShowFolderDialog = new Interaction<Unit, IStorageFolder?>();

            // 初始化命令
            BrowsePathCommand = ReactiveCommand.CreateFromTask(BrowsePath);
            PreviewCommand = ReactiveCommand.CreateFromTask(PreviewReport);
            ExportCommand = ReactiveCommand.CreateFromTask(ExportReport);
            ResetCommand = ReactiveCommand.Create(Reset);
            OpenFileCommand = ReactiveCommand.Create<string>(OpenFile);

            // 监听属性变化
            this.WhenAnyValue(x => x.SelectedSystem)
                .Subscribe(system => LoadPlatforms(system));
            
            this.WhenAnyValue(x => x.SelectedPlatform)
                .Subscribe(platform => LoadModules(platform));

            this.WhenAnyValue(
                x => x.SavePath,
                x => x.FileName,
                x => x.IsExporting)
                .Subscribe(_ =>
                {
                    this.RaisePropertyChanged(nameof(CanPreview));
                    this.RaisePropertyChanged(nameof(CanExport));
                });

            LoadSystems();
            InitializeDefaults();
        }

        private void InitializeDefaults()
        {
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        private void LoadSystems()
        {
            try
            {
                var systems = _systemService.GetAllSystems();
                Systems = new ObservableCollection<SystemModel>(systems);
                Platforms.Clear();
                Modules.Clear();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading systems: {ex.Message}");
            }
        }

        private void LoadPlatforms(SystemModel? system)
        {
            Platforms.Clear();
            Modules.Clear();
            if (system != null)
            {
                try
                {
                    var platforms = _systemService.GetPlatformsBySystemId(system.Id);
                    Platforms = new ObservableCollection<PlatformModel>(platforms);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading platforms: {ex.Message}");
                }
            }
        }

        private void LoadModules(PlatformModel? platform)
        {
            Modules.Clear();
            if (platform != null)
            {
                try
                {
                    var modules = _systemService.GetModulesByPlatformId(platform.Id);
                    Modules = new ObservableCollection<ModuleModel>(modules);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error loading modules: {ex.Message}");
                }
            }
        }

        private async Task BrowsePath()
        {
            var folder = await ShowFolderDialog.Handle(Unit.Default).FirstAsync();
            if (folder != null)
            {
                SavePath = folder.Path.LocalPath;
            }
        }

        private async Task PreviewReport()
        {
            try
            {
                ProgressMessage = "正在生成预览...";
                IsExporting = true;
                ExportProgress = 0;

                await Task.Delay(500); // 模拟预览生成
                ExportProgress = 50;
                
                // TODO: 实现实际的预览功能
                
                ExportProgress = 100;
                ProgressMessage = "预览生成完成";
                
                await Task.Delay(1000);
                IsExporting = false;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error previewing report: {ex.Message}");
                IsExporting = false;
            }
        }

        private async Task ExportReport()
        {
            if (string.IsNullOrEmpty(SavePath) || string.IsNullOrEmpty(FileName))
            {
                return;
            }

            try
            {
                IsExporting = true;
                ExportProgress = 0;
                ProgressMessage = "开始导出报表...";

                // 确定报表类型
                string reportType = "FullReport";
                if (IsWeibullReport) reportType = "WeibullReport";
                else if (IsSystemReport) reportType = "SystemReport";
                else if (IsDataSummaryReport) reportType = "DataSummaryReport";
                else if (IsCustomReport) reportType = "CustomReport";

                // 确定实体ID
                string? entityId = _selectedModule?.Id ?? _selectedPlatform?.Id ?? _selectedSystem?.Id;
                if (entityId == null)
                {
                    ProgressMessage = "请选择要导出的系统、平台或模组";
                    await Task.Delay(2000);
                    IsExporting = false;
                    return;
                }

                // 确定文件格式
                string format = "Excel";
                string extension = ".xlsx";
                if (IsPdfFormat)
                {
                    format = "PDF";
                    extension = ".pdf";
                }
                else if (IsWordFormat)
                {
                    format = "Word";
                    extension = ".docx";
                }

                // 构建完整文件路径
                string fullFileName = FileName.EndsWith(extension) ? FileName : FileName + extension;
                string fullPath = System.IO.Path.Combine(SavePath, fullFileName);

                ExportProgress = 20;
                ProgressMessage = "正在收集数据...";

                await Task.Delay(500);
                ExportProgress = 40;
                ProgressMessage = "正在生成报表...";

                // 生成报表
                await _reportService.GenerateReportAsync(reportType, entityId, fullPath, format);

                ExportProgress = 80;
                ProgressMessage = "正在保存文件...";

                await Task.Delay(500);
                ExportProgress = 100;
                ProgressMessage = "报表导出完成！";

                // 添加到导出历史
                ExportHistory.Insert(0, new ExportHistoryItem
                {
                    FileName = fullFileName,
                    ExportTime = DateTime.Now,
                    FilePath = fullPath
                });

                // 保持最多10条历史记录
                while (ExportHistory.Count > 10)
                {
                    ExportHistory.RemoveAt(ExportHistory.Count - 1);
                }

                await Task.Delay(1500);
                IsExporting = false;
            }
            catch (Exception ex)
            {
                ProgressMessage = $"导出失败: {ex.Message}";
                Console.WriteLine($"Error exporting report: {ex.Message}");
                await Task.Delay(3000);
                IsExporting = false;
            }
        }

        private void Reset()
        {
            SelectedSystem = null;
            SelectedPlatform = null;
            SelectedModule = null;
            IsWeibullReport = true;
            IsSystemReport = false;
            IsDataSummaryReport = false;
            IsCustomReport = false;
            StartDate = DateTime.Now.AddMonths(-1);
            EndDate = DateTime.Now;
            IncludeDetailData = true;
            IncludeCharts = true;
            IncludeStatistics = true;
            IncludeConfidenceInterval = true;
            AddCoverPage = true;
            AddTableOfContents = true;
            IsExcelFormat = true;
            IsPdfFormat = false;
            IsWordFormat = false;
            SavePath = string.Empty;
            FileName = $"Report_{DateTime.Now:yyyyMMdd_HHmmss}";
        }

        private void OpenFile(string filePath)
        {
            try
            {
                if (System.IO.File.Exists(filePath))
                {
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = filePath,
                        UseShellExecute = true
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error opening file: {ex.Message}");
            }
        }
    }
}
