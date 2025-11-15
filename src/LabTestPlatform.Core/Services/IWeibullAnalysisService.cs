using System.Threading.Tasks;
using LabTestPlatform.Analysis;

namespace LabTestPlatform.Core.Services;

public interface IWeibullAnalysisService
{
    Task<WeibullResult> AnalyzeModuleAsync(int moduleId, string testType, double confidenceLevel);
}
