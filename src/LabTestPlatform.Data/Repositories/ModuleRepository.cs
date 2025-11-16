using Dapper;
using LabTestPlatform.Data.Context;
using LabTestPlatform.Data.Entities;
using System.Collections.Generic;

namespace LabTestPlatform.Data.Repositories
{
    public class ModuleRepository : IModuleRepository
    {
        private readonly IDbConnectionFactory _connectionFactory;

        public ModuleRepository(IDbConnectionFactory connectionFactory)
        {
            _connectionFactory = connectionFactory;
        }

        public IEnumerable<ModuleEntity> GetByPlatformId(string platformId)
        {
            using var connection = _connectionFactory.CreateConnection();
            return connection.Query<ModuleEntity>("SELECT * FROM Modules WHERE PlatformId = @PlatformId", new { PlatformId = platformId });
        }

        public ModuleEntity GetById(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            return connection.QuerySingleOrDefault<ModuleEntity>("SELECT * FROM Modules WHERE Id = @Id", new { Id = id });
        }

        public void Add(ModuleEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("INSERT INTO Modules (Id, Name, PlatformId) VALUES (@Id, @Name, @PlatformId)", entity);
        }

        public void Update(ModuleEntity entity)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("UPDATE Modules SET Name = @Name WHERE Id = @Id", entity);
        }

        public void Delete(string id)
        {
            using var connection = _connectionFactory.CreateConnection();
            connection.Execute("DELETE FROM Modules WHERE Id = @Id", new { Id = id });
        }
    }
}