using GameServer.Models;
using GameServer.Services;
using Microsoft.AspNetCore.Mvc;


namespace GameServer.Controllers {

    [ApiController]
    [Route("api/global")]
    public class GlobalController : ControllerBase {
        
        private readonly GlobalServices _globalService;
        
        public GlobalController(GlobalServices globalService) {
            _globalService = globalService;
        }

        [HttpGet("get")]
        public async Task<IActionResult> GetAllVillages() {
            var villages = await _globalService.GetAllVillagesAsync();
            return Ok(villages);
        }

        [HttpGet("post")]
        public async Task<IActionResult> CreateVillage() {
            var newVillage = new Village("Bertrand", 1, 1);
            await _globalService.CreateVillageAsync(newVillage);
            var message = "Un nouveau village a été créé";
            return StatusCode(201, message);
        }

    }

}