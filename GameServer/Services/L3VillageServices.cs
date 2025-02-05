using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;

namespace GameServer.Services;




public class L3VillageServices {

    private readonly L1UserServices _userServices; private readonly L2PlayerServices _playerServices;
    private readonly IMongoCollection<Village> _villages;


    public L3VillageServices(MongoDBContext context, L1UserServices userServices, L2PlayerServices playerServices)
    {
        _userServices = userServices; _playerServices = playerServices;
        _villages = context.GetCollection<Village>("Villages");
    }


    public async Task<Village?> GetIdentity(Player player, string villageId)
    {
        try {
            ObjectId id = ObjectId.Parse(villageId); 
            foreach (var v in player.allVillages ?? new List<ObjectId>()) {
                if(v == id) { return await _villages.Find(v => v._id == id).FirstOrDefaultAsync(); }
            }
            return null;
        } catch { return null;}
    }




    // méthode special à enlever plus tard
    public async Task<List<VillageDto>?> GetAllVillagesAsync()
    {
        try {
            List<Village> allVillages = await _villages.Find(village => true).ToListAsync();
            return allVillages?.Select(b => b.ToDto()).ToList();
        } catch { return null; }
    } 

    public async Task<VillageDto?> GetDatas(ClaimsPrincipal claimUser, string idPlayer, string idVillage)
    {
        try {
            User? user = await _userServices.GetIdentity(claimUser); if( user == null) { return null; }
            Player? player = await _playerServices.GetIdentity(user, idPlayer); if( player == null) { return null; }
            Village? village = await GetIdentity(player, idVillage); if( village == null) { return null; }
            return village.ToDto();
        } catch { return null; }
    }



    public async Task<bool> UpgradeBuildingAsync(ClaimsPrincipal claimUser, string idPlayer, string idVillage, int buildingType)
    {
        try {
            User? user = await _userServices.GetIdentity(claimUser); if( user == null) { return false; }
            Player? player = await _playerServices.GetIdentity(user, idPlayer); if( player == null) { return false; }
            Village? village = await GetIdentity(player, idVillage); if( village == null) { return false; }
            if (village.buildings[buildingType].Upgrade() == true) {
                await _villages.ReplaceOneAsync(v => v._id == village._id, village);
                return true;
            } else { return false; }
        } catch { return false; }
    }


}