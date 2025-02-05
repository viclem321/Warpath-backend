using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Models;




public class User
{
    [BsonId]
    public ObjectId Id { get; set; }

    [BsonElement("username")]
    public string? Username { get; set; }

    [BsonElement("passwordHash")]
    public string? PasswordHash { get; set; }

    public ObjectId? playerId { get; set; }
}