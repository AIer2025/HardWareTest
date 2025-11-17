using LabTestPlatform.Data.Entities;
using System.Collections.Generic;

namespace LabTestPlatform.Data.Repositories
{
    public interface ISystemRepository
    {
        IEnumerable<SystemEntity> GetAll();
        SystemEntity? GetById(string id);
        void Add(SystemEntity entity);
        void Update(SystemEntity entity);
        void Delete(string id);
    }
}