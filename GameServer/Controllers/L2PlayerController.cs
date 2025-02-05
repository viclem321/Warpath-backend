using Microsoft.AspNetCore.Mvc;
using GameServer.Services;
using GameServer.DTOs;

namespace GameServer.Controllers;





[Route("api/user/{userId}/player/{playerId}")]
[ApiController]
public class L2PlayerController : ControllerBase
{

    private readonly L2PlayerServices _playerServices;

    public L2PlayerController(L2PlayerServices playerServices) {
        _playerServices = playerServices;
    }



    [HttpGet("getDatas")]
    public async Task<IActionResult> GetDatas(string playerId)
    {
        PlayerDto? player = await _playerServices.GetDatas(User, playerId);
        if(player != null) { return Ok(player); } else { return BadRequest("Impossible de d'obtenir les données de ce joueur.");}
    }

    [HttpPost("createNewVillage")]
    public async Task<IActionResult> CreateNewVillage(string playerId)
    {
        PlayerDto? newPlayer = await _playerServices.CreateNewVillage(User, playerId);
        if(newPlayer != null) { return Ok(newPlayer); } else { return BadRequest("Impossible de créer un nouveau village.");}
    }

}



