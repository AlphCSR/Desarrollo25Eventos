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
    /// <summary>
    /// Inicializa el servicio de auditoría con la colección de auditoría.
    /// </summary>
    private readonly IMongoCollection<AuditLog> _collection;

    static MongoAuditService()
    {
        try
        {
            BsonSerializer.RegisterSerializer(new GuidSerializer(GuidRepresentation.Standard));
        }
        catch (BsonSerializationException) {}
    }

    /// <summary>
    /// Inicializa el servicio de auditoría con la colección de auditoría.
    /// </summary>
    public MongoAuditService(IMongoClient client, IConfiguration config)
    {
        var database = client.GetDatabase(config["MongoDb:DatabaseName"]);
        _collection = database.GetCollection<AuditLog>("AuditLogs");
    }

    /// <summary>
    /// Registra un nuevo registro de auditoría en la base de datos.
    /// </summary>
    /// <param name="log">El registro de auditoría a registrar.</param>
    public async Task LogAsync(AuditLog log)
    {
        await _collection.InsertOneAsync(log);
    }
}