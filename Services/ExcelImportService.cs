using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using OfficeOpenXml;
using LabTestPlatform.Models;

namespace LabTestPlatform.Services
{
    /// <summary>
    /// Excel导入服务
    /// </summary>
    public class ExcelImportService
    {
        public ExcelImportService()
        {
            // 设置EPPlus许可证上下文
            ExcelPackage.LicenseContext = LicenseContext.NonCommercial;
        }

        /// <summary>
        /// 从Excel导入测试数据
        /// </summary>
        public async Task<List<TestDataEntity>> ImportTestDataFromExcelAsync(string filePath, int moduleId)
        {
            var testDataList = new List<TestDataEntity>();

            using var package = new ExcelPackage(new FileInfo(filePath));
            var worksheet = package.Workbook.Worksheets[0]; // 读取第一个工作表
            
            // 假设Excel格式:
            // 列A: 测试时间, 列B: 测试类型, 列C: 测试值, 列D: 单位
            // 列E: 温度, 列F: 湿度, 列G: 电压, 列H: 电流
            // 列I: 压力, 列J: 运行小时数, 列K: 循环次数, 列L: 备注
            
            int rowCount = worksheet.Dimension?.Rows ?? 0;
            
            // 从第2行开始读取(假设第1行是标题)
            for (int row = 2; row <= rowCount; row++)
            {
                try
                {
                    var testData = new TestDataEntity
                    {
                        ModuleID = moduleId,
                        TestTime = ParseDateTime(worksheet.Cells[row, 1].Text) ?? DateTime.Now,
                        TestType = worksheet.Cells[row, 2].Text?.Trim() ?? "未知",
                        TestValue = ParseDouble(worksheet.Cells[row, 3].Text) ?? 0,
                        Unit = worksheet.Cells[row, 4].Text?.Trim(),
                        Temperature = ParseDouble(worksheet.Cells[row, 5].Text),
                        Humidity = ParseDouble(worksheet.Cells[row, 6].Text),
                        Voltage = ParseDouble(worksheet.Cells[row, 7].Text),
                        Current = ParseDouble(worksheet.Cells[row, 8].Text),
                        Pressure = ParseDouble(worksheet.Cells[row, 9].Text),
                        OperatingHours = ParseDouble(worksheet.Cells[row, 10].Text),
                        CycleCount = ParseInt(worksheet.Cells[row, 11].Text),
                        Remarks = worksheet.Cells[row, 12].Text?.Trim(),
                        Status = "Normal",
                        ImportSource = "Excel",
                        CreateTime = DateTime.Now
                    };
                    
                    testDataList.Add(testData);
                }
                catch (Exception ex)
                {
                    // 记录错误行但继续处理
                    Console.WriteLine($"导入第{row}行时出错: {ex.Message}");
                }
            }

            return testDataList;
        }

        /// <summary>
        /// 导出测试数据到Excel
        /// </summary>
        public async Task<string> ExportTestDataToExcelAsync(IEnumerable<TestDataEntity> testDataList, string fileName)
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("测试数据");
            
            // 设置标题行
            worksheet.Cells[1, 1].Value = "测试时间";
            worksheet.Cells[1, 2].Value = "模组名称";
            worksheet.Cells[1, 3].Value = "模组编码";
            worksheet.Cells[1, 4].Value = "测试类型";
            worksheet.Cells[1, 5].Value = "测试值";
            worksheet.Cells[1, 6].Value = "单位";
            worksheet.Cells[1, 7].Value = "温度(°C)";
            worksheet.Cells[1, 8].Value = "湿度(%)";
            worksheet.Cells[1, 9].Value = "电压(V)";
            worksheet.Cells[1, 10].Value = "电流(A)";
            worksheet.Cells[1, 11].Value = "压力";
            worksheet.Cells[1, 12].Value = "运行小时数";
            worksheet.Cells[1, 13].Value = "循环次数";
            worksheet.Cells[1, 14].Value = "状态";
            worksheet.Cells[1, 15].Value = "操作员";
            worksheet.Cells[1, 16].Value = "备注";
            
            // 设置标题行样式
            using (var range = worksheet.Cells[1, 1, 1, 16])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightGray);
            }
            
            // 填充数据
            int row = 2;
            foreach (var data in testDataList)
            {
                worksheet.Cells[row, 1].Value = data.TestTime.ToString("yyyy-MM-dd HH:mm:ss");
                worksheet.Cells[row, 2].Value = data.ModuleName;
                worksheet.Cells[row, 3].Value = data.ModuleCode;
                worksheet.Cells[row, 4].Value = data.TestType;
                worksheet.Cells[row, 5].Value = data.TestValue;
                worksheet.Cells[row, 6].Value = data.Unit;
                worksheet.Cells[row, 7].Value = data.Temperature;
                worksheet.Cells[row, 8].Value = data.Humidity;
                worksheet.Cells[row, 9].Value = data.Voltage;
                worksheet.Cells[row, 10].Value = data.Current;
                worksheet.Cells[row, 11].Value = data.Pressure;
                worksheet.Cells[row, 12].Value = data.OperatingHours;
                worksheet.Cells[row, 13].Value = data.CycleCount;
                worksheet.Cells[row, 14].Value = data.Status;
                worksheet.Cells[row, 15].Value = data.Operator;
                worksheet.Cells[row, 16].Value = data.Remarks;
                row++;
            }
            
            // 自动调整列宽
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            
            // 保存文件
            var filePath = Path.Combine("Exports", fileName);
            Directory.CreateDirectory("Exports");
            await package.SaveAsAsync(new FileInfo(filePath));
            
            return filePath;
        }

        /// <summary>
        /// 生成Excel导入模板
        /// </summary>
        public async Task<string> GenerateImportTemplateAsync(string fileName = "ImportTemplate.xlsx")
        {
            using var package = new ExcelPackage();
            var worksheet = package.Workbook.Worksheets.Add("测试数据导入模板");
            
            // 设置标题行
            worksheet.Cells[1, 1].Value = "测试时间";
            worksheet.Cells[1, 2].Value = "测试类型";
            worksheet.Cells[1, 3].Value = "测试值";
            worksheet.Cells[1, 4].Value = "单位";
            worksheet.Cells[1, 5].Value = "温度(°C)";
            worksheet.Cells[1, 6].Value = "湿度(%)";
            worksheet.Cells[1, 7].Value = "电压(V)";
            worksheet.Cells[1, 8].Value = "电流(A)";
            worksheet.Cells[1, 9].Value = "压力";
            worksheet.Cells[1, 10].Value = "运行小时数";
            worksheet.Cells[1, 11].Value = "循环次数";
            worksheet.Cells[1, 12].Value = "备注";
            
            // 添加示例数据
            worksheet.Cells[2, 1].Value = "2025-11-15 10:00:00";
            worksheet.Cells[2, 2].Value = "寿命测试";
            worksheet.Cells[2, 3].Value = 1000;
            worksheet.Cells[2, 4].Value = "小时";
            worksheet.Cells[2, 5].Value = 25.5;
            worksheet.Cells[2, 6].Value = 60;
            worksheet.Cells[2, 7].Value = 220;
            worksheet.Cells[2, 8].Value = 5.2;
            
            // 设置标题行样式
            using (var range = worksheet.Cells[1, 1, 1, 12])
            {
                range.Style.Font.Bold = true;
                range.Style.Fill.PatternType = OfficeOpenXml.Style.ExcelFillStyle.Solid;
                range.Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.LightBlue);
            }
            
            // 自动调整列宽
            worksheet.Cells[worksheet.Dimension.Address].AutoFitColumns();
            
            // 保存文件
            var filePath = Path.Combine("Templates", fileName);
            Directory.CreateDirectory("Templates");
            await package.SaveAsAsync(new FileInfo(filePath));
            
            return filePath;
        }

        // 辅助方法
        private DateTime? ParseDateTime(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (DateTime.TryParse(text, out var result)) return result;
            return null;
        }

        private double? ParseDouble(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (double.TryParse(text, out var result)) return result;
            return null;
        }

        private int? ParseInt(string text)
        {
            if (string.IsNullOrWhiteSpace(text)) return null;
            if (int.TryParse(text, out var result)) return result;
            return null;
        }
    }
}
