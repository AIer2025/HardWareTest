using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public interface IModuleRepository
{
    Task<IEnumerable<ModuleEntity>> GetAllAsync();
    Task<ModuleEntity?> GetByIdAsync(int moduleId);
    Task<IEnumerable<ModuleEntity>> GetByPlatformIdAsync(int platformId);
    Task<int> InsertAsync(ModuleEntity module);
    Task<bool> UpdateAsync(ModuleEntity module);
    Task<bool> DeleteAsync(int moduleId);
}
