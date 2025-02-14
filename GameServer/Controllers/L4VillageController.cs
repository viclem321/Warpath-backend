using GameServer.DTOs;
using GameServer.Models;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;




[ApiController]
[Route("api/user/{username}/player/{playerName}/map/{posX}&{posY}/village")]
public class L4VillageController : ControllerBase {
    
    private readonly L1UserServices _userServices; private readonly L2PlayerServices _playerServices; private readonly L3MapServices _mapServices; private readonly L4VillageServices _villageServices;
    
    public L4VillageController(L1UserServices userServices, L2PlayerServices playerServices, L3MapServices mapServices, L4VillageServices villageServices) {
        _userServices = userServices; _playerServices = playerServices; _mapServices = mapServices; _villageServices = villageServices;
    }



    // méthode special à enlever plus tard
    [HttpGet("getAllVillages")]
    public async Task<IActionResult> GetAllVillages() {
        List<VillageDto>? villages = await _villageServices.GetAllVillagesAsync();
        if(villages == null) { return BadRequest("Impossible d'accéder à tous les villages."); }
        return Ok(villages);
    }



    [HttpGet("getAllDatas")]
    public async Task<IActionResult> AllDatas(string playerName, int? posX, int? posY) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                int indexTile = _mapServices.GetIndexMapTile(posX ?? -1, posY ?? -1);
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile)) {
                        VillageDto? village = await _villageServices.GetAllDatas(mapTile.dataId);
                        if(village != null) { 
                            await _mapServices.OneTileReleaseLock(mapTile._id); await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                            return Ok(village); 
                        }
                    }
                    await _mapServices.OneTileReleaseLock(mapTile._id);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest("Impossible d'obtenir les informations sur ce village.");
    }


    [HttpPost("upgradeBuilding")]
    public async Task<IActionResult> UpgradeBuilding(string playerName, int? posX, int? posY, [FromBody] BuildingType buildingType) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                int indexTile = _mapServices.GetIndexMapTile(posX ?? -1, posY ?? -1);
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile)) {
                        if ( await _villageServices.UpgradeBuildingAsync(mapTile.dataId, buildingType) ) {
                            await _mapServices.OneTileReleaseLock(indexTile);  await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                            return Ok($"Le batiment {buildingType} a été upgrade.");
                        }
                    }
                    await _mapServices.OneTileReleaseLock(indexTile);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest($"Impossible d'upgrader le batiment {buildingType}.");
    }
}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

