using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;

namespace LabTestPlatform.Data.Repositories
{
    public class SystemRepository : ISystemRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public SystemRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<SystemEntity> GetAll()
        {
            using var connection = _connectionFactory.CreateConnection();
            return connection.Query<SystemEntity>("SELECT * FROM Systems");
        }

        public SystemEntity GetById(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return connection.QuerySingleOrDefault<SystemEntity>("SELECT * FROM Systems WHERE Id = @Id", new { Id = id });
        }

        public void Add(SystemEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("INSERT INTO Systems (Id, Name) VALUES (@Id, @Name)", entity);
        }

        public void Update(SystemEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("UPDATE Systems SET Name = @Name WHERE Id = @Id", entity);
        }

        public void Delete(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("DELETE FROM Systems WHERE Id = @Id", new { Id = id });
        }
    }
}