using System;
using System.Threading.Tasks;

namespace LabTestPlatform.Core.Services;

public class ReportService : IReportService
{
    public async Task<string> GeneratePdfReportAsync(int analysisId, string savePath)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("PDF报告生成功能待实现");
    }

    public async Task<string> GenerateExcelReportAsync(int analysisId, string savePath)
    {
        await Task.CompletedTask;
        throw new NotImplementedException("Excel报告生成功能待实现");
    }
}
