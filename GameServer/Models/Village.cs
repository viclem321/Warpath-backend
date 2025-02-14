using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using GameServer.DTOs;

namespace GameServer.Models;




public class Village {
    [BsonId]
    public ObjectId _id;  // dont initialize or manage this one
    public int lockUntil;
    public List<Building> buildings;


    public Village(){
        lockUntil = 0;
        buildings = new List<Building> { new Hq(), new Scierie(), new Ferme(), new Mine(), new Entrepot() };
    }

    public VillageDto ToDto() {
        return new VillageDto { buildings = this.buildings?.Select(b => b.ToDto()).ToList() };
    }
}





/*

ALL BUILDINGS INDEX      
    [0] = Hq
    [1] = Scierie
    [2] = Ferme
    [3] = Mine
    [4] = Entrepot

*/