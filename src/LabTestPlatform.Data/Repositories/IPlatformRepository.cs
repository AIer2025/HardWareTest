using LabTestPlatform.Data.Entities;
using System.Collections.Generic;

namespace LabTestPlatform.Data.Repositories
{
    public interface IPlatformRepository
    {
        IEnumerable<PlatformEntity> GetBySystemId(string systemId);
        PlatformEntity GetById(string id);
        void Add(PlatformEntity entity);
        void Update(PlatformEntity entity);
        void Delete(string id);
    }
}