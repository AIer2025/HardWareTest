using LabTestPlatform.Core.Models;
using LabTestPlatform.Data.Repositories;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace LabTestPlatform.Core.Services
{
    public class SystemService : ISystemService
    {
        private readonly ISystemRepository _systemRepository;
        private readonly IPlatformRepository _platformRepository;
        private readonly IModuleRepository _moduleRepository;

        public SystemService(ISystemRepository systemRepository, IPlatformRepository platformRepository, IModuleRepository moduleRepository)
        {
            _systemRepository = systemRepository;
            _platformRepository = platformRepository;
            _moduleRepository = moduleRepository;
        }

        // System operations
        // 修正：确保此实现存在
        public IEnumerable<SystemModel> GetAllSystems()
        {
            return _systemRepository.GetAll().Select(e => new SystemModel { Id = e.Id, Name = e.Name });
        }

        public SystemModel GetSystemById(string id)
        {
            var e = _systemRepository.GetById(id);
            return new SystemModel { Id = e.Id, Name = e.Name };
        }

        public void AddSystem(SystemModel system)
        {
            _systemRepository.Add(new SystemEntity { Id = system.Id, Name = system.Name });
        }

        public void UpdateSystem(SystemModel system)
        {
            _systemRepository.Update(new SystemEntity { Id = system.Id, Name = system.Name });
        }

        public void DeleteSystem(string id)
        {
            _systemRepository.Delete(id);
        }

        // Platform operations
        // 修正：确保此实现存在
        public IEnumerable<PlatformModel> GetPlatformsBySystemId(string systemId)
        {
            return _platformRepository.GetBySystemId(systemId).Select(e => new PlatformModel { Id = e.Id, Name = e.Name, SystemId = e.SystemId });
        }

        public PlatformModel GetPlatformById(string id)
        {
            var e = _platformRepository.GetById(id);
            return new PlatformModel { Id = e.Id, Name = e.Name, SystemId = e.SystemId };
        }

        public void AddPlatform(PlatformModel platform)
        {
            _platformRepository.Add(new PlatformEntity { Id = platform.Id, Name = platform.Name, SystemId = platform.SystemId });
        }

        public void UpdatePlatform(PlatformModel platform)
        {
            _platformRepository.Update(new PlatformEntity { Id = platform.Id, Name = platform.Name, SystemId = platform.SystemId });
        }

        public void DeletePlatform(string id)
        {
            _platformRepository.Delete(id);
        }

        // Module operations
        // 修正：确保此实现存在
        public IEnumerable<ModuleModel> GetModulesByPlatformId(string platformId)
        {
            return _moduleRepository.GetByPlatformId(platformId).Select(e => new ModuleModel { Id = e.Id, Name = e.Name, PlatformId = e.PlatformId });
        }

        public ModuleModel GetModuleById(string id)
        {
            var e = _moduleRepository.GetById(id);
            return new ModuleModel { Id = e.Id, Name = e.Name, PlatformId = e.PlatformId };
        }

        public void AddModule(ModuleModel module)
        {
            _moduleRepository.Add(new ModuleEntity { Id = module.Id, Name = module.Name, PlatformId = module.PlatformId });
        }

        public void UpdateModule(ModuleModel module)
        {
            _moduleRepository.Update(new ModuleEntity { Id = module.Id, Name = module.Name, PlatformId = module.PlatformId });
        }

        public void DeleteModule(string id)
        {
            _moduleRepository.Delete(id);
        }
    }
}