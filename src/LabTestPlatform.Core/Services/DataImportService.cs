using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using LabTestPlatform.Core.Models;
using LabTestPlatform.Data.Entities;
using LabTestPlatform.Data.Repositories;
using OfficeOpenXml;

namespace LabTestPlatform.Core.Services;

public class DataImportService : IDataImportService
{
    private readonly ITestDataRepository _testDataRepository;
    private readonly IModuleRepository _moduleRepository;

    public DataImportService(
        ITestDataRepository testDataRepository,
        IModuleRepository moduleRepository)
    {
        _testDataRepository = testDataRepository;
        _moduleRepository = moduleRepository;
        
        ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
    }

    public async Task<bool> ImportManualDataAsync(TestData testData)
    {
        if (!ValidateTestData(testData, out string error))
        {
            throw new ArgumentException(error);
        }

        var entity = MapToEntity(testData);
        var id = await _testDataRepository.InsertAsync(entity);
        return id > 0;
    }

    public async Task<ImportResult> ImportFromExcelAsync(string filePath, string operatorName)
    {
        var result = new ImportResult
        {
            BatchCode = GenerateBatchCode()
        };

        try
        {
            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0];
            var rowCount = worksheet.Dimension.Rows;

            var validData = new List<TestDataEntity>();

            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var testData = ReadRowFromExcel(worksheet, row);
                    
                    if (ValidateTestData(testData, out string error))
                    {
                        var entity = MapToEntity(testData);
                        entity.Operator = operatorName;
                        validData.Add(entity);
                        result.SuccessCount++;
                    }
                    else
                    {
                        result.FailCount++;
                        result.Errors.Add($"第{row}行: {error}");
                    }
                }
                catch (Exception ex)
                {
                    result.FailCount++;
                    result.Errors.Add($"第{row}行: {ex.Message}");
                }
            }

            result.TotalCount = rowCount - 1;

            if (validData.Any())
            {
                await _testDataRepository.BatchInsertAsync(validData);
            }
        }
        catch (Exception ex)
        {
            result.Errors.Add($"文件读取错误: {ex.Message}");
        }

        return result;
    }

    public bool ValidateTestData(TestData testData, out string errorMessage)
    {
        errorMessage = string.Empty;

        if (testData.ModuleId <= 0)
        {
            errorMessage = "模组ID无效";
            return false;
        }

        if (testData.TestValue < 0)
        {
            errorMessage = "测试值不能为负数";
            return false;
        }

        if (string.IsNullOrWhiteSpace(testData.TestType))
        {
            errorMessage = "测试类型不能为空";
            return false;
        }

        return true;
    }

    public async Task<string> GenerateExcelTemplateAsync(string savePath)
    {
        using var package = new ExcelPackage();
        var worksheet = package.Workbook.Worksheets.Add("测试数据导入模板");

        worksheet.Cells[1, 1].Value = "模组编码*";
        worksheet.Cells[1, 2].Value = "测试时间*";
        worksheet.Cells[1, 3].Value = "测试值*";
        worksheet.Cells[1, 4].Value = "测试单位";
        worksheet.Cells[1, 5].Value = "测试类型*";
        worksheet.Cells[1, 6].Value = "失效时间";
        worksheet.Cells[1, 7].Value = "备注";

        using (var range = worksheet.Cells[1, 1, 1, 7])
        {
            range.Style.Font.Bold = true;
            range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
            range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
        }

        worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();

        var fileName = Path.Combine(savePath, $"测试数据导入模板_{DateTime.Now:yyyyMMddHHmmss}.xlsx");
        await package.SaveAsAsync(new FileInfo(fileName));
        
        return fileName;
    }

    private TestData ReadRowFromExcel(ExcelWorksheet worksheet, int row)
    {
        return new TestData
        {
            ModuleCode = worksheet.Cells[row, 1].Text,
            TestTime = DateTime.Parse(worksheet.Cells[row, 2].Text),
            TestValue = decimal.Parse(worksheet.Cells[row, 3].Text),
            TestUnit = worksheet.Cells[row, 4].Text,
            TestType = worksheet.Cells[row, 5].Text,
            FailureTime = decimal.TryParse(worksheet.Cells[row, 6].Text, out decimal ft) ? ft : null,
            Remarks = worksheet.Cells[row, 7].Text
        };
    }

    private TestDataEntity MapToEntity(TestData model)
    {
        return new TestDataEntity
        {
            ModuleId = model.ModuleId,
            TestTime = model.TestTime,
            TestValue = model.TestValue,
            TestUnit = model.TestUnit,
            TestType = model.TestType,
            FailureTime = model.FailureTime,
            Operator = model.Operator,
            Remarks = model.Remarks
        };
    }

    private string GenerateBatchCode()
    {
        return $"BATCH{DateTime.Now:yyyyMMddHHmmss}";
    }
}
