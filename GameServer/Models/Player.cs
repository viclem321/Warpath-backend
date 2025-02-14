using GameServer.DTOs;
using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace GameServer.Models;




public class Player
{
    [BsonId]
    public string pseudo;  // index
    public int lockUntil;
    public List<int> allMapVillages;

    public Player(string pPseudo, List<int> pListMapVillages) {
        lockUntil = 0;
        pseudo = pPseudo;
        allMapVillages = pListMapVillages;
    }



    public PlayerDto ToDto() {
        return new PlayerDto
        {
            pseudo = this.pseudo,
            allMapVillages = this.allMapVillages
        };
    }

}