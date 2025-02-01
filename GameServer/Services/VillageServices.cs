using MongoDB.Driver;
using MongoDB.Bson;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;

namespace GameServer.Services;




public class VillageServices {


    private readonly IMongoCollection<Village> _villages;


    public VillageServices(MongoDBContext context)
    {
        _villages = context.GetCollection<Village>("Villages");
    }



    public async Task<List<VillageDto>?> GetAllVillagesAsync()
    {
        try {
            List<Village> allVillages = await _villages.Find(village => true).ToListAsync();
            return allVillages?.Select(b => b.ToDto()).ToList();
        } catch { return null; }
    } 

    public async Task<VillageDto?> GetVillageByIdAsync(string idVillage)
    {
        try {
        Village village = await _villages.Find(village => village._id == ObjectId.Parse(idVillage)).FirstOrDefaultAsync();
        if(village != null) { return village.ToDto(); }   else { return null; }
        } catch { return null;}
    }

    public async Task<bool> CreateVillageAsync()
    {
        try {
            Village village = new Village("bertrand", 0, 1);
            await _villages.InsertOneAsync(village);
            return true;
        } catch { return false; }
    }



    public async Task<bool> UpgradeBuildingAsync(string? idVillage, int buildingType)
    {
        try {
            Village newVillage = await _villages.Find(village => village._id == ObjectId.Parse(idVillage)).FirstOrDefaultAsync();
            if (newVillage.buildings[buildingType].Upgrade() == true) {
                await _villages.ReplaceOneAsync(village => village._id == ObjectId.Parse(idVillage), newVillage);
                return true;
            } else { return false; }
        } catch { return false; }
    }


}