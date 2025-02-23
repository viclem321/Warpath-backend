using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using GameServer.DTOs;

namespace GameServer.Models;




public class Village {
    [BsonId]
    public ObjectId _id;  // dont initialize or manage this one
    public int lockUntil;
    public List<ObjectId> buildings;
    public UpgradeAction upgradeAction1;


    public Village(){
        lockUntil = 0;
        buildings = new List<ObjectId>();
        upgradeAction1 = new UpgradeAction();
    }
}




public class UpgradeAction {
    public bool active = false;
    public BuildingType buildingType;
    public DateTime endUpgradeAt;

    public bool isDisponible() {
        return !active;
    }

    public bool newActionBegin(BuildingType pBuildingType, DateTime endAt) {
        if(isDisponible()) {
            buildingType = pBuildingType; endUpgradeAt = endAt; active = true;
            return true;
        }
        return false;
    }

    public bool endAction() {
        if(!isDisponible()) { active = false; return true; }
        return false;
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