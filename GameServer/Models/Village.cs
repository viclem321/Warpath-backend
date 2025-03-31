using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Driver;
using Warpath.Shared.Catalogue;
using Warpath.Shared.DTOs;

namespace GameServer.Models;




public class Village {
    [BsonId]
    public ObjectId _id;  // dont initialize or manage this one
    public int lockUntil;
    public string owner;
    public List<ObjectId> buildings;
    public UpgradeAction upgradeAction1;



    public Village(){
        lockUntil = 0; owner = "";
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

    public UpgradeActionDto ToDto() {
        return new UpgradeActionDto { active = this.active, buildingType = this.buildingType, endUpgradeAt = this.endUpgradeAt };
    }
}




public class RapportFight {
    [BsonId]
    public ObjectId _id;
    public string attacker; public string defenser;
    public DateTime fightAt;
    public int nSoldatsAttacker; public int nSoldatsDefenser;
    public int nSoldatsSurvivedAttacker; public int nSoldatsSurvivedDefenser;
    public string winner;

    public RapportFight() {
        attacker = ""; defenser = ""; fightAt = DateTime.UtcNow; nSoldatsAttacker = 0; nSoldatsDefenser = 0; nSoldatsSurvivedAttacker = 0; nSoldatsSurvivedDefenser = 0; winner = "";
    }

    public RapportFight(string pAttacker, string pDefenser, DateTime pFightAt, int pNSoldatsAttack, int pNSoldatsDefenser, int pNSoldatsSurvivedAttacker, int pNSoldatsSurvivedDefenser, string pWinner) {
        attacker = pAttacker; defenser = pDefenser; fightAt = pFightAt; nSoldatsAttacker = pNSoldatsAttack; nSoldatsDefenser = pNSoldatsDefenser; nSoldatsSurvivedAttacker = pNSoldatsSurvivedAttacker; nSoldatsSurvivedDefenser = pNSoldatsSurvivedDefenser; winner = pWinner;
    }

    public RapportFightDto ToDto() {
        return new RapportFightDto { attacker = this.attacker, defenser = this.defenser, fightAt = this.fightAt, nSoldatsAttacker = this.nSoldatsAttacker, nSoldatsDefenser = this.nSoldatsDefenser, nSoldatsSurvivedAttacker = this.nSoldatsSurvivedAttacker, nSoldatsSurvivedDefenser = this.nSoldatsSurvivedDefenser, winner = this.winner };
    }

    
}
