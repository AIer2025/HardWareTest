// ================================================================
// WeibullAnalysisService.cs - 修复版本
// 修复内容：正确传递删尾标记（is_censored）到Weibull分析引擎
// ================================================================

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabTestPlatform.Analysis;
using LabTestPlatform.Core.Models;
using LabTestPlatform.Data.Repositories;

namespace LabTestPlatform.Core.Services;

public class WeibullAnalysisService : IWeibullAnalysisService
{
    private readonly ITestDataRepository _testDataRepository;
    private readonly IWeibullEngine _weibullEngine;

    public WeibullAnalysisService(
        ITestDataRepository testDataRepository,
        IWeibullEngine weibullEngine)
    {
        _testDataRepository = testDataRepository;
        _weibullEngine = weibullEngine;
    }

    /// <summary>
    /// 分析指定模组的Weibull参数 - 修复版
    /// 修复内容：正确处理删尾数据（is_censored字段）
    /// </summary>
    /// <param name="moduleId">模组ID</param>
    /// <param name="testType">测试类型</param>
    /// <param name="confidenceLevel">置信水平（默认95%）</param>
    /// <returns>Weibull分析结果</returns>
    public async Task<WeibullResult> AnalyzeModuleAsync(int moduleId, string testType, double confidenceLevel)
    {
        // ===== 步骤1: 获取测试数据 =====
        var testData = await _testDataRepository.GetByModuleIdAndTestTypeAsync(moduleId, testType);
        
        // ===== 步骤2: 过滤有效数据（必须有失效时间） =====
        var validData = testData
            .Where(t => t.FailureTime.HasValue && t.FailureTime.Value > 0)
            .OrderBy(t => t.FailureTime)  // 按失效时间排序
            .ToList();
        
        // 数据验证
        if (validData.Count < 3)
        {
            throw new InvalidOperationException(
                $"模组 {moduleId} 的有效数据不足3个（当前{validData.Count}个），无法进行Weibull分析。" +
                $"至少需要3个数据点才能计算置信区间。");
        }
        
        // ===== 步骤3: 提取失效时间数组 =====
        var failureTimes = validData
            .Select(t => (double)t.FailureTime!.Value)
            .ToArray();
        
        // ===== 步骤4: 提取删尾标记数组 =====
        // 🔴 关键修复：这里必须提取IsCensored字段！
        // IsCensored = true  → 删尾数据（is_censored = 1）
        // IsCensored = false → 失效数据（is_censored = 0）
        var isCensored = validData
            .Select(t => t.IsCensored)
            .ToArray();
        
        // ===== 步骤5: 统计数据并输出日志（用于验证） =====
        int failureCount = isCensored.Count(c => !c);  // 失效数据数量
        int censoredCount = isCensored.Count(c => c);   // 删尾数据数量
        
        // 输出详细日志
        Console.WriteLine($"[Weibull分析] 模组{moduleId} - {testType}");
        Console.WriteLine($"  总数据量: {validData.Count}");
        Console.WriteLine($"  失效数据: {failureCount} 个");
        Console.WriteLine($"  删尾数据: {censoredCount} 个");
        Console.WriteLine($"  删尾比例: {censoredCount * 100.0 / validData.Count:F2}%");
        
        // 警告：如果删尾比例过高，提示用户
        if (censoredCount > failureCount)
        {
            Console.WriteLine($"  ⚠️ 警告：删尾数据多于失效数据，可能影响估计精度！");
        }
        
        // ===== 步骤6: 执行Weibull分析 =====
        // 🔴 关键修复：传递 isCensored 数组，而不是 null！
        var result = _weibullEngine.Analyze(failureTimes, isCensored, confidenceLevel);
        
        // ===== 步骤7: 验证结果的一致性 =====
        if (result.FailureCount != failureCount)
        {
            throw new InvalidOperationException(
                $"内部错误：失效数量不匹配！" +
                $"期望{failureCount}个，实际{result.FailureCount}个");
        }
        
        // ===== 步骤8: 输出分析结果 =====
        Console.WriteLine($"  分析结果:");
        Console.WriteLine($"    β (形状参数) = {result.Beta:F4} [{result.BetaLower:F4} - {result.BetaUpper:F4}]");
        Console.WriteLine($"    η (尺度参数) = {result.Eta:F2}h [{result.EtaLower:F2} - {result.EtaUpper:F2}]");
        Console.WriteLine($"    R² (拟合优度) = {result.RSquared:F6}");
        Console.WriteLine($"    MTTF = {result.MTTF:F2}h");
        Console.WriteLine($"    B10寿命 = {result.B10Life:F2}h");
        Console.WriteLine($"    B50寿命 = {result.B50Life:F2}h");
        Console.WriteLine($"    B90寿命 = {result.B90Life:F2}h");
        
        // R²质量评估
        if (result.RSquared > 0.95)
            Console.WriteLine($"    ✅ 拟合优秀 (R² > 0.95)");
        else if (result.RSquared > 0.90)
            Console.WriteLine($"    ✅ 拟合良好 (R² > 0.90)");
        else if (result.RSquared > 0.85)
            Console.WriteLine($"    ⚠️ 拟合可接受 (R² > 0.85)");
        else
            Console.WriteLine($"    ❌ 拟合较差 (R² ≤ 0.85)，建议检查数据或考虑其他分布");
        
        return result;
    }

    /// <summary>
    /// 根据模块 ID 获取测试数据
    /// </summary>
    public IEnumerable<TestData> GetTestDataByModuleId(string moduleId)
    {
        if (!int.TryParse(moduleId, out int moduleIdInt))
        {
            return Enumerable.Empty<TestData>();
        }
        
        var entities = _testDataRepository.GetByModuleIdAsync(moduleIdInt).GetAwaiter().GetResult();
        return entities.Select(e => new TestData
        {
            TestId = e.TestId,
            ModuleId = e.ModuleId,
            ModuleCode = string.Empty,
            TestTime = e.TestTime,
            TestValue = e.TestValue,
            TestUnit = e.TestUnit,
            TestType = e.TestType,
            TestCycle = null,
            FailureTime = e.FailureTime,
            FailureMode = e.FailureMode,
            IsCensored = e.IsCensored,  // 保留删尾标记
            Temperature = e.Temperature,
            Humidity = e.Humidity,
            Operator = e.Operator,
            Remarks = e.Remarks
        });
    }

    /// <summary>
    /// 执行完整的威布尔分析（使用WeibullEngine）
    /// </summary>
    public WeibullResult AnalyzeWithEngine(double[] failureTimes, bool[] isCensored, double confidenceLevel = 0.95)
    {
        if (failureTimes == null || failureTimes.Length == 0)
        {
            throw new ArgumentException("失效时间数据不能为空", nameof(failureTimes));
        }

        // 如果没有提供删尾标记，默认全部为失效数据
        if (isCensored == null)
        {
            Console.WriteLine("⚠️ 警告：未提供删尾标记，假设全部为失效数据");
            isCensored = new bool[failureTimes.Length];
        }

        // 调用 WeibullEngine 进行完整分析
        return _weibullEngine.Analyze(failureTimes, isCensored, confidenceLevel);
    }

    /// <summary>
    /// 简化的参数计算方法（仅用于快速估计）
    /// 注意：此方法不支持删尾数据，仅供参考
    /// </summary>
    public (double beta, double eta) CalculateWeibullParameters(double[] failures, double[] suspensions)
    {
        if (failures == null || failures.Length == 0)
        {
            return (1.0, 1.0);
        }

        Console.WriteLine("⚠️ 注意：CalculateWeibullParameters 方法不支持删尾数据");
        Console.WriteLine("   建议使用 AnalyzeModuleAsync 方法进行完整分析");

        // 使用秩回归法计算（简化版本，不考虑删尾）
        var sortedFailures = failures.OrderBy(f => f).ToArray();
        int n = sortedFailures.Length;
        
        double[] lnT = new double[n];
        double[] lnLnF = new double[n];
        
        for (int i = 0; i < n; i++)
        {
            double medianRank = (i + 0.3) / (n + 0.4);
            lnT[i] = Math.Log(sortedFailures[i]);
            lnLnF[i] = Math.Log(-Math.Log(1 - medianRank));
        }
        
        // 线性回归计算斜率和截距
        double sumX = lnT.Sum();
        double sumY = lnLnF.Sum();
        double sumXY = lnT.Zip(lnLnF, (x, y) => x * y).Sum();
        double sumX2 = lnT.Sum(x => x * x);
        
        double beta = (n * sumXY - sumX * sumY) / (n * sumX2 - sumX * sumX);
        double intercept = (sumY - beta * sumX) / n;
        double eta = Math.Exp(-intercept / beta);
        
        return (beta, eta);
    }

    /// <summary>
    /// 获取失效概率（中位秩）
    /// </summary>
    public double[] GetFailureProbabilities(int count)
    {
        if (count <= 0) return Array.Empty<double>();
        
        double[] probabilities = new double[count];
        for (int i = 0; i < count; i++)
        {
            // Bernard中位秩方法
            probabilities[i] = (i + 0.3) / (count + 0.4);
        }
        return probabilities;
    }
}

// ================================================================
// 使用示例
// ================================================================

/*
// 示例1：标准调用（推荐）
var result = await weibullService.AnalyzeModuleAsync(
    moduleId: 1, 
    testType: "LIFE_TEST", 
    confidenceLevel: 0.95
);

Console.WriteLine($"β = {result.Beta:F4}");
Console.WriteLine($"η = {result.Eta:F2}h");
Console.WriteLine($"B10 = {result.B10Life:F2}h");

// 示例2：手动指定数据
var failureTimes = new double[] { 100, 200, 300, 400, 500 };
var isCensored = new bool[] { false, false, false, true, true };  // 前3个失效，后2个删尾

var result2 = weibullService.AnalyzeWithEngine(
    failureTimes, 
    isCensored, 
    confidenceLevel: 0.95
);

// 示例3：验证失效数量
Console.WriteLine($"总样本: {result2.SampleSize}");
Console.WriteLine($"失效数: {result2.FailureCount}");  // 应该是 3
*/
