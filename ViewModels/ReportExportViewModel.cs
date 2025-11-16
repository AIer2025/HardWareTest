using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using LabTestPlatform.Core.Services;

namespace LabTestPlatform.UI.ViewModels;

public class ReportExportViewModel : ViewModelBase
{
    private readonly IReportService _reportService;
    private string _statusMessage = string.Empty;

    public ReportExportViewModel(IServiceProvider serviceProvider)
    {
        _reportService = serviceProvider.GetRequiredService<IReportService>();

        ExportPdfCommand = ReactiveCommand.CreateFromTask(ExportPdfAsync);
        ExportExcelCommand = ReactiveCommand.CreateFromTask(ExportExcelAsync);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public ReactiveCommand<Unit, Unit> ExportPdfCommand { get; }
    public ReactiveCommand<Unit, Unit> ExportExcelCommand { get; }

    private async Task ExportPdfAsync()
    {
        StatusMessage = "PDF导出功能待实现";
        await Task.CompletedTask;
    }

    private async Task ExportExcelAsync()
    {
        StatusMessage = "Excel导出功能待实现";
        await Task.CompletedTask;
    }
}
