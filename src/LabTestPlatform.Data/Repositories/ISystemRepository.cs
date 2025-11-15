using System.Collections.Generic;
using System.Threading.Tasks;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public interface ISystemRepository
{
    Task<IEnumerable<SystemEntity>> GetAllAsync();
    Task<SystemEntity?> GetByIdAsync(int systemId);
    Task<int> InsertAsync(SystemEntity system);
    Task<bool> UpdateAsync(SystemEntity system);
    Task<bool> DeleteAsync(int systemId);
}
