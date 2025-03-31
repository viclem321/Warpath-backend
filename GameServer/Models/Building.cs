using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using MongoDB.Bson;
using Warpath.Shared.Catalogue;
using Warpath.Shared.DTOs;

namespace GameServer.Models;


// Here, when add or remove a Building, dont forget to actualize BsonKnownTypes, BsonDiscriminator and CreateVillage


[BsonDiscriminator(Required = true)]
[ BsonKnownTypes( typeof(Hq), typeof(ResourceBuilding), typeof(Entrepot), typeof(CampMilitaire), typeof(Caserne) ) ] 
public abstract class Building {
    public ObjectId _id { get; set; }
    public int level;
    public bool isInConstruction;

    public Building() { level = 0; isInConstruction = false; }


    public (bool, JObject, JObject) GetIdentityForUpgrade() {
        JObject? buildingCata; JObject? requiredUpgrade = null;
        buildingCata = CatalogueGlobal.buildings[this.GetType().Name];
        if(this.level + 1 <= (int)(buildingCata["MaxLevel"] ?? -1) ) { requiredUpgrade = (JObject?)buildingCata?["Levels"]?[this.level + 1]?["Required"]; }

        if(buildingCata != null && requiredUpgrade != null) { return (true, buildingCata, requiredUpgrade); }
        else { return (false, new JObject(), new JObject()); }
    }
    
    public (bool, DateTime) StartUpgrade() {
        try {
            JObject building = CatalogueGlobal.buildings[this.GetType().Name];

            if( isInConstruction == false && level + 1 <= ((int?)building?["MaxLevel"] ?? 0) ) {
                int timeToBuildInSecondes = (int)(building?["Levels"]?[level + 1]?["TimeToBuild"] ?? int.MaxValue); if(timeToBuildInSecondes != int.MaxValue) {
                    DateTime endAt = DateTime.UtcNow; endAt = endAt.AddSeconds(timeToBuildInSecondes);
                    isInConstruction = true;
                    return (true, endAt);
                }
            }
        } catch { Console.WriteLine("Erreur critique dans Building StartUpgrade."); }
        return (false, new DateTime());
    }
    public bool EndUpgrade(DateTime endAt) {
        try {
            JObject building = CatalogueGlobal.buildings[this.GetType().Name];
            DateTime now = DateTime.UtcNow;

            if( isInConstruction == true && level + 1 <= ((int?)building?["MaxLevel"] ?? 0) && now >= endAt ) {
                level += 1; isInConstruction = false;
                return true;
            }
        } catch { Console.WriteLine("Erreur critique dans Building EndUpgrade."); }
        return false;
    }
    public abstract BuildingDto ToDto();
}



[BsonDiscriminator("Hq")]
public class Hq : Building {

    public Hq() { }
    public override BuildingDto ToDto()
    {
        return new HqDTO { buildingType = this.GetType().Name,  level = this.level, isInConstruction = this.isInConstruction };
    }
}



[BsonDiscriminator(Required = true)]
[BsonKnownTypes(typeof(Scierie), typeof(Ferme), typeof(Mine))]
public abstract class ResourceBuilding : Building {
    public int quantity { get; set; }
    public DateTime lastHarvest  { get; set; }

    public ResourceBuilding() { quantity = 0; lastHarvest = DateTime.UtcNow; }

    public override BuildingDto ToDto()
    {
        return new ResourceBuildingDto { buildingType = this.GetType().Name, level = this.level, isInConstruction = this.isInConstruction,  quantity = this.quantity, lastHarvest = this.lastHarvest };
    }
    public static ResourceType? FindResourceTypeWithBuildingType(BuildingType buildingType) {
        if(buildingType == BuildingType.Scierie) { return ResourceType.Wood;}
        else if(buildingType == BuildingType.Ferme) { return ResourceType.Food;}
        else if(buildingType == BuildingType.Mine) { return ResourceType.Oil;}
        else return null;
    }
    
}
[BsonDiscriminator("Scierie")]
public class Scierie : ResourceBuilding {
    public override BuildingDto ToDto()
    {
        return new ScierieDTO { buildingType = this.GetType().Name, level = this.level, isInConstruction = this.isInConstruction,  quantity = this.quantity, lastHarvest = this.lastHarvest };
    }
}
[BsonDiscriminator("Ferme")]
public class Ferme : ResourceBuilding {
    public override BuildingDto ToDto()
    {
        return new FermeDTO { buildingType = this.GetType().Name, level = this.level, isInConstruction = this.isInConstruction,  quantity = this.quantity, lastHarvest = this.lastHarvest };
    }
}
[BsonDiscriminator("Mine")]
public class Mine : ResourceBuilding {
    public override BuildingDto ToDto()
    {
        return new MineDTO { buildingType = this.GetType().Name, level = this.level, isInConstruction = this.isInConstruction,  quantity = this.quantity, lastHarvest = this.lastHarvest };
    }
}


[BsonDiscriminator("Entrepot")]
public class Entrepot : Building {
    public List<int> stock { get; set; }

    public Entrepot() { stock = new List<int> { 0, 0, 0 }; }  // 0 = wood,  1 = food,  2 = oil

    public bool ConsommeRessource(int woodCost, int foodCost, int oilCost) {
        if(stock[(int)ResourceType.Wood] >= woodCost && stock[(int)ResourceType.Food] >= foodCost && stock[(int)ResourceType.Oil] >= oilCost) {
            stock[(int)ResourceType.Wood] -= woodCost; stock[(int)ResourceType.Food] -= foodCost; stock[(int)ResourceType.Oil] -= oilCost; return true;
        } else { return false; }
    }

    public bool FeedStock(ResourceType? resourceType, int quantity) {
        if(resourceType != null) {
            JObject entrepotCatalogue = CatalogueGlobal.buildings["Entrepot"];
            int capacityEntrepot = (int?)entrepotCatalogue?["Levels"]?[this.level]?["Capacity"] ?? int.MaxValue; if(capacityEntrepot != int.MaxValue) {
                stock[(int)(resourceType ?? 0)] += quantity; if(stock[(int)(resourceType ?? 0)] >= capacityEntrepot) { stock[(int)(resourceType ?? 0)] = capacityEntrepot; }
                return true;
            }
        }
        return false;
    }

    public override BuildingDto ToDto()
    {
        return new EntrepotDTO { buildingType = this.GetType().Name, level = this.level, isInConstruction = this.isInConstruction, stock = this.stock };
    }
}

[BsonDiscriminator("CampMilitaire")]
public class CampMilitaire : Building {

    public int nSoldats; public int nSoldatsDisponible;

    public CampMilitaire() { nSoldats = 0; nSoldatsDisponible = 0; }

    public bool AddTroops(int nTroops) {
        if(level > 0) {
            nSoldats += nTroops; nSoldatsDisponible += nTroops;  return true;
        }
        return false;
    }

    public int sendTroopsToAction(int nTroops) {
        if(nTroops > 0 && nTroops <= nSoldatsDisponible) { nSoldatsDisponible -= nTroops; return nTroops;}
        return int.MaxValue;
    }


    public override BuildingDto ToDto()
    {
        return new CampMilitaireDTO { buildingType = this.GetType().Name,  level = this.level, isInConstruction = this.isInConstruction, nSoldats = this.nSoldats, nSoldatsDisponible = this.nSoldatsDisponible };
    }

}



[BsonDiscriminator("Caserne")]
public class Caserne : Building {

    public bool isTraining; public DateTime endTrainingAt; public int nSoldatsTraining;

    public Caserne() { isTraining = false; endTrainingAt = new DateTime(); nSoldatsTraining = 0; }

    public bool TrainingTroops(int nTroopsToTrain) {
        if(level > 0 && nTroopsToTrain > 0 && isTraining == false) {
            int caserneSpeedTrainingBonusPurcent = (int?)CatalogueGlobal.buildings?["Caserne"]?["Levels"]?[level]?["SpeedTrainingBonusPurcent"] ?? 0;
            JObject soldatCatalogue = CatalogueGlobal.troops["Soldat"];
            // time to train without bonus
            int timeToTrain = nTroopsToTrain * (int)(soldatCatalogue["TimeForTraining"] ?? 0);
            // time to train with bonus  
            double bonusCaserneFacteur = 1 - caserneSpeedTrainingBonusPurcent / 100.0;
            int timeToTrainWithBonus = (int)(timeToTrain * bonusCaserneFacteur);
            // update Caserne
            nSoldatsTraining = nTroopsToTrain; endTrainingAt = DateTime.UtcNow.AddSeconds(timeToTrainWithBonus); isTraining = true;
            return true;
        }
        return false;
    }
    public int EndTrainingTroops() {
        if(isTraining == true) {
            if(DateTime.UtcNow >= endTrainingAt) {
                int nSoldatsTrained = nSoldatsTraining;  nSoldatsTraining = 0; isTraining = false; 
                return nSoldatsTrained;
            }
        }
        return int.MaxValue;
    }
    public override BuildingDto ToDto()
    {
        return new CaserneDTO { buildingType = this.GetType().Name,  level = this.level, isInConstruction = this.isInConstruction, isTraining = this.isTraining, nSoldatsTraining = this.nSoldatsTraining, endTrainingAt = this.endTrainingAt };
    }
}


