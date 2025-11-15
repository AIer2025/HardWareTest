using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper;
using LabTestPlatform.Models;
using MySql.Data.MySqlClient;

namespace LabTestPlatform.Data
{
    /// <summary>
    /// 测试数据仓储
    /// </summary>
    public class TestDataRepository
    {
        private readonly DatabaseConnection _dbConnection;

        public TestDataRepository(DatabaseConnection dbConnection)
        {
            _dbConnection = dbConnection;
        }

        /// <summary>
        /// 获取所有系统
        /// </summary>
        public async Task<IEnumerable<SystemEntity>> GetAllSystemsAsync()
        {
            using var connection = _dbConnection.GetConnection();
            var sql = "SELECT * FROM System WHERE Status = 'Active' ORDER BY SystemName";
            return await connection.QueryAsync<SystemEntity>(sql);
        }

        /// <summary>
        /// 根据系统ID获取平台
        /// </summary>
        public async Task<IEnumerable<PlatformEntity>> GetPlatformsBySystemIdAsync(int systemId)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                SELECT p.*, s.SystemName 
                FROM Platform p
                INNER JOIN System s ON p.SystemID = s.SystemID
                WHERE p.SystemID = @SystemId AND p.Status = 'Active'
                ORDER BY p.PlatformName";
            return await connection.QueryAsync<PlatformEntity>(sql, new { SystemId = systemId });
        }

        /// <summary>
        /// 根据平台ID获取模组
        /// </summary>
        public async Task<IEnumerable<ModuleEntity>> GetModulesByPlatformIdAsync(int platformId)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                SELECT m.*, p.PlatformName, s.SystemName
                FROM Module m
                INNER JOIN Platform p ON m.PlatformID = p.PlatformID
                INNER JOIN System s ON p.SystemID = s.SystemID
                WHERE m.PlatformID = @PlatformId AND m.Status = 'Active'
                ORDER BY m.ModuleName";
            return await connection.QueryAsync<ModuleEntity>(sql, new { PlatformId = platformId });
        }

        /// <summary>
        /// 获取所有模组
        /// </summary>
        public async Task<IEnumerable<ModuleEntity>> GetAllModulesAsync()
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                SELECT m.*, p.PlatformName, s.SystemName
                FROM Module m
                INNER JOIN Platform p ON m.PlatformID = p.PlatformID
                INNER JOIN System s ON p.SystemID = s.SystemID
                WHERE m.Status = 'Active'
                ORDER BY s.SystemName, p.PlatformName, m.ModuleName";
            return await connection.QueryAsync<ModuleEntity>(sql);
        }

        /// <summary>
        /// 插入测试数据
        /// </summary>
        public async Task<int> InsertTestDataAsync(TestDataEntity testData)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                INSERT INTO TestData 
                (ModuleID, TestTime, TestType, TestValue, Unit, Temperature, Humidity, 
                 Voltage, Current, Pressure, OperatingHours, CycleCount, Status, Operator, ImportSource, Remarks)
                VALUES 
                (@ModuleID, @TestTime, @TestType, @TestValue, @Unit, @Temperature, @Humidity,
                 @Voltage, @Current, @Pressure, @OperatingHours, @CycleCount, @Status, @Operator, @ImportSource, @Remarks);
                SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(sql, testData);
        }

        /// <summary>
        /// 批量插入测试数据
        /// </summary>
        public async Task<int> BatchInsertTestDataAsync(IEnumerable<TestDataEntity> testDataList)
        {
            using var connection = _dbConnection.GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                var sql = @"
                    INSERT INTO TestData 
                    (ModuleID, TestTime, TestType, TestValue, Unit, Temperature, Humidity, 
                     Voltage, Current, Pressure, OperatingHours, CycleCount, Status, Operator, ImportSource, Remarks)
                    VALUES 
                    (@ModuleID, @TestTime, @TestType, @TestValue, @Unit, @Temperature, @Humidity,
                     @Voltage, @Current, @Pressure, @OperatingHours, @CycleCount, @Status, @Operator, @ImportSource, @Remarks)";
                
                var result = await connection.ExecuteAsync(sql, testDataList, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        /// <summary>
        /// 根据模组ID获取测试数据
        /// </summary>
        public async Task<IEnumerable<TestDataEntity>> GetTestDataByModuleIdAsync(int moduleId, DateTime? startDate = null, DateTime? endDate = null)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                SELECT t.*, m.ModuleName, m.ModuleCode
                FROM TestData t
                INNER JOIN Module m ON t.ModuleID = m.ModuleID
                WHERE t.ModuleID = @ModuleId";
            
            if (startDate.HasValue)
                sql += " AND t.TestTime >= @StartDate";
            if (endDate.HasValue)
                sql += " AND t.TestTime <= @EndDate";
            
            sql += " ORDER BY t.TestTime DESC";
            
            return await connection.QueryAsync<TestDataEntity>(sql, new 
            { 
                ModuleId = moduleId, 
                StartDate = startDate, 
                EndDate = endDate 
            });
        }

        /// <summary>
        /// 保存威布尔分析结果
        /// </summary>
        public async Task<int> SaveWeibullAnalysisResultAsync(WeibullAnalysisResultEntity result)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                INSERT INTO WeibullAnalysisResult 
                (ModuleID, AnalysisTime, AnalysisType, DataCount, ShapeParameter, ScaleParameter, LocationParameter,
                 MTBF, ConfidenceLevel, RSquared, EstimationMethod, ReliabilityAt1000h, ReliabilityAt5000h, 
                 ReliabilityAt10000h, B10Life, B50Life, Analyst, Status, Remarks)
                VALUES 
                (@ModuleID, @AnalysisTime, @AnalysisType, @DataCount, @ShapeParameter, @ScaleParameter, @LocationParameter,
                 @MTBF, @ConfidenceLevel, @RSquared, @EstimationMethod, @ReliabilityAt1000h, @ReliabilityAt5000h,
                 @ReliabilityAt10000h, @B10Life, @B50Life, @Analyst, @Status, @Remarks);
                SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(sql, result);
        }

        /// <summary>
        /// 获取威布尔分析结果
        /// </summary>
        public async Task<IEnumerable<WeibullAnalysisResultEntity>> GetWeibullResultsByModuleIdAsync(int moduleId)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                SELECT w.*, m.ModuleName, m.ModuleCode
                FROM WeibullAnalysisResult w
                INNER JOIN Module m ON w.ModuleID = m.ModuleID
                WHERE w.ModuleID = @ModuleId
                ORDER BY w.AnalysisTime DESC";
            return await connection.QueryAsync<WeibullAnalysisResultEntity>(sql, new { ModuleId = moduleId });
        }

        /// <summary>
        /// 获取威布尔分析结果详情
        /// </summary>
        public async Task<WeibullAnalysisResultEntity?> GetWeibullResultByIdAsync(int resultId)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                SELECT w.*, m.ModuleName, m.ModuleCode
                FROM WeibullAnalysisResult w
                INNER JOIN Module m ON w.ModuleID = m.ModuleID
                WHERE w.ResultID = @ResultId";
            return await connection.QueryFirstOrDefaultAsync<WeibullAnalysisResultEntity>(sql, new { ResultId = resultId });
        }

        /// <summary>
        /// 保存分析报告
        /// </summary>
        public async Task<int> SaveAnalysisReportAsync(AnalysisReportEntity report)
        {
            using var connection = _dbConnection.GetConnection();
            var sql = @"
                INSERT INTO AnalysisReport 
                (ResultID, ReportName, ReportType, ReportFormat, FilePath, FileSize, GenerateTime, Generator, Status, Remarks)
                VALUES 
                (@ResultID, @ReportName, @ReportType, @ReportFormat, @FilePath, @FileSize, @GenerateTime, @Generator, @Status, @Remarks);
                SELECT LAST_INSERT_ID();";
            return await connection.ExecuteScalarAsync<int>(sql, report);
        }
    }
}
