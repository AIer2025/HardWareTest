using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Analysis;
using LabTestPlatform.Core.Models;

namespace LabTestPlatform.Core.Services;

public interface IWeibullAnalysisService
{
    Task<WeibullResult> AnalyzeModuleAsync(int moduleId, string testType, double confidenceLevel);
    
    // 修改：接受 string 类型的 moduleId 以匹配 ModuleModel.Id 的类型
    IEnumerable<TestData> GetTestDataByModuleId(string moduleId);
    (double beta, double eta) CalculateWeibullParameters(double[] failures, double[] suspensions);
    double[] GetFailureProbabilities(int count);
    
    /// <summary>
    /// 执行完整的威布尔分析（使用WeibullEngine）
    /// </summary>
    /// <param name="failureTimes">失效时间数组</param>
    /// <param name="isCensored">删尾标记数组（true表示删尾，false表示失效）</param>
    /// <param name="confidenceLevel">置信水平（默认0.95）</param>
    /// <returns>完整的威布尔分析结果</returns>
    WeibullResult AnalyzeWithEngine(double[] failureTimes, bool[] isCensored, double confidenceLevel = 0.95);
}
