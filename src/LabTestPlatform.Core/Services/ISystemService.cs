using LabTestPlatform.Core.Models;
using System.Collections.Generic;

namespace LabTestPlatform.Core.Services
{
    public interface ISystemService
    {
        // System operations
        // 修正：确保以下方法存在
        IEnumerable<SystemModel> GetAllSystems();
        SystemModel GetSystemById(string id);
        void AddSystem(SystemModel system);
        void UpdateSystem(SystemModel system);
        void DeleteSystem(string id);

        // Platform operations
        // 修正：确保以下方法存在
        IEnumerable<PlatformModel> GetPlatformsBySystemId(string systemId);
        PlatformModel GetPlatformById(string id);
        void AddPlatform(PlatformModel platform);
        void UpdatePlatform(PlatformModel platform);
        void DeletePlatform(string id);

        // Module operations
        // 修正：确保以下方法存在
        IEnumerable<ModuleModel> GetModulesByPlatformId(string platformId);
        ModuleModel GetModuleById(string id);
        void AddModule(ModuleModel module);
        void UpdateModule(ModuleModel module);
        void DeleteModule(string id);
    }
}