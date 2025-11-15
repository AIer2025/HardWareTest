using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public interface IPlatformRepository
{
    Task<IEnumerable<PlatformEntity>> GetBySystemIdAsync(int systemId);
    Task<PlatformEntity?> GetByIdAsync(int platformId);
    Task<int> InsertAsync(PlatformEntity platform);
    Task<bool> UpdateAsync(PlatformEntity platform);
    Task<bool> DeleteAsync(int platformId);
}
