using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using GameServer.DTOs;

namespace GameServer.Models;




public class Village {
    [BsonId]
    public ObjectId _id { get; set; }
    public string owner {get; set; }
    public int positionX {get; set; }  public int positionY { get; set; }
    //BUILDINGS
    public List<Building> buildings { get; set; }


    public Village(string pOwner, int pX, int pY){
        //general (dont need to initialise Id)
        owner = pOwner;
        positionX = pX;   positionY = pY;
        //buildings
        buildings = new List<Building> { new Hq(), new Scierie(), new Ferme(), new Mine(), new Entrepot() };
    }

    public VillageDto ToDto() {
        return new VillageDto
        {
            id = this._id.ToString(), owner = this.owner, positionX = this.positionX,  positionY = this.positionY, buildings = this.buildings?.Select(b => b.ToDto()).ToList()
        };
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