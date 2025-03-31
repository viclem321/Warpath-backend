using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using Warpath.Shared.Catalogue;
using Warpath.Shared.DTOs;
using GameServer.Models;
using GameServer.Datas;

namespace GameServer.Services;




public class L4VillageServices {

    private readonly IMongoCollection<Village> _villages;  private readonly IMongoCollection<Building> _buildings;
    private readonly L5BuildingServices _buildingServices;

    public L4VillageServices(MongoDBContext context, L5BuildingServices buildingServices)
    {
        _villages = context.GetCollection<Village>("Villages");   _buildings = context.GetCollection<Building>("Buildings");
        _buildingServices = buildingServices;
    }


    public async Task<Village?> GetIdentity(ObjectId? villageId)
    {
        if(villageId == null) { return null; }
        try {
            return await _villages.Find(Builders<Village>.Filter.Eq(v => v._id, villageId)).FirstOrDefaultAsync();
        } catch { return null; }
    }

    public async Task<Village?> GetIdentityWithLock(ObjectId? villageId)
    {
        if(villageId == null) { return null; }
        int i = 0;
        while(i < 4) {
            int currentTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int newLockUntil = currentTimestamp + 20;

            var filter = Builders<Village>.Filter.And( Builders<Village>.Filter.Eq( "_id", villageId ), Builders<Village>.Filter.Lt("lockUntil", currentTimestamp) );
            var update = Builders<Village>.Update.Set("lockUntil", newLockUntil);
            var options = new FindOneAndUpdateOptions<Village> { ReturnDocument = ReturnDocument.After };

            try { Village village = await _villages.FindOneAndUpdateAsync(filter, update, options); if(village != null) { return village; } } catch { return null; }
            Thread.Sleep(150); i++;
        }
        Console.WriteLine("Impossible d'accéder à un village locked après 5 essaies."); return null;
    }

    public async Task<bool> ReleaseLock(Village village)
    {
        try {
            var update = Builders<Village>.Update.Set("lockUntil", 0);
            await _villages.UpdateOneAsync(Builders<Village>.Filter.Eq("_id", village._id), update);
            return true;
        } catch { Console.WriteLine("Problem in ReleaseLock a Village."); return false; }
    }


    public async Task<VillageDto?> ToDto(Village village) {
        VillageDto newVillageDto = new VillageDto(new List<BuildingDto>(), new UpgradeActionDto());
        newVillageDto.owner = village.owner; newVillageDto.upgradeAction1 = village.upgradeAction1.ToDto();
        List<Building>? buildings = await _buildingServices.GetIdentityAllBuildings(village); if( buildings != null) {
            foreach (Building b in buildings) {
                newVillageDto.buildings.Add(_buildingServices.ToDto(b));
            }
            return newVillageDto;
        }
        return null;
    }

    public async Task<Village?> CreateNewVillage(string pOwner) {
        Village newVillage = new Village(); newVillage.owner = pOwner;
        // attention, obligé de rajouter les batiments à chaque ajout de batiment. à changer..;
        List<Building> allBuildings = new List<Building> { new Hq(), new Scierie(), new Ferme(), new Mine(), new Entrepot(), new CampMilitaire(), new Caserne() };
        try {
            // insert all buildings
            await _buildings.InsertManyAsync(allBuildings);
            foreach (var building in allBuildings) { newVillage.buildings.Add(building._id); }
            // insert village
            try { 
                await _villages.InsertOneAsync(newVillage); return newVillage; 
            } catch { var ids = allBuildings.Select(d => d._id).ToList();  try { await _buildings.DeleteManyAsync(d => ids.Contains(d._id));} catch {;}  Console.WriteLine("Impossible de creer un village en BDD"); }
        } catch(Exception exx) { var ids = allBuildings.Select(d => d._id).ToList();  try { await _buildings.DeleteManyAsync(d => ids.Contains(d._id));} catch {;} Console.WriteLine($"Impossible d'insérer des buildings en BDD {exx}"); }
        return null;
    }
    public async Task<bool> DeleteVillage(Village village) {
        // delete all buildings
        List<ObjectId> allBuildingsId = village.buildings;
        try { 
            await _buildings.DeleteManyAsync( Builders<Building>.Filter.In(b => b._id, allBuildingsId) ); 
            // delete village
            try { 
                await _villages.DeleteOneAsync( Builders<Village>.Filter.Eq(v => v._id, village._id)); return true; 
            } catch { Console.WriteLine("Impossible de supprimer un village en BDD");}
        } catch { Console.WriteLine("Impossible de supprimer des buildings."); }
        return false;
    }




    // CONTROLLER CALL THESE -------------

    // méthode special à enlever plus tard
    public async Task<List<VillageDto>?> GetAllVillagesAsync()
    {
        List<Village> allVillages = await _villages.Find(village => true).ToListAsync(); if( allVillages != null) {
            List<VillageDto> villageDtos = new List<VillageDto>();
            foreach(var v in allVillages) {
                VillageDto? newVillageDto = await ToDto(v); if(newVillageDto != null) {
                    villageDtos.Add(newVillageDto);
                }
            }
            return villageDtos;
        }
        return null;
    } 



    public async Task<VillageDto?> GetVillageDatas(ObjectId? villageId)
    {
        Village? village = await GetIdentityWithLock(villageId); if( village != null) {
            VillageDto? newVillageDto = await ToDto(village); if(newVillageDto != null ) {
                await ReleaseLock(village); return newVillageDto;
            }
            await ReleaseLock(village);
        }
        return null;
    }



    public async Task<UpgradeActionDto?> StartUpgradeBuildingAsync(ObjectId? villageId, BuildingType buildingType)
    {
        Village? village = await GetIdentityWithLock(villageId); if( village != null) {
            // upgrade building
            (bool success, Building building, Entrepot entrepot, DateTime endAt) = await _buildingServices.StartUpgradeBuildingAsync(village, buildingType);
            if(success == true) {
                bool successNewAction = village.upgradeAction1.newActionBegin(buildingType, endAt); if(successNewAction) {
                    // update DBB
                    try {
                        var result = await _buildings.ReplaceOneAsync(e => e._id == entrepot._id, entrepot); if(result.MatchedCount > 0) { 
                            var result2 = await _buildings.ReplaceOneAsync(b => b._id == building._id, building); if(result2.MatchedCount > 0) {
                                var result3 = await _villages.ReplaceOneAsync(v => v._id == village._id, village); if(result3.MatchedCount > 0) {
                                    await ReleaseLock(village); return village.upgradeAction1.ToDto();
                                }
                            }
                        }
                        Console.WriteLine("Erreur critique dans l'update de la BDD dans StartupgradeBuilding");
                    } catch { Console.WriteLine("Erreur critique dans l'update de la BDD dans StartupgradeBuilding 2"); }
                } 
            }
            await ReleaseLock(village);
        }
        return null;
    }


    public async Task<bool> EndUpgradeAction1Async(ObjectId? villageId)
    {
        Village? village = await GetIdentityWithLock(villageId); if( village != null) {
            if(!village.upgradeAction1.isDisponible())
            {
                BuildingType buildingToUpgrade = village.upgradeAction1.buildingType;
                DateTime upgradeEndAt = village.upgradeAction1.endUpgradeAt;

                (bool success1, Building building) = await _buildingServices.EndUpgradeBuildingAsync(village, buildingToUpgrade, upgradeEndAt); if(success1 == true) {
                    if (village.upgradeAction1.endAction() ) {
                        // update BDD
                        try {
                            var result = await _buildings.ReplaceOneAsync(b => b._id == building._id, building); if(result.MatchedCount > 0) {
                                var result2 = await _villages.ReplaceOneAsync(v => v._id == village._id, village); if(result2.MatchedCount > 0) {
                                    await ReleaseLock(village); return true;
                                }
                            }
                            Console.WriteLine("Erreur critique dans l'update de la BDD dans EndupgradeBuilding");
                        } catch { Console.WriteLine("Erreur critique dans l'update de la BDD dans EndupgradeBuilding 2"); }
                    }
                }
            }
            await ReleaseLock(village);
        }
        return false;
    }



}