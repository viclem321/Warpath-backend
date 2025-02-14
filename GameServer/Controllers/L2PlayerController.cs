using Microsoft.AspNetCore.Mvc;
using GameServer.Services;
using GameServer.DTOs;
using GameServer.Models;

namespace GameServer.Controllers;





[Route("api/user/{userName}/player/{playerName}")]
[ApiController]
public class L2PlayerController : ControllerBase
{

    private readonly L1UserServices _userServices; private readonly L2PlayerServices _playerServices;

    public L2PlayerController(L1UserServices userServices, L2PlayerServices playerServices) {
        _playerServices = playerServices;   _userServices = userServices;
    }



    [HttpGet("getDatas")]
    public async Task<IActionResult> GetDatas(string playerName) 
    {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                PlayerDto? playerDto = _playerServices.GetDatas(player); if(playerDto != null) {
                    await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                    return Ok(playerDto);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest("Impossible de d'obtenir les données de ce joueur.");
    }


    [HttpPost("createNewVillage")]
    public async Task<IActionResult> CreateNewVillage(string playerName, [FromBody] CreateNewVillageModel model)
    {
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            Player? player = await _playerServices.GetIdentityWithLock(user, playerName); if(player != null) {
                int newVillage = await _playerServices.CreateNewVillageAsync(player, model.locationX ?? 0, model.locationY ?? 0); if(newVillage != int.MaxValue) {
                    await _playerServices.ReleaseLock(player);  await _userServices.ReleaseLock(user);
                    return Ok(newVillage);
                }
                await _playerServices.ReleaseLock(player);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest("Impossible de créer un nouveau village pour ce joueur.");
    }

}







// MODELS USED FOR CLIENT REQUEST -------------------------------------------

public class CreateNewVillageModel {
    public int? locationX { get; set; } public int? locationY { get; set; }
}
