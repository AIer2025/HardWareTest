using System.Threading.Tasks;
using Dapper;
using LabTestPlatform.Data.Context;

namespace LabTestPlatform.Data.Repositories;

public class WeibullAnalysisRepository : IWeibullAnalysisRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public WeibullAnalysisRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<int> SaveAnalysisResultAsync(int moduleId, double beta, double eta, double mttf)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            INSERT INTO tb_weibull_analysis (module_id, analysis_time, data_count, beta, eta, mttf)
            VALUES (@ModuleId, NOW(), 0, @Beta, @Eta, @Mttf);
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<int>(sql, new { ModuleId = moduleId, Beta = beta, Eta = eta, Mttf = mttf });
    }
}
