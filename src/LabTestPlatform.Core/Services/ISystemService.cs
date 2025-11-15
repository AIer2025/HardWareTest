using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Core.Models;

namespace LabTestPlatform.Core.Services;

public interface ISystemService
{
    Task<IEnumerable<SystemModel>> GetAllSystemsAsync();
    Task<IEnumerable<PlatformModel>> GetPlatformsBySystemIdAsync(int systemId);
    Task<IEnumerable<ModuleModel>> GetModulesByPlatformIdAsync(int platformId);
    Task<IEnumerable<ModuleModel>> GetAllModulesAsync();
}
