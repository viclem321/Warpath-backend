using GameServer.DTOs;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;

namespace GameServer.Controllers;




[ApiController]
[Route("api/user/{userId}/player/{playerId}/village/{villageId}")]
public class L3VillageController : ControllerBase {
    
    private readonly L3VillageServices _villageServices;
    
    public L3VillageController(L3VillageServices villageServices) {
        _villageServices = villageServices;
    }



    // méthode special à enlever plus tard
    [HttpGet("getAllVillages")]
    public async Task<IActionResult> GetAllVillages() {
        List<VillageDto>? villages = await _villageServices.GetAllVillagesAsync();
        if(villages == null) { return BadRequest("Impossible d'accéder à tous les villages."); }
        return Ok(villages);
    }



    [HttpGet("getDatas")]
    public async Task<IActionResult> GetDatas(string playerId, string villageId) {
        VillageDto? village = await _villageServices.GetDatas(User, playerId, villageId);
        if(village != null) { return Ok(village); }   else { return BadRequest("Ce village n'existe pas ou est inaccesible"); }
    }


    [HttpPost("upgradeBuilding")]
    public async Task<IActionResult> UpgradeBuilding(string playerId, string villageId, [FromBody] int buildingType) {
        bool result = await _villageServices.UpgradeBuildingAsync(User, playerId, villageId, buildingType);
        if(result) { return Ok($"Le batiment {buildingType} a été upgrade."); } else { return BadRequest($"Echec de l'upgrade du batiment {buildingType}."); }
    }

}






// STRUCTURE REQUEST FROM CIENT ---------------------------------------

