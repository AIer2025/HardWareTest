using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace LabTestPlatform.Data.Repositories
{
    /// <summary>
    /// 平台数据仓储 - 操作 tb_platform 表
    /// </summary>
    public class PlatformRepository : IPlatformRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PlatformRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// 根据系统ID获取平台列表
        /// </summary>
        public IEnumerable<PlatformEntity> GetBySystemId(string systemId)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    platform_id as PlatformId,
                    system_id as SystemId,
                    platform_code as PlatformCode,
                    platform_name as PlatformName,
                    platform_type as PlatformType,
                    serial_number as SerialNumber,
                    description as Description,
                    install_date as InstallDate,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_platform 
                WHERE system_id = @SystemId 
                  AND is_active = 1
                ORDER BY platform_code";

            return connection.Query<PlatformEntity>(sql, new { SystemId = systemId });
        }

        /// <summary>
        /// 根据ID获取平台
        /// </summary>
        public PlatformEntity? GetById(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    platform_id as PlatformId,
                    system_id as SystemId,
                    platform_code as PlatformCode,
                    platform_name as PlatformName,
                    platform_type as PlatformType,
                    serial_number as SerialNumber,
                    description as Description,
                    install_date as InstallDate,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_platform 
                WHERE platform_id = @Id";

            return connection.QuerySingleOrDefault<PlatformEntity>(sql, new { Id = id });
        }

        /// <summary>
        /// 根据平台编码获取平台
        /// </summary>
        public PlatformEntity? GetByCode(string platformCode)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    platform_id as PlatformId,
                    system_id as SystemId,
                    platform_code as PlatformCode,
                    platform_name as PlatformName,
                    platform_type as PlatformType,
                    serial_number as SerialNumber,
                    description as Description,
                    install_date as InstallDate,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_platform 
                WHERE platform_code = @PlatformCode";

            return connection.QuerySingleOrDefault<PlatformEntity>(sql, new { PlatformCode = platformCode });
        }

        /// <summary>
        /// 获取所有启用的平台
        /// </summary>
        public IEnumerable<PlatformEntity> GetAll()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    platform_id as PlatformId,
                    system_id as SystemId,
                    platform_code as PlatformCode,
                    platform_name as PlatformName,
                    platform_type as PlatformType,
                    serial_number as SerialNumber,
                    description as Description,
                    install_date as InstallDate,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_platform 
                WHERE is_active = 1
                ORDER BY system_id, platform_code";

            return connection.Query<PlatformEntity>(sql);
        }

        /// <summary>
        /// 添加新平台
        /// </summary>
        public void Add(PlatformEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO tb_platform 
                (
                    system_id,
                    platform_code,
                    platform_name,
                    platform_type,
                    serial_number,
                    description,
                    install_date,
                    is_active,
                    created_by
                ) 
                VALUES 
                (
                    @SystemId,
                    @PlatformCode,
                    @PlatformName,
                    @PlatformType,
                    @SerialNumber,
                    @Description,
                    @InstallDate,
                    @IsActive,
                    @CreatedBy
                )";

            connection.Execute(sql, entity);
        }

        /// <summary>
        /// 更新平台信息
        /// </summary>
        public void Update(PlatformEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE tb_platform 
                SET 
                    system_id = @SystemId,
                    platform_name = @PlatformName,
                    platform_type = @PlatformType,
                    serial_number = @SerialNumber,
                    description = @Description,
                    install_date = @InstallDate,
                    is_active = @IsActive,
                    updated_by = @UpdatedBy,
                    update_time = CURRENT_TIMESTAMP
                WHERE platform_id = @PlatformId";

            connection.Execute(sql, entity);
        }

        /// <summary>
        /// 删除平台 (软删除)
        /// </summary>
        public void Delete(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE tb_platform 
                SET 
                    is_active = 0,
                    update_time = CURRENT_TIMESTAMP
                WHERE platform_id = @Id";

            connection.Execute(sql, new { Id = id });
        }

        /// <summary>
        /// 检查平台编码是否已存在
        /// </summary>
        public bool CodeExists(string platformCode, int? excludeId = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "SELECT COUNT(*) FROM tb_platform WHERE platform_code = @PlatformCode";
            
            if (excludeId.HasValue)
            {
                sql += " AND platform_id != @ExcludeId";
                return connection.ExecuteScalar<int>(sql, new { PlatformCode = platformCode, ExcludeId = excludeId }) > 0;
            }
            
            return connection.ExecuteScalar<int>(sql, new { PlatformCode = platformCode }) > 0;
        }

        /// <summary>
        /// 获取指定系统下的平台数量
        /// </summary>
        public int GetCountBySystemId(int systemId)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT COUNT(*) 
                FROM tb_platform 
                WHERE system_id = @SystemId 
                  AND is_active = 1";

            return connection.ExecuteScalar<int>(sql, new { SystemId = systemId });
        }
    }
}
