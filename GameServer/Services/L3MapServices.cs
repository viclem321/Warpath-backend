using MongoDB.Driver;
using MongoDB.Bson;
using System.Security.Claims;
using Warpath.Shared.Catalogue;
using Warpath.Shared.DTOs;
using GameServer.Models;
using GameServer.Datas;
using GameServer.Hubs;

namespace GameServer.Services;




public class L3MapServices {
    private readonly L4VillageServices _villageServices;  private readonly L5BuildingServices _buildingServices; private readonly GameHub _gameHub;

    private readonly IMongoCollection<MapTile> _mapTiles; private readonly int gridDimensionsX = CatalogueGlobal.gridDimension; int gridDimensionsY = CatalogueGlobal.gridDimension; private readonly IMongoCollection<Village> _villages;   private readonly IMongoCollection<Building> _buildings;  private readonly IMongoCollection<RapportFight> _rapportsFight;

    public L3MapServices(MongoDBContext context, L4VillageServices villageServices, L5BuildingServices buildingServices, GameHub gameHub)
    {
        _villageServices = villageServices; _buildingServices = buildingServices; _gameHub = gameHub;
        _mapTiles = context.GetCollection<MapTile>("MapTiles"); _villages = context.GetCollection<Village>("Villages");  _buildings = context.GetCollection<Building>("Buildings"); _rapportsFight = context.GetCollection<RapportFight>("RapportsFight");

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
    public bool VerifyIfIndexIsOk(int indexTile) {
        if(indexTile < 0 || indexTile >= gridDimensionsX * gridDimensionsY) { return false; }
        return true;
    }



    public async Task<MapTile?> GetIdentityOneTile(int indexTile) {
        if(VerifyIfIndexIsOk(indexTile) == false) return null;
        try { return await _mapTiles.Find(Builders<MapTile>.Filter.Eq(t => t._id, indexTile)).FirstOrDefaultAsync(); }
        catch { return null; }
    }

    public async Task<MapTile?> GetIdentityOneTileWithLock(int indexTile) { 
        if(VerifyIfIndexIsOk(indexTile) == false) return null;
        int i = 0;
        while(i < 4) {
            int currentTimestamp = (int)DateTimeOffset.UtcNow.ToUnixTimeSeconds();
            int newLockUntil = currentTimestamp + 15;

            var filter = Builders<MapTile>.Filter.And( Builders<MapTile>.Filter.Eq( "_id", indexTile ), Builders<MapTile>.Filter.Lt("lockUntil", currentTimestamp) );
            var update = Builders<MapTile>.Update.Set("lockUntil", newLockUntil);
            var options = new FindOneAndUpdateOptions<MapTile> { ReturnDocument = ReturnDocument.After };

            try { MapTile mapTile = await _mapTiles.FindOneAndUpdateAsync(filter, update, options); if(mapTile != null) { return mapTile; } } catch { return null; }
            Thread.Sleep(100); i++;
        }
        Console.WriteLine("Impossible d'accéder à une mapTile locked après 5 essaies."); return null;
    }

    public async Task<bool> OneTileReleaseLock(int indexTile)
    {
        if(VerifyIfIndexIsOk(indexTile) == false) return false;
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

    public bool OneTileCanBeSeeByPlayer(Player player, int indexTile)
    {
        var tileCoords = GetCoordMapTile(indexTile, gridDimensionsX); if(tileCoords.x == int.MaxValue) { return false; }
        foreach (var v in player.allMapVillages) {
            var coord = GetCoordMapTile(v, gridDimensionsX); if(coord.x == int.MaxValue) { return false; }
            if(tileCoords.x > coord.x - 3 && tileCoords.x < coord.x + 3 && tileCoords.y > coord.y - 3 && tileCoords.y < coord.y + 3) {
                return true;
            }
        }
        return false;
    }



    public async Task<int> CreateNewVillage(int indexMapTile, string pPseudoPlayer) {
        // récuperation de la TileMap + lock si dispo
        MapTile? mapTile = await GetIdentityOneTileWithLock(indexMapTile); if(mapTile != null) {
            if(mapTile.type == TileType.Empty) {
                Village? newVillage = await _villageServices.CreateNewVillage(pPseudoPlayer); if(newVillage != null) {
                    var filter = Builders<MapTile>.Filter.Eq(t => t._id, mapTile._id);  var update = Builders<MapTile>.Update.Set(t => t.type, TileType.Village).Set(t => t.dataId, newVillage._id);
                    try { 
                        await _mapTiles.UpdateOneAsync(filter, update); 
                        await OneTileReleaseLock(mapTile._id); return mapTile._id;
                    } catch {await _villageServices.DeleteVillage(newVillage);}
                }
            }
            await OneTileReleaseLock(mapTile._id);
        }
        return int.MaxValue;
    }

    public async Task<bool> DeleteVillage(int indexVillage) {
        MapTile? mapTile = await GetIdentityOneTileWithLock(indexVillage); if(mapTile != null) {
            if(mapTile.type == TileType.Village) {
                Village? village = await _villageServices.GetIdentityWithLock(mapTile.dataId); if(village != null) {
                    bool successDelete = await _villageServices.DeleteVillage(village); if (successDelete) {
                        var filter = Builders<MapTile>.Filter.Eq(t => t._id, mapTile._id);  var update = Builders<MapTile>.Update.Set(t => t.type, TileType.Empty).Set(t => t.dataId, null);
                        try { 
                            await _mapTiles.UpdateOneAsync(filter, update); 
                            await OneTileReleaseLock(mapTile._id); return true;
                        } catch { Console.WriteLine("Erreur critique, impossible de supprimer un village d'une MapTile"); }
                    }
                    await _villageServices.ReleaseLock(village);
                }
            }
            await OneTileReleaseLock(mapTile._id);
        }
        return false;
    }





    // CONTROLLER CALL THESE -------------

    // Attention, requete extremement couteuse
    public async Task<MapDto?> GetAllMapAsync() {
        try {
            Map newMap = new Map(null);
            List<MapTile> mapTiles = await _mapTiles.Find(Builders<MapTile>.Filter.Empty).ToListAsync();
            List<string> villageNames = new List<string>(new string[mapTiles.Count]); int counter1 = 0;
            foreach(MapTile mapTile in mapTiles) {
                if(mapTile.type == TileType.Village) {
                    Village village = await _villages.Find(Builders<Village>.Filter.Eq(v => v._id, mapTile.dataId)).FirstOrDefaultAsync();
                    villageNames[counter1] = village.owner;
                }
                counter1 ++;
            }
            newMap.mapTiles = mapTiles;
            return newMap.ToDto(villageNames);
        } catch { Console.WriteLine("Probleme d'import de la Map."); return null; }
    }


    public async Task<MapTile?> GetOneTile(Player player, int indexTile) {
        // verify if this player can get this tile
        if( OneTileCanBeSeeByPlayer(player, indexTile) ) {
            // get tile
            return await GetIdentityOneTile(indexTile);
        }
        return null;
    }


    public async Task<RapportFightDto?> GetOneRapport(string pRapport) {
        try {
        RapportFight? rapport = await _rapportsFight.Find(Builders<RapportFight>.Filter.Eq(r => r._id, new ObjectId(pRapport))).FirstOrDefaultAsync();
        if(rapport != null) {
            return rapport.ToDto();
        }
        } catch {  return null; }
        return null;
    }



    public async Task<(bool, RapportFightDto?)> Attack(MapTile mapTile, int nSoldatsToSend, int indexTileToAttack) {
        Village? village = await _villageServices.GetIdentityWithLock(mapTile.dataId); if(village != null) {
            CampMilitaire? campMilitaire = (CampMilitaire?)await _buildingServices.GetIdentityOneBuilding(village, BuildingType.CampMilitaire); if(campMilitaire != null) {
                int nSoldatsAttack = campMilitaire.sendTroopsToAction(nSoldatsToSend); if(nSoldatsAttack != int.MaxValue) {
                    if(indexTileToAttack != mapTile._id) {
                        MapTile? mapTileDefenser =  await GetIdentityOneTileWithLock(indexTileToAttack); if (mapTileDefenser != null) {
                            if( mapTileDefenser.type == TileType.Village ) {
                                Village? villageDefenser = await _villageServices.GetIdentityWithLock(mapTileDefenser.dataId); if(villageDefenser != null) {
                                    CampMilitaire? campMilitaireDefenser = (CampMilitaire?)await _buildingServices.GetIdentityOneBuilding(villageDefenser, BuildingType.CampMilitaire); if(campMilitaireDefenser != null) {
                                        int nSoldatsDefenser = campMilitaireDefenser.nSoldatsDisponible;
                                        // fight
                                        int nSoldatsAttackSurvived = nSoldatsAttack - nSoldatsDefenser; if(nSoldatsAttackSurvived < 0) { nSoldatsAttackSurvived = 0; }
                                        int nSoldatsAttackLost = nSoldatsAttack - nSoldatsAttackSurvived;
                                        int nSoldatsDefendSurvived = nSoldatsDefenser - nSoldatsAttack; if(nSoldatsDefendSurvived < 0) { nSoldatsDefendSurvived = 0; }
                                        int nSoldatsDefendLost = nSoldatsDefenser - nSoldatsDefendSurvived;
                                        string attackWinner = ""; if(nSoldatsAttackSurvived > 0) { attackWinner = village.owner; } else { attackWinner = villageDefenser.owner; }
                                        // actualize defenser
                                        campMilitaireDefenser.nSoldats -= nSoldatsDefendLost;
                                        campMilitaireDefenser.nSoldatsDisponible = nSoldatsDefendSurvived;
                                        // actualize attaquant
                                        campMilitaire.nSoldats -= nSoldatsAttackLost;
                                        campMilitaire.nSoldatsDisponible += nSoldatsAttackSurvived;
                                        // update BDD
                                        try {
                                            var filter = Builders<Building>.Filter.Eq(e => e._id, campMilitaireDefenser._id);
                                            var result = await _buildings.ReplaceOneAsync(filter, campMilitaireDefenser); if(result.MatchedCount > 0) {
                                                var filter2 = Builders<Building>.Filter.Eq(e => e._id, campMilitaire._id);
                                                var result2 = await _buildings.ReplaceOneAsync(filter2, campMilitaire); if(result2.MatchedCount > 0) {
                                                    // If all is ok
                                                    RapportFight newRapport = new RapportFight(village.owner, villageDefenser.owner, DateTime.UtcNow, nSoldatsAttack, nSoldatsDefenser, nSoldatsAttackSurvived, nSoldatsDefendSurvived, attackWinner);
                                                    try { await _rapportsFight.InsertOneAsync(newRapport); } catch { Console.WriteLine("Error in insert Rapport");}
                                                    // send message to defender with id of the rapport
                                                    await _gameHub.SendMessageToOne(villageDefenser.owner, "ReceiveAttackNotification", newRapport._id.ToString());
                                                    await _villageServices.ReleaseLock(villageDefenser);  await OneTileReleaseLock(mapTileDefenser._id);  await _villageServices.ReleaseLock(village);
                                                    
                                                    return (true, newRapport.ToDto());
                                                }
                                            }
                                        } catch { ; }

                                    }
                                    await _villageServices.ReleaseLock(villageDefenser);
                                }
                            }
                            await OneTileReleaseLock(mapTileDefenser._id);
                        }
                    }
                }
            }
            await _villageServices.ReleaseLock(village);
        }
        return (false, null);
    }




}