using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace LabTestPlatform.Data.Repositories
{
    /// <summary>
    /// 系统数据仓储 - 操作 tb_system 表
    /// </summary>
    public class SystemRepository : ISystemRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SystemRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// 获取所有启用的系统
        /// </summary>
        public IEnumerable<SystemEntity> GetAll()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    system_id as SystemId,
                    system_code as SystemCode,
                    system_name as SystemName,
                    description as Description,
                    location as Location,
                    install_date as InstallDate,
                    warranty_end_date as WarrantyEndDate,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_system 
                WHERE is_active = 1
                ORDER BY system_code";

            return connection.Query<SystemEntity>(sql);
        }

        /// <summary>
        /// 根据ID获取系统
        /// </summary>
        /// <param name="id">系统ID</param>
        public SystemEntity? GetById(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    system_id as SystemId,
                    system_code as SystemCode,
                    system_name as SystemName,
                    description as Description,
                    location as Location,
                    install_date as InstallDate,
                    warranty_end_date as WarrantyEndDate,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_system 
                WHERE system_id = @Id";

            return connection.QuerySingleOrDefault<SystemEntity>(sql, new { Id = id });
        }

        /// <summary>
        /// 根据系统编码获取系统
        /// </summary>
        /// <param name="systemCode">系统编码</param>
        public SystemEntity? GetByCode(string systemCode)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    system_id as SystemId,
                    system_code as SystemCode,
                    system_name as SystemName,
                    description as Description,
                    location as Location,
                    install_date as InstallDate,
                    warranty_end_date as WarrantyEndDate,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_system 
                WHERE system_code = @SystemCode";

            return connection.QuerySingleOrDefault<SystemEntity>(sql, new { SystemCode = systemCode });
        }

        /// <summary>
        /// 添加新系统
        /// </summary>
        public void Add(SystemEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO tb_system 
                (
                    system_code, 
                    system_name, 
                    description, 
                    location, 
                    install_date, 
                    warranty_end_date, 
                    is_active, 
                    created_by
                ) 
                VALUES 
                (
                    @SystemCode, 
                    @SystemName, 
                    @Description, 
                    @Location, 
                    @InstallDate, 
                    @WarrantyEndDate, 
                    @IsActive, 
                    @CreatedBy
                )";

            connection.Execute(sql, entity);
        }

        /// <summary>
        /// 更新系统信息
        /// </summary>
        public void Update(SystemEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE tb_system 
                SET 
                    system_name = @SystemName,
                    description = @Description,
                    location = @Location,
                    install_date = @InstallDate,
                    warranty_end_date = @WarrantyEndDate,
                    is_active = @IsActive,
                    updated_by = @UpdatedBy,
                    update_time = CURRENT_TIMESTAMP
                WHERE system_id = @SystemId";

            connection.Execute(sql, entity);
        }

        /// <summary>
        /// 删除系统 (软删除 - 仅标记为不活跃)
        /// </summary>
        public void Delete(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE tb_system 
                SET 
                    is_active = 0,
                    update_time = CURRENT_TIMESTAMP
                WHERE system_id = @Id";

            connection.Execute(sql, new { Id = id });
        }

        /// <summary>
        /// 物理删除系统 (慎用)
        /// </summary>
        public void HardDelete(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = "DELETE FROM tb_system WHERE system_id = @Id";
            
            connection.Execute(sql, new { Id = id });
        }

        /// <summary>
        /// 检查系统编码是否已存在
        /// </summary>
        public bool CodeExists(string systemCode, int? excludeId = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "SELECT COUNT(*) FROM tb_system WHERE system_code = @SystemCode";
            
            if (excludeId.HasValue)
            {
                sql += " AND system_id != @ExcludeId";
                return connection.ExecuteScalar<int>(sql, new { SystemCode = systemCode, ExcludeId = excludeId }) > 0;
            }
            
            return connection.ExecuteScalar<int>(sql, new { SystemCode = systemCode }) > 0;
        }
    }
}
