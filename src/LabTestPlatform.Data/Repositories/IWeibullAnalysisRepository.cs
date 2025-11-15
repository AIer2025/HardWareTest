using System.Threading.Tasks;

namespace LabTestPlatform.Data.Repositories;

public interface IWeibullAnalysisRepository
{
    Task<int> SaveAnalysisResultAsync(int moduleId, double beta, double eta, double mttf);
}
