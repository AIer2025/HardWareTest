using System.Threading.Tasks;

namespace LabTestPlatform.Core.Services;

public interface IReportService
{
    Task<string> GeneratePdfReportAsync(int analysisId, string savePath);
    Task<string> GenerateExcelReportAsync(int analysisId, string savePath);
}
