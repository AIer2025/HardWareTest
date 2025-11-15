using System.Collections.Generic;
using System.Threading.Tasks;
using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;

namespace LabTestPlatform.Data.Repositories;

public class TestDataRepository : ITestDataRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public TestDataRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory;
    }

    public async Task<IEnumerable<TestDataEntity>> GetByModuleIdAsync(int moduleId)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = "SELECT * FROM tb_test_data WHERE module_id = @ModuleId ORDER BY test_time DESC";
        return await conn.QueryAsync<TestDataEntity>(sql, new { ModuleId = moduleId });
    }

    public async Task<IEnumerable<TestDataEntity>> GetByModuleIdAndTestTypeAsync(int moduleId, string testType)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            SELECT * FROM tb_test_data 
            WHERE module_id = @ModuleId AND test_type = @TestType
            AND failure_time IS NOT NULL
            ORDER BY failure_time ASC";
        return await conn.QueryAsync<TestDataEntity>(sql, new { ModuleId = moduleId, TestType = testType });
    }

    public async Task<long> InsertAsync(TestDataEntity testData)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            INSERT INTO tb_test_data (
                module_id, test_time, test_value, test_unit, test_type,
                failure_time, failure_mode, is_censored, temperature,
                humidity, operator, remarks
            ) VALUES (
                @ModuleId, @TestTime, @TestValue, @TestUnit, @TestType,
                @FailureTime, @FailureMode, @IsCensored, @Temperature,
                @Humidity, @Operator, @Remarks
            );
            SELECT LAST_INSERT_ID();";
        return await conn.ExecuteScalarAsync<long>(sql, testData);
    }

    public async Task<int> BatchInsertAsync(IEnumerable<TestDataEntity> testDataList)
    {
        using var conn = await _connectionFactory.CreateConnectionAsync();
        var sql = @"
            INSERT INTO tb_test_data (
                module_id, test_time, test_value, test_unit, test_type,
                failure_time, failure_mode, is_censored, temperature,
                humidity, operator, remarks
            ) VALUES (
                @ModuleId, @TestTime, @TestValue, @TestUnit, @TestType,
                @FailureTime, @FailureMode, @IsCensored, @Temperature,
                @Humidity, @Operator, @Remarks
            )";
        return await conn.ExecuteAsync(sql, testDataList);
    }
}
