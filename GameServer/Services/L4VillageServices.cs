using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;

namespace GameServer.Services;




public class L4VillageServices {

    private readonly IMongoCollection<Village> _villages;

    public L4VillageServices(MongoDBContext context)
    {
        _villages = context.GetCollection<Village>("Villages");
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
            int newLockUntil = currentTimestamp + 15;

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




    // CONTROLLER CALL THESE -------------

    // méthode special à enlever plus tard
    public async Task<List<VillageDto>?> GetAllVillagesAsync()
    {
        try {
            List<Village> allVillages = await _villages.Find(village => true).ToListAsync();
            return allVillages?.Select(b => b.ToDto()).ToList();
        } catch { return null; }
    } 



    public async Task<VillageDto?> GetAllDatas(ObjectId? villageId)
    {
        Village? village = await GetIdentityWithLock(villageId); if( village != null) {
            await ReleaseLock(village); return village.ToDto();
        }
        return null;
    }



    public async Task<bool> UpgradeBuildingAsync(ObjectId? villageId, BuildingType buildingType)
    {
        Village? village = await GetIdentityWithLock(villageId); if( village != null) {
            // upgrade building
            if( Enum.IsDefined(typeof(BuildingType), buildingType) ) {
                if (village.buildings[(int)buildingType].Upgrade() == true) {
                    try { await _villages.ReplaceOneAsync(v => v._id == village._id, village); await ReleaseLock(village); return true; } catch { Console.WriteLine("Problem to replace a village when upgrade building."); }
                }
            }
            await ReleaseLock(village);
        }
        return false;
    }


}