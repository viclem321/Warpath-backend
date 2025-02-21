using GameServer.DTOs;
using GameServer.Models;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;




[ApiController]
[Route("api/user/{username}/player/{playerName}/map/{indexTile}/village/building")]
public class L5BuildingController : ControllerBase {
    
    private readonly L1UserServices _userServices; private readonly L2PlayerServices _playerServices; private readonly L3MapServices _mapServices; private readonly L4VillageServices _villageServices; private readonly L5BuildingServices _buildingServices;
    
    public L5BuildingController(L1UserServices userServices, L2PlayerServices playerServices, L3MapServices mapServices, L4VillageServices villageServices, L5BuildingServices buildingServices) {
        _userServices = userServices; _playerServices = playerServices; _mapServices = mapServices; _villageServices = villageServices; _buildingServices = buildingServices;
    }



    // méthode special à enlever plus tard
    [HttpGet("getAllBuildings")]
    public async Task<IActionResult> GetAllBuildings() {
        List<BuildingDto>? buildings = await _buildingServices.GetAllBuildingsAsync();
        if(buildings == null) { return BadRequest("Impossible d'accéder à tous les buildings."); }
        return Ok(buildings);
    }



    [HttpGet("{buildingType}/getAllDatas")]
    public async Task<IActionResult> AllDatas(string playerName, int? indexTile, BuildingType? buildingType) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile)) {
                        Village? village = await _villageServices.GetIdentityWithLock(mapTile.dataId); if(village != null) {
                            BuildingDto? buildingDto = await _buildingServices.GetOneBuildingDatas(village, buildingType ?? BuildingType.Hq); if (buildingDto != null) {
                                await _villageServices.ReleaseLock(village); await _mapServices.OneTileReleaseLock(mapTile._id); await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                                return Ok(buildingDto);
                            }
                            await _villageServices.ReleaseLock(village);
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


    [HttpPost("{buildingType}/upgradeBuilding")]
    public async Task<IActionResult> UpgradeBuilding(string playerName, int? indexTile, BuildingType? buildingType) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile) ) {
                        if ( await _villageServices.UpgradeBuildingAsync(mapTile.dataId, buildingType ?? BuildingType.Hq) ) {
                            await _mapServices.OneTileReleaseLock(indexTile ?? -1);  await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                            return Ok($"Le batiment {buildingType} a été upgrade.");
                        }
                    }
                    await _mapServices.OneTileReleaseLock(indexTile ?? 0);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest($"Impossible d'upgrader le batiment {buildingType}.");
    }





    [HttpPost("{buildingType}/recolt")]
    public async Task<IActionResult> Recolt(string playerName, int? indexTile, BuildingType? buildingType) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile)) {
                        Village? village = await _villageServices.GetIdentityWithLock(mapTile.dataId); if(village != null) {
                            int nRessources = await _buildingServices.RecoltAsync(village, buildingType ?? BuildingType.Hq); if(nRessources != int.MaxValue) {
                                await _villageServices.ReleaseLock(village); await _mapServices.OneTileReleaseLock(indexTile ?? -1); await _playerServices.ReleaseLock(player); await _userServices.ReleaseLock(user);
                                return Ok($"Le batiment {buildingType} a été récolté. L'entrepot contient maintenant {nRessources}.");
                            }
                            await _villageServices.ReleaseLock(village);
                        }
                    }
                    await _mapServices.OneTileReleaseLock(indexTile ?? -1);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest($"Erreur lors de la recolte du batiment: {buildingType}.");
    }



    [HttpPost("caserne/trainingTroops/{nSoldats}")]
    public async Task<IActionResult> TrainingTroops(string playerName, int? indexTile, int? nSoldats) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile)) {
                        Village? village = await _villageServices.GetIdentityWithLock(mapTile.dataId); if(village != null) {
                            bool sucessTraining = await _buildingServices.TrainingAsync(village, nSoldats ?? 0); if(sucessTraining == true) {
                                await _villageServices.ReleaseLock(village); await _mapServices.OneTileReleaseLock(indexTile ?? -1); await _playerServices.ReleaseLock(player); await _userServices.ReleaseLock(user);
                                return Ok($"Le caserne a commencé à entrainer {nSoldats} avec succes.");
                            }
                            await _villageServices.ReleaseLock(village);
                        }
                    }
                    await _mapServices.OneTileReleaseLock(indexTile ?? -1);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest($"Impossible d'entrainer {nSoldats} soldats.");
    }


    [HttpPost("caserne/endTraining")]
    public async Task<IActionResult> EndTraining(string playerName, int? indexTile) {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                MapTile? mapTile =  await _mapServices.GetIdentityOneTileWithLock(indexTile ?? -1); if (mapTile != null) {
                    if( mapTile.type == TileType.Village && _mapServices.OneTileIsOwnedByPlayer(player, mapTile)) {
                        Village? village = await _villageServices.GetIdentityWithLock(mapTile.dataId); if(village != null) {
                            bool sucessEndTraining = await _buildingServices.EndTrainingAsync(village); if(sucessEndTraining == true) {
                                await _villageServices.ReleaseLock(village); await _mapServices.OneTileReleaseLock(indexTile ?? -1); await _playerServices.ReleaseLock(player); await _userServices.ReleaseLock(user);
                                return Ok($"Les soldats ont fini d'être entrainer.");
                            }
                            await _villageServices.ReleaseLock(village);
                        }
                    }
                    await _mapServices.OneTileReleaseLock(indexTile ?? -1);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest($"Impossible de terminer l'entrainement des soldats maintenant.");
    }





}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

