using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Models;




public class User
{
    [BsonId]
    public string username;  // index
    public int lockUntil;
    public string passwordHash;
    public string player;

    public User(string pUsername, string pPasswordHash, string pPlayer) {
        lockUntil = 0;
        username = pUsername;  passwordHash = pPasswordHash;
        player = pPlayer;
    }
}