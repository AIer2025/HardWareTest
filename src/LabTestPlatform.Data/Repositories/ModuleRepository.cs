using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public class ModuleRepository : IModuleRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ModuleRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<ModuleEntity>> GetAllAsync()
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_module WHERE is_active = 1 ORDER BY module_code";
        return await conn.QueryAsync<ModuleEntity>(sql);
    }

    public async Task<ModuleEntity?> GetByIdAsync(int moduleId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_module WHERE module_id = @ModuleId";
        return await conn.QueryFirstOrDefaultAsync<ModuleEntity>(sql, new { ModuleId = moduleId });
    }

    public async Task<IEnumerable<ModuleEntity>> GetByPlatformIdAsync(int platformId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_module WHERE platform_id = @PlatformId AND is_active = 1";
        return await conn.QueryAsync<ModuleEntity>(sql, new { PlatformId = platformId });
    }

    public async Task<int> InsertAsync(ModuleEntity module)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            INSERT INTO tb_module (platform_id, module_code, module_name, module_type, manufacturer, is_active)
            VALUES (@PlatformId, @ModuleCode, @ModuleName, @ModuleType, @Manufacturer, @IsActive);
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<int>(sql, module);
    }

    public async Task<bool> UpdateAsync(ModuleEntity module)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            UPDATE tb_module SET
                module_name = @ModuleName,
                module_type = @ModuleType,
                manufacturer = @Manufacturer,
                is_active = @IsActive
            WHERE module_id = @ModuleId";
        var affected = await conn.ExecuteAsync(sql, module);
        return affected > 0;
    }

    public async Task<bool> DeleteAsync(int moduleId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "UPDATE tb_module SET is_active = 0 WHERE module_id = @ModuleId";
        var affected = await conn.ExecuteAsync(sql, new { ModuleId = moduleId });
        return affected > 0;
    }
}
