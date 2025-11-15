using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using LabTestPlatform.Models;
using LabTestPlatform.Services;

namespace LabTestPlatform.Services
{
    /// <summary>
    /// PDF报告生成服务
    /// </summary>
    public class ReportGenerationService
    {
        public ReportGenerationService()
        {
            // 设置QuestPDF许可证
            QuestPDF.Settings.License = LicenseType.Community;
        }

        /// <summary>
        /// 生成威布尔分析PDF报告
        /// </summary>
        public async Task<string> GenerateWeibullReportAsync(
            WeibullAnalysisResultEntity analysisResult,
            ModuleEntity module,
            TestDataEntity[] testData)
        {
            var fileName = $"威布尔分析报告_{module.ModuleCode}_{DateTime.Now:yyyyMMdd_HHmmss}.pdf";
            var filePath = Path.Combine("Reports", fileName);
            Directory.CreateDirectory("Reports");

            await Task.Run(() =>
            {
                Document.Create(container =>
                {
                    container.Page(page =>
                    {
                        page.Size(PageSizes.A4);
                        page.Margin(2, Unit.Centimetre);
                        page.PageColor(Colors.White);
                        page.DefaultTextStyle(x => x.FontSize(12).FontFamily("Microsoft YaHei"));

                        // 页眉
                        page.Header()
                            .BorderBottom(1)
                            .PaddingBottom(10)
                            .Row(row =>
                            {
                                row.RelativeItem().Column(column =>
                                {
                                    column.Item().Text("实验室测试平台").Bold().FontSize(20);
                                    column.Item().Text("威布尔可靠性分析报告").FontSize(16);
                                });
                                row.ConstantItem(100).AlignRight().Text($"报告日期: {DateTime.Now:yyyy-MM-dd}");
                            });

                        // 内容
                        page.Content()
                            .PaddingVertical(20)
                            .Column(column =>
                            {
                                // 基本信息
                                column.Item().Element(c => BuildBasicInfo(c, module, analysisResult));
                                column.Item().PaddingTop(20);

                                // 威布尔参数
                                column.Item().Element(c => BuildWeibullParameters(c, analysisResult));
                                column.Item().PaddingTop(20);

                                // 可靠性指标
                                column.Item().Element(c => BuildReliabilityMetrics(c, analysisResult));
                                column.Item().PaddingTop(20);

                                // 数据统计
                                column.Item().Element(c => BuildDataStatistics(c, testData));
                                column.Item().PaddingTop(20);

                                // 分析结论
                                column.Item().Element(c => BuildConclusion(c, analysisResult));
                            });

                        // 页脚
                        page.Footer()
                            .BorderTop(1)
                            .PaddingTop(10)
                            .AlignCenter()
                            .Text(x =>
                            {
                                x.Span("第 ");
                                x.CurrentPageNumber();
                                x.Span(" 页 / 共 ");
                                x.TotalPages();
                                x.Span(" 页");
                            });
                    });
                })
                .GeneratePdf(filePath);
            });

            return filePath;
        }

        private void BuildBasicInfo(IContainer container, ModuleEntity module, WeibullAnalysisResultEntity result)
        {
            container.Border(1).Padding(10).Column(column =>
            {
                column.Item().Text("基本信息").Bold().FontSize(16);
                column.Item().PaddingTop(10);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"模组名称: {module.ModuleName}");
                    row.RelativeItem().Text($"模组编码: {module.ModuleCode}");
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"系统名称: {module.SystemName}");
                    row.RelativeItem().Text($"平台名称: {module.PlatformName}");
                });

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"分析时间: {result.AnalysisTime:yyyy-MM-dd HH:mm:ss}");
                    row.RelativeItem().Text($"分析方法: {result.EstimationMethod}");
                });

                column.Item().Text($"分析人员: {result.Analyst ?? "系统"}");
            });
        }

        private void BuildWeibullParameters(IContainer container, WeibullAnalysisResultEntity result)
        {
            container.Border(1).Padding(10).Column(column =>
            {
                column.Item().Text("威布尔分布参数").Bold().FontSize(16);
                column.Item().PaddingTop(10);

                column.Item().Row(row =>
                {
                    row.RelativeItem().Text($"形状参数 (β): {result.ShapeParameter:F4}");
                    row.RelativeItem().Text($"尺度参数 (η): {result.ScaleParameter:F2}");
                });

                if (result.LocationParameter.HasValue && result.LocationParameter.Value != 0)
                {
                    column.Item().Text($"位置参数 (γ): {result.LocationParameter:F2}");
                }

                column.Item().Text($"拟合优度 (R²): {result.RSquared:F4}");

                column.Item().PaddingTop(10);
                column.Item().Text("参数解释:").Bold();
                column.Item().Text(GetShapeParameterInterpretation(result.ShapeParameter ?? 1));
            });
        }

        private void BuildReliabilityMetrics(IContainer container, WeibullAnalysisResultEntity result)
        {
            container.Border(1).Padding(10).Column(column =>
            {
                column.Item().Text("可靠性指标").Bold().FontSize(16);
                column.Item().PaddingTop(10);

                column.Item().Text($"平均无故障时间 (MTBF): {result.MTBF:F2} 小时");

                column.Item().PaddingTop(10);
                column.Item().Text("B寿命:").Bold();
                column.Item().Text($"  B10寿命: {result.B10Life:F2} 小时 (10%失效时间)");
                column.Item().Text($"  B50寿命: {result.B50Life:F2} 小时 (50%失效时间)");

                column.Item().PaddingTop(10);
                column.Item().Text("特定时间点可靠度:").Bold();
                column.Item().Text($"  1000小时: {result.ReliabilityAt1000h:P2}");
                column.Item().Text($"  5000小时: {result.ReliabilityAt5000h:P2}");
                column.Item().Text($"  10000小时: {result.ReliabilityAt10000h:P2}");
            });
        }

        private void BuildDataStatistics(IContainer container, TestDataEntity[] testData)
        {
            if (testData == null || testData.Length == 0) return;

            var values = testData.Select(t => t.TestValue).ToArray();
            var min = values.Min();
            var max = values.Max();
            var avg = values.Average();
            var std = Math.Sqrt(values.Select(v => Math.Pow(v - avg, 2)).Average());

            container.Border(1).Padding(10).Column(column =>
            {
                column.Item().Text("测试数据统计").Bold().FontSize(16);
                column.Item().PaddingTop(10);

                column.Item().Text($"数据点数量: {testData.Length}");
                column.Item().Text($"最小值: {min:F2}");
                column.Item().Text($"最大值: {max:F2}");
                column.Item().Text($"平均值: {avg:F2}");
                column.Item().Text($"标准差: {std:F2}");
            });
        }

        private void BuildConclusion(IContainer container, WeibullAnalysisResultEntity result)
        {
            container.Border(1).Padding(10).Column(column =>
            {
                column.Item().Text("分析结论").Bold().FontSize(16);
                column.Item().PaddingTop(10);

                var beta = result.ShapeParameter ?? 1;
                string failurePattern;
                string recommendation;

                if (beta < 1)
                {
                    failurePattern = "早期失效模式 (β < 1): 失效率随时间递减,表明产品存在早期失效问题。";
                    recommendation = "建议加强筛选测试,改进制造工艺,减少早期缺陷。";
                }
                else if (beta > 1 && beta < 3)
                {
                    failurePattern = "随机失效模式 (β ≈ 1): 失效率基本恒定,属于产品正常使用期。";
                    recommendation = "产品处于稳定期,建议继续监控并优化维护策略。";
                }
                else
                {
                    failurePattern = "耗损失效模式 (β > 1): 失效率随时间递增,表明产品进入老化期。";
                    recommendation = "建议制定预防性维护计划,考虑在达到B10寿命前进行更换。";
                }

                column.Item().Text(failurePattern);
                column.Item().PaddingTop(5);
                column.Item().Text(recommendation);

                if (result.RSquared.HasValue && result.RSquared < 0.95)
                {
                    column.Item().PaddingTop(10);
                    column.Item().Text($"注意: R²值为{result.RSquared:F4},拟合优度较低,建议增加测试数据量或检查数据质量。")
                        .FontColor(Colors.Red.Medium);
                }
            });
        }

        private string GetShapeParameterInterpretation(double beta)
        {
            if (beta < 1)
                return $"β = {beta:F2} < 1: 失效率递减,早期失效特征,可能存在制造缺陷";
            else if (beta < 1.5)
                return $"β = {beta:F2} ≈ 1: 失效率基本恒定,随机失效,指数分布特征";
            else if (beta < 3)
                return $"β = {beta:F2}: 失效率缓慢增长,正常磨损老化";
            else
                return $"β = {beta:F2} > 3: 失效率快速增长,严重老化或疲劳失效";
        }
    }
}
