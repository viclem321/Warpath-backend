using MongoDB.Bson.Serialization.Attributes;
using Newtonsoft.Json.Linq;
using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;
using GameServer.Catalogue;
using System.Linq.Expressions;

namespace GameServer.Services;




public class L5BuildingServices {

    private readonly IMongoCollection<Building> _buildings;

    public L5BuildingServices(MongoDBContext context)
    {
        _buildings = context.GetCollection<Building>("Buildings");
    }


    public async Task<Building?> GetIdentityOneBuilding(Village village, BuildingType buildingType)
    {
        if( Enum.IsDefined(typeof(BuildingType), buildingType) ) {
            ObjectId buildingId = village.buildings[(int)buildingType];
            try { return await _buildings.Find(Builders<Building>.Filter.Eq(b => b._id, buildingId)).FirstOrDefaultAsync(); }  catch { return null; }
        }
        return null;
    }

    public async Task<List<Building>?> GetIdentityAllBuildings(Village village)
    {
        List<ObjectId> allBuildingsId = village.buildings;
        var filter = Builders<Building>.Filter.In(b => b._id, allBuildingsId);
        List<Building>? allBuildingsReceived = new List<Building>();
        try { allBuildingsReceived = await _buildings.Find(filter).ToListAsync(); } catch { Console.WriteLine("Problem in access to buildings in DBB"); allBuildingsReceived = null; }
        if(allBuildingsReceived != null) {
            //reorganize order of buildings and verify the number of buildings received
            bool successOrdered = true;
            List<Building> allBuildingsOrdered = new List<Building>();
            foreach (var id in allBuildingsId)
            {
                var building = allBuildingsReceived.FirstOrDefault(b => b._id == id);
                if (building != null) { allBuildingsOrdered.Add(building); } else { successOrdered = false; break; }
            }
            if(successOrdered == true) { return allBuildingsOrdered; }
        }
        return null;
    }


    public BuildingDto ToDto(Building building) {
        return building.ToDto();
    }





    // CONTROLLER CALL THESE -------------

    // méthode special à enlever plus tard
    public async Task<List<BuildingDto>?> GetAllBuildingsAsync()
    {
        try {
            List<Building> allBuildings = await _buildings.Find(building => true).ToListAsync();
            return allBuildings?.Select(b => b.ToDto()).ToList();
        } catch { return null; }
    } 



    public async Task<BuildingDto?> GetOneBuildingDatas(Village village, BuildingType buildingType)
    {
        if( Enum.IsDefined(typeof(BuildingType), buildingType) ) {
            Building? building = await GetIdentityOneBuilding(village, buildingType); if( building != null) {
                return building.ToDto();
            }
        }
        return null;
    }



    public async Task<(bool, Building, Entrepot, DateTime)> StartUpgradeBuildingAsync(Village village, BuildingType buildingType)
    {
        Building? building = await GetIdentityOneBuilding(village, buildingType); if( building != null) {
            // get all datas about this building and verify if all is ok in Catalogue
            (bool getCatalogue, JObject buildingCata, JObject required) = building.GetIdentityForUpgrade(); if (getCatalogue == true) {
                // recup all buildings which are required and verify if they have the good requirements
                bool allRequirementsAreOk = true;
                foreach (var property in required.Properties()) {
                    string buildingTypeRequire = property.Name; int buildingLevelRequire = (int)property.Value;
                    var filter = Builders<Building>.Filter.And( Builders<Building>.Filter.In("_id", village.buildings), Builders<Building>.Filter.Eq("_t", buildingTypeRequire) );
                    bool propertyOk = false;
                    List<Building> allBuildingsRequired = await _buildings.Find(filter).ToListAsync(); if(allBuildingsRequired != null) {
                        foreach(Building b in allBuildingsRequired) { if(b.level >= buildingLevelRequire) { propertyOk = true; break; } }
                    }
                    if(propertyOk == true) { continue; } else { allRequirementsAreOk = false; break; }
                }
                if(allRequirementsAreOk) {
                    // verify if entrepot has the necessary rss and update it
                    Entrepot? entrepot = (Entrepot?)await GetIdentityOneBuilding(village, BuildingType.Entrepot); if(entrepot != null) {
                        JObject? cost = (JObject?)buildingCata["Levels"]?[building.level + 1]?["Cost"]; if (cost != null) {
                            int woodCost = (int)(cost["wood"] ?? int.MaxValue); int foodCost = (int)(cost["food"] ?? int.MaxValue); int oilCost = (int)(cost["oil"] ?? int.MaxValue);
                            if(entrepot.ConsommeRessource(woodCost, foodCost, oilCost)) {
                                // upgrade building
                                (bool successUpgrade, DateTime endAt) = building.StartUpgrade();  if(successUpgrade) {
                                    return (true, building, entrepot, endAt);
                                }
                            }
                        }
                    }
                }
            }
        }
        return (false, new Hq(), new Entrepot(), new DateTime());
    }


    public async Task<(bool, Building)> EndUpgradeBuildingAsync(Village village, BuildingType buildingType, DateTime endAt)
    {
        Building? building = await GetIdentityOneBuilding(village, buildingType); if( building != null) {
            // upgrade building
            bool successUpgrade = building.EndUpgrade(endAt);  if(successUpgrade) { return (true, building); }
        }
        return (false, new Hq());
    }





    public async Task<int> RecoltAsync(Village village, BuildingType buildingType)
    {
        Building? building = await GetIdentityOneBuilding(village, buildingType); if(building != null) {
            if (building is ResourceBuilding resourceBuilding) {
                // get all datas about this building
                DateTime lastHarvest = resourceBuilding.lastHarvest;
                JObject buildingCata = CatalogueGlobal.buildings[resourceBuilding.GetType().Name];
                int production = (int?)buildingCata?["Levels"]?[resourceBuilding.level]?["Production"] ?? 0;
                int capacity = (int?)buildingCata?["Levels"]?[resourceBuilding.level]?["Capacity"] ?? 0;
                //calcul quantity of ressources won
                DateTime now = DateTime.UtcNow;
                double minutesEcoulees = (now - lastHarvest).TotalMinutes;
                int ressourcesWon = production * (int)minutesEcoulees; if( ressourcesWon > capacity) { ressourcesWon = capacity; }
                // remplir l'entrepot
                Entrepot? entrepot = (Entrepot?)await GetIdentityOneBuilding(village, BuildingType.Entrepot); if(entrepot != null) {
                    ResourceType? resourceType = ResourceBuilding.FindResourceTypeWithBuildingType(buildingType); 
                    if(entrepot.FeedStock(resourceType, ressourcesWon)) {
                        // update DBB
                        try { 
                            // remettre le lastHarvest à Now
                            var filter = Builders<Building>.Filter.Eq(b => b._id, resourceBuilding._id);  
                            var update = Builders<Building>.Update.Set(b => ((ResourceBuilding)b).lastHarvest, now);
                            await _buildings.UpdateOneAsync(filter, update);
                            // update entreprot in DBB
                            var filter2 = Builders<Building>.Filter.Eq(e => e._id, entrepot._id);
                            var result2 = await _buildings.ReplaceOneAsync(filter2, entrepot); if(result2.MatchedCount > 0) { 
                                return entrepot.stock[(int)(resourceType ?? 0)]; 
                            } 
                            Console.WriteLine("Erreur critique update DBB dans RecoltAsync");
                        } catch { Console.WriteLine("Erreur critique update DBB dans RecoltAsync 2"); }
                    }
                }
            }
        }
        return int.MaxValue;
    }




    public async Task<bool> TrainingAsync(Village village, int nSoldats)
    {
        // update entrepot
        Entrepot? entrepot = (Entrepot?)await GetIdentityOneBuilding(village, BuildingType.Entrepot); if(entrepot != null) {
            JObject soldatCata = CatalogueGlobal.troops["Soldat"]; JObject? soldatCost = (JObject?)soldatCata["Cost"];
            int woodCost = (int)(soldatCost?["wood"] ?? 0); int foodCost = (int)(soldatCost?["food"] ?? 0); int oilCost = (int)(soldatCost?["oil"] ?? 0);
            if(entrepot.ConsommeRessource(woodCost*nSoldats, foodCost*nSoldats, oilCost*nSoldats) == true) { 
                // update caserne
                Caserne? caserne = (Caserne?)await GetIdentityOneBuilding(village, BuildingType.Caserne); if(caserne != null) {
                    if (caserne.TrainingTroops(nSoldats)) {
                        // update DBB
                        try {
                            var filter = Builders<Building>.Filter.Eq(e => e._id, entrepot._id);
                            var result = await _buildings.ReplaceOneAsync(filter, entrepot); if(result.MatchedCount > 0) {
                                var filter2 = Builders<Building>.Filter.Eq(e => e._id, caserne._id);
                                var result2 = await _buildings.ReplaceOneAsync(filter2, caserne); if(result2.MatchedCount > 0) {
                                    return true;
                                }
                            }
                            Console.WriteLine("Erreur critique dans TrainingAsync update DBB");
                        } catch { Console.WriteLine("Erreur critique dans TrainingAsync update DBB 2"); }
                    }
                }
            }
        }
        return false;
    }

    public async Task<bool> EndTrainingAsync(Village village)
    {
        // update caserne
        Caserne? caserne = (Caserne?)await GetIdentityOneBuilding(village, BuildingType.Caserne); if(caserne != null) {
            int nSoldatsTrained = caserne.EndTrainingTroops(); if (nSoldatsTrained != int.MaxValue) {
                // update CampMilitaire
                CampMilitaire? campMilitaire = (CampMilitaire?)await GetIdentityOneBuilding(village, BuildingType.CampMilitaire); if(campMilitaire != null) {
                    if(campMilitaire.AddTroops(nSoldatsTrained)) {
                        // update DBB
                        try {
                            var filter = Builders<Building>.Filter.Eq(e => e._id, caserne._id);
                            var result = await _buildings.ReplaceOneAsync(filter, caserne); if(result.MatchedCount > 0) {
                                var filter2 = Builders<Building>.Filter.Eq(e => e._id, campMilitaire._id);
                                var result2 = await _buildings.ReplaceOneAsync(filter2, campMilitaire); if(result2.MatchedCount > 0) {
                                    return true;
                                }
                            }
                            Console.WriteLine("Erreur critique dans EndTrainingAsync update DBB");
                        } catch { Console.WriteLine("Erreur critique dans EndTrainingAsync update DBB 2"); }
                    }
                }
            }
        }
        return false;
    }


}