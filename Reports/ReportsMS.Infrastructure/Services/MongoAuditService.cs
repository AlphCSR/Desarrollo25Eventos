using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
using ReportsMS.Domain.Entities;
using ReportsMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace ReportsMS.Infrastructure.Services
{
    public class MongoAuditService : IAuditService
    {
        private readonly IMongoCollection<AuditLog> _collection;

        public MongoAuditService(IMongoClient client, IConfiguration config)
        {
            var dbName = config["MongoDb:DatabaseName"] ?? "AuditingDB"; 
            var database = client.GetDatabase(dbName);
            _collection = database.GetCollection<AuditLog>("AuditLogs");
        }

        public async Task LogAsync(AuditLog log)
        {
            await _collection.InsertOneAsync(log);
        }

        public async Task<IEnumerable<AuditLog>> GetLatestLogsAsync(int count)
        {
            return await _collection.Find(new BsonDocument())
                .SortByDescending(x => x.Timestamp)
                .Limit(count)
                .ToListAsync();
        }
    }
}
