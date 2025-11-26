using MongoDB.Driver;
using MongoDB.Bson;
using MongoDB.Bson.Serialization;
using MongoDB.Bson.Serialization.Serializers;
using System.Threading.Tasks;
using UsersMS.Domain.Entities;
using UsersMS.Application.Interfaces;
using Microsoft.Extensions.Configuration;

public class MongoAuditService : IAuditService
{
    private readonly IMongoCollection<AuditLog> _collection;

    static MongoAuditService()
    {
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch (BsonSerializationException)
        {
            // Already registered, ignore
        }
    }

    public MongoAuditService(IMongoClient client, IConfiguration config)
    {
        var database = client.GetDatabase(config["MongoDb:DatabaseName"]);
        _collection = database.GetCollection<AuditLog>("AuditLogs");
    }

    public async Task LogAsync(AuditLog log)
    {
        await _collection.InsertOneAsync(log);
    }
}