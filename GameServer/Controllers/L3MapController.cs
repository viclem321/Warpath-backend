using GameServer.DTOs;
using GameServer.Models;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;




[ApiController]
[Route("api/user/{userName}/player/{playerName}/map")]
public class L3MapController : ControllerBase {
    
    private readonly L1UserServices _userServices; private readonly L2PlayerServices _playerServices; private readonly L3MapServices _mapServices;
    
    public L3MapController(L1UserServices userServices, L2PlayerServices playerServices, L3MapServices mapServices) {
        _userServices = userServices; _playerServices = playerServices; _mapServices = mapServices;
    }



    // méthode special à enlever plus tard
    [HttpGet("getAllMap")]
    public async Task<IActionResult> GetAllMap() {
        List<MapTile> map = await _mapServices.GetAllMapAsync(); return Ok(map);
    }


    [HttpGet("{indexTile}/getOneTile")]
    public async Task<IActionResult> GetOneTile(string playerName, int? indexTile) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetOneTile(player, indexTile ?? -1); if (mapTile != null) {
                    await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                    return Ok(mapTile);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest("Impossible de d'obtenir les informations sur cette tuile.");
    }




    [HttpPost("{indexTile}/attack/{indexTileToAttack}")]
    public async Task<IActionResult> Attack(string playerName, int? indexTile, int? indexTileToAttack, [FromBody] int? nSoldats) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile)) {
                        (bool successOperation, bool successAttack) = await _mapServices.Attack(mapTile, nSoldats ?? 0, indexTileToAttack ?? -1);
                        if(successOperation) {
                            await _mapServices.OneTileReleaseLock(indexTile ?? -1);  await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                            if(successAttack) { return Ok($"Vous avez vaincu le village situé à la position {indexTileToAttack}."); }
                            else { return Ok($"Vous avez été vaincu par le village situé à la position {indexTileToAttack}."); }
                        }
                    }
                    await _mapServices.OneTileReleaseLock(indexTile ?? -1);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest($"Impossible d'attaquer ce village.");
    }




}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

