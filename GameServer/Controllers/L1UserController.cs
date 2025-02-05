using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using GameServer.Services;
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



    [HttpPost("user/{userId}/createPlayer")]
    public async Task<IActionResult> CreatePlayer([FromBody] string newPseudo)
    {
        PlayerDto? newPlayer = await _userServices.CreatePlayer(User, newPseudo);
        if(newPlayer != null) { return Ok(newPlayer); } else { return BadRequest("Impossible de créer un nouveau Player.");}
    }

}







// MODELS USED FOR CLIENT REQUEST -------------------------------------------
public class RegisterModel {
    public string? Username { get; set; } public string? Password { get; set; }
}

public class LoginModel {
    public string? Username { get; set; } public string? Password { get; set; }
}



