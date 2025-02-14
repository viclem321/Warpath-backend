using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GameServer.Services;
using GameServer.Models;
using GameServer.DTOs;

namespace GameServer.Controllers;





[Route("api")]
[ApiController]
public class L1UserController : ControllerBase
{

    private readonly L1UserServices _userServices;

    public L1UserController(L1UserServices userServices) {
        _userServices = userServices;
    }


    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterModel model)
    {
        bool result = await _userServices.RegisterAsync(model.Username, model.Password);
        if(result == true) { return Ok("Inscription enrigistré avec succès! "); } else { return BadRequest("Impossible de créer votre compte avec ces identifiants.");}
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginModel model)
    {
        string? token = await _userServices.LoginAsync(model.Username, model.Password);
        if(token != null) { return Ok(token); } else { return BadRequest("Impossible de se connecter avec ces identifiants.");}
    }



    [HttpPost("user/{userName}/createPlayer")]
    public async Task<IActionResult> CreatePlayer([FromBody] string newPseudo)
    {
        // get identity of user and verify if there isnt already a player for this user
        User? user = await _userServices.GetIdentityWithLock(User); if( user != null) {
            PlayerDto? newPlayer = await _userServices.CreatePlayer(user, newPseudo); if(newPlayer != null) {
                await _userServices.ReleaseLock(user);
                return Ok(newPlayer);
            }
            await _userServices.ReleaseLock(user);
        }
        return BadRequest("Impossible de créer un nouveau Player.");
    }

    [HttpGet("user/{userName}/getPlayerName")]
    public async Task<IActionResult> GetPlayerName()
    {
        User? user = await _userServices.GetIdentity(User); if( user != null) {
            string playerName = _userServices.GetPlayerName(user);
            if(playerName != "") { return Ok(playerName); }
        }
        return BadRequest("Impossible d'obtenir le Player pour cet user.");
    }

}







// MODELS USED FOR CLIENT REQUEST -------------------------------------------
public class RegisterModel {
    public string? Username { get; set; } public string? Password { get; set; }
}

public class LoginModel {
    public string? Username { get; set; } public string? Password { get; set; }
}



