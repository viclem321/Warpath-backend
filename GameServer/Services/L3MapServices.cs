using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using GameServer.Models;
using GameServer.Datas;
using GameServer.DTOs;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace GameServer.Services;




public class L3MapServices {

    private readonly IMongoCollection<MapTile> _mapTiles; private readonly int gridDimensionsX = 50; int gridDimensionsY = 50;

    public L3MapServices(MongoDBContext context)
    {
        _mapTiles = context.GetCollection<MapTile>("MapTiles");

        bool isEmpty = _mapTiles.CountDocuments(Builders<MapTile>.Filter.Empty) == 0;
        if(isEmpty) {
            var mapTiles = new List<MapTile>();
            for (int i = 0; i < gridDimensionsX * gridDimensionsY; i++)
            {
                mapTiles.Add(new MapTile(i, TileType.Empty, null));
            }
            try {  _mapTiles.InsertMany(mapTiles); Console.WriteLine("MapTiles insérées avec succès !"); }
            catch { Console.WriteLine($"Impossible de créer la map en BDD"); Environment.Exit(1); }
        }
    }

    public int GetIndexMapTile(int pX, int pY) {
        if(pX >= gridDimensionsX || pX < 0 || pY >= gridDimensionsY || pY < 0 ) { return int.MaxValue; }
        return pY * gridDimensionsX + pX;
    }
    public (int x, int y) GetCoordMapTile(int pIndex, int pWidth) {
        if(pIndex >= gridDimensionsX * gridDimensionsY || pIndex < 0 ) { return (int.MaxValue, int.MaxValue); }
        int x = pIndex % pWidth; int y = pIndex / pWidth;
        return (x, y);
    }



    public async Task<MapTile?> GetIdentityOneTile(int indexTile) {
        try { return await _mapTiles.Find(Builders<MapTile>.Filter.Eq(t => t._id, indexTile)).FirstOrDefaultAsync(); }
        catch { return null; }
    }

    public async Task<MapTile?> GetIdentityOneTileWithLock(int indexTile) { 
        int i = 0;
        while(i < 4) {
            int currentTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int newLockUntil = currentTimestamp + 15;

            var filter = Builders<MapTile>.Filter.And( Builders<MapTile>.Filter.Eq( "_id", indexTile ), Builders<MapTile>.Filter.Lt("lockUntil", currentTimestamp) );
            var update = Builders<MapTile>.Update.Set("lockUntil", newLockUntil);
            var options = new FindOneAndUpdateOptions<MapTile> { ReturnDocument = ReturnDocument.After };

            try { MapTile mapTile = await _mapTiles.FindOneAndUpdateAsync(filter, update, options); if(mapTile != null) { return mapTile; } } catch { return null; }
            Thread.Sleep(180); i++;
        }
        Console.WriteLine("Impossible d'accéder à une mapTile locked après 5 essaies."); return null;
    }

    public async Task<bool> OneTileReleaseLock(int indexTile)
    {
        try {
            var update = Builders<MapTile>.Update.Set("lockUntil", 0);
            await _mapTiles.UpdateOneAsync(Builders<MapTile>.Filter.Eq("_id", indexTile), update);
            return true;
        } catch { Console.WriteLine("Probleme in ReleaseLock a MapTile."); return false; }
    }



    public bool OneTileIsOwnedByPlayer(Player player, MapTile mapTile)
    {
        foreach(var v in player.allMapVillages) {  if(v == mapTile._id) { return true; }  }
        return false;
    }

    public bool OneTileCanBeSeeByPlayer(Player player, int posX, int posY)
    {
        foreach (var v in player.allMapVillages) {
            var coord = GetCoordMapTile(v, gridDimensionsX); if(coord.x == int.MaxValue) { return false; }
            if(posX > coord.x - 3 && posX < coord.x + 3 && posY > coord.y - 3 && posY < coord.y + 3) {
                return true;
            }
        }
        return false;
    }





    // CONTROLLER CALL THESE -------------

    // Attention, requete extremement couteuse
    public async Task<List<MapTile>> GetAllMapAsync() {
        try {
        List<MapTile> mapTiles = await _mapTiles.Find(Builders<MapTile>.Filter.Empty).ToListAsync();
        return mapTiles;
        } catch { Console.WriteLine("Probleme d'import des TilesMap."); return new List<MapTile>(); }
    }


    public async Task<MapTile?> GetOneTile(Player player, int posX, int posY) {
        // verify if this player can get this tile
        if( OneTileCanBeSeeByPlayer(player, posX, posY) ) {
            // get tile
            int index = GetIndexMapTile(posX, posY); if(index != int.MaxValue) {
                return await GetIdentityOneTile(index);
            }
        }
        return null;
    }




}