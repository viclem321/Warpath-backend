using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;
using ZstdSharp.Unsafe;

namespace GameServer.Services;




public class L2PlayerServices {


    private readonly L3MapServices _mapServices;
    private readonly IMongoCollection<Player> _players;  private readonly IMongoCollection<Village> _villages;  private readonly IMongoCollection<MapTile> _mapTiles;

    public L2PlayerServices(MongoDBContext context, L3MapServices mapServices)
    {
        _mapServices = mapServices;
        _players = context.GetCollection<Player>("Players");  _villages = context.GetCollection<Village>("Villages");  _mapTiles = context.GetCollection<MapTile>("MapTiles");
    }
    

    public async Task<Player?> GetIdentity(User user, string playerName)
    {
        try {
             if(user.player != playerName) { return null; }
            Player player = await _players.Find(p => p.pseudo == playerName).FirstOrDefaultAsync(); if (player == null) { return null; }
            return player;
        } catch { return null;}
    }

    public async Task<Player?> GetIdentityWithLock(User user, string playerName)
    {
            if(user.player != playerName) { return null; }
            
            int i = 0;
            while(i < 4) {
                int currentTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
                int newLockUntil = currentTimestamp + 20;

                var filter = Builders<Player>.Filter.And( Builders<Player>.Filter.Eq( "pseudo", playerName ), Builders<Player>.Filter.Lt("lockUntil", currentTimestamp) );
                var update = Builders<Player>.Update.Set("lockUntil", newLockUntil);
                var options = new FindOneAndUpdateOptions<Player> { ReturnDocument = ReturnDocument.After };

                try { Player player = await _players.FindOneAndUpdateAsync(filter, update, options); if(player != null) { return player; } } catch { return null; }
                Thread.Sleep(100); i++;
            }
            Console.WriteLine("Impossible d'accéder à un joueur locked après 5 essaies."); return null;
    }

    public async Task<bool> ReleaseLock(Player player)
    {
        try {
            var update = Builders<Player>.Update.Set("lockUntil", 0);
            await _players.UpdateOneAsync(Builders<Player>.Filter.Eq("pseudo", player.pseudo), update);
            return true;
        } catch { Console.WriteLine("Problem in ReleaseLock player."); return false; }
    }


    public bool PlayerOwnATile(Player player, int indexMapTile)
    {
        foreach(var v in player.allMapVillages) {  if(v == indexMapTile) { return true; }  }
        return false;
    }




    // CONTROLLER CALL THESE -------------
    
    public PlayerDto? GetDatas(Player player)
    {
        return player.ToDto();
    }

    public async Task<int> CreateNewVillageAsync(Player player, int indexNewVillage)
    {
        int index = await _mapServices.CreateNewVillage(indexNewVillage); if(index != int.MaxValue) {
            // ajout des coordonées du new village dans le player
            try { 
                await _players.UpdateOneAsync( Builders<Player>.Filter.Eq(p => p.pseudo, player.pseudo), Builders<Player>.Update.Push(p => p.allMapVillages, index) );
                return index;
            } catch { await _mapServices.DeleteVillage(index); }
        }
        return int.MaxValue;
    }


    public async Task<bool> DeleteVillageAsync(Player player, int indexVillage)
    {
        if (PlayerOwnATile(player, indexVillage)) {
            bool successDelete = await _mapServices.DeleteVillage(indexVillage); if(successDelete == true) {
                var filter = Builders<Player>.Filter.Eq(p => p.pseudo, player.pseudo);  var update = Builders<Player>.Update.Pull(p => p.allMapVillages, indexVillage);
                try { await _players.UpdateOneAsync(filter, update); return true; } catch {  Console.WriteLine("Erreur critique, impossible de supprimer un village d'un player"); }
            }
        }
        return false;
    }

    



}