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
}
