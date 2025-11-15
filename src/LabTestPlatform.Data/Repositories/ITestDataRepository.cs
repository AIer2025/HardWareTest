using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public interface ITestDataRepository
{
    Task<IEnumerable<TestDataEntity>> GetByModuleIdAsync(int moduleId);
    Task<IEnumerable<TestDataEntity>> GetByModuleIdAndTestTypeAsync(int moduleId, string testType);
    Task<long> InsertAsync(TestDataEntity testData);
    Task<int> BatchInsertAsync(IEnumerable<TestDataEntity> testDataList);
}
