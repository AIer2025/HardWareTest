using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using LabTestPlatform.Core.Services;
using LabTestPlatform.Core.Models;

namespace LabTestPlatform.UI.ViewModels;

public class ReportExportViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private readonly ISystemService _systemService;
    
    // Report type
    private bool _isWeibullReport = true;
    private bool _isSystemReport;
    private bool _isDataSummaryReport;
    private bool _isCustomReport;

    // Data range
    private ObservableCollection<SystemModel> _systems = new();
    private SystemModel? _selectedSystem;
    private ObservableCollection<PlatformModel> _platforms = new();
    private PlatformModel? _selectedPlatform;
    private ObservableCollection<ModuleModel> _modules = new();
    private ModuleModel? _selectedModule;
    private DateTime? _startDate;
    private DateTime? _endDate;

    // Report options
    private bool _includeDetailData = true;
    private bool _includeCharts = true;
    private bool _includeStatistics = true;
    private bool _includeConfidenceInterval = true;
    private bool _addCoverPage = true;
    private bool _addTableOfContents = true;

    // Output format
    private bool _isExcelFormat = true;
    private bool _isPdfFormat;
    private bool _isWordFormat;

    // Save path
    private string _savePath = string.Empty;
    private string _fileName = "报告";

    // Progress
    private bool _isExporting;
    private double _exportProgress;
    private string _progressMessage = string.Empty;

    // Export history
    private ObservableCollection<ExportHistoryItem> _exportHistory = new();

    public ReportExportViewModel(IServiceProvider serviceProvider)
    {
        _reportService = serviceProvider.GetRequiredService<IReportService>();
        _systemService = serviceProvider.GetRequiredService<ISystemService>();

        // Initialize commands
        BrowsePathCommand = ReactiveCommand.Create(BrowsePath);
        PreviewCommand = ReactiveCommand.CreateFromTask(PreviewAsync, 
            this.WhenAnyValue(x => x.CanPreview));
        ExportCommand = ReactiveCommand.CreateFromTask(ExportAsync,
            this.WhenAnyValue(x => x.CanExport));
        ResetCommand = ReactiveCommand.Create(Reset);
        OpenFileCommand = ReactiveCommand.Create<string>(OpenFile);

        // Load initial data
        _ = LoadSystemsAsync();

        // Setup cascading selections
        this.WhenAnyValue(x => x.SelectedSystem)
            .WhereNotNull()
            .Subscribe(async _ => await LoadPlatformsAsync());

        this.WhenAnyValue(x => x.SelectedPlatform)
            .WhereNotNull()
            .Subscribe(async _ => await LoadModulesAsync());
    }

    #region Report Type Properties
    public bool IsWeibullReport
    {
        get => _isWeibullReport;
        set => this.RaiseAndSetIfChanged(ref _isWeibullReport, value);
    }

    public bool IsSystemReport
    {
        get => _isSystemReport;
        set => this.RaiseAndSetIfChanged(ref _isSystemReport, value);
    }

    public bool IsDataSummaryReport
    {
        get => _isDataSummaryReport;
        set => this.RaiseAndSetIfChanged(ref _isDataSummaryReport, value);
    }

    public bool IsCustomReport
    {
        get => _isCustomReport;
        set => this.RaiseAndSetIfChanged(ref _isCustomReport, value);
    }
    #endregion

    #region Data Range Properties
    public ObservableCollection<SystemModel> Systems
    {
        get => _systems;
        set => this.RaiseAndSetIfChanged(ref _systems, value);
    }

    public SystemModel? SelectedSystem
    {
        get => _selectedSystem;
        set => this.RaiseAndSetIfChanged(ref _selectedSystem, value);
    }

    public bool HasSelectedSystem => SelectedSystem != null;

    public ObservableCollection<PlatformModel> Platforms
    {
        get => _platforms;
        set => this.RaiseAndSetIfChanged(ref _platforms, value);
    }

    public PlatformModel? SelectedPlatform
    {
        get => _selectedPlatform;
        set => this.RaiseAndSetIfChanged(ref _selectedPlatform, value);
    }

    public bool HasSelectedPlatform => SelectedPlatform != null;

    public ObservableCollection<ModuleModel> Modules
    {
        get => _modules;
        set => this.RaiseAndSetIfChanged(ref _modules, value);
    }

    public ModuleModel? SelectedModule
    {
        get => _selectedModule;
        set => this.RaiseAndSetIfChanged(ref _selectedModule, value);
    }

    public DateTime? StartDate
    {
        get => _startDate;
        set => this.RaiseAndSetIfChanged(ref _startDate, value);
    }

    public DateTime? EndDate
    {
        get => _endDate;
        set => this.RaiseAndSetIfChanged(ref _endDate, value);
    }
    #endregion

    #region Report Options Properties
    public bool IncludeDetailData
    {
        get => _includeDetailData;
        set => this.RaiseAndSetIfChanged(ref _includeDetailData, value);
    }

    public bool IncludeCharts
    {
        get => _includeCharts;
        set => this.RaiseAndSetIfChanged(ref _includeCharts, value);
    }

    public bool IncludeStatistics
    {
        get => _includeStatistics;
        set => this.RaiseAndSetIfChanged(ref _includeStatistics, value);
    }

    public bool IncludeConfidenceInterval
    {
        get => _includeConfidenceInterval;
        set => this.RaiseAndSetIfChanged(ref _includeConfidenceInterval, value);
    }

    public bool AddCoverPage
    {
        get => _addCoverPage;
        set => this.RaiseAndSetIfChanged(ref _addCoverPage, value);
    }

    public bool AddTableOfContents
    {
        get => _addTableOfContents;
        set => this.RaiseAndSetIfChanged(ref _addTableOfContents, value);
    }
    #endregion

    #region Output Format Properties
    public bool IsExcelFormat
    {
        get => _isExcelFormat;
        set => this.RaiseAndSetIfChanged(ref _isExcelFormat, value);
    }

    public bool IsPdfFormat
    {
        get => _isPdfFormat;
        set => this.RaiseAndSetIfChanged(ref _isPdfFormat, value);
    }

    public bool IsWordFormat
    {
        get => _isWordFormat;
        set => this.RaiseAndSetIfChanged(ref _isWordFormat, value);
    }
    #endregion

    #region Save Path Properties
    public string SavePath
    {
        get => _savePath;
        set => this.RaiseAndSetIfChanged(ref _savePath, value);
    }

    public string FileName
    {
        get => _fileName;
        set => this.RaiseAndSetIfChanged(ref _fileName, value);
    }
    #endregion

    #region Progress Properties
    public bool IsExporting
    {
        get => _isExporting;
        set => this.RaiseAndSetIfChanged(ref _isExporting, value);
    }

    public double ExportProgress
    {
        get => _exportProgress;
        set => this.RaiseAndSetIfChanged(ref _exportProgress, value);
    }

    public string ProgressMessage
    {
        get => _progressMessage;
        set => this.RaiseAndSetIfChanged(ref _progressMessage, value);
    }

    public bool CanPreview => !string.IsNullOrEmpty(FileName);
    public bool CanExport => !string.IsNullOrEmpty(FileName) && !string.IsNullOrEmpty(SavePath);
    #endregion

    #region Export History
    public ObservableCollection<ExportHistoryItem> ExportHistory
    {
        get => _exportHistory;
        set => this.RaiseAndSetIfChanged(ref _exportHistory, value);
    }
    #endregion

    #region Commands
    public ReactiveCommand<Unit, Unit> BrowsePathCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviewCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }
    public ReactiveCommand<string, Unit> OpenFileCommand { get; }
    #endregion

    #region Private Methods
    private async Task LoadSystemsAsync()
    {
        try
        {
            var systems = await _systemService.GetAllSystemsAsync();
            Systems = new ObservableCollection<SystemModel>(systems);
        }
        catch (Exception ex)
        {
            ProgressMessage = $"加载系统列表失败: {ex.Message}";
        }
    }

    private async Task LoadPlatformsAsync()
    {
        Platforms.Clear();
        SelectedPlatform = null;
        Modules.Clear();
        SelectedModule = null;

        if (SelectedSystem == null) return;

        try
        {
            var platforms = await _systemService.GetPlatformsBySystemIdAsync(SelectedSystem.SystemId);
            Platforms = new ObservableCollection<PlatformModel>(platforms);
        }
        catch (Exception ex)
        {
            ProgressMessage = $"加载平台列表失败: {ex.Message}";
        }
    }

    private async Task LoadModulesAsync()
    {
        Modules.Clear();
        SelectedModule = null;

        if (SelectedPlatform == null) return;

        try
        {
            var modules = await _systemService.GetModulesByPlatformIdAsync(SelectedPlatform.PlatformId);
            Modules = new ObservableCollection<ModuleModel>(modules);
        }
        catch (Exception ex)
        {
            ProgressMessage = $"加载模组列表失败: {ex.Message}";
        }
    }

    private void BrowsePath()
    {
        // TODO: Implement file dialog
        SavePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
    }

    private async Task PreviewAsync()
    {
        ProgressMessage = "预览功能待实现";
        await Task.CompletedTask;
    }

    private async Task ExportAsync()
    {
        try
        {
            IsExporting = true;
            ExportProgress = 0;
            ProgressMessage = "开始导出...";

            // Simulate export progress
            for (int i = 0; i <= 100; i += 10)
            {
                ExportProgress = i;
                ProgressMessage = $"导出进度: {i}%";
                await Task.Delay(200);
            }

            // Add to export history
            ExportHistory.Insert(0, new ExportHistoryItem
            {
                FileName = FileName,
                FilePath = System.IO.Path.Combine(SavePath, FileName),
                ExportTime = DateTime.Now
            });

            ProgressMessage = "导出完成!";
        }
        catch (Exception ex)
        {
            ProgressMessage = $"导出失败: {ex.Message}";
        }
        finally
        {
            IsExporting = false;
        }
    }

    private void Reset()
    {
        SelectedSystem = null;
        SelectedPlatform = null;
        SelectedModule = null;
        StartDate = null;
        EndDate = null;
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
        FileName = "报告";
        ProgressMessage = "已重置所有设置";
    }

    private void OpenFile(string filePath)
    {
        try
        {
            if (System.IO.File.Exists(filePath))
            {
                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = filePath,
                    UseShellExecute = true
                });
            }
        }
        catch (Exception ex)
        {
            ProgressMessage = $"打开文件失败: {ex.Message}";
        }
    }
    #endregion
}

public class ExportHistoryItem
{
    public string FileName { get; set; } = string.Empty;
    public string FilePath { get; set; } = string.Empty;
    public DateTime ExportTime { get; set; }
}
