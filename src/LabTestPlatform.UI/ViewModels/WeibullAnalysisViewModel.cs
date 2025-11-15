using System;
using System.Reactive;
using System.Threading.Tasks;
using ReactiveUI;
using Microsoft.Extensions.DependencyInjection;
using LabTestPlatform.Core.Services;
using LabTestPlatform.Analysis;

namespace LabTestPlatform.UI.ViewModels;

public class WeibullAnalysisViewModel : ViewModelBase
{
    private readonly IWeibullAnalysisService _analysisService;
    private WeibullResult? _analysisResult;
    private string _statusMessage = string.Empty;
    private bool _isAnalyzing = false;

    public WeibullAnalysisViewModel(IServiceProvider serviceProvider)
    {
        _analysisService = serviceProvider.GetRequiredService<IWeibullAnalysisService>();

        AnalyzeCommand = ReactiveCommand.CreateFromTask(PerformAnalysisAsync);
    }

    public WeibullResult? AnalysisResult
    {
        get => _analysisResult;
        set => this.RaiseAndSetIfChanged(ref _analysisResult, value);
    }

    public string StatusMessage
    {
        get => _statusMessage;
        set => this.RaiseAndSetIfChanged(ref _statusMessage, value);
    }

    public bool IsAnalyzing
    {
        get => _isAnalyzing;
        set => this.RaiseAndSetIfChanged(ref _isAnalyzing, value);
    }

    public ReactiveCommand<Unit, Unit> AnalyzeCommand { get; }

    private async Task PerformAnalysisAsync()
    {
        try
        {
            IsAnalyzing = true;
            StatusMessage = "正在进行威布尔分析...";

            var result = await _analysisService.AnalyzeModuleAsync(1, "LIFE_TEST", 95.0);
            AnalysisResult = result;
            StatusMessage = "分析完成！";
        }
        catch (Exception ex)
        {
            StatusMessage = $"分析失败: {ex.Message}";
        }
        finally
        {
            IsAnalyzing = false;
        }
    }
}
