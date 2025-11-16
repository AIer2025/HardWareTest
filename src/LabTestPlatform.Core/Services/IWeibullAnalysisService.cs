using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Analysis;
using LabTestPlatform.Core.Models;

namespace LabTestPlatform.Core.Services;

public interface IWeibullAnalysisService
{
    Task<WeibullResult> AnalyzeModuleAsync(int moduleId, string testType, double confidenceLevel);
    
    // 添加缺失的方法
    IEnumerable<TestData> GetTestDataByModuleId(int moduleId);
    (double beta, double eta) CalculateWeibullParameters(double[] failures, double[] suspensions);
    double[] GetFailureProbabilities(int count);
}
