using MongoDB.Driver;
using MongoDB.Bson;
using System.Threading.Tasks;
using System.Collections.Generic;
using BookingMS.Domain.Entities;
using BookingMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;

public class MongoAuditService : IAuditService
{
    private readonly IMongoCollection<AuditLog> _collection;

    public MongoAuditService(IMongoClient client, IConfiguration config)
    {
        var database = client.GetDatabase(config["MongoDb:DatabaseName"]);
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
