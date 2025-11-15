using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using LabTestPlatform.Core.Models;
using LabTestPlatform.Data.Repositories;

namespace LabTestPlatform.Core.Services;

public class SystemService : ISystemService
{
    private readonly ISystemRepository _systemRepository;
    private readonly IPlatformRepository _platformRepository;
    private readonly IModuleRepository _moduleRepository;

    public SystemService(
        ISystemRepository systemRepository,
        IPlatformRepository platformRepository,
        IModuleRepository moduleRepository)
    {
        _systemRepository = systemRepository;
        _platformRepository = platformRepository;
        _moduleRepository = moduleRepository;
    }

    public async Task<IEnumerable<SystemModel>> GetAllSystemsAsync()
    {
        var entities = await _systemRepository.GetAllAsync();
        return entities.Select(e => new SystemModel
        {
            SystemId = e.SystemId,
            SystemCode = e.SystemCode,
            SystemName = e.SystemName,
            Description = e.Description,
            Location = e.Location
        });
    }

    public async Task<IEnumerable<PlatformModel>> GetPlatformsBySystemIdAsync(int systemId)
    {
        var entities = await _platformRepository.GetBySystemIdAsync(systemId);
        return entities.Select(e => new PlatformModel
        {
            PlatformId = e.PlatformId,
            SystemId = e.SystemId,
            PlatformCode = e.PlatformCode,
            PlatformName = e.PlatformName,
            PlatformType = e.PlatformType
        });
    }

    public async Task<IEnumerable<ModuleModel>> GetModulesByPlatformIdAsync(int platformId)
    {
        var entities = await _moduleRepository.GetByPlatformIdAsync(platformId);
        return entities.Select(e => new ModuleModel
        {
            ModuleId = e.ModuleId,
            PlatformId = e.PlatformId,
            ModuleCode = e.ModuleCode,
            ModuleName = e.ModuleName,
            ModuleType = e.ModuleType,
            Manufacturer = e.Manufacturer
        });
    }

    public async Task<IEnumerable<ModuleModel>> GetAllModulesAsync()
    {
        var entities = await _moduleRepository.GetAllAsync();
        return entities.Select(e => new ModuleModel
        {
            ModuleId = e.ModuleId,
            PlatformId = e.PlatformId,
            ModuleCode = e.ModuleCode,
            ModuleName = e.ModuleName,
            ModuleType = e.ModuleType,
            Manufacturer = e.Manufacturer
        });
    }
}
