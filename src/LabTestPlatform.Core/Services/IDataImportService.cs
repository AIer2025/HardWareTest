using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Core.Models;

namespace LabTestPlatform.Core.Services;

public interface IDataImportService
{
    Task<bool> ImportManualDataAsync(TestData testData);
    Task<ImportResult> ImportFromExcelAsync(string filePath, string operatorName);
    bool ValidateTestData(TestData testData, out string errorMessage);
    Task<string> GenerateExcelTemplateAsync(string savePath);
}

public class ImportResult
{
    public int TotalCount { get; set; }
    public int SuccessCount { get; set; }
    public int FailCount { get; set; }
    public List<string> Errors { get; set; } = new();
    public string BatchCode { get; set; } = string.Empty;
}
