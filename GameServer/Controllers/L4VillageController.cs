using Microsoft.AspNetCore.Mvc;
using Warpath.Shared.Catalogue;
using Warpath.Shared.DTOs;
using GameServer.Models;
using GameServer.Services;

namespace GameServer.Controllers;




[ApiController]
[Route("api/user/{username}/player/{playerName}/map/{villageTile}/village")]
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



    [HttpGet("getVillageDatas")]
    public async Task<IActionResult> GetVillageDatas(string playerName, int? villageTile) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(villageTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile) ) {
                        VillageDto? village = await _villageServices.GetVillageDatas(mapTile.dataId); if(village != null) { 
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



    [HttpPost("endUpgradeAction1")]
    public async Task<IActionResult> EndUpgradeAction1(string playerName, int? villageTile) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(villageTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile) ) {
                        if ( await _villageServices.EndUpgradeAction1Async(mapTile.dataId) ) {
                            await _mapServices.OneTileReleaseLock(villageTile ?? -1);  await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                            return Ok($"Le batiment a bien été upgradé");
                        }
                    }
                    await _mapServices.OneTileReleaseLock(villageTile ?? 0);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest($"Impossible de terminer la construction.");
    }


}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

