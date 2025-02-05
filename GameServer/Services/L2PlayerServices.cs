using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;

namespace GameServer.Services;




public class L2PlayerServices {


    private readonly L1UserServices _userServices;
    private readonly IMongoCollection<Player> _players;  private readonly IMongoCollection<Village> _villages;

    public L2PlayerServices(MongoDBContext context, L1UserServices userServices)
    {
        _userServices = userServices;
        _players = context.GetCollection<Player>("Players");  _villages = context.GetCollection<Village>("Villages");
    }
    

    public async Task<Player?> GetIdentity(User user, string playerId)
    {
        try {
            ObjectId id = ObjectId.Parse(playerId); if(user.playerId != id) { return null; }
            Player player = await _players.Find(p => p._id == id).FirstOrDefaultAsync(); if (player == null) { return null; }
            return player;
        } catch { return null;}
    }



    public async Task<PlayerDto?> GetDatas(ClaimsPrincipal claimUser, string playerId)
    {
        try {
            User? user = await _userServices.GetIdentity(claimUser); if( user == null) { return null; }
            Player? player = await GetIdentity(user, playerId); if( player == null) { return null; }
            return player.ToDto();
        } catch { return null; }
    }

    public async Task<PlayerDto?> CreateNewVillage(ClaimsPrincipal claimUser, string playerId)
    {
        try {
            User? user = await _userServices.GetIdentity(claimUser); if( user == null) { return null; }
            Player? player = await GetIdentity(user, playerId); if( player == null) { return null; }

            Village newVillage = new Village(player.pseudo ?? "", 0, 0);
            await _villages.InsertOneAsync(newVillage);
            await _players.UpdateOneAsync( Builders<Player>.Filter.Eq(p => p._id, player._id), Builders<Player>.Update.Push(p => p.allVillages, newVillage._id) );
            player?.allVillages?.Add(newVillage._id);
            return player?.ToDto();
        } catch { return null; }
    } 

    


}