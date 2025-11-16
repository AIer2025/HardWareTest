using System;
using ReactiveUI;
using System.Reactive;
using Microsoft.Extensions.DependencyInjection;
using LabTestPlatform.Core.Services;

namespace LabTestPlatform.UI.ViewModels;

public class MainWindowViewModel : ViewModelBase
{
    private ViewModelBase _currentPage;
    private readonly IServiceProvider _serviceProvider;

    public MainWindowViewModel(IServiceProvider serviceProvider)
    {
        _serviceProvider = serviceProvider;
        
        ShowSystemManagementCommand = ReactiveCommand.Create(ShowSystemManagement);
        ShowDataImportCommand = ReactiveCommand.Create(ShowDataImport);
        ShowWeibullAnalysisCommand = ReactiveCommand.Create(ShowWeibullAnalysis);
        ShowReportExportCommand = ReactiveCommand.Create(ShowReportExport);

        _currentPage = new SystemManagementViewModel(serviceProvider);
    }

    public ViewModelBase CurrentPage
    {
        get => _currentPage;
        set => this.RaiseAndSetIfChanged(ref _currentPage, value);
    }

    public ReactiveCommand<Unit, Unit> ShowSystemManagementCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowDataImportCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowWeibullAnalysisCommand { get; }
    public ReactiveCommand<Unit, Unit> ShowReportExportCommand { get; }

    private void ShowSystemManagement()
    {
        CurrentPage = new SystemManagementViewModel(_serviceProvider);
    }

    private void ShowDataImport()
    {
        CurrentPage = new DataImportViewModel(_serviceProvider);
    }

    private void ShowWeibullAnalysis()
    {
        CurrentPage = new WeibullAnalysisViewModel(_serviceProvider);
    }

    private void ShowReportExport()
    {
        CurrentPage = new ReportExportViewModel(_serviceProvider);
    }
}
