using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public class PlatformRepository : IPlatformRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public PlatformRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<PlatformEntity>> GetBySystemIdAsync(int systemId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_platform WHERE system_id = @SystemId AND is_active = 1";
        return await conn.QueryAsync<PlatformEntity>(sql, new { SystemId = systemId });
    }

    public async Task<PlatformEntity?> GetByIdAsync(int platformId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_platform WHERE platform_id = @PlatformId";
        return await conn.QueryFirstOrDefaultAsync<PlatformEntity>(sql, new { PlatformId = platformId });
    }

    public async Task<int> InsertAsync(PlatformEntity platform)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            INSERT INTO tb_platform (system_id, platform_code, platform_name, platform_type, is_active)
            VALUES (@SystemId, @PlatformCode, @PlatformName, @PlatformType, @IsActive);
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<int>(sql, platform);
    }

    public async Task<bool> UpdateAsync(PlatformEntity platform)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            UPDATE tb_platform SET
                platform_name = @PlatformName,
                platform_type = @PlatformType,
                is_active = @IsActive
            WHERE platform_id = @PlatformId";
        var affected = await conn.ExecuteAsync(sql, platform);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int platformId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE tb_platform SET is_active = 0 WHERE platform_id = @PlatformId";
        var affected = await conn.ExecuteAsync(sql, new { PlatformId = platformId });
        return affected > 0;
    }
}
