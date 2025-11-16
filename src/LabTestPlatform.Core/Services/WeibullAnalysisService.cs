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

    public async Task<WeibullResult> AnalyzeModuleAsync(int moduleId, string testType, double confidenceLevel)
    {
        var testData = await _testDataRepository.GetByModuleIdAndTestTypeAsync(moduleId, testType);
        var failureTimes = testData
            .Where(t => t.FailureTime.HasValue)
            .Select(t => (double)t.FailureTime!.Value)
            .ToList();

        return _weibullEngine.Analyze(failureTimes.ToArray(), null, confidenceLevel);
    }

    public IEnumerable<TestData> GetTestDataByModuleId(int moduleId)
    {
        var entities = _testDataRepository.GetByModuleIdAsync(moduleId).GetAwaiter().GetResult();
        return entities.Select(e => new TestData
        {
            TestId = e.TestId,
            ModuleId = e.ModuleId,
            ModuleCode = string.Empty, // TestDataEntity 没有此字段
            TestTime = e.TestTime,
            TestValue = e.TestValue,
            TestUnit = e.TestUnit,
            TestType = e.TestType,
            TestCycle = null, // TestDataEntity 没有此字段
            FailureTime = e.FailureTime,
            FailureMode = e.FailureMode,
            IsCensored = e.IsCensored,
            Temperature = e.Temperature,
            Humidity = e.Humidity,
            Operator = e.Operator,
            Remarks = e.Remarks
        });
    }

    public (double beta, double eta) CalculateWeibullParameters(double[] failures, double[] suspensions)
    {
        if (failures == null || failures.Length == 0)
        {
            return (1.0, 1.0);
        }

        // 使用最大似然估计或其他方法计算威布尔参数
        // 这里使用简化的线性回归方法
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

    public double[] GetFailureProbabilities(int count)
    {
        if (count <= 0) return Array.Empty<double>();
        
        double[] probabilities = new double[count];
        for (int i = 0; i < count; i++)
        {
            // 使用中位秩方法计算失效概率
            probabilities[i] = (i + 0.3) / (count + 0.4);
        }
        return probabilities;
    }
}
