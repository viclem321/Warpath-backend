using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;

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
        VillageDto newVillageDto = new VillageDto(new List<BuildingDto>());
        List<Building>? buildings = await _buildingServices.GetIdentityAllBuildings(village); if( buildings != null) {
            foreach (Building b in buildings) {
                newVillageDto.buildings.Add(_buildingServices.ToDto(b));
            }
            return newVillageDto;
        }
        return null;
    }

    public async Task<Village?> CreateNewVillage() {
        Village newVillage = new Village();
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



    public async Task<VillageDto?> GetAllDatas(ObjectId? villageId)
    {
        Village? village = await GetIdentityWithLock(villageId); if( village != null) {
            VillageDto? newVillageDto = await ToDto(village); if(newVillageDto != null ) {
                await ReleaseLock(village); return newVillageDto;
            }
            await ReleaseLock(village);
        }
        return null;
    }



    public async Task<bool> UpgradeBuildingAsync(ObjectId? villageId, BuildingType buildingType)
    {
        Village? village = await GetIdentityWithLock(villageId); if( village != null) {
            // upgrade building
            bool success = await _buildingServices.UpgradeBuildingAsync(village, buildingType);
            if(success == true) { await ReleaseLock(village); return true; }
            await ReleaseLock(village);
        }
        return false;
    }



}