using GameServer.DTOs;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;




[ApiController]
[Route("api/village")]
public class VillageController : ControllerBase {
    
    private readonly VillageServices _villageServices;
    
    public VillageController(VillageServices villageServices) {
        _villageServices = villageServices;
    }



    [HttpGet("getAllVillages")]
    public async Task<IActionResult> GetAllVillages() {
        List<VillageDto>? villages = await _villageServices.GetAllVillagesAsync();
        if(villages == null) { return BadRequest("Impossible d'accéder à tous les villages."); }
        return Ok(villages);
    }

    [HttpGet("getMyVillage/{idVillage}")]
    public async Task<IActionResult> GetOneVillage(string idVillage) {
        VillageDto? village = await _villageServices.GetVillageByIdAsync(idVillage);
        if(village != null) { return Ok(village); }   else { return BadRequest("Ce village n'existe pas ou est inaccesible"); }
    }

    [HttpPost("createVillage")]
    public async Task<IActionResult> CreateVillage() {
        bool result = await _villageServices.CreateVillageAsync();
        if(result == true) { return StatusCode(201, "Un nouveau village a été créé."); }
        else {return BadRequest("Impossible de creer un village.");}
    }

    [HttpPost("upgradeBuilding")]
    public async Task<IActionResult> UpgradeBuilding([FromBody] RequestUpgradeBuilding req) {
        bool result = await _villageServices.UpgradeBuildingAsync(req.idVillage, req.buildingType);
        if(result) { return Ok($"Le batiment {req.buildingType} a été upgrade."); }
        else { return BadRequest($"Echec de l'upgrade du batiment {req.buildingType}."); }
    }

}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

//for upgradeBuilding request
public class RequestUpgradeBuilding {
    public string? idVillage;
    public int buildingType;
}
