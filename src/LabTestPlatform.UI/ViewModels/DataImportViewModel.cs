using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reactive;
using System.Reactive.Linq;
using System.Threading.Tasks;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using LabTestPlatform.Core.Services;
using LabTestPlatform.Core.Models;

namespace LabTestPlatform.UI.ViewModels;

public class DataImportViewModel : ViewModelBase
{
    private readonly IDataImportService _importService;
    private readonly ISystemService _systemService;
    
    // 文件相关
    private string _selectedFilePath = string.Empty;
    private string _fileInfo = string.Empty;
    
    // 导入选项
    private bool _skipDuplicates = true;
    private bool _validateData = true;
    private bool _refreshAfterImport = true;
    
    // 预览数据
    private bool _hasPreviewData = false;
    private int _previewRowCount = 0;
    private ObservableCollection<string> _previewData = new();
    
    // 导入状态
    private bool _isImporting = false;
    private double _importProgress = 0;
    private string _progressMessage = string.Empty;
    
    // 导入结果
    private bool _hasImportResult = false;
    private int _successCount = 0;
    private int _failureCount = 0;
    private string _elapsedTime = string.Empty;

    // Interaction for file selection
    public Interaction<Unit, string?> ShowOpenFileDialog { get; } = new();

    public DataImportViewModel(IServiceProvider serviceProvider)
    {
        _importService = serviceProvider.GetRequiredService<IDataImportService>();
        _systemService = serviceProvider.GetRequiredService<ISystemService>();

        // 初始化命令
        BrowseFileCommand = ReactiveCommand.CreateFromTask(BrowseFileAsync);
        PreviewCommand = ReactiveCommand.CreateFromTask(PreviewDataAsync, 
            this.WhenAnyValue(x => x.CanPreview));
        ImportCommand = ReactiveCommand.CreateFromTask(ImportDataAsync, 
            this.WhenAnyValue(x => x.CanImport));
        ResetCommand = ReactiveCommand.Create(ResetForm);
    }

    #region 属性

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set
        {
            this.RaiseAndSetIfChanged(ref _selectedFilePath, value);
            UpdateFileInfo();
            this.RaisePropertyChanged(nameof(CanPreview));
            this.RaisePropertyChanged(nameof(CanImport));
        }
    }

    public string FileInfo
    {
        get => _fileInfo;
        set => this.RaiseAndSetIfChanged(ref _fileInfo, value);
    }

    public bool SkipDuplicates
    {
        get => _skipDuplicates;
        set => this.RaiseAndSetIfChanged(ref _skipDuplicates, value);
    }

    public bool ValidateData
    {
        get => _validateData;
        set => this.RaiseAndSetIfChanged(ref _validateData, value);
    }

    public bool RefreshAfterImport
    {
        get => _refreshAfterImport;
        set => this.RaiseAndSetIfChanged(ref _refreshAfterImport, value);
    }

    public bool HasPreviewData
    {
        get => _hasPreviewData;
        set => this.RaiseAndSetIfChanged(ref _hasPreviewData, value);
    }

    public int PreviewRowCount
    {
        get => _previewRowCount;
        set => this.RaiseAndSetIfChanged(ref _previewRowCount, value);
    }

    public ObservableCollection<string> PreviewData
    {
        get => _previewData;
        set => this.RaiseAndSetIfChanged(ref _previewData, value);
    }

    public bool IsImporting
    {
        get => _isImporting;
        set
        {
            this.RaiseAndSetIfChanged(ref _isImporting, value);
            this.RaisePropertyChanged(nameof(CanPreview));
            this.RaisePropertyChanged(nameof(CanImport));
        }
    }

    public double ImportProgress
    {
        get => _importProgress;
        set => this.RaiseAndSetIfChanged(ref _importProgress, value);
    }

    public string ProgressMessage
    {
        get => _progressMessage;
        set => this.RaiseAndSetIfChanged(ref _progressMessage, value);
    }

    public bool HasImportResult
    {
        get => _hasImportResult;
        set => this.RaiseAndSetIfChanged(ref _hasImportResult, value);
    }

    public int SuccessCount
    {
        get => _successCount;
        set => this.RaiseAndSetIfChanged(ref _successCount, value);
    }

    public int FailureCount
    {
        get => _failureCount;
        set => this.RaiseAndSetIfChanged(ref _failureCount, value);
    }

    public string ElapsedTime
    {
        get => _elapsedTime;
        set => this.RaiseAndSetIfChanged(ref _elapsedTime, value);
    }

    public bool CanPreview => !string.IsNullOrEmpty(SelectedFilePath) && !IsImporting;
    public bool CanImport => !string.IsNullOrEmpty(SelectedFilePath) && !IsImporting && HasPreviewData;

    #endregion

    #region 命令

    public ReactiveCommand<Unit, Unit> BrowseFileCommand { get; }
    public ReactiveCommand<Unit, Unit> PreviewCommand { get; }
    public ReactiveCommand<Unit, Unit> ImportCommand { get; }
    public ReactiveCommand<Unit, Unit> ResetCommand { get; }

    #endregion

    #region 方法

    private async Task BrowseFileAsync()
    {
        try
        {
            // 使用Interaction请求View层打开文件对话框
            var filePath = await ShowOpenFileDialog.Handle(Unit.Default);
            
            if (!string.IsNullOrEmpty(filePath))
            {
                SelectedFilePath = filePath;
            }
        }
        catch (Exception ex)
        {
            FileInfo = $"文件选择失败: {ex.Message}";
        }
    }

    private async Task PreviewDataAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
            return;

        try
        {
            IsImporting = true;
            ProgressMessage = "正在加载预览数据...";
            HasPreviewData = false;
            PreviewData.Clear();

            // 读取Excel文件并预览前几行
            var previewLines = await Task.Run(() => ReadExcelPreview(SelectedFilePath, 10));
            
            PreviewData = new ObservableCollection<string>(previewLines);
            PreviewRowCount = previewLines.Count;
            HasPreviewData = previewLines.Count > 0;

            if (HasPreviewData)
            {
                ProgressMessage = $"已加载 {PreviewRowCount} 行预览数据";
            }
            else
            {
                ProgressMessage = "文件为空或格式不正确";
            }
        }
        catch (Exception ex)
        {
            ProgressMessage = $"预览失败: {ex.Message}";
            HasPreviewData = false;
        }
        finally
        {
            IsImporting = false;
        }
    }

    private async Task ImportDataAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
            return;

        try
        {
            IsImporting = true;
            HasImportResult = false;
            ImportProgress = 0;
            ProgressMessage = "正在准备导入...";

            var stopwatch = Stopwatch.StartNew();

            // 执行导入
            var result = await Task.Run(async () =>
            {
                // 更新进度
                for (int i = 0; i <= 100; i += 10)
                {
                    ImportProgress = i;
                    ProgressMessage = $"正在导入数据... {i}%";
                    await Task.Delay(100);
                }

                // 实际导入逻辑
                return await _importService.ImportFromExcelAsync(SelectedFilePath, "admin");
            });

            stopwatch.Stop();

            // 设置结果
            SuccessCount = result.SuccessCount;
            FailureCount = result.FailCount;
            ElapsedTime = $"{stopwatch.Elapsed.TotalSeconds:F2} 秒";
            HasImportResult = true;

            ImportProgress = 100;
            ProgressMessage = "导入完成！";

            // 如果设置了导入后刷新，可以在这里触发刷新
            if (RefreshAfterImport)
            {
                // 触发界面刷新或通知其他视图
            }
        }
        catch (Exception ex)
        {
            ProgressMessage = $"导入失败: {ex.Message}";
            FailureCount = PreviewRowCount;
            SuccessCount = 0;
            HasImportResult = true;
        }
        finally
        {
            IsImporting = false;
        }
    }

    private void ResetForm()
    {
        SelectedFilePath = string.Empty;
        FileInfo = string.Empty;
        HasPreviewData = false;
        PreviewData.Clear();
        PreviewRowCount = 0;
        HasImportResult = false;
        SuccessCount = 0;
        FailureCount = 0;
        ElapsedTime = string.Empty;
        ImportProgress = 0;
        ProgressMessage = string.Empty;
        SkipDuplicates = true;
        ValidateData = true;
        RefreshAfterImport = true;
    }

    private void UpdateFileInfo()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
        {
            FileInfo = string.Empty;
            return;
        }

        try
        {
            var fileInfo = new FileInfo(SelectedFilePath);
            FileInfo = $"文件大小: {FormatFileSize(fileInfo.Length)} | 修改时间: {fileInfo.LastWriteTime:yyyy-MM-dd HH:mm:ss}";
        }
        catch (Exception ex)
        {
            FileInfo = $"无法读取文件信息: {ex.Message}";
        }
    }

    private string FormatFileSize(long bytes)
    {
        string[] sizes = { "B", "KB", "MB", "GB" };
        double len = bytes;
        int order = 0;
        while (len >= 1024 && order < sizes.Length - 1)
        {
            order++;
            len = len / 1024;
        }
        return $"{len:0.##} {sizes[order]}";
    }

    private List<string> ReadExcelPreview(string filePath, int maxRows)
    {
        var preview = new List<string>();
        
        try
        {
            // 这里需要使用EPPlus或其他Excel库来读取
            // 简化版本：只是返回一些模拟数据作为示例
            using (var package = new OfficeOpenXml.ExcelPackage(new FileInfo(filePath)))
            {
                var worksheet = package.Workbook.Worksheets.FirstOrDefault();
                if (worksheet != null)
                {
                    int rowCount = Math.Min(worksheet.Dimension?.Rows ?? 0, maxRows);
                    int colCount = worksheet.Dimension?.Columns ?? 0;

                    for (int row = 1; row <= rowCount; row++)
                    {
                        var rowData = new List<string>();
                        for (int col = 1; col <= colCount; col++)
                        {
                            var cellValue = worksheet.Cells[row, col].Value?.ToString() ?? "";
                            rowData.Add(cellValue);
                        }
                        preview.Add(string.Join(" | ", rowData));
                    }
                }
            }
        }
        catch (Exception ex)
        {
            preview.Add($"读取失败: {ex.Message}");
        }

        return preview;
    }

    #endregion
}
