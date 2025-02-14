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
                Thread.Sleep(200); i++;
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




    // CONTROLLER CALL THESE -------------
    
    public PlayerDto? GetDatas(Player player)
    {
        return player.ToDto();
    }

    public async Task<int> CreateNewVillageAsync(Player player, int locX, int locY)
    {
        // trouver l'index 1D correspondant aux cooerdonnés
        int indexMapTile = _mapServices.GetIndexMapTile(locX, locY); if(indexMapTile == int.MaxValue) { return int.MaxValue; }
        // récuperation de la TileMap + lock si dispo
        MapTile? mapTile = await _mapServices.GetIdentityOneTileWithLock(indexMapTile); if(mapTile != null) {
            if(mapTile.type == TileType.Empty) {
                try {
                    // insert du nouveau village dans la collection Village
                    Village newVillage = new Village();
                    await _villages.InsertOneAsync(newVillage);
                    try {
                        // insert du nouveau village dans la MapTile
                        var filter = Builders<MapTile>.Filter.Eq(t => t._id, mapTile._id);  var update = Builders<MapTile>.Update.Set(t => t.type, TileType.Village).Set(t => t.dataId, newVillage._id);
                        await _mapTiles.UpdateOneAsync(filter, update);
                        try {
                            // ajout des coordonées du new village dans le player
                            await _players.UpdateOneAsync( Builders<Player>.Filter.Eq(p => p.pseudo, player.pseudo), Builders<Player>.Update.Push(p => p.allMapVillages, indexMapTile) );
                            // unlock + return index
                            await _mapServices.OneTileReleaseLock(mapTile._id);
                            return indexMapTile;
                        } catch {  Console.WriteLine("Attention, Impossible d'insérer un nouveau Village dans le Player.");  var filter2 = Builders<MapTile>.Filter.Eq(t => t._id, mapTile._id);  var update2 = Builders<MapTile>.Update.Set(t => t.type, TileType.Empty); await _mapTiles.UpdateOneAsync(filter2, update2);  await _villages.DeleteOneAsync( Builders<Village>.Filter.Eq(t => t._id, newVillage._id));  }
                    } catch { Console.WriteLine("Attention, Impossible d'insérer un nouveau Village sur une MapTile.");  await _villages.DeleteOneAsync( Builders<Village>.Filter.Eq(t => t._id, newVillage._id)); }
                } catch { Console.WriteLine("Attention, Impossible d'insérer un nouveau Village."); }
            }
            await _mapServices.OneTileReleaseLock(mapTile._id);
        }
        return int.MaxValue;
    } 

    



}