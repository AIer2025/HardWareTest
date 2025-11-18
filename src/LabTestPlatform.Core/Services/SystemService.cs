using LabTestPlatform.Core.Models;
using LabTestPlatform.Data.Repositories;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace LabTestPlatform.Core.Services
{
    /// <summary>
    /// 系统服务 - 处理系统、平台、模组的业务逻辑
    /// </summary>
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

        #region System Operations

        public IEnumerable<SystemModel> GetAllSystems()
        {
            return _systemRepository.GetAll().Select(e => new SystemModel 
            { 
                Id = e.SystemId.ToString(),      // 映射 SystemId -> Id
                Name = e.SystemName               // 映射 SystemName -> Name
            });
        }

        public SystemModel? GetSystemById(string id)
        {
            var entity = _systemRepository.GetById(id);
            if (entity == null)
                return null;
                
            return new SystemModel 
            { 
                Id = entity.SystemId.ToString(), 
                Name = entity.SystemName 
            };
        }

        public void AddSystem(SystemModel system)
        {
            var entity = new SystemEntity
            {
                SystemCode = system.Id,          // 使用Id作为SystemCode
                SystemName = system.Name,
                IsActive = true
            };
            
            _systemRepository.Add(entity);
        }

        public void UpdateSystem(SystemModel system)
        {
            // 先获取现有实体以保留其他字段
            var existing = _systemRepository.GetById(system.Id);
            if (existing != null)
            {
                existing.SystemName = system.Name;
                _systemRepository.Update(existing);
            }
        }

        public void DeleteSystem(string id)
        {
            _systemRepository.Delete(id);
        }

        #endregion

        #region Platform Operations

        public IEnumerable<PlatformModel> GetPlatformsBySystemId(string systemId)
        {
            return _platformRepository.GetBySystemId(systemId).Select(e => new PlatformModel 
            { 
                Id = e.PlatformId.ToString(),    // 映射 PlatformId -> Id
                Name = e.PlatformName,            // 映射 PlatformName -> Name
                SystemId = e.SystemId.ToString()  // 映射 SystemId (int) -> SystemId (string)
            });
        }

        public PlatformModel? GetPlatformById(string id)
        {
            var entity = _platformRepository.GetById(id);
            if (entity == null)
                return null;
                
            return new PlatformModel 
            { 
                Id = entity.PlatformId.ToString(), 
                Name = entity.PlatformName,
                SystemId = entity.SystemId.ToString()
            };
        }

        public void AddPlatform(PlatformModel platform)
        {
            var entity = new PlatformEntity
            {
                PlatformCode = platform.Id,           // 使用Id作为PlatformCode
                PlatformName = platform.Name,
                SystemId = int.Parse(platform.SystemId), // 转换为int
                IsActive = true
            };
            
            _platformRepository.Add(entity);
        }

        public void UpdatePlatform(PlatformModel platform)
        {
            // 先获取现有实体以保留其他字段
            var existing = _platformRepository.GetById(platform.Id);
            if (existing != null)
            {
                existing.PlatformName = platform.Name;
                existing.SystemId = int.Parse(platform.SystemId);
                _platformRepository.Update(existing);
            }
        }

        public void DeletePlatform(string id)
        {
            _platformRepository.Delete(id);
        }

        #endregion

        #region Module Operations

        public IEnumerable<ModuleModel> GetModulesByPlatformId(string platformId)
        {
            return _moduleRepository.GetByPlatformId(platformId).Select(e => new ModuleModel 
            { 
                Id = e.ModuleId.ToString(),      // 映射 ModuleId -> Id
                Name = e.ModuleName,              // 映射 ModuleName -> Name
                PlatformId = e.PlatformId.ToString() // 映射 PlatformId (int) -> PlatformId (string)
            });
        }

        public ModuleModel? GetModuleById(string id)
        {
            var entity = _moduleRepository.GetById(id);
            if (entity == null)
                return null;
                
            return new ModuleModel 
            { 
                Id = entity.ModuleId.ToString(), 
                Name = entity.ModuleName,
                PlatformId = entity.PlatformId.ToString()
            };
        }

        public void AddModule(ModuleModel module)
        {
            var entity = new ModuleEntity
            {
                ModuleCode = module.Id,              // 使用Id作为ModuleCode
                ModuleName = module.Name,
                PlatformId = int.Parse(module.PlatformId), // 转换为int
                IsActive = true
            };
            
            _moduleRepository.Add(entity);
        }

        public void UpdateModule(ModuleModel module)
        {
            // 先获取现有实体以保留其他字段
            var existing = _moduleRepository.GetById(module.Id);
            if (existing != null)
            {
                existing.ModuleName = module.Name;
                existing.PlatformId = int.Parse(module.PlatformId);
                _moduleRepository.Update(existing);
            }
        }

        public void DeleteModule(string id)
        {
            _moduleRepository.Delete(id);
        }

        #endregion
    }
}
