using LabTestPlatform.Data.Entities;
using System.Collections.Generic;

namespace LabTestPlatform.Data.Repositories
{
    public interface IModuleRepository
    {
        IEnumerable<ModuleEntity> GetByPlatformId(string platformId);
        ModuleEntity? GetById(string id);
        void Add(ModuleEntity entity);
        void Update(ModuleEntity entity);
        void Delete(string id);
    }
}