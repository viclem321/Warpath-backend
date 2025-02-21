using GameServer.Models;

namespace GameServer.DTOs;



public class VillageDto
{
    public List<BuildingDto> buildings { get; set; }

    public VillageDto(List<BuildingDto> pBuildings) {
        buildings = pBuildings;
    }

}



