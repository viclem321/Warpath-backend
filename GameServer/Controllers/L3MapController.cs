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


    [HttpGet("getOneTile/{posX}&{posY}")]
    public async Task<IActionResult> GetOneTile(string playerName, int? posX, int? posY) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetOneTile(player, posX ?? 0, posY ?? 0); if (mapTile != null) {
                    await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                    return Ok(mapTile);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest("Impossible de d'obtenir les informations sur cette tuile.");
    }
}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

