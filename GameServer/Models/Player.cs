using GameServer.DTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Models;




public class Player
{
    [BsonId]
    public ObjectId _id { get; set; }
    public string pseudo { get; set; }
    public List<ObjectId> allVillages { get; set; }

    public Player(string pPseudo, List<ObjectId>? pListVillages) {
        pseudo = pPseudo;
        allVillages = pListVillages ?? new List<ObjectId>();
    }



    public PlayerDto ToDto() {
        return new PlayerDto
        {
            id = this._id.ToString(),
            pseudo = this.pseudo,
            allVillages = this.allVillages?.Select(v => v.ToString()).ToList()
        };
    }

}