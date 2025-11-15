using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public class SystemRepository : ISystemRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public SystemRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<SystemEntity>> GetAllAsync()
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_system WHERE is_active = 1 ORDER BY system_code";
        return await conn.QueryAsync<SystemEntity>(sql);
    }

    public async Task<SystemEntity?> GetByIdAsync(int systemId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_system WHERE system_id = @SystemId";
        return await conn.QueryFirstOrDefaultAsync<SystemEntity>(sql, new { SystemId = systemId });
    }

    public async Task<int> InsertAsync(SystemEntity system)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            INSERT INTO tb_system (system_code, system_name, description, location, is_active)
            VALUES (@SystemCode, @SystemName, @Description, @Location, @IsActive);
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<int>(sql, system);
    }

    public async Task<bool> UpdateAsync(SystemEntity system)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            UPDATE tb_system SET
                system_name = @SystemName,
                description = @Description,
                location = @Location,
                is_active = @IsActive
            WHERE system_id = @SystemId";
        var affected = await conn.ExecuteAsync(sql, system);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int systemId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE tb_system SET is_active = 0 WHERE system_id = @SystemId";
        var affected = await conn.ExecuteAsync(sql, new { SystemId = systemId });
        return affected > 0;
    }
}
