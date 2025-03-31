using Warpath.Shared.Catalogue;
using Warpath.Shared.DTOs;
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
        MapDto? mapDto = await _mapServices.GetAllMapAsync(); if(mapDto != null) {
            return Ok(mapDto);
        }
        else { return BadRequest("Impossible de d'envoyer la MapDto au client."); }
    }


    [HttpGet("{indexTile}/getOneTile")]
    public async Task<IActionResult> GetOneTile(string playerName, int? indexTile) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetOneTile(player, indexTile ?? -1); if (mapTile != null) {
                    await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                    return Ok(mapTile.ToDto(player.pseudo));
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
                        (bool successOperation, RapportFightDto? rapport) = await _mapServices.Attack(mapTile, nSoldats ?? 0, indexTileToAttack ?? -1);
                        if(successOperation && rapport != null) {
                            await _mapServices.OneTileReleaseLock(indexTile ?? -1);  await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                            return Ok(rapport);
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

    [HttpGet("getOneRapport/{idRapport}")]
    public async Task<IActionResult> GetOneRapport(string? idRapport) {
        RapportFightDto? rapportFightDto = await _mapServices.GetOneRapport(idRapport ?? ""); if(rapportFightDto != null) {
            return Ok(rapportFightDto);
        }
        return BadRequest("Impossible d'obtenir ce rapport.");
    }




}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

