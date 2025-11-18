using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;
using System.Linq;

namespace LabTestPlatform.Data.Repositories
{
    /// <summary>
    /// 模组数据仓储 - 操作 tb_module 表
    /// </summary>
    public class ModuleRepository : IModuleRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ModuleRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        /// <summary>
        /// 根据平台ID获取模组列表
        /// </summary>
        public IEnumerable<ModuleEntity> GetByPlatformId(string platformId)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    module_id as ModuleId,
                    platform_id as PlatformId,
                    module_code as ModuleCode,
                    module_name as ModuleName,
                    module_type as ModuleType,
                    manufacturer as Manufacturer,
                    model_number as ModelNumber,
                    serial_number as SerialNumber,
                    manufacture_date as ManufactureDate,
                    rated_life as RatedLife,
                    description as Description,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_module 
                WHERE platform_id = @PlatformId 
                  AND is_active = 1
                ORDER BY module_code";

            return connection.Query<ModuleEntity>(sql, new { PlatformId = platformId });
        }

        /// <summary>
        /// 根据ID获取模组
        /// </summary>
        public ModuleEntity? GetById(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    module_id as ModuleId,
                    platform_id as PlatformId,
                    module_code as ModuleCode,
                    module_name as ModuleName,
                    module_type as ModuleType,
                    manufacturer as Manufacturer,
                    model_number as ModelNumber,
                    serial_number as SerialNumber,
                    manufacture_date as ManufactureDate,
                    rated_life as RatedLife,
                    description as Description,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_module 
                WHERE module_id = @Id";

            return connection.QuerySingleOrDefault<ModuleEntity>(sql, new { Id = id });
        }

        /// <summary>
        /// 根据模组编码获取模组
        /// </summary>
        public ModuleEntity? GetByCode(string moduleCode)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    module_id as ModuleId,
                    platform_id as PlatformId,
                    module_code as ModuleCode,
                    module_name as ModuleName,
                    module_type as ModuleType,
                    manufacturer as Manufacturer,
                    model_number as ModelNumber,
                    serial_number as SerialNumber,
                    manufacture_date as ManufactureDate,
                    rated_life as RatedLife,
                    description as Description,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_module 
                WHERE module_code = @ModuleCode";

            return connection.QuerySingleOrDefault<ModuleEntity>(sql, new { ModuleCode = moduleCode });
        }

        /// <summary>
        /// 获取所有启用的模组
        /// </summary>
        public IEnumerable<ModuleEntity> GetAll()
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    module_id as ModuleId,
                    platform_id as PlatformId,
                    module_code as ModuleCode,
                    module_name as ModuleName,
                    module_type as ModuleType,
                    manufacturer as Manufacturer,
                    model_number as ModelNumber,
                    serial_number as SerialNumber,
                    manufacture_date as ManufactureDate,
                    rated_life as RatedLife,
                    description as Description,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_module 
                WHERE is_active = 1
                ORDER BY platform_id, module_code";

            return connection.Query<ModuleEntity>(sql);
        }

        /// <summary>
        /// 根据模组类型获取模组列表
        /// </summary>
        public IEnumerable<ModuleEntity> GetByType(string moduleType)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    module_id as ModuleId,
                    platform_id as PlatformId,
                    module_code as ModuleCode,
                    module_name as ModuleName,
                    module_type as ModuleType,
                    manufacturer as Manufacturer,
                    model_number as ModelNumber,
                    serial_number as SerialNumber,
                    manufacture_date as ManufactureDate,
                    rated_life as RatedLife,
                    description as Description,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_module 
                WHERE module_type = @ModuleType 
                  AND is_active = 1
                ORDER BY module_code";

            return connection.Query<ModuleEntity>(sql, new { ModuleType = moduleType });
        }

        /// <summary>
        /// 添加新模组
        /// </summary>
        public void Add(ModuleEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                INSERT INTO tb_module 
                (
                    platform_id,
                    module_code,
                    module_name,
                    module_type,
                    manufacturer,
                    model_number,
                    serial_number,
                    manufacture_date,
                    rated_life,
                    description,
                    is_active,
                    created_by
                ) 
                VALUES 
                (
                    @PlatformId,
                    @ModuleCode,
                    @ModuleName,
                    @ModuleType,
                    @Manufacturer,
                    @ModelNumber,
                    @SerialNumber,
                    @ManufactureDate,
                    @RatedLife,
                    @Description,
                    @IsActive,
                    @CreatedBy
                )";

            connection.Execute(sql, entity);
        }

        /// <summary>
        /// 更新模组信息
        /// </summary>
        public void Update(ModuleEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE tb_module 
                SET 
                    platform_id = @PlatformId,
                    module_name = @ModuleName,
                    module_type = @ModuleType,
                    manufacturer = @Manufacturer,
                    model_number = @ModelNumber,
                    serial_number = @SerialNumber,
                    manufacture_date = @ManufactureDate,
                    rated_life = @RatedLife,
                    description = @Description,
                    is_active = @IsActive,
                    updated_by = @UpdatedBy,
                    update_time = CURRENT_TIMESTAMP
                WHERE module_id = @ModuleId";

            connection.Execute(sql, entity);
        }

        /// <summary>
        /// 删除模组 (软删除)
        /// </summary>
        public void Delete(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                UPDATE tb_module 
                SET 
                    is_active = 0,
                    update_time = CURRENT_TIMESTAMP
                WHERE module_id = @Id";

            connection.Execute(sql, new { Id = id });
        }

        /// <summary>
        /// 检查模组编码是否已存在
        /// </summary>
        public bool CodeExists(string moduleCode, int? excludeId = null)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            var sql = "SELECT COUNT(*) FROM tb_module WHERE module_code = @ModuleCode";
            
            if (excludeId.HasValue)
            {
                sql += " AND module_id != @ExcludeId";
                return connection.ExecuteScalar<int>(sql, new { ModuleCode = moduleCode, ExcludeId = excludeId }) > 0;
            }
            
            return connection.ExecuteScalar<int>(sql, new { ModuleCode = moduleCode }) > 0;
        }

        /// <summary>
        /// 获取指定平台下的模组数量
        /// </summary>
        public int GetCountByPlatformId(int platformId)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT COUNT(*) 
                FROM tb_module 
                WHERE platform_id = @PlatformId 
                  AND is_active = 1";

            return connection.ExecuteScalar<int>(sql, new { PlatformId = platformId });
        }

        /// <summary>
        /// 根据制造商获取模组列表
        /// </summary>
        public IEnumerable<ModuleEntity> GetByManufacturer(string manufacturer)
        {
            using var connection = _connectionFactory.CreateConnection();
            
            const string sql = @"
                SELECT 
                    module_id as ModuleId,
                    platform_id as PlatformId,
                    module_code as ModuleCode,
                    module_name as ModuleName,
                    module_type as ModuleType,
                    manufacturer as Manufacturer,
                    model_number as ModelNumber,
                    serial_number as SerialNumber,
                    manufacture_date as ManufactureDate,
                    rated_life as RatedLife,
                    description as Description,
                    create_time as CreateTime,
                    update_time as UpdateTime,
                    is_active as IsActive,
                    created_by as CreatedBy,
                    updated_by as UpdatedBy
                FROM tb_module 
                WHERE manufacturer = @Manufacturer 
                  AND is_active = 1
                ORDER BY module_code";

            return connection.Query<ModuleEntity>(sql, new { Manufacturer = manufacturer });
        }
    }
}
