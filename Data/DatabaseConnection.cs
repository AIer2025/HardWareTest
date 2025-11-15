using System;
using System.Data;
using MySql.Data.MySqlClient;
using Microsoft.Extensions.Configuration;

namespace LabTestPlatform.Data
{
    /// <summary>
    /// 数据库连接管理
    /// </summary>
    public class DatabaseConnection : IDisposable
    {
        private readonly string _connectionString;
        private MySqlConnection? _connection;

        public DatabaseConnection(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("MySQL") 
                ?? throw new InvalidOperationException("MySQL连接字符串未配置");
        }

        public DatabaseConnection(string connectionString)
        {
            _connectionString = connectionString;
        }

        /// <summary>
        /// 获取数据库连接
        /// </summary>
        public MySqlConnection GetConnection()
        {
            if (_connection == null || _connection.State != ConnectionState.Open)
            {
                _connection = new MySqlConnection(_connectionString);
                _connection.Open();
            }
            return _connection;
        }

        /// <summary>
        /// 测试连接
        /// </summary>
        public bool TestConnection()
        {
            try
            {
                using var connection = new MySqlConnection(_connectionString);
                connection.Open();
                return true;
            }
            catch
            {
                return false;
            }
        }

        /// <summary>
        /// 执行事务
        /// </summary>
        public T ExecuteTransaction<T>(Func<MySqlConnection, MySqlTransaction, T> action)
        {
            using var connection = GetConnection();
            using var transaction = connection.BeginTransaction();
            try
            {
                var result = action(connection, transaction);
                transaction.Commit();
                return result;
            }
            catch
            {
                transaction.Rollback();
                throw;
            }
        }

        public void Dispose()
        {
            if (_connection != null)
            {
                if (_connection.State == ConnectionState.Open)
                {
                    _connection.Close();
                }
                _connection.Dispose();
                _connection = null;
            }
        }
    }
}
