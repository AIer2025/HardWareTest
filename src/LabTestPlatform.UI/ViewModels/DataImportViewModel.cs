using System;
using System.Collections.ObjectModel;
using System.Reactive;
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
    private string _selectedFilePath = string.Empty;
    private string _importResult = string.Empty;
    private bool _isImporting = false;

    public DataImportViewModel(IServiceProvider serviceProvider)
    {
        _importService = serviceProvider.GetRequiredService<IDataImportService>();
        _systemService = serviceProvider.GetRequiredService<ISystemService>();

        ImportExcelCommand = ReactiveCommand.CreateFromTask(ImportExcelAsync);
        DownloadTemplateCommand = ReactiveCommand.CreateFromTask(DownloadTemplateAsync);
    }

    public string SelectedFilePath
    {
        get => _selectedFilePath;
        set => this.RaiseAndSetIfChanged(ref _selectedFilePath, value);
    }

    public string ImportResult
    {
        get => _importResult;
        set => this.RaiseAndSetIfChanged(ref _importResult, value);
    }

    public bool IsImporting
    {
        get => _isImporting;
        set => this.RaiseAndSetIfChanged(ref _isImporting, value);
    }

    public ReactiveCommand<Unit, Unit> ImportExcelCommand { get; }
    public ReactiveCommand<Unit, Unit> DownloadTemplateCommand { get; }

    private async Task ImportExcelAsync()
    {
        if (string.IsNullOrEmpty(SelectedFilePath))
        {
            ImportResult = "请先选择文件";
            return;
        }

        try
        {
            IsImporting = true;
            ImportResult = "正在导入...";

            var result = await _importService.ImportFromExcelAsync(SelectedFilePath, "admin");
            
            ImportResult = $"导入完成！\n总记录数: {result.TotalCount}\n成功: {result.SuccessCount}\n失败: {result.FailCount}";
        }
        catch (Exception ex)
        {
            ImportResult = $"导入失败: {ex.Message}";
        }
        finally
        {
            IsImporting = false;
        }
    }

    private async Task DownloadTemplateAsync()
    {
        try
        {
            var savePath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            var filePath = await _importService.GenerateExcelTemplateAsync(savePath);
            ImportResult = $"模板已保存到: {filePath}";
        }
        catch (Exception ex)
        {
            ImportResult = $"模板生成失败: {ex.Message}";
        }
    }
}
