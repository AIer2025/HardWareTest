using System.Linq;
using System.Threading.Tasks;
using LabTestPlatform.Analysis;
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

        //return _weibullEngine.Analyze(failureTimes, confidenceLevel);
        // <--- 修正点：
        // 1. (针对错误1) 将 'failureTimes' (List<double>) 转换为 'failureTimes.ToArray()' (double[])
        // 2. (针对错误2) 在 'failureTimes' 和 'confidenceLevel' 之间插入 'null' 作为第二个参数。
        //    这是因为 'Analyze' 方法的签名需要 (double[] data, bool[]? censored, double confidenceLevel)
        //    你原来的调用 'Analyze(failureTimes, confidenceLevel)' 导致参数类型错位。
        return _weibullEngine.Analyze(failureTimes.ToArray(), null, confidenceLevel);
    }
}
