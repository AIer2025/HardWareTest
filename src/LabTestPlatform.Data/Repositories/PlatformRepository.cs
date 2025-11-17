using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;

namespace LabTestPlatform.Data.Repositories
{
    public class PlatformRepository : IPlatformRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public PlatformRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<PlatformEntity> GetBySystemId(string systemId)
        {
            using var connection = _connectionFactory.CreateConnection();
            return connection.Query<PlatformEntity>("SELECT * FROM Platforms WHERE SystemId = @SystemId", new { SystemId = systemId });
        }

        public PlatformEntity? GetById(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return connection.QuerySingleOrDefault<PlatformEntity>("SELECT * FROM Platforms WHERE Id = @Id", new { Id = id });
        }

        public void Add(PlatformEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("INSERT INTO Platforms (Id, Name, SystemId) VALUES (@Id, @Name, @SystemId)", entity);
        }

        public void Update(PlatformEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("UPDATE Platforms SET Name = @Name WHERE Id = @Id", entity);
        }

        public void Delete(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("DELETE FROM Platforms WHERE Id = @Id", new { Id = id });
        }
    }
}