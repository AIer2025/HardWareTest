using System.Data;
using System.Threading.Tasks;

namespace LabTestPlatform.Data.Context;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
    Task<IDbConnection> CreateConnectionAsync();
}
